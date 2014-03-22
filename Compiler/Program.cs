using System;
using System.IO;
using LanguageGrammar;
using System.Collections.Generic;

namespace Compiler
{
	class MainClass
	{
		public static short countInString (string str1, string str2)
		{
			short count = 0;
			int index = -1;
			index = str1.IndexOf (str2, index + 1);
			if (index == -1)
				return 0;
			while (index != -1)
			{
				count += 1;
				index = str1.IndexOf (str2, index + 1);
			}

			return count;
		}

		public static void Main (string[] args)
		{
			/*string grammar = File.ReadAllText ("./../../language");
			Grammar g = new Grammar (grammar);
			List<Terminal> l = new List<Terminal> ();
			l.Add (Grammar.EndMarker);
			Item i = new Item (g.startProduction, 0, l);
			State s = new State (0, i);

			g.createGoTo ();*/
			Parser p = new Parser ();
			//p.startParsing ();
			//CodeGenerator codeGen = new CodeGenerator (p.startParsing (), p.symTableTree);
			//Console.WriteLine (codeGen.generate ());
			InterCodeGen intercodegen = new InterCodeGen (p.startParsing (), p.symTableTree);
			string intercode = intercodegen.generate ();
			Console.WriteLine (intercode);
			//Console.WriteLine (new MachineCodeGen (intercode).genMachinCode ());
		}
	}
}
