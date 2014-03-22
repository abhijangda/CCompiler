using System;
using System.Collections.Generic;
/*
namespace Compiler
{
	public class ParseTreeNode
	{
		public List <ParseTreeNode> listNodes;
		public ParseTreeNode ()
		{
			listNodes = new List<ParseTreeNode> ();
		}

		public virtual void travelNode (int level = 0)
		{
			for (int k = 0; k < level*2; k ++)
				Console.Write (" ");
			Console.WriteLine (ToString ());
			if (listNodes != null)
			{
				//Console.WriteLine ("sdfsdfsdffs");
				foreach (ParseTreeNode n in listNodes)
				{
					n.travelNode (level + 1);
				}
			}
		}
	}
	public class declarationList : ParseTreeNode
	{
	}
	public class param_list : ParseTreeNode
	{
	}
	public class fun_declaration : ParseTreeNode
	{
	}
	public class type_specifier : ParseTreeNode
	{
		public override string ToString ()
		{
			return ((TokenParseNode)listNodes [0]).t.ToString ();
		}
	}
	public class var_decl_id : ParseTreeNode
	{
	}
	public class var_decl_initialize : ParseTreeNode
	{
	}
	public class var_decl_list : ParseTreeNode
	{
	}
	public class var_declaration : ParseTreeNode
	{
	}
	public class declaration : ParseTreeNode
	{
	}
	public class local_declarations : ParseTreeNode
	{
	}

	public class statement_list : ParseTreeNode
	{
	}
	public class compound_stmt : ParseTreeNode
	{
		public SymbolTable symbolTable {get; private set;}
		public compound_stmt (SymbolTable symTable)
		{
			symbolTable = symTable;
		}
	}
	public class statement : ParseTreeNode
	{
	}
	public class param_id : ParseTreeNode
	{
	}
	public class param_type_list  : ParseTreeNode
	{
	}

	public class expression : ParseTreeNode
	{
		public Type castTo;
		public virtual Type getType (SymbolTable symTable)
		{
			foreach (ParseTreeNode n in listNodes)
			{
				if (n is simple_expression)
					return Type.Integer;

				if (n is and_expression)
					return Type.Integer;

				if (n is unary_rel_expression)
					return Type.Integer;

				if (n is rel_expression)
					return Type.Integer;

				if (n is sum_expression)
					return ((sum_expression)n).type;

				if (n is term)
					return ((term)n).type;

				if (n is constant || n is mutable)
					return symTable.getTypeForNode ((IDParseNode)n.listNodes [0]);

				if (n is call)
					return symTable.getType (((IDParseNode)n.listNodes [0]).id);
			}

			return null;
		}

		public override string ToString ()
		{
			if (castTo != null)
				return "expression " + "castTo " + castTo.ToString ();

			return "expression ";
		}
	}

	public class rel_expression : expression
	{
	}
	public class unary_rel_expression : expression
	{
	}
	public class and_expression : expression
	{
	}
	public class simple_expression : expression
	{
	}
	public class break_stmt : ParseTreeNode
	{
	}
	public class return_stmt : ParseTreeNode
	{
	}
	public class selection_stmt : ParseTreeNode
	{
	}
	public class iteration_stmt : ParseTreeNode
	{
	}
	public class expression_stmt  : ParseTreeNode
	{
	}		
			
	public class args : ParseTreeNode
	{
	}
	public class call : ParseTreeNode
	{
	}
	public class immutable : ParseTreeNode
	{
	}
	public class mutable : ParseTreeNode
	{
	}
	public class unary_expression : ParseTreeNode
	{
	}	
	public class mulop : ParseTreeNode
	{
	}
	public class term : expression
	{
		public Type type;

		public override Type getType (SymbolTable symTable)
		{
			return type;
		}
	}
	public class sumop : ParseTreeNode
	{
	}
	public class sum_expression : expression
	{
		public Type type;
		public override Type getType (SymbolTable symTable)
		{
			return type;
		}
	}

	public class relop : ParseTreeNode
	{
	}		
			
	public class constant : ParseTreeNode
	{
	}	

	public class arg_list : ParseTreeNode
	{
	}			

	public class IDParseNode : ParseTreeNode
	{
		public string id {get; private set;}
		public Type castTo;

		public IDParseNode (string _id)
		{
			castTo = null;
			id = _id;
		}

		public override void travelNode (int level = 0)
		{
			for (int k = 0; k < level*2; k++)
				Console.Write (" ");
			Console.WriteLine ("ID Node id = " + id + " castTo " + castTo);
		}
	}

	public class NumExprParseNode : IDParseNode
	{
		public NumExprParseNode (string s) : base (s)
		{
		}
	}

	public class ParseTree
	{
		public ParseTree ()
		{
		}
	}

	public class TokenParseNode : ParseTreeNode
	{
		public Token t;
		public TokenParseNode (Token _t)
		{
			t = _t;
		}

		public override void travelNode (int level = 0)
		{
			for (int k = 0; k < level*2; k ++)
				Console.Write (" ");

			Console.WriteLine (t.ToString ());
		}
	}
}*/

