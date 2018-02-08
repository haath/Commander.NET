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
	}

	class Program
	{

		static void Main(string[] args)
		{
			string[] y = { "-i", "123" };
			CommanderParser.Parse<Test>(y);
		}
	}
}
