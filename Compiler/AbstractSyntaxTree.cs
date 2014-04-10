using System;
using System.Collections.Generic;

namespace Compiler
{
	public abstract class ASTNode 
	{
		static int labels = 0;
		public List <ASTNode> listNodes;
		protected string currentLabel, endLabel;
		public static int tempCount = 0;
		protected static string getTemporaryName ()
		{
			tempCount += 1;
			return "t" + tempCount;
		}

		public virtual void travelNode (int level = 0)
		{
			for (int i = 0; i < level * 2; i++)
				Console.Write (" ");
			Console.WriteLine (ToString ());
			if (listNodes != null)
			{
				//Console.WriteLine ("sdfsdfsdffs");
				foreach (ASTNode n in listNodes)
				{
					n.travelNode (level + 1);
				}
		    }
			else
			{
				Console.WriteLine ("LIST IS NULL");
			}
		}

		public virtual void addNodes (List<ASTNode> nodes)
		{
			foreach (ASTNode n in nodes)
			{
				if (n is TokenNode)
				{
					TokenNode tokenNode = ((TokenNode)n);
					if (tokenNode.token.tag == Tag.Comma || 
					    tokenNode.token.tag == Tag.EndBigBracket ||
					    tokenNode.token.tag == Tag.EndBrace ||
					    tokenNode.token.tag == Tag.StartBigBracket ||
					    tokenNode.token.tag == Tag.StartBrace ||
					    tokenNode.token.tag == Tag.StartParenthesis ||
					    tokenNode.token.tag == Tag.EndParenthesis ||
					    tokenNode.token.tag == Tag.EndOfLine ||
					    tokenNode.token.tag == Tag.If ||
					    tokenNode.token.tag == Tag.While)
						continue;
				}
				listNodes.Add (n);
			}
		}

		public virtual string generateCode (SymbolTable symTable)
		{
			string s = "";
			if (listNodes != null)
			{
				foreach (ASTNode node in listNodes)
					s += node.generateCode (symTable);
			}

			return s;
		}

		public string generateLabel ()
		{
			labels += 1;
			return "L" + labels.ToString ();
		}
	}

	public class EmptyNode : ASTNode
	{
		
	}

	public abstract class ExprNode : ASTNode
	{
		public abstract Type getType (SymbolTable symTable);
		public Type castTo;

		public virtual string reduce (SymbolTable symTable)
		{
			return generateCode (symTable);
		}
	}

	public class TokenNode : ASTNode
	{
		public Token token;
		public TokenNode (Token t)
		{
			token = t;
		}

		public override string ToString ()
		{
			return token.ToString ();
		}

		public override string generateCode (SymbolTable symTable)
		{
			return token.ToString ();
		}
	}

	public class CharacterNode : ExprNode
	{
		public char value;

		public CharacterNode (char value)
		{
			this.value = value;
		}

		public override void travelNode (int level = 0)
		{
			for (int i = 0; i < level * 2; i++)
				Console.Write (" ");
			if (castTo != null)
				Console.WriteLine ("Character value = " + value + " castTo = " + castTo);
			else
				Console.WriteLine ("Character value = " + value);
		}

		public override Compiler.Type getType (SymbolTable symTable)
		{
			return Type.Character;
		}

		public override string generateCode (SymbolTable symTable)
		{
			return value.ToString ();
		}
	}

	public class StringNode : ExprNode
	{
		public string value;

		public StringNode (string value)
		{
			this.value = value;
		}

		public override void travelNode (int level = 0)
		{
			for (int i = 0; i < level * 2; i++)
				Console.Write (" ");
			if (castTo != null)
				Console.WriteLine ("String value = " + value + " castTo = " + castTo);
			else
				Console.WriteLine ("String value = " + value);
		}

		public override Compiler.Type getType (SymbolTable symTable)
		{
			return new ArrayType (Type.Character, value.Length);
		}

		public override string generateCode (SymbolTable symTable)
		{
			return value.ToString ();
		}
	}

	public class NumExprNode : ExprNode
	{
		public string value;

		public NumExprNode (string value)
		{
			this.value = value;
		}

		public override void travelNode (int level = 0)
		{
			for (int i = 0; i < level * 2; i++)
				Console.Write (" ");
			if (castTo != null)
				Console.WriteLine ("NumExpr value = " + value + " castTo = " + castTo);
			else
				Console.WriteLine ("NumExpr value = " + value);
		}

		public override Compiler.Type getType (SymbolTable symTable)
		{
			if (value.Contains ("."))
				return Type.Double;
			return Type.Integer;
		}

		public override string generateCode (SymbolTable symTable)
		{
			return value;
		}
	}

	public class IDNode : ExprNode
	{
		public string id;

		public IDNode (string _id)
		{
			id = _id;
		}

		public override void travelNode (int level = 0)
		{
			for (int k = 0; k < level * 2; k++)
				Console.Write (" ");
			Console.WriteLine ("ID ASTNode id = " + id);
		}

		public override Type getType (SymbolTable symTable)
		{
			return symTable.getType (id);
		}

		public override string generateCode (SymbolTable symTable)
		{
			return id;
		}
	}

	public class ThreeAddressCode
	{
		public string place, code;
		public ThreeAddressCode (string place, string code)
		{
			this.place = place;
			this.code = code;
		}
	}

	public class BinaryOperatorNode : ExprNode
	{
		public Word op;
		public ExprNode expr1, expr2;

		public BinaryOperatorNode (Word t = null)
		{
			op = t;
			expr1 = null;
			expr2 = null;
		}

		public override void travelNode (int level = 0)
		{
			for (int i = 0; i < level * 2; i++)
				Console.Write (" ");
			if (op != null)
				Console.Write ("BinaryNode type = " + op.ToString ());
			else
				Console.Write ("BinaryNode");
			if (castTo != null)
				Console.Write (" castTo = " + castTo);
			Console.WriteLine ();
			expr1.travelNode (level + 1);
			expr2.travelNode (level + 1);
		}

		public override void addNodes (List<ASTNode> nodes)
		{
			foreach (ASTNode node in nodes)
			{
				if (node is TokenNode && ((TokenNode)node).token is Word)
				{
					op = ((Word)((TokenNode)node).token);
				}
				else
				{
					if (expr1 == null)
						expr1 = (ExprNode)node;
					else
						expr2 = (ExprNode)node;
				}
			}
		}

		public override Type getType (SymbolTable symTable)
		{
			if (castTo != null)
				return castTo;

			ExprNode n1 = expr1, n2 = expr2;

			Type t1 = n1.getType (symTable);
			if (n1.castTo != null)
				t1 = n1.castTo;

			Type t2 = n2.getType (symTable);
			if (n2.castTo != null)
				t2 = n2.castTo;

			Type t = t1;
			if (t1 != t2)
			{
				t = Type.max (t1, t2);
			}

			return t;
		}

		public ThreeAddressCode genTAC (SymbolTable symTable)
		{
			ThreeAddressCode tac1, tac2;
			if (expr1 is BinaryOperatorNode)
				tac1 = 	((BinaryOperatorNode)expr1).genTAC (symTable);
			else
			{
				if (expr1 is NumExprNode)
				{
					tac1 = new ThreeAddressCode (((NumExprNode)expr1).value, "");
				}
				else if (expr1 is IDArrayNode)
				{
					string code = ((IDArrayNode)expr1).generateCode (symTable);
					string id = code;
					if (code.Contains ("\n"))
						id = code.Trim ().Substring (code.Trim ().LastIndexOf ("\n") + 1);
					code = code.Substring (0, code.LastIndexOf (id));
					tac1 = new ThreeAddressCode (id, code);
				}
				else
				{
					tac1 = new ThreeAddressCode (((IDNode)expr1).id, "");
				}
			}

			if (expr2 is BinaryOperatorNode)
				tac2 = ((BinaryOperatorNode)expr2).genTAC (symTable);
			else
			{
				if (expr2 is NumExprNode)
				{
					tac2 = new ThreeAddressCode (((NumExprNode)expr2).value, "");
				}
				else if (expr2 is IDArrayNode)
				{
					string code = ((IDArrayNode)expr2).generateCode (symTable);
					string id = code;
					if (code.Contains ("\n"))
						id = code.Trim ().Substring (code.Trim ().LastIndexOf ("\n") + 1);
					code = code.Substring (0, code.LastIndexOf (id));
					tac2 = new ThreeAddressCode (id, code);
				}
				else
				{
					tac2 = new ThreeAddressCode (((IDNode)expr2).id, "");
				}
			}
			string t = getTemporaryName ();
			return new ThreeAddressCode (t, tac1.code + tac2.code + t + " = " + tac1.place + " " + op.ToString () + " " + tac2.place + "\n");
		}

		public override string generateCode (SymbolTable symTable)
		{
			tempCount = 0;
			ThreeAddressCode tac1, tac2;
			if (expr1 is BinaryOperatorNode)
				tac1 = 	((BinaryOperatorNode)expr1).genTAC (symTable);
			else
			{
				if (expr1 is NumExprNode)
				{
					tac1 = new ThreeAddressCode (((NumExprNode)expr1).value, "");
				}
				else if (expr1 is IDArrayNode)
				{
					string code = ((IDArrayNode)expr1).generateCode (symTable);
					string id = code;
					if (code.Contains ("\n"))
						id = code.Trim ().Substring (code.Trim ().LastIndexOf ("\n") + 1);
					code = code.Substring (0, code.LastIndexOf (id));
					tac1 = new ThreeAddressCode (id, code);
				}
				else
				{
					tac1 = new ThreeAddressCode (((IDNode)expr1).id, "");
				}
			}

			if (expr2 is BinaryOperatorNode)
				tac2 = 	((BinaryOperatorNode)expr2).genTAC (symTable);
			else
			{
				if (expr2 is NumExprNode)
				{
					tac2 = new ThreeAddressCode (((NumExprNode)expr2).value, "");
				}
				else if (expr2 is IDArrayNode)
				{
					string code = ((IDArrayNode)expr2).generateCode (symTable);
					string id = code;
					if (code.Contains ("\n"))
						id = code.Trim ().Substring (code.Trim ().LastIndexOf ("\n") + 1);
					code = code.Substring (0, code.LastIndexOf (id));
					tac2 = new ThreeAddressCode (id, code);
				}
				else if (expr2 is StringNode)
				{
					tac2 = new ThreeAddressCode ("\"" + ((StringNode)expr2).value + "\"", "");
				}
				else if (expr2 is CharacterNode)
				{
					tac2 = new ThreeAddressCode ("\'" + ((CharacterNode)expr2).value.ToString ()+"\'", "");
				}
				else
				{
					tac2 = new ThreeAddressCode (((IDNode)expr2).id, "");
				}

			}
			string t = getTemporaryName ();
			if (op != Word.Equal)
				return tac1.code+ tac2.code + t + " = " + tac1.place + " " + op.ToString () + " " + tac2.place + "\n";
			else
				return tac1.code + tac2.code + tac1.place + " " + op.ToString () + " " + tac2.place + "\n";
		}
	}

	public class UnaryOperatorNode : ExprNode
	{
		public ExprNode child1;
		public Word op;

		public UnaryOperatorNode (Word t = null)
		{
			op = t;
			listNodes = new List<ASTNode> ();
		}

		public override void addNodes (List<ASTNode> nodes)
		{
			foreach (ASTNode node in nodes)
			{
				if (node is TokenNode && ((TokenNode)node).token is Word)
				{
					op = ((Word)((TokenNode)node).token);
				}
			}

			base.addNodes (nodes);
		}

		public override Type getType (SymbolTable symTable)
		{
			return null;
		}

		public override string generateCode (SymbolTable symTable)
		{
			return "";
		}
	}

	public abstract class StatementNode : ASTNode
	{
		protected StatementNode ()
		{
			listNodes = new List<ASTNode> ();
		}
	}

	public class IfThenNode : StatementNode
	{
		ExprNode expr;
		StatementNode stmt;

		public override void addNodes (List<ASTNode> nodes)
		{
			foreach (ASTNode n in nodes)
			{
				if (n is TokenNode)
					continue;
				if (n is ExprNode)
					expr = (ExprNode)n;
				if (n is StatementNode)
					stmt = (StatementNode)n;
			}
		}

		public override void travelNode (int level)
		{
			for (int i = 0; i < level * 2; i++)
				Console.Write (" ");
			Console.WriteLine ("IfThen Node ");
			expr.travelNode (level + 1);
			stmt.travelNode (level + 1);
		}

		public override string generateCode (SymbolTable symTable)
		{
			string exprCode =  expr.generateCode (symTable);
			string s = "";
			if (exprCode.Contains ("\n"))
				s = exprCode.Trim ().Substring (exprCode.Trim ().LastIndexOf ("\n")+1);
			else
				s = exprCode;

			string toReturn = "";
			string t ;

			if (s.Contains ("="))
			{
				t = getTemporaryName ();
				toReturn = exprCode + t + " = " + s.Substring (0, s.IndexOf (" =")) + "\n";
			}
			else
			{
				t = exprCode;
			}
			string label = generateLabel ();
			toReturn += "IfFalse " + t + " goto " + label + "\n" + stmt.generateCode (symTable) + "\n" + label + ":" + "\n";
			return toReturn;
		}
	}

	public class CompoundStatementNode : StatementNode
	{
	}

	public class WhileNode : StatementNode
	{
		ExprNode expr;
		StatementNode stmt;

		public override void addNodes (List<ASTNode> nodes)
		{
			foreach (ASTNode n in nodes)
			{
				if (n is TokenNode)
					continue;
				if (n is ExprNode)
					expr = (ExprNode)n;
				if (n is StatementNode)
					stmt = (StatementNode)n;
			}
		}

		public override string generateCode (SymbolTable symTable)
		{
			currentLabel = generateLabel ();
			endLabel = generateLabel ();
			return currentLabel + ":\n" + "IfFalse " + expr.generateCode (symTable) + " goto " + endLabel + "\n" +
				stmt.generateCode (symTable) + "goto " + currentLabel + "\n" + endLabel + ":\n";
		}
	}

	public class DeclarationNode : StatementNode
	{
		TypeNode type;
		IDNode id;

		public override void addNodes (List<ASTNode> nodes)
		{
			foreach (ASTNode n in nodes)
			{
				if (n is TokenNode)
					continue;
				if (n is TypeNode)
					type = (TypeNode)n;
				if (n is IDNode)
					id = (IDNode)n;
			}
		}

		public override void travelNode (int level)
		{
			for (int i = 0; i < level * 2; i++)
				Console.Write (" ");
			Console.WriteLine ("DeclarationNode ");
			type.travelNode (level + 1);
			id.travelNode (level + 1);
		}
	}

	public class ContinueNode : StatementNode
	{
		public override void addNodes (List<ASTNode> nodes)
		{
			foreach (ASTNode n in nodes)
			{
				if (n is TokenNode)
					continue;
			}
		}
	}

	public class BreakNode : StatementNode
	{
		public override void addNodes (List<ASTNode> nodes)
		{
			foreach (ASTNode n in nodes)
			{
				if (n is TokenNode)
					continue;
			}
		}
	}

	public class ReturnNode : StatementNode
	{
		ExprNode expr;

		public override void addNodes (List<ASTNode> nodes)
		{
			foreach (ASTNode n in nodes)
			{
				if (n is TokenNode)
					continue;
				if (n is ExprNode)
					expr = (ExprNode)n;
			}
		}

		public override string generateCode (SymbolTable symTable)
		{
			if (expr == null)
				return "return \n";
			else
				return "return " + expr.generateCode (symTable) + "\n";
		}
	}

	public class ExpressionStmtNode : StatementNode
	{
		ExprNode expr;

		public override void travelNode (int level)
		{
			for (int i = 0; i < level * 2; i++)
				Console.Write (" ");
			Console.WriteLine ("ExpressionStmtNode ");
			expr.travelNode (level + 1);
		}

		public override void addNodes (List<ASTNode> nodes)
		{
			foreach (ASTNode n in nodes)
			{
				if (n is TokenNode)
					continue;
				if (n is ExprNode)
					expr = (ExprNode)n;
			}
		}

		public override string generateCode (SymbolTable symTable)
		{
			string s = "";
			return s + expr.generateCode (symTable);
		}
	}

	public class LocalDeclarationsNode : StatementNode
	{

	}

	public class StatementListNode : StatementNode
	{
	}

	public class DeclarationsListNode : StatementNode
	{
	}

	public class FunctionNode : StatementNode
	{
		IDNode id;
		TypeNode returnType;
		FunctionParamsList paramsList;
		StatementNode stmt;

		public override void travelNode (int level)
		{
			for (int i = 0; i < level * 2; i++)
				Console.Write (" ");
			Console.WriteLine ("FunctionNode " + id.id.ToString () + " " + returnType.ToString ());
			if (paramsList != null)
				paramsList.travelNode (level + 1);
			stmt.travelNode (level + 1);
		}

		public override string generateCode (SymbolTable symTable)
		{
			string toReturn = "func " + id.generateCode (symTable) + ":\n";
			toReturn += stmt.generateCode (symTable);
			if (!toReturn.Substring (toReturn.TrimEnd ().LastIndexOf ("\n")+1).Contains ("return"))
				toReturn += "return\n";
			return toReturn;
		}

		public override void addNodes (List<ASTNode> nodes)
		{
			foreach (ASTNode n in nodes)
			{
				if (n is IDNode)
					id = (IDNode)n;

				else if (n is TypeNode)
					returnType = (TypeNode)n;

				else if (n is FunctionParamsNode)
				{
					paramsList = new FunctionParamsList ();
					paramsList.listNodes.Add (n);
				}

				else if (n is FunctionParamsList)
				{
					paramsList = (FunctionParamsList)n;
				}

				else if (n is StatementNode)
					stmt = (StatementNode)n;
			}
		}
	}

	public class VariableDeclarationNode : DeclarationNode
	{
		public override string generateCode (SymbolTable symTable)
		{
			//return listNodes [0].generateCode (symTable) + " " + listNodes [1].generateCode (symTable) + "\n";
			return "";
		}
	}

	public class FunctionDeclarationNode : StatementNode
	{
		public override string generateCode (SymbolTable symTable)
		{
			return "";
		}
	}

	public class FunctionParamsNode : ASTNode
	{
		public FunctionParamsNode ()
		{
			listNodes = new List<ASTNode> ();
		}

		public override string generateCode (SymbolTable symTable)
		{
			string s = "";
			foreach (ASTNode node in listNodes)
				s += node.generateCode (symTable) + " ";
			return s;
		}
	}

	public class FunctionParamsList : ASTNode
	{
		public FunctionParamsList ()
		{
			listNodes = new List<ASTNode> ();
		}

		public override string generateCode (SymbolTable symTable)
		{
			string s = "";
			foreach (ASTNode node in listNodes)
				s += node.generateCode (symTable) + " ";
			return s;
		}
	}

	public class TypeNode : ASTNode
	{
		public Type type;

		public override void addNodes (List<ASTNode> nodes)
		{
			type = (Type)((TokenNode)nodes [0]).token;
		}
		public override void travelNode (int level = 0)
		{
			for (int i = 0; i < level * 2; i++)
				Console.Write (" ");
			Console.WriteLine ("Type ASTNode Type = " + type.ToString ());
		}

		public override string ToString ()
		{
			return "Type ASTNode Type = " + type.ToString ();
		}

		public override string generateCode (SymbolTable symTable)
		{
			return type.ToString ();
		}
	}

	public class FunctionCall : ExprNode
	{
		public FunctionCall ()
		{
			listNodes = new List<ASTNode> ();
		}

		public override Type getType (SymbolTable symTable)
		{
			return null;
		}

		public override string generateCode (SymbolTable symTable)
		{
			string toReturn = "";
			IDNode id = null;
			int length = 0;
			foreach (ASTNode n in listNodes)
			{
				if (n is FunctionCallArgument)
				{
					toReturn += n.generateCode (symTable);
					length++;
				}
				else if (n is IDNode)
				{
					id = (IDNode)n;
				}
			}

			toReturn = "call " + id.generateCode (symTable) + "\n" + toReturn;
			return toReturn;
		}
	}

	public class FunctionCallArgument : ExprNode
	{
		public FunctionCallArgument ()
		{
			listNodes = new List<ASTNode> ();
		}

		public override Type getType (SymbolTable symTable)
		{
			return null;
		}

		public override string generateCode (SymbolTable symTable)
		{
			string toReturn = "";
			foreach (ASTNode n in listNodes)
			{
				if (n is FunctionCallArgument)
					toReturn += n.generateCode (symTable);
				else if (n is ExprNode)
					toReturn += "param " + n.generateCode (symTable) + "\n";
			}
			return toReturn;
		}
	}

	public class IDArrayNode : IDNode
	{
		public ExprNode index;

		public IDArrayNode () : base ("")
		{
			listNodes = new List<ASTNode> ();
			index = null;
		}

		public override void travelNode (int level = 0)
		{
			for (int i = 0; i < level * 2; i++)
				Console.Write (" ");
			Console.WriteLine ("ArrayNode id = " + id);
			if (index != null)
				index.travelNode (level + 1);
		}

		public override void addNodes (List<ASTNode> nodes)
		{
			foreach (ASTNode node in nodes)
			{
				if (node is IDNode && id == "")
				{
					id = ((IDNode)node).id;
				}

				else if (node is ExprNode && index == null)
					index = (ExprNode)node;
			}
		}

		public override string generateCode (SymbolTable symTable)
		{
			string exprCode = "";
			if (index != null)
				exprCode = index.generateCode (symTable);
			string s = "";
			if (exprCode.Contains ("\n"))
				s = exprCode.Trim ().Substring (exprCode.Trim ().LastIndexOf ("\n")+1);
			else
				s = exprCode;

			string toReturn = "";
			string t;
			if (s.Contains ("="))
			{
				t = getTemporaryName ();
				toReturn = exprCode + t + " = " + s.Substring (0, s.IndexOf (" =")) + "\n";
			}
			else
			{
				t = exprCode;
			}

			toReturn += id + " [ " + t + " ] ";
			return toReturn;
		}
	}
}