using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Commander.NET;
using Commander.NET.Attributes;
using Commander.NET.Exceptions;
using Commander.NET.Interfaces;

namespace Tests
{
	class Program
	{

		static void Main(string[] argc)
		{
			string[] args = { "push", "origin", "master" };

			CommanderParser<Git> parser = new CommanderParser<Git>();

			Git git = parser.Parse(args);

			if (git.Commit != null)
			{
				Console.WriteLine("Commiting: " + git.Commit.Message);
			}
			else if (git.Push != null)
			{
				Console.WriteLine("Pushing to: " + git.Push.Remote + " " + git.Push.Branch);
			}

			Console.WriteLine(parser.Usage());
		}
	}

	class Commit
	{
		[Parameter("-m")]
		public string Message;
	}

	class Push
	{
		[PositionalParameter(0, "remote")]
		public string Remote;

		[PositionalParameter(1, "branch")]
		public string Branch;
	}

	class Git
	{
		[Command("commit")]
		public Commit Commit;

		[Command("push")]
		public Push Push;

		[Parameter("-v", Description = "Increase verbosity")]
		public bool Verbose;

		[PositionalParameter(0, "remote", Description = "desc")]
		public string Remote = "asdf";
	}
}
