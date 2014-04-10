using System;
using System.Collections.Generic;

namespace Compiler
{
	public class FunctionType : Type
	{
		public List<Type> listParamTypes;
		public Type returnType;
		public enum DeclarationType
		{
			Definition,
			Declaration,
		}
		public DeclarationType decl_type;

		public FunctionType (Type return_type, List<Type> param_types, DeclarationType decl_type) : base (return_type.width)
		{
			returnType = return_type;
			listParamTypes = param_types;
			this.decl_type = decl_type;
		}
	}

	public class ArrayType : Type
	{
		public Type type;
		public ExprNode widthNode;
		public ArrayType (Type type, int width) : base (type.width * width)
		{
			this.type = type;
			this.width = width;
		}

		public ArrayType (Type type, ExprNode node) : base (type.width * 1)
		{
			this.type = type;
			this.widthNode = node;
		}
	}

	public class SymbolTable
	{
		public SymbolTable parent;
		public List<SymbolTable> children;
		public Dictionary <string, Type> dict {get; private set;}
		protected SymbolTable prev;
		public string func;

		public SymbolTable (SymbolTable parent = null)
		{
			dict = new Dictionary<string, Type> ();
			this.parent = parent;
			children = new List<SymbolTable> ();
		}

		public SymbolTable getFuncTable (string func)
		{
			if (this.func == func)
				return this;

			foreach (SymbolTable symTable in children)
			{
				SymbolTable table = symTable.getFuncTable (func);
				if (table != null)
					return table;
			}

			return null;
		}

		public void put (string w, Type t)
		{
			dict.Add (w, t);
		}

		/*public Type getTypeForNode (IDParseNode node)
		{
			if (node is NumExprParseNode)
			{
				if (node.id.Contains ("."))
					return Type.Double;

				return Type.Integer;
			}

			return getType (node.id);
		}*/

		public Type getTypeForNode (ASTNode node)
		{

			if (node is NumExprNode)
			{
				if (((NumExprNode)node).value.Contains ("."))
					return Type.Double;

				return Type.Integer;
			}

			if (node is IDNode)
				return getType (((IDNode)node).id);

			return null;
		}

		public Type getType (string w)
		{
			SymbolTable symTable = this;
			do
			{
				if (symTable.dict.ContainsKey (w))
					return symTable.dict [w];

				symTable = symTable.parent;
			}
			while (symTable != null);

			return null;
		}

		public static void printSpaces (int level)
		{
			for (int k = 0; k < level*2; k++)
			{
				Console.Write (" ");
			}
		}

		public void print (int level = 0)
		{
			Console.WriteLine ("FUNC IS "+func);
			Dictionary <string, Type>.Enumerator e = dict.GetEnumerator ();
			do
			{
				printSpaces (level);
				Console.WriteLine (e.Current.Key + " " + e.Current.Value);
			}
			while (e.MoveNext ());
			foreach (SymbolTable symt in children)
				symt.print (level + 1);
		}

		public void addFromTable (SymbolTable table)
		{
			Dictionary <string, Type>.Enumerator e = table.dict.GetEnumerator ();
			do
			{
				if (e.Current.Key != null)
					dict.Add (e.Current.Key, e.Current.Value);
			}
			while (e.MoveNext ());
		}
	}
}