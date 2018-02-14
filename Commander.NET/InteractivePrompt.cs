using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Commander.NET
{
    public class InteractivePrompt
	{
		static int outCol, outRow, outHeight = 24;

		string prompt;
		object _lock;

		public InteractivePrompt(string prompt = ">")
		{
			this.prompt = prompt;
			_lock = new object();

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
				Console.SetCursorPosition(0, Console.CursorTop - 1);
				ClearCurrentConsoleLine();
				Console.Write(prompt + " ");
			}
			string line = Console.ReadLine();
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

				int inCol, inRow;
				inCol = Console.CursorLeft;
				inRow = Console.CursorTop;

				int outLines = MessageRowCount(outCol, line) + 1;
				int outBottom = outRow + outLines;
				if (outBottom > outHeight)
					outBottom = outHeight;
				if (inRow <= outBottom)
				{
					int scrollCount = outBottom - inRow + 1;
					Console.MoveBufferArea(0, inRow, Console.BufferWidth, 1, 0, inRow + scrollCount);
					inRow += scrollCount;
				}
				if (outRow + outLines > outHeight)
				{
					int scrollCount = outRow + outLines - outHeight;
					Console.MoveBufferArea(0, scrollCount, Console.BufferWidth, outHeight - scrollCount, 0, 0);
					outRow -= scrollCount;
					Console.SetCursorPosition(outCol, outRow);
				}
				Console.SetCursorPosition(outCol, outRow);

				Console.WriteLine(line);

				outCol = Console.CursorLeft;
				outRow = Console.CursorTop;
				Console.SetCursorPosition(inCol, inRow);
			}
		}

		static int MessageRowCount(int startCol, string msg)
		{
			string[] lines = msg.Split('\n');
			int result = 0;
			foreach (string line in lines)
			{
				result += (startCol + line.Length) / Console.BufferWidth;
				startCol = 0;
			}
			return result + lines.Length - 1;
		}

		static void ClearCurrentConsoleLine()
		{
			int currentLineCursor = Console.CursorTop;
			Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write(new string(' ', Console.WindowWidth));
			Console.SetCursorPosition(0, currentLineCursor);
		}
	}
}
