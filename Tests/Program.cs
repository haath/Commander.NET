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
		public string ID;

		[Parameter("-n", "--name", Description = "the name")]
		public string Name;

		public override string ToString()
		{
			return ID + " " + Name;
		}
	}

	class Program
	{

		static void Main(string[] args)
		{
			string[] y = { "-i", "123", "--name", "george" };
			Test t = CommanderParser.Parse<Test>(y);
			Console.WriteLine(t);
		}
	}
}
