using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
			if (IsLinux())
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

		public void Write(string format, params object[] args)
		{
			Write(string.Format(format, args));
		}
		
		public void WriteLine()
		{
			WriteLine("");
		}

		public void WriteLine(object line)
		{
			WriteLine(line.ToString());
		}

		public void WriteLine(string format, params object[] args)
		{
			WriteLine(string.Format(format, args));
		}


		public Task<string> ReadLineAsync()
		{
			return Task.Run(() => ReadLine());
		}

		public abstract string ReadLine();

		public abstract void Write(string text);

		public abstract void WriteLine(string line);


		static bool IsLinux()
		{
			string windir = Environment.GetEnvironmentVariable("windir");
			if (!string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir))
			{
				return false;
			}
			else if (File.Exists(@"/proc/sys/kernel/ostype"))
			{
				string osType = File.ReadAllText(@"/proc/sys/kernel/ostype");
				if (osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase))
				{
					// Note: Android gets here too
					return true;
				}
				else
				{
					return false;
				}
			}
			else if (File.Exists(@"/System/Library/CoreServices/SystemVersion.plist"))
			{
				// Note: iOS gets here too
				return true;
			}
			else
			{
				return true;
			}
		}
	}
}
