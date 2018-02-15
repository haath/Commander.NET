using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

using Commander.NET;
using Commander.NET.Attributes;
using Commander.NET.Exceptions;
using Commander.NET.Interfaces;

namespace Tests
{
	public class Program
	{
		public static InteractivePrompt Prompt;

		static void Main(string[] argc)
		{
			string[] args = { "push", "origin", "master" };

			Prompt = InteractivePrompt.GetPrompt();
			Prompt.WriteLine(Prompt.GetType());

			Task.Run(() =>
			{
				int i = 0;
				while (true)
				{
					Prompt.WriteLine(i++);


					Thread.Sleep(500);
				}
			});

			while (true)
			{
				string l = Prompt.ReadLine();
				Prompt.WriteLine("=> " + l);
			}
		}
	}

	class Commit
	{
		[Parameter("-m")]
		public string Message;
	}

	class Push : ICommand
	{
		[PositionalParameter(0, "remote")]
		public string Remote;

		[PositionalParameter(1, "branch")]
		public string Branch;

		void ICommand.Execute(object parent)
		{
			Program.Prompt.WriteLine(456);
		}
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

		[CommandHandler]
		public void PushCommand(Push push)
		{
			Program.Prompt.WriteLine(123);
		}
	}
}
