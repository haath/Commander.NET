using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Commander.NET;
using Commander.NET.Attributes;

namespace Tests
{
	class Test
	{
		[Parameter("i", "--id", Description = "the ID")]
		public int ID = 12;

		[Parameter("-n", "--name", Description = "the name")]
		public string Name;

		[PositionalParameter(1, "positional1")]
		public string Positional1;

		[Parameter("-s", "--stuff", Description = "some stuff")]
		public string[] Stuff;

		[Parameter("-f")]
		public bool Flag;

		[PositionalParameter(0, "positional0", Description = "this is a very nice argument")]
		public string Positional0;

		public override string ToString()
		{
			string s = ID + " " + Name + " " + Flag + " ";
			foreach (string st in Stuff)
			{
				s += "," + st;
			}
			s += "\n" + Positional0 + " " + Positional1;
			return s;
		}
	}

	class Program
	{

		static void Main(string[] args)
		{
			string[] y = { "-i", "123", "positional0", "--name", "george", "-s", "one,two", "-f", "positional1" };
			Test t = CommanderParser.Parse<Test>(y);
			Console.WriteLine(t);
			Console.WriteLine();
			Console.WriteLine(CommanderParser.Usage<Test>());
		}
	}
}
