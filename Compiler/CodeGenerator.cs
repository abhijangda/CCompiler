using System;
using System.Text.RegularExpressions;

namespace Compiler
{
	public class InterCodeGen
	{
		ASTNode rootNode;
		SymbolTable rootSymTable;
		public InterCodeGen (ASTNode rootNode, SymbolTable symTable)
		{
			this.rootNode = rootNode;
			this.rootSymTable = symTable;
		}

		public string generate ()
		{
			string code = rootNode.generateCode (rootSymTable);
			return code;
		}
	}

	public class MachineCodeGen
	{
		string intercode;
		public MachineCodeGen (string intercode)
		{
			this.intercode = intercode;
		}

		public string genMachinCode ()
		{
			string machineCode = "section .data\nsection .text\n\tglobal _start\n";
			MatchCollection mc = Regex.Matches (intercode, @"^extern\s[\w_][\w_\d]+",
			                                    RegexOptions.Multiline);
			foreach (Match m in mc)
			{
				machineCode += "\t"+m.Value;
			}
			return machineCode;
		}
	}
}

