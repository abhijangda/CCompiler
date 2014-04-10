using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Compiler;

namespace LanguageGrammar
{
	public abstract class Symbol
	{
		protected string value;
		protected Production production;

		protected Symbol (string _v, Production _p)
		{
			value = _v;
			production = _p;
		}

		public override string ToString ()
		{
			return value.Trim ();
		}
	}

	public class Terminal : Symbol
	{
		public Terminal (string _v, Production _p) : base (_v, _p)
		{
		}

		public override string ToString ()
		{
			return value.Trim ();
		}
	}

	public class ID : Terminal
	{
		public string id;

		public ID (string value, Production _p) : base ("ID", _p)
		{
			id = value;
		}
	}
	
	public class Num : Terminal
	{
		public string num;
		public Num (string _num, Production _p) : base ("num", _p)
		{
			num = _num;
		}
	}

	public class NonTerminal : Symbol
	{
		public NonTerminal (string _v, Production _p) : base (_v, _p)
		{
		}

		public override string ToString ()
		{
			return value.Trim ();
		}
	}

	public class Production
	{
		public NonTerminal head {get; private set;}
		public List <Symbol> body {get; private set;}
		string productionStr;

	    public Production (string s)
		{
			short i = (short)s.IndexOf ("->");
			if (i == -1)
				throw new Exception ("Cannot find '->' ");

			head = new NonTerminal (s.Substring (0, i), this);
			body = new List<Symbol> ();
			foreach (string _s in s.Substring (i + 2).Split (" ".ToCharArray (), 100))
			{
				if (_s == "")
					continue;

				short count = MainClass.countInString (_s, "'");
				if (count != 0 && count % 2 == 0)
					body.Add (new Terminal (_s.Trim ().Substring (1, _s.Trim ().Length - 2), this));

				else
					body.Add (new NonTerminal (_s.Trim (), this));
			}

			productionStr = s;
		}

		public override string ToString ()
		{
			return productionStr;
		}
	}

	public class Item
	{
		public short dotAt {get; private set;}
		public Production production {get; private set;}
		//public List<Terminal> lookaheads;
		public Terminal lookahead;

		public Item (Production p, short _dotAt, Terminal _lookahead)
		{
			dotAt = _dotAt;
			production = p;
			lookahead = _lookahead;
		}

		public override string ToString ()
		{
			string s = production.head.ToString () + " -> ";
			for (int i = 0; i < dotAt; i++)
				s += production.body [i].ToString () + " ";

			s += ".";

			for (int i = dotAt; i < production.body.Count; i++)
				s += production.body [i].ToString () + " ";

			if (lookahead != null)
				s += ", [ " + lookahead.ToString () + " ]";

			return s;
		}

		public override bool Equals (object obj)
		{
			if (! (obj is Item))
				return false;
			Item _item = (Item)obj;
			if (production == _item.production && _item.dotAt == dotAt)
			{
				if (lookahead.ToString () != _item.lookahead.ToString ())
					return false;

				return true;
			}
			return false;
		}
	}

	public class State : List<Item>
	{
		public short stateNumber;
		public Dictionary <string, State> dictStateOnSymbol;
		public Item closureOf;

		public State (short no, Item _closureOf) : base ()
		{
			closureOf = _closureOf;
			stateNumber = no;
			dictStateOnSymbol = new Dictionary<string, State> ();
			if (closureOf != null)
				Add (closureOf);
		}

		public override bool Equals (object obj)
		{
			if (!(obj is State))
				return false;

			State s2 = (State)obj;

			foreach (Item _item in this)
			{
				foreach (Item item in s2)
				{
					if (!_item.Equals (item))
						return false;
				}
			}

			return true;
		}

	}
    
	public class ActionTable
	{
		public class Action
		{
			public enum ActionType
			{
				Shift,
				Reduce,
				Accept,
			}

			public ActionType type {get; private set;}
			public int toState {get; private set;}
			public int atState {get; private set;}
			public Production reduceTo;

			public Action (ActionType _t, int _toState, int _atState, Production _r)
			{
				type = _t;
				toState = _toState;
				atState = _atState;
				reduceTo = _r;
			}
		}

		Dictionary <string, List<Action>> dict;

		public ActionTable ()
		{
			dict = new Dictionary<string, List<Action>> ();
		}

		public void AddEntry (string s, ActionTable.Action.ActionType type,
		                      int atState, int toState, Production reduceto = null)
		{
			//Console.WriteLine ("Adding " + s + " " + atState + " " + toState);
			List<Action> l;
			if (!dict.ContainsKey (s))
			{
				//Console.WriteLine ("Dont contains " + s);
				l = new List<Action> ();
				dict.Add (s, l);
			}

			dict.TryGetValue (s, out l);
			foreach (ActionTable.Action act in l)
				if (act.atState == atState && act.toState == toState && act.type == type)
					return;

			l.Add (new Action (type, toState, atState, reduceto));
		}

		public Action getAction (string s, int currentState)
		{
			foreach (Action se in dict [s])
			{
				if (se.atState == currentState)
					return se;
			}

			return null;
		}

		public override string ToString ()
		{
			string s = "";
			Dictionary <string, List<Action>>.Enumerator e = dict.GetEnumerator ();
			do
			{
				s += e.Current.Key;

				if (e.Current.Value != null)
				{
					foreach (Action a in dict [e.Current.Key])
					{
						s += " " + a.atState.ToString () + " " + a.toState.ToString () + " " + a.type.ToString ();
						if (a.type == Action.ActionType.Reduce)
							s += " " + a.reduceTo.ToString ();
					}
				}

				s += "\n";
			}
			while (e.MoveNext ());

			return s;
		}
	}


	public class GoToTable
	{
		private class StateEntry 
		{
			public int atState, toState;
			public string overSymbol;

			public StateEntry (int at, int to, string _o)
			{
				atState = at;
				toState = to;
				overSymbol = _o;
			}
		}

		Dictionary <string, List <StateEntry>> dict;

		public GoToTable ()
		{
			dict = new Dictionary<string, List<StateEntry>> ();
		}

		public void AddEntry (string s, int atState, int toState)
		{
			//Console.WriteLine ("Adding " + s + " " + atState + " " + toState);
			List<StateEntry> l;
			if (!dict.ContainsKey (s))
			{
				//Console.WriteLine ("Dont contains " + s);
				l = new List<StateEntry> ();
				dict.Add (s, l);
			}

			dict.TryGetValue (s, out l);
			l.Add (new StateEntry (atState, toState, s));
		}

		public int getState (string s, int currentState)
		{
			foreach (StateEntry se in dict [s])
			{
				if (se.atState == currentState)
					return se.toState;
			}
			return -1;
		}

		public override string ToString ()
		{
			Dictionary <string, List<StateEntry>>.Enumerator e = dict.GetEnumerator ();
			string s = "";
			do
			{
				s += e.Current.Key + " ";
				if (e.Current.Value != null)
				{
					foreach (StateEntry i in e.Current.Value)
					{
						s +=  " at " + i.atState + " to " + i.toState;
					}
				}
				s += "\n";
			}
			while (e.MoveNext ());
			return s;
		}
	}

	public class Grammar
	{
		string stringRep;
		public Dictionary <string, List<Production> > dict;
		public Production startProduction;
		private bool start;
		public List<Symbol> listSymbol;
		public List<State> listState;
		public static Terminal EndMarker, Epsilon;
		public Dictionary <string, List<Terminal>> firstDict;
		public GoToTable gotoTable;
		public ActionTable actionTable;

		static Grammar ()
		{
			EndMarker = new Terminal ("$", null);
			Epsilon = new Terminal ("epsilon", null);
		}

		private void addToDict (string production_str)
		{
			if (production_str.Trim () == "")
				return;
			//Console.WriteLine ("adding "+ production_str);
			Production p = new Production (production_str);
			List<Production> listP;
			if (dict.ContainsKey (p.head.ToString ()))
			{
				dict.TryGetValue (p.head.ToString (), out listP);
				listP.Add (p);
			}

			else
			{
				listP = new List<Production> ();
				listP.Add (p);
				dict.Add (p.head.ToString (), listP);
			}

			if (start == false)
			{
				startProduction = p;
				start = true;
			}

			foreach (Symbol s in p.body)
			{
				bool contains = false;
				foreach (Symbol sym in listSymbol)
				{
					if (sym.ToString () == s.ToString ())
					{
						contains = true;
						break;
					}
				}

				if (!contains)
					listSymbol.Add (s);
			}
		}

		public List<Terminal> First (Symbol X)
		{
			if (firstDict.ContainsKey (X.ToString ()))
			{
				return firstDict [X.ToString ()];
			}

			List<Terminal> symbol = new List<Terminal> ();
			if (X is Terminal)
				symbol.Add ((Terminal)X);

			else
			{
				List<Production> production = dict [X.ToString ()];
				foreach (Production p in production)
				{
					if (p.body [0].ToString () != X.ToString ())
					{
						List<Terminal> listS = First (p.body [0]);
						foreach (Terminal s in listS)
						{
							if (!symbol.Contains (s))
								symbol.Add (s);
						}
					}
				}
			}
			firstDict.Add (X.ToString (), symbol);
			return symbol;
		}

		public Grammar (string s)
		{
			firstDict = new Dictionary<string, List<Terminal>> ();
			start = false;
			dict = new Dictionary<string, List<Production>> ();
			stringRep = s;
			listSymbol = new List<Symbol> ();
			MatchCollection mc = Regex.Matches (s, ".+");
			actionTable = new ActionTable ();

			foreach (Match m in mc)
			{
				addToDict (m.Value);
			}

			stringRep = s;
			gotoTable = new GoToTable ();
		}

		public void Closure (ref State listItem)
		{
			int i = 0;
			while (i < listItem.Count)
			{
				//Console.WriteLine ("I="+i);
				if (listItem [i].dotAt < listItem [i].production.body.Count)
				{
					Symbol s = listItem [i].production.body [listItem [i].dotAt];
					if (listItem.stateNumber == 9)
						Console.WriteLine ("Symbol is " + s);
					if (s is NonTerminal)
					{
						NonTerminal nt = (NonTerminal)s;
						//Console.WriteLine (nt);
						foreach (Production p in dict [nt.ToString ()])
						{
							bool present = false;
							List<Terminal> listFirstTerminal;
							//Console.WriteLine (p);
							if (listItem [i].dotAt + 1 < listItem [i].production.body.Count)
							{
								Symbol beta = listItem [i].production.body [listItem [i].dotAt + 1];

								//As here no production derives empty string hence 
								//no need to create a new NonTerminal of beta a
								listFirstTerminal = First (beta);
							}

							else
							{
								listFirstTerminal = new List<Terminal> ();
								listFirstTerminal.AddRange (First (listItem [i].lookahead));
							}

							foreach (Terminal _lookahead in listFirstTerminal)
							{
								bool found = false;
								foreach (Item item in listItem)
								{
									if (item.production == p && item.dotAt == 0)
									{
										if (item.lookahead.ToString () == _lookahead.ToString ())
										{
											found = true;
											break;
										}
									}
								}

								if (found == false)
								{
									listItem.Add (new Item (p, 0, _lookahead));
								}
							}
						}
					}
				}
				//Console.WriteLine (i);
				i++;
			}
		}

		public State goTo (State I, Symbol X)
		{
			State listItem = new State (((short)listState.Count), null);
			foreach (Item item in I)
			{
				if (item.dotAt < item.production.body.Count &&
				    X.ToString () == item.production.body [item.dotAt].ToString ())
				{
					listItem.Add (new Item (item.production, (short)(item.dotAt + 1), item.lookahead));
				}
			}

			if (listItem.Count != 0)
			{
				Closure (ref listItem);
			}

			return listItem;
		}

		public void items ()
		{
			List<Terminal> l = new List<Terminal> ();
			l.Add (Grammar.EndMarker);

			State I0 = new State (0, new Item (startProduction, 0, Grammar.EndMarker));
			Closure (ref I0);
			listState = new List<State> ();
			listState.Add (I0);
			//return;
			//Console.WriteLine (I0.ToString ());
			int i = 0;
			while (i < listState.Count)
			{
				foreach (Symbol s in listSymbol)
				{
					State gotoState = goTo (listState [i], s);
					if (gotoState.Count != 0)
					{
						bool isSameItem = false;
						foreach (State s2 in listState)
						{
							isSameItem = false;
							if (s2.Count == gotoState.Count)
							{
								foreach (Item item2 in s2)
								{
									isSameItem = false;
									foreach (Item item in gotoState)
									{
										if (item2.dotAt == item.dotAt &&
										    item2.production.ToString () == item.production.ToString ())
										{
											if (item.lookahead.ToString () == item2.lookahead.ToString ())
											{
												isSameItem = true;
												break;
											}
										}
									}

									if (isSameItem == false)
									{
										break;
									}
								}
							}

							if (isSameItem == true)
							{
								gotoState = s2;
								break;
							}
						}

//						Console.WriteLine ("present is " + isSameItem);
						if (!listState [i].dictStateOnSymbol.ContainsKey (s.ToString ()))
						{
							//Console.WriteLine ("Adding transition from State " + listState [i].stateNumber + " to  " + gotoState.stateNumber + " over "+ s);
							listState [i].dictStateOnSymbol.Add (s.ToString (), gotoState);
							if (isSameItem == true)
							{
								/*foreach (Item item in gotoState)
								{
									Console.Write (item.ToString () + ",,,,");
									//foreach (Terminal t in item.lookaheads)
									//	Console.Write (t.ToString ());

									Console.WriteLine ();					
								}*/
							}
							else
							    listState.Add (gotoState);
						}
					}
				}
				i++;
			}


		}

		public void createActionTable ()
		{
			foreach (State state in listState)
			{
				foreach (Item item in state)
				{
					//foreach (Terminal lookahead in item.lookaheads)
					Terminal lookahead = item.lookahead;
					{
						if (item.dotAt < item.production.body.Count)
						{
							Symbol p = item.production.body [item.dotAt];
							if (p is Terminal)
							{
								if (state.dictStateOnSymbol.ContainsKey (p.ToString ()))
								{
									State next = state.dictStateOnSymbol [p.ToString ()];
									actionTable.AddEntry (p.ToString (),
									                      ActionTable.Action.ActionType.Shift, 
									                      state.stateNumber,
									                      next.stateNumber);
								}
							}
						}

						else
						{
							if (item.production.head.ToString () != "stmts`" )//&& item.production.body [item.dotAt -1] is Terminal)
							{
								actionTable.AddEntry (
									lookahead.ToString (),
									ActionTable.Action.ActionType.Reduce, 
									state.stateNumber, -1, item.production);
							}

							else if (item.production.head.ToString () == "stmts`" && lookahead == Grammar.EndMarker)
							{
								actionTable.AddEntry (lookahead.ToString (),
								                      ActionTable.Action.ActionType.Accept,
								                      state.stateNumber, -1, item.production);
							}
						}
					}
				}
			}
		}

		public void createGoToTable ()
		{
			foreach (State st in listState)
			{
				Dictionary<string, State>.Enumerator e = st.dictStateOnSymbol.GetEnumerator ();
				do
				{
					string s = e.Current.Key;
					bool found = false;
					foreach (Symbol sym in listSymbol)
					{
						if (sym is NonTerminal && sym.ToString () == s)
						{
							found = true;
							break;
						}
						
					}

					if (found == true)
					{
						gotoTable.AddEntry (s, st.stateNumber, e.Current.Value.stateNumber);
					}
				}
				while (e.MoveNext ());
			}
		}

		public void displayStates ()
		{
			foreach (State sta in listState)
			{
				Console.WriteLine ("State {0}", sta.stateNumber);
				foreach (Item item in sta)
				{
					Console.WriteLine (item.ToString ());
				}

				Console.Write ("\n\n\n\n\n\n");
			}

			//Console.WriteLine (listState.Count);
			Console.WriteLine (gotoTable.ToString ());
			Console.WriteLine (actionTable.ToString ());
		}
	}
}