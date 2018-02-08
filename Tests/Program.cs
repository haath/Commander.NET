using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Commander.NET;
using Commander.NET.Attributes;

namespace Tests
{
	class Program
	{
		[Parameter("x")]
		int x;

		static void Main(string[] args)
		{
			string[] y = { "asdf" };
			Console.WriteLine(args.Length);
		}
	}
}
