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
		public int ID;

		[Parameter("-n", "--name", Description = "the name")]
		public string Name;

		[Parameter("-s", "--stuff", Description = "some stuff")]
		public string[] Stuff;

		[Parameter("-f")]
		public bool Flag;

		[PositionalParameter(0)]
		public string Positional0;

		[PositionalParameter(1)]
		public string Positional1;

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
		}
	}
}
