using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

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
			string s = "";
			Dictionary <string, Type>.Enumerator e = rootSymTable.dict.GetEnumerator ();
			do
			{
				if (e.Current.Value != null)
				{
					if (e.Current.Value is FunctionType)
					{
						FunctionType ftype = (FunctionType)e.Current.Value;
						if (ftype.decl_type == FunctionType.DeclarationType.Declaration)
						{
							s += "extern " + e.Current.Key + "\n";
						}
					}
				}
			}
			while (e.MoveNext ());
			string code = s + rootNode.generateCode (rootSymTable);
			return code;
		}
	}

	public class VariableDescriptor
	{
		public string name {get; set;}
		public string memory_loc {get; set;}
		public string register {get; set;}
		public int size {get; set;}
		public bool isImmediate;
		public string value;
		public Type type;

		public VariableDescriptor (string name, int size, Type type = null, bool immediate = false, string value = "")
		{
			this.name = name;
			this.size = size;
			memory_loc = "";
			register = "";
			isImmediate = immediate;
			this.value = value;
			this.type = type;
		}

		public string get_mem_type ()
		{
			if (type.width == 4)
				return "dword ";
			if (type.width == 2)
				return "word ";
			if (type.width == 8)
				return "qword ";
			if (type.width == 1)
				return "byte ";
			return "";
		}

		public string get_loc ()
		{
			if (register != "")
				return register;

			return get_mem_type () + memory_loc;
		}
	}
	
	public class MemoryPos 
	{
		public int memory_pos;

		public MemoryPos ()
		{
			memory_pos = 0;
		}
	}

	public class MachineCodeGen
	{
		string intercode;
		Dictionary <string, VariableDescriptor> vd_dict;
		Stack <Dictionary <string, VariableDescriptor>> stack_vd_dict;
		Dictionary <string, VariableDescriptor> register_dict;
		Stack <Dictionary <string, VariableDescriptor> > register_dict_stack;
		Stack <MemoryPos> stack_memory_pos;
		Dictionary <string, List<string>> register_parts;
		MemoryPos memory_pos;
		string[] registers;
		int registers_iter;
		bool _reg_enum_traversed;
		bool reg_enum_traversed
		{
			get
			{
				return _reg_enum_traversed;
			}

			set
			{
				if (value == true)
				{
					registers_iter = 0;
				}

				_reg_enum_traversed = value;
			}
		}

		public MachineCodeGen (string intercode)
		{
			this.intercode = intercode;
			registers = new string[] {"rax", "rbx", "rcx", "rdx",
									  "rsi", "rdi", "rbp", "rsp",
									  "r8", "r9", "r10", "r11", 
									  "r12", "r13", "r14", "r15"};
			register_dict_stack = new Stack<Dictionary<string, VariableDescriptor>> ();
			registers_iter = 0;
			reg_enum_traversed = false;
			register_parts = new Dictionary<string, List<string>> ();
			for (int i = 0; i < 4; i++)
			{
				List <string> l = new List <string> ();
				string reg = registers [i];
				l.Add (reg.Substring (1).Replace ('x', 'l'));
				l.Add (reg.Substring (1));
				l.Add (reg.Replace ('r', 'e'));
				l.Add (reg);
				register_parts.Add (registers [i], l);
			}

			for (int i = 4; i < 8; i++)
			{
				List <string> l = new List <string> ();
				string reg = registers [i];
				l.Add (reg.Replace ('r', 'e'));
				l.Add (reg.Replace ('r', 'e'));
				l.Add (reg.Replace ('r', 'e'));
				l.Add (reg);
				register_parts.Add (registers [i], l);
			}

			for (int i = 8; i < registers.Length; i++)
			{
				List <string> l = new List <string> ();
				string reg = registers [i];
				l.Add (reg + "b");
				l.Add (reg + "w");
				l.Add (reg + "d");
				l.Add (reg);
				register_parts.Add (registers [i], l);
			}
			stack_memory_pos = new Stack<MemoryPos> ();
			stack_vd_dict = new Stack<Dictionary<string, VariableDescriptor>> ();
		}

		public void push_vd_dict ()
		{
			vd_dict = new Dictionary<string, VariableDescriptor> ();
			stack_vd_dict.Push (vd_dict);
		}

		public void pop_vd_dict ()
		{
			stack_vd_dict.Pop ();
		}

		public void push_memory_pos ()
		{
			memory_pos = new MemoryPos ();
			stack_memory_pos.Push (memory_pos);
		}

		public void pop_memory_push ()
		{
			stack_memory_pos.Pop ();
		}

		public void push_register_dict ()
		{
			register_dict = new Dictionary <string, VariableDescriptor> ();
			foreach (string s in registers)
				register_dict.Add (s, null);

			register_dict_stack.Push (register_dict);
		}

		public void pop_register_dict ()
		{
			register_dict_stack.Pop ();
		}

		public string allocate_register (ref VariableDescriptor vd)
		{
			if (vd.register != "")
				return "";

			if (!reg_enum_traversed)
			{
				do
				{
					if (register_dict[registers[registers_iter]] == null)
					{
						break;
					}
					registers_iter ++;
				}
				while (registers_iter < registers.Length);
				if (registers_iter == registers.Length)
					reg_enum_traversed = true;
			}

			if (register_dict[registers[registers_iter]] == null)
			{
				vd.register = registers[registers_iter];
				register_dict [vd.register] = vd;
				if (vd.isImmediate)
					return "\tmov " + registers[registers_iter] + ", " + vd.value + "\n";
			}

			string toReturn = "";
			toReturn += copy_reg_to_mem (registers[registers_iter], register_dict[registers[registers_iter]]);
			register_dict[registers[registers_iter]].register = "";
			if (vd.isImmediate)
			{
				toReturn += "\tmov " + registers[registers_iter] + ", " + vd.value + "\n";
			}
			else
			{
				toReturn += copy_mem_to_reg (registers[registers_iter], vd);
			}
			return toReturn;
		}

		public string allocate_memory (ref VariableDescriptor vd, int memory_pos)
		{
			vd.memory_loc = "[rbp - " + memory_pos + "]";
			return "\tsub rsp, " + vd.size.ToString () + "\n";
		}

		public string copy_mem_to_mem_int (VariableDescriptor vd1, VariableDescriptor vd2)
		{
			return "\tmov eax, dword " + vd1.memory_loc + "\n\tmov dword " + vd2.memory_loc + ", eax\n";
		}

		public string get_part_of_reg (string register, int size)
		{
			int i = size - 1;
			if (size == 4)
				i = 2;
			else if (size == 8)
				i = 3;

			return register_parts [register][i];
		}

		public int get_size_for_type (string type)
		{
			if (type == "byte")
				return 1;
			else if (type == "word")
				return 2;
			else if (type == "dword")
				return 4;
			else if (type == "qword")
				return 8;

			return 0;
		}

		public string get_part_of_reg (string register, string type)
		{
			return get_part_of_reg (register, get_size_for_type (type));
		}

		public string copy_mem_to_reg (string register, VariableDescriptor vd)
		{
			if (vd.get_loc ().Contains ("byte"))
				return "\tmov " + get_part_of_reg (register, 1) + ", " + vd.get_loc () + "\n";
			if (vd.get_loc ().Contains ("dword"))
				return "\tmov " + get_part_of_reg (register, 4) + ", " + vd.get_loc () + "\n";
			if (vd.get_loc ().Contains ("qword"))
				return "\tmov " + get_part_of_reg (register, 8) + ", " + vd.get_loc () + "\n";
			if (vd.get_loc ().Contains ("word"))
				return "\tmov " + get_part_of_reg (register, 2) + ", " + vd.get_loc () + "\n";
			return "";
		}

		public string copy_reg_to_mem (string register, VariableDescriptor vd)
		{
			if (vd.get_mem_type () == "byte ")
				return "\tmov " + vd.get_loc () + ", " + get_part_of_reg (register, 1) + "\n";
			if (vd.get_mem_type ().Contains ("word"))
				return "\tmov " + vd.get_loc () + ", " + get_part_of_reg (register, 2) + "\n";
			if (vd.get_mem_type ().Contains ("dword"))
				return "\tmov " + vd.get_loc () + ", " + get_part_of_reg (register, 4) + "\n";
			if (vd.get_mem_type ().Contains ("qword"))
				return "\tmov " + vd.get_loc () + ", " + get_part_of_reg (register, 8) + "\n";
			return "";
		}

		public string do_arith (ref VariableDescriptor resultvd, ref VariableDescriptor vd1,
		                        ref VariableDescriptor vd2, string op)
		{
			string opcode = "";
			if (op == "+")
			{
				opcode = "\tadd ";
			}
			else if (op == "-")
			{
				opcode = "\tsub ";
			}
			else if (op == "*")
			{
				opcode = "\tmul ";
			}
			else if (op == "/")
			{
				opcode = "\tdiv ";
			}
			string toReturn = "";
			toReturn += opcode + vd1.register + ", " + vd2.get_loc () +"\n";
			resultvd.register = vd1.register;
			vd1.register = "";
			register_dict [resultvd.register] = resultvd;
			return toReturn;
		}

		public string copy_vd_to_vd (ref VariableDescriptor vdres, ref VariableDescriptor vdop)
		{
			if (vdop.register != "")
			{
				return "\tmov " + vdres.memory_loc + ", " + vdop.register + "\n";
			}
			string toreturn = allocate_register (ref vdres);
			return toreturn;
		}
	
		public string setup_stack_frame ()
		{
			return 	"\tpush rbp\n\tmov rbp, rsp\n\n";
		}

		public string destroy_stack_frame ()
		{
			return "\n\tmov rsp, rbp\n\tpop rbp\n";
		}

		public bool isConstant (string variable, out string type)
		{
			if (variable.Length == 3 && variable.Substring (0, 1) == "\'" && variable.Substring (2, 1) == "\'")
			{
				type = "byte";
				return true;
			}

			int l;
			if (int.TryParse (variable, out l))
			{
				type = "dword";
				return true;
			}

			long a;
			if (long.TryParse (variable, out a))
			{
				type = "qword";
				return true;
			}
			type = "";
			return false;
		}

		public string genMachinCode (SymbolTable symTable)
		{
			string machineCode = "section .data\nsection .bss\nsection .text\n\tglobal main\n";
			MatchCollection linesmc = Regex.Matches (intercode, @".+", 
			                                         RegexOptions.Multiline);
			int iter = -1;
			//while (++iter < linesmc.Count)
			//	Console.WriteLine (linesmc [iter].Value);
			iter = 0;
			do
			{
				Match m = Regex.Match (linesmc[iter].Value, @"^extern\s[\w_][\w_\d]+");
				if (m.Success)
					machineCode += "\t" + m.Value + "\n";
				else
				{
					iter--;
					break;
				}
				iter ++;
			}
			while (true);

			SymbolTable curSymTable = null;
			do
			{
				Match m = null;
				string[] operators = new string [] {"+", "*", "/", "-"};
				int size = 4;

				/*Function definitions*/
				m = Regex.Match (linesmc [iter].Value, @"func\s+\w[\w\d]*:");
				if (m.Success)
				{
					string func = m.Value.Substring ("func".Length, 
					                                 m.Value.Length - "func".Length - 1).Trim ();
					machineCode += "\n\t" + func + ":\n";
					push_memory_pos ();
					push_vd_dict ();
					push_register_dict ();
					machineCode += setup_stack_frame ();
					curSymTable = symTable.getFuncTable (func);
					iter++;
					continue;
				}

				/*Return statement*/
				m = Regex.Match (linesmc [iter].Value, @"return\s*[\w\d]*");
				if (m.Success)
				{
					string variable = m.Value.Substring ("return".Length).Trim ();
					string type;

					if (isConstant (variable, out type))
					{
						machineCode += "\tmov " + get_part_of_reg ("rax", type) + ", " + variable + "\n";
					}
					else if (variable != "")
					{
						VariableDescriptor vd = vd_dict [variable];
						machineCode += copy_mem_to_reg ("rax", vd);
					}
					machineCode += destroy_stack_frame ();
					machineCode += "\tret\n\n";
					pop_memory_push ();
					pop_register_dict ();
					pop_vd_dict ();
				}

				foreach (string op in operators)
				{
					m = Regex.Match (linesmc[iter].Value, @"[\w\d]+\s*=\s*[\w\d]+\s*\" + op + @"\s*[\w\d]+");

					if (m.Success)
					{
						string result = m.Value.Substring (0, m.Value.IndexOf (" ="));
						string op1 = m.Value.Substring (m.Value.IndexOf ("=")+1, m.Value.IndexOf (" "+op) - m.Value.IndexOf ("=") - 1).Trim ();
						string op2 = m.Value.Substring (m.Value.IndexOf (op)+1).Trim ();


						if (!vd_dict.ContainsKey (result))
						{
							if (curSymTable.dict.ContainsKey (result))
							{
								size = curSymTable.dict [result].width;
								vd_dict.Add (result, new VariableDescriptor (result, curSymTable.dict [result].width, curSymTable.dict [result]));
							}
							else
								vd_dict.Add (result, new VariableDescriptor (result, size));
						}

						VariableDescriptor vd = vd_dict [result];
						VariableDescriptor vd1 = null, vd2 = null;
						int l;
						string type;
						if (isConstant (op1, out type))
						{
							/* Operand 1 is a constant */
							vd1 = new VariableDescriptor ("Constant" + op1, get_size_for_type (type), null, true, op1);
							machineCode += allocate_register (ref vd1);
						}

						else
						{
							vd1 = vd_dict [op1];
							machineCode += allocate_register (ref vd1);
						}

						if (int.TryParse (op2, out l))
						{
							vd2 = new VariableDescriptor ("Constant" + op2, get_size_for_type (type), null, true, op2);
							machineCode += allocate_register (ref vd2);
						}

						else
						{
							vd2 = vd_dict [op2];
						}

						memory_pos.memory_pos += size;
						machineCode += allocate_memory (ref vd, memory_pos.memory_pos);
						machineCode += do_arith (ref vd, ref vd1, ref vd2, op);
						machineCode += copy_reg_to_mem (vd.register, vd);
						break;
					}
				}

				if (m != null && m.Success)
				{
					iter ++;
					continue;
				}

				m = Regex.Match (linesmc[iter].Value, @"[\w\d]+\s*=\s*[\w\d]+");
				if (m.Success)
				{
					string result = m.Value.Substring (0, m.Value.IndexOf ("=")).Trim ();
					string op = m.Value.Substring (m.Value.IndexOf ("=")+1).Trim ();
					int l;
					VariableDescriptor res;
					if (!vd_dict.ContainsKey (result))
					{
						size = curSymTable.dict [result].width;
						res = new VariableDescriptor (result, size, curSymTable.dict [result]);
						vd_dict.Add (result, res);
						memory_pos.memory_pos += size;
						machineCode += allocate_memory (ref res, memory_pos.memory_pos);
					}
					else
						res = vd_dict [result];

					if (int.TryParse (op, out l))
					{
						machineCode += "\tmov " + res.get_loc () + ", " + l.ToString () + "\n";
					}
					else
					{
						VariableDescriptor opvd = vd_dict [op];
						machineCode += copy_vd_to_vd (ref res, ref opvd);
					}
					iter++;
					continue;
				}

				/*For Strings*/
				m = Regex.Match (linesmc[iter].Value, "[\\w\\d]+\\s*=\\s*\".+\"");
				if (m.Success)
				{
					string result = m.Value.Substring (0, m.Value.IndexOf ("=")).Trim ();
					string op = m.Value.Substring (m.Value.IndexOf ("=")+1).Replace ('"', ' ').Trim ();
					VariableDescriptor vd = null;
					if (vd_dict.ContainsKey (result))
						vd = vd_dict [result];
					else
					{
						ArrayType t = (ArrayType)curSymTable.dict [result];
						vd = new VariableDescriptor (result, t.width * t.type.width, t);
						memory_pos.memory_pos += vd.size;
						vd_dict.Add (result, vd);
						machineCode += allocate_memory (ref vd, memory_pos.memory_pos);
					}

					int i = vd.size, pos;
					foreach (char c in op.ToCharArray ())
					{
						if (i == 0)
							break;
						pos = memory_pos.memory_pos + i - vd.size;
						machineCode += "\tmov byte " + "[rbp - " + pos + "], \'" + c + "\'\n";
						i--;
					}
					pos = memory_pos.memory_pos + i - vd.size;
					machineCode += "\tmov byte " + "[rbp - " + pos + "], 0\n";
				}

				m = Regex.Match (linesmc[iter].Value, @"IfFalse\s+[\w\d]+\s+goto\s+[\w\d]+");
				if (m.Success)
				{
					string op = m.Value.Substring ("ifFalse".Length, m.Value.IndexOf ("goto") - "ifFalse".Length).Trim ();
					VariableDescriptor vd = vd_dict [op];
					machineCode += "\tsub " + vd.get_loc () + ", 0\n";
					string label = m.Value.Substring (m.Value.IndexOf ("goto") + "goto".Length).Trim ();
					machineCode += "\tjne " + label + "\n";
					iter++;
					continue;
				}

				m = Regex.Match (linesmc[iter].Value, @"goto\s+[\w\d]+");
				if (m.Success)
				{
					string label = m.Value.Substring (m.Value.IndexOf ("goto") + "goto".Length).Trim ();
					machineCode += "\tjmp " + label + "\n";
					iter++;
					continue;
				}

				m = Regex.Match (linesmc [iter].Value, @"call\s+\w[\w\d]+");
				if (m.Success)
				{
					int i = 0;
					int l;
					string[] func_registers = new string[] {"rdi", "rsi", "rdx", "rcx", "r8", "r9"};
					iter ++;

					push_register_dict ();
					while (iter < linesmc.Count && i < func_registers.Length && linesmc [iter].Value.Contains ("param"))
					{
						string arg = linesmc [iter].Value.Substring ("param".Length).Trim ();
						if (int.TryParse (arg, out l))
						{
							machineCode += "\tmov " + func_registers [i] + ", " + arg + "\n";
						}
						else
						{
							VariableDescriptor vd = vd_dict [arg];
							if (vd.get_loc ().Contains ("[") && vd.type is ArrayType)
							{
								machineCode += "\tmov " + func_registers [i] + ", rbp\n";
								machineCode += "\tsub " + func_registers [i] + ", " + vd.get_loc ().Substring (vd.get_loc ().IndexOf ("-")+1, vd.get_loc ().IndexOf ("]") - vd.get_loc ().IndexOf ("-") - 1).Trim () + "\n";
							}

							else
							{
								machineCode += copy_mem_to_reg (func_registers [i], vd);
							}
						}
						i++;
						iter++;							
					}

					machineCode += allign_stack (memory_pos.memory_pos);
					machineCode += "\tmov rax, 0\n\tcall " + m.Value.Substring ("call".Length).Trim () + "\n";
					pop_register_dict ();
					continue;
				}

				iter ++;
			}
			while (iter < linesmc.Count);

			return machineCode;
		}

		public string get_func_register (string reg, VariableDescriptor vd)
		{
			return "";
		}

		public string allign_stack (int mem_pos)
		{
			if (mem_pos % 16 == 0)
				return "";

			int i = 16 - mem_pos %16;
			return "\tsub rsp, " + i + "\n";
		}
	}
}