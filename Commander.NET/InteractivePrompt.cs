using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Commander.NET
{
    public class InteractivePrompt
	{
		static int outHeight = 24;

		string prompt;
		object _lock;

		string[] outputBuffer = new string[50];
		int outputIndex = 0;

		StringBuilder inputBuffer;
		int inputIndex = 0;

		string[] inputHistory = new string[50];
		int historyIndex = 0;

		public InteractivePrompt(string prompt = ">")
		{
			this.prompt = prompt;
			_lock = new object();

			Clear();
		}

		public void Clear()
		{
			for (int i = 0; i < outHeight; i++)
			{
				WriteLine();
			}
		}

		public T ReadCommand<T>() where T : new()
		{
			string line = ReadLine();
			string[] args = Utils.SplitArgumentsLine(line);
			return CommanderParser.Parse<T>(args);
		}

		public string ReadLine()
		{
			lock (_lock)
			{
				inputBuffer = new StringBuilder();
			}

			while (true)
			{
				ConsoleKeyInfo key = Console.ReadKey();

				if (key.Key == ConsoleKey.Enter)
				{
					break;
				}

				lock (_lock)
				{
					AppendKey(inputBuffer, key);
					Refresh();
				}
			}

			string line;

			lock (_lock)
			{
				line = inputBuffer.ToString();
				inputBuffer = null;
				inputIndex = 0;

				inputHistory[historyIndex] = line;
				historyIndex = (historyIndex + 1) % inputHistory.Length;

				Refresh();
			}

			return line;
		}

		public void WriteLine()
		{
			WriteLine("");
		}

		public void WriteLine(object line)
		{
			WriteLine(line.ToString());
		}

		public void WriteLine(string line)
		{
			lock (_lock)
			{
				outputBuffer[outputIndex] = line;
				outputIndex = (outputIndex + 1) % outputBuffer.Length;
				Refresh();
			}
		}

		void Refresh()
		{
			StringBuilder output = new StringBuilder();

			for (int i = outputIndex; i < outputIndex + outputBuffer.Length - 1; i++)
			{
				string line = outputBuffer[i % outputBuffer.Length];

				if (line != null)
					output.AppendLine(line);
			}

			output.AppendLine();
			output.Append(prompt + " ");

			if (inputBuffer != null)
			{
				output.Append(inputBuffer);
			}

			Console.Clear();
			Console.Write(output);

			Console.CursorLeft = prompt.Length + 1 + inputIndex;
		}

		void AppendKey(StringBuilder str, ConsoleKeyInfo key)
		{
			int index;
			switch (key.Key)
			{
				case ConsoleKey.Backspace:
					if (inputIndex > 0)
						str.Remove(--inputIndex, 1);
					break;

				case ConsoleKey.LeftArrow:
					if (inputIndex > 0)
						inputIndex--;
					break;

				case ConsoleKey.RightArrow:
					if (inputIndex < str.Length)
						inputIndex++;
					break;

				case ConsoleKey.UpArrow:
					index = historyIndex > 0 ? historyIndex - 1 : inputHistory.Length - 1;
					if (inputHistory[index] != null)
					{
						inputHistory[historyIndex] = str.ToString();
						historyIndex = index;
						str.Clear().Append(inputHistory[historyIndex]);
						inputIndex = str.Length;
					}
					break;

				case ConsoleKey.DownArrow:
					index = (historyIndex + 1) % inputHistory.Length;
					if (inputHistory[index] != null)
					{
						inputHistory[historyIndex] = str.ToString();
						historyIndex = index;
						str.Clear().Append(inputHistory[historyIndex]);
						inputIndex = str.Length;
					}
					break;

				case ConsoleKey.Home:
					inputIndex = 0;
					break;

				case ConsoleKey.End:
					inputIndex = str.Length;
					break;

				default:
					str.Insert(inputIndex++, key.KeyChar);
					break;
			}
		}
	}
}
