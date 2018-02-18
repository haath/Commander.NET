using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Commander.NET
{
	internal class WindowsInteractivePrompt : InteractivePrompt
	{
		static int outCol, outRow;

		public WindowsInteractivePrompt(string prompt = ">") : base(prompt)
		{
			Clear();
		}

		public override string ReadLine()
		{
			lock (_lock)
			{
				Console.SetCursorPosition(0, Math.Max(Console.CursorTop - 1, 0));
				ClearCurrentConsoleLine();
				Console.Write(prompt + " ");
			}
			string line = Console.ReadLine();
			return line;
		}

		public override void Write(string text)
		{
			lock (_lock)
			{
				foreach (string line in text.Split('\n'))
				{
					_Write(line, false);
				}
			}
		}

		public override void WriteLine(string line)
		{
			lock (_lock)
			{
				foreach (string l in line.Split('\n'))
				{
					_Write(l, true);
				}
			}
		}

		void _Write(string line, bool newLine)
		{
			int inCol, inRow;
			inCol = Console.CursorLeft;
			inRow = Console.CursorTop;

			int outLines = MessageRowCount(outCol, line) + (newLine ? 1 : 0);
			int outBottom = outRow + outLines;
			if (outBottom > OUT_HEIGHT)
				outBottom = OUT_HEIGHT;
			if (inRow <= outBottom)
			{
				int scrollCount = outBottom - inRow + 1;
				Console.MoveBufferArea(0, inRow, Console.BufferWidth, 1, 0, inRow + scrollCount);
				inRow += scrollCount;
			}
			if (outRow + outLines > OUT_HEIGHT)
			{
				int scrollCount = outRow + outLines - OUT_HEIGHT;
				Console.MoveBufferArea(0, scrollCount, Console.BufferWidth, Math.Max(OUT_HEIGHT - scrollCount, 0), 0, 0);
				outRow -= scrollCount;
				Console.SetCursorPosition(outCol, outRow);
			}
			Console.SetCursorPosition(outCol, outRow);

			if (newLine)
				Console.WriteLine(line);
			else
				Console.Write(line);

			outCol = Console.CursorLeft;
			outRow = Console.CursorTop;
			Console.SetCursorPosition(inCol, inRow);
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
