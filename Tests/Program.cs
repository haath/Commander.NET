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

		public override string ToString()
		{
			string s = ID + " " + Name + " ";
			foreach (string st in Stuff)
			{
				s += "," + st;
			}
			return s;
		}
	}

	class Program
	{

		static void Main(string[] args)
		{
			string[] y = { "-i", "123", "--name", "george", "-s", "one,two" };
			Test t = CommanderParser.Parse<Test>(y);
			Console.WriteLine(t);
		}
	}
}
