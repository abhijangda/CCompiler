using System;
using System.IO;
using LanguageGrammar;
using System.Collections.Generic;

namespace Compiler
{
	public class Parser
	{
		Lexer lexer;
		Grammar g;
		Stack <int> stackStates;
		Stack <Symbol> stackSymbols;
		public SymbolTable symTableTree;

		public Parser ()
		{
			string grammar = File.ReadAllText ("./../../language");
			g = new Grammar (grammar);
			g.items ();
			g.createActionTable ();
			g.createGoToTable ();

			//g.displayStates ();
			lexer = new Lexer ();
			stackStates = new Stack<int> ();
			stackSymbols = new Stack<Symbol> ();
			stackStates.Push (0);

			symTableTree = new SymbolTable (null);
		}

		public void startParsing2 ()
		{
			char c = (char)Console.Read ();
			string sym_str = c.ToString ();

			while (true)
			{
				ActionTable.Action action = g.actionTable.getAction (sym_str, stackStates.Peek ());
				if (action.type == ActionTable.Action.ActionType.Shift)
				{
					stackStates.Push (action.toState);
					c = (char)Console.Read ();
					sym_str = c.ToString ();
				}

				else if (action.type == ActionTable.Action.ActionType.Reduce)
				{
					Production p = action.reduceTo;
					for (int i = 0; i < p.body.Count; i++)
					{
						stackStates.Pop ();
					}

					stackStates.Push (g.gotoTable.getState (p.head.ToString (), stackStates.Peek ()));
					stackSymbols.Push (p.head);
				}

				else if (action.type == ActionTable.Action.ActionType.Accept)
				{
					Console.WriteLine ("Accepted");
					break;

				}
			}
		}

		public void startParsing3 ()
		{
			while (true)
			{
				Token t = lexer.scan ();
				Console.WriteLine (t.ToString ());
			}
		}
		private ASTNode getNode (Production p, SymbolTable symTable)
		{

			if (p.head.ToString () == "type-specifier")
				return new TypeNode ();
			if (p.body.Count == 1 && !(p.body [0] is Terminal))
				return null;
			if (p.head.ToString () == "param-id")
				return new IDArrayNode ();
			if (p.head.ToString () == "var-declaration")
				return new VariableDeclarationNode ();
			if (p.head.ToString () == "declarationList")
				return new DeclarationsListNode ();
			if (p.head.ToString () == "param-type-list")
				return new FunctionParamsNode ();
			if (p.head.ToString () == "param-list")
				return new FunctionParamsList ();
			if (p.head.ToString () == "fun-declaration")
				return new FunctionDeclarationNode ();
			if (p.head.ToString () == "fun-definition")
				return new FunctionNode ();
			if (p.head.ToString () == "compound-stmt")
				return new CompoundStatementNode ();
			if (p.head.ToString () == "local-declarations")
				return new LocalDeclarationsNode ();
			if (p.head.ToString () == "statement-list")
				return new StatementListNode ();
			if (p.head.ToString () == "selection-stmt")
				return new IfThenNode ();
			if (p.head.ToString () == "iteration-stmt")
				return new WhileNode ();
			if (p.head.ToString () == "return-stmt")
				return new ReturnNode ();
			if (p.head.ToString () == "break-stmt")
				return new BreakNode ();
			if (p.head.ToString () == "continue-stmt")
				return new ContinueNode ();
			if (p.head.ToString () == "expression-stmt")
				return new ExpressionStmtNode ();
			if (p.head.ToString () == "expression" ||
			    p.head.ToString () == "term" ||
			    p.head.ToString () == "simple-expression" ||
			    p.head.ToString () == "and-expression" ||
			    p.head.ToString () == "sum-expression" ||
			    p.head.ToString () == "unary-expression" ||
			    p.head.ToString () == "immutable")
				return new BinaryOperatorNode ();

			if (p.head.ToString () == "unary-rel-expression")
				return new UnaryOperatorNode ();

			if (p.head.ToString () == "call")
				return new FunctionCall ();

			if (p.head.ToString () == "arg-list")
				return new FunctionCallArgument ();

			if (((p.head.ToString () == "var-decl-id" ||
			    p.head.ToString () == "mutable")) &&
			    p.body.Count > 1)
				return new IDArrayNode ();

			return null;
		}

		public ASTNode startParsing ()
		{
			Token t = lexer.scan ();
			string sym_str = "";
			sym_str = t.ToString ();
			Stack <string> symbolsStack = new Stack<string> ();
			//Stack <ASTNode> nodeStack = new Stack<ASTNode> ();
			SymbolTable currentSymTable = symTableTree;
			Stack <ASTNode> nodeStack = new Stack<ASTNode> ();
			SymbolTable paramTable = new SymbolTable ();

			if (t!= null && t.tag == Tag.ID)
			{
				sym_str = "ID";
				//idStack.Push (t.ToString ());
				//nodeStack.Push (new IDNode (t.ToString ()));
				nodeStack.Push (new IDNode (t.ToString ()));
			}
			else if (t != null && t.tag == Tag.Character)
			{
				sym_str = "character";
				nodeStack.Push (new CharacterNode (t.ToString () [0]));
			}
			else if (t != null && t.tag == Tag.Number)
			{
				sym_str = "num";
				//numStack.Push (t.ToString ());
				//nodeStack.Push (new NumExprNode (t.ToString ()));
				nodeStack.Push (new NumExprNode (t.ToString ()));
			}

			else if (t!= null)
			{
				sym_str = t.ToString ();
				nodeStack.Push (new TokenNode (t));
			}

			//Console.WriteLine ("Pushing into nodeStack " + nodeStack.Peek ().ToString ());

			while (true)
			{
				ActionTable.Action action = null;

				//if (t!= null)
				//	Console.WriteLine ("symstr " + sym_str + " " + t.ToString ());
	
				action = g.actionTable.getAction (sym_str, stackStates.Peek ());

				if (action == null)
				{
					if (t!= null)
						Console.WriteLine ("Error with action " + sym_str + " " + t.ToString () + " " + stackStates.Peek ());
					else
						Console.WriteLine ("Error with action " + sym_str + " " + stackStates.Peek ());

//					Console.WriteLine ("wrwedfffffffffffffff");
//					foreach (Terminal to in g.firstDict ["blockstmt"])
//						Console.Write (to + " ");
//					Console.WriteLine ("FFFFFFFFFFF");
					return null;
				}

				if (action.type == ActionTable.Action.ActionType.Shift)
				{
					stackStates.Push (action.toState);
					//Console.WriteLine ("Pushing " + action.toState);
					t = lexer.scan ();
					//Console.WriteLine ("Readed " + t.ToString ());
					if (t!= null && t.tag == Tag.ID)
					{
						sym_str = "ID";
						//idStack.Push (t.ToString ());
						nodeStack.Push (new IDNode (t.ToString ()));
					}
					else if (t != null && t.tag == Tag.Character)
					{
						sym_str = "character";
						nodeStack.Push (new CharacterNode (t.ToString () [0]));
					}
					else if (t != null && t.tag == Tag.String)
					{
						sym_str = "string";
						nodeStack.Push (new StringNode (t.ToString ()));
					}
					else if (t != null && (t.tag == Tag.Number || t.tag == Tag.Real))
					{
						sym_str = "num";
						//numStack.Push (t.ToString ());
						nodeStack.Push (new NumExprNode (t.ToString ()));
					}

					else if (t!= null)
					{
						sym_str = t.ToString ();
						if (t.ToString () != "$")
							nodeStack.Push (new TokenNode (t));
					}

					//if (t != null && t.ToString () != "$")
					//	Console.WriteLine ("Pushing into nodeStack " + sym_str + " " + nodeStack.Peek ().ToString ());

					if (sym_str == "{")
					{
						SymbolTable symTable = new SymbolTable (currentSymTable);
						currentSymTable.children.Add (symTable);
						currentSymTable = symTable;
						//Console.WriteLine (" CREATING SYM TABLE ");
					}

					else if (sym_str == "}")
					{
						currentSymTable = currentSymTable.parent;
						//Console.WriteLine (" PARENTING SYMBOL TABLE ");
					}
				}

				else if (action.type == ActionTable.Action.ActionType.Reduce)
				{
					Production p = action.reduceTo;
					List<ASTNode> nodesToReduce = new List<ASTNode> ();
					ASTNode newNode = getNode (p, currentSymTable);
					ASTNode topNode = null;

					if (sym_str != "$")
				    	 topNode = nodeStack.Pop ();

					for (int i = 0; i < p.body.Count; i++)
					{
						stackStates.Pop ();
					    if (newNode != null)
						{
							ASTNode n = nodeStack.Pop ();
							Console.WriteLine ("popping " + n.ToString ());
							nodesToReduce.Add (n);
						}
					}
					//Console.WriteLine ("GOT " + p + " " + nodesToReduce.Count);

					TypeNode typeNode = null;

					if (p.head.ToString () == "var-declaration" ||
					    p.head.ToString () == "param-type-list" ||
					    p.head.ToString () == "fun-declaration" ||
					    p.head.ToString () == "fun-definition")
					{
						foreach (ASTNode node in nodesToReduce)
						{
							//Console.Write (node + " ");
							if (node is TypeNode)
							{
								typeNode = (TypeNode)node;
								break;
							}
						}
					}

					if (p.head.ToString () == "var-declaration")
					{
						for (int j = 0; j < nodesToReduce.Count; j++)
						{
							ASTNode node = nodesToReduce [j];
							if (node is IDArrayNode)
							{
								int width = -1, k;
								for (k = 0; k < node.listNodes.Count; k++)
								{
									if (node.listNodes [k] is NumExprNode)
									{
										width = int.Parse (((NumExprNode)node.listNodes [k]).value);
									}
									else if (node.listNodes [k] is ExprNode)
									{
										width = -2;
									}
								}

								//Console.WriteLine ("WIDTH IS " + width);
								if (width > -1)
								{
									currentSymTable.put (((IDNode)node).id, new ArrayType (typeNode.type, width));
								}
								else if (width == -2)
								{
									currentSymTable.put (((IDNode)node).id, 
									                     new ArrayType (typeNode.type,
									                                    (ExprNode)node.listNodes [k]));
								}

							}
							else if (node is IDNode)
							{
								currentSymTable.put (((IDNode)node).id, typeNode.type);
								//Console.WriteLine ("putting " + ((IDNode)((StatementNode)node).listNodes [0]).id + " with " + type.ToString ());
							}
						}
					}
					else if (p.head.ToString () == "param-type-list")
					{
						Console.WriteLine (p.ToString ());
						foreach (ASTNode node in nodesToReduce)
						{
							Console.WriteLine ("P  " + node);
							if (node is FunctionParamsNode)
							{
								paramTable.put (((IDNode)((FunctionParamsNode)node).listNodes [1]).id, typeNode.type);
								//Console.WriteLine ("putting PARAM" + (param_id)node + " with " + type.ToString ());
							}
						}
					}

					else if (p.head.ToString () == "fun-definition" ||
					         p.head.ToString () == "fun-declaration")
					{
						foreach (ASTNode node in nodesToReduce)
						{
							if (node is IDNode)
							{
								List<Type> listTypes = new List<Type> ();
								foreach (ASTNode node2 in nodesToReduce)
								{
									if (node2 is FunctionParamsList)
									{
										foreach (ASTNode node3 in node2.listNodes)
										{
											if (node3 is FunctionParamsNode)
											{
												listTypes.Add (((TypeNode)node3.listNodes [0]).type);
												if (p.head.ToString () == "fun-definition")
													currentSymTable.children [currentSymTable.children.Count - 1].put (((IDNode)node3.listNodes [1]).id, 
													                                                                   ((TypeNode)node3.listNodes [0]).type);
											}
										}
									}

									else if (node2 is FunctionParamsNode)
									{
										listTypes.Add (((TypeNode)node2.listNodes [0]).type);
										if (p.head.ToString () == "fun-definition")
											currentSymTable.children [currentSymTable.children.Count - 1].put (((IDNode)node2.listNodes [1]).id, ((TypeNode)node2.listNodes [0]).type);
									}
								}

								//Console.WriteLine ("PUTTING FUNCTION TYPE " + typeNode.type.ToString ());
								try
								{
									if (p.head.ToString () == "fun-definition")
										currentSymTable.put (((IDNode)node).id, new FunctionType(typeNode.type, listTypes, 
									                                                         FunctionType.DeclarationType.Definition));
									else
										currentSymTable.put (((IDNode)node).id, new FunctionType(typeNode.type, listTypes, 
									                                                         FunctionType.DeclarationType.Declaration));
								}

								catch (ArgumentException)
								{
									FunctionType type = (FunctionType)currentSymTable.getType (((IDNode)node).id);
									if (type.decl_type == FunctionType.DeclarationType.Declaration)
										type.decl_type = FunctionType.DeclarationType.Definition;
									else
									{
										//Console.WriteLine ("Error, function " + ((IDNode)node).id + " defined twice");
										return null;
									}
								}

								//currentSymTable.children [currentSymTable.children.Count - 1].addFromTable (paramTable);

								paramTable = new SymbolTable ();
								//Console.WriteLine ("putting " + (IDNode)node + " with " + typeNode.type.ToString ());
							}
						}
					}

					if (nodesToReduce.Count != 0)
					{
						nodesToReduce.Reverse ();
						newNode.addNodes (nodesToReduce);
						typeCheck (currentSymTable, newNode);
						nodeStack.Push (newNode);
					}

					if (topNode != null)
						nodeStack.Push (topNode);

					stackStates.Push (g.gotoTable.getState (p.head.ToString (), stackStates.Peek ()));
					t = null;
				}

				else if (action.type == ActionTable.Action.ActionType.Accept)
				{
					//Console.WriteLine ("Accepted");
					//Console.WriteLine ("ASTNode stack count is " + nodeStack.Count);
					nodeStack.Peek ().travelNode ();
					symTableTree.print ();
					return nodeStack.Peek ();
				}
			}
		}
	
		void typeCheck (SymbolTable currSymTable, ASTNode node)
		{
			if (node is BinaryOperatorNode)
			{
				BinaryOperatorNode bnode = (BinaryOperatorNode)node;
				if (bnode.op == Word.LogicalAnd || bnode.op == Word.LogicalOR)
				{
					ExprNode n1 = null, n2 = null;

					foreach (ASTNode pnode in node.listNodes)
					{
						if (pnode is ExprNode)
						{
							if (n1 == null)
								n1 = (ExprNode)(pnode);
							else
								n2 = (ExprNode)(pnode);
						}
					}

					Type t1 = n1.getType (currSymTable);
					Type t2 = n2.getType (currSymTable);
	
					if (t1 != Type.Integer)
						n1.castTo = Type.Integer;

					if (t2 != Type.Integer)
						n2.castTo = Type.Integer;
				}
				else if (bnode.op == Word.Addition ||
				         bnode.op == Word.Subtraction ||
				         bnode.op == Word.Division ||
				         bnode.op == Word.Multiply)
				{
					ExprNode n1 = bnode.expr1, n2 = bnode.expr2;

					Type t1 = n1.getType (currSymTable);
					Type t2 = n2.getType (currSymTable);
					Type t = t1;

					if (t1 != t2 && t1 != null && t2 != null)
					{
						t = Type.max (t1, t2);
						if (t == null)
						{
							Console.WriteLine ("Error Types do not match");
							return;
						}

						if (t != t1)
							n1.castTo = t;
						else
							n2.castTo = t;
					}
				}
				else if (bnode.op == Word.Equal)
				{
					ExprNode n1 = bnode.expr1, n2 = bnode.expr2;

					Type t1 = n1.getType (currSymTable);
					Type t2 = n2.getType (currSymTable);
					Type t = t1;
					if (t1 != t2)
					{
						t = Type.min (t1, t2);
						n2.castTo = t;
					}
				}
			}

			else if (node is FunctionCall)
			{
				IDNode idnode = (IDNode)node.listNodes [0];
				FunctionType type = (FunctionType)idnode.getType (currSymTable);
				int i = 1, j = 0;
				while (i < node.listNodes.Count && j < type.listParamTypes.Count)
				{
					if (node.listNodes [i] is FunctionCallArgument)
					{
						TypeNode tnode = (TypeNode)node.listNodes [i].listNodes [0];
						if (type.listParamTypes [j] != tnode.type)
						{
							Type t = Type.max (type.listParamTypes [j], tnode.type);
							if (t == type.listParamTypes [j])
								((ExprNode)node.listNodes [i]).castTo = t;

							else
							{
								Console.WriteLine ("Error with type expected " + type.listParamTypes [j]);
								break;	
							}
						}
						j++;
					}
					i++;
				}
			}

			else if (node is IDArrayNode)
			{
				bool firstExprNode = false;
				foreach (ASTNode n in node.listNodes)
				{
					if (n is ExprNode && firstExprNode == true)
					{
						if (((ExprNode)n).getType (currSymTable) != Type.Integer)
							Console.WriteLine ("Warning: Array subscript is not an Integer");

						break;
					}
					else if (n is ExprNode)
					{
						firstExprNode = true;
					}
				}
			}
		}
	}
}