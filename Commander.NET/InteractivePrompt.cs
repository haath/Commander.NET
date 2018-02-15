using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET
{
    public abstract class InteractivePrompt
    {
		protected const int OUT_HEIGHT = 24;

		protected string prompt;
		protected object _lock;

		internal InteractivePrompt(string prompt = ">")
		{
			this.prompt = prompt;
			_lock = new object();
		}

		public static InteractivePrompt GetPrompt(string prompt = ">")
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				return new LinuxInteractivePrompt(prompt);
			}
			else
			{
				return new WindowsInteractivePrompt(prompt);
			}
		}

		public T ReadCommand<T>() where T : new()
		{
			string line = ReadLine();
			string[] args = Utils.SplitArgumentsLine(line);
			return CommanderParser.Parse<T>(args);
		}

		public Task<T> ReadCommandAsync<T>() where T : new()
		{
			return Task.Run(() => ReadCommand<T>());
		}

		public void Clear()
		{
			string newLines = new string('\n', OUT_HEIGHT - 1);
			WriteLine(newLines);
		}
		
		public void WriteLine()
		{
			WriteLine("");
		}

		public void WriteLine(object line)
		{
			WriteLine(line.ToString());
		}

		public Task<string> ReadLineAsync()
		{
			return Task.Run(() => ReadLine());
		}

		public abstract string ReadLine();

		public abstract void WriteLine(string line);
	}
}
