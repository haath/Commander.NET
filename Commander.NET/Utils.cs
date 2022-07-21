using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Commander.NET.Attributes;

namespace Commander.NET
{
	internal static class Utils
	{
		internal static IEnumerable<MemberInfo> GetParameterMembers<T, Q>(BindingFlags flags) where Q : Attribute
		{
			foreach (MemberInfo member in typeof(T).GetTypeInfo().GetProperties(flags))
			{
				if (member.GetCustomAttribute<Q>() != null)
					yield return member;
			}
			foreach (MemberInfo member in typeof(T).GetTypeInfo().GetFields(flags))
			{
				if (member.GetCustomAttribute<Q>() != null)
					yield return member;
			}
		}

		internal static IEnumerable<MethodInfo> GetMethods<T, Q>(BindingFlags flags) where Q : Attribute
		{
			foreach (MethodInfo method in typeof(T).GetTypeInfo().GetMethods(flags))
			{
				if (method.GetCustomAttribute<Q>() != null)
					yield return method;
			}
		}

		internal static Type Type(this MemberInfo member)
		{
			if (member is PropertyInfo)
			{
				return (member as PropertyInfo).PropertyType;
			}
			else if (member is FieldInfo)
			{
				return (member as FieldInfo).FieldType;
			}
			return null;
		}

		internal static bool Matches(this string input, string regex)
		{
			return Regex.Match(input, regex).Success;
		}

		internal static T[] Concat<T>(this T[] x, T[] y)
		{
			if (x == null) return y;
			if (y == null) return x;
			int oldLen = x.Length;
			Array.Resize<T>(ref x, x.Length + y.Length);
			Array.Copy(y, 0, x, oldLen, y.Length);
			return x;
		}

		internal static string[] NormalizeParameterNames(this string[] names)
		{
			return names.Select(n =>
			{
				if (n.Matches(@"^-[a-zA-Z0-9_]$") || n.Matches(@"^--[a-zA-Z0-9_-]{2,}$"))
					return n;
				else if (n.Matches(@"^[a-zA-Z0-9_]$") && !n.StartsWith("-"))
					return "-" + n;
				else if (n.Matches(@"^[a-zA-Z0-9_-]{2,}$") && !n.StartsWith("-"))
					return "--" + n;
				else
					throw new FormatException("Invalid parameter name: " + n);
			})
			.OrderBy(n => n)
			.ToArray();
		}

		internal static List<string> GetCommandNames<T>(BindingFlags bindingFlags)
		{
			List<string> names = new List<string>();
			foreach (MemberInfo member in GetParameterMembers<T, CommandAttribute>(bindingFlags))
			{
				CommandAttribute cmd = member.GetCustomAttribute<CommandAttribute>();

				names.AddRange(cmd.Names);
			}
			return names;
		}

		internal static MemberInfo GetCommandWithName<T>(string name, BindingFlags bindingFlags)
		{
			foreach (MemberInfo member in GetParameterMembers<T, CommandAttribute>(bindingFlags))
			{
				CommandAttribute cmd = member.GetCustomAttribute<CommandAttribute>();

				if (cmd.Names.Contains(name))
					return member;
			}
			return null;
		}
		/*
		internal static string[] SplitArgumentsLine(string line)
		{
			List<string> args = new List<string>();

			StringBuilder curArg = new StringBuilder();
			char curQuote = char.MinValue;

			Action reset = () =>
			{
				args.Add(curArg.ToString());
				curArg = new StringBuilder();
				curQuote = char.MinValue;
			};

			foreach (char c in line)
			{
				if (curQuote == char.MinValue)
				{
					if (c == ' ')
					{
						reset();
					}
					else if (c == '\'')
					{
						reset();
						curQuote = '\'';
					}
					else if (c == '"')
					{
						reset();
						curQuote = '"';
					}
					else
					{
						curArg.Append(c);
					}
				}
				else
				{
					if (c == curQuote)
					{
						reset();
					}
					else
					{
						curArg.Append(c);
					}
				}
			}

			reset();

			return args.Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();
		}
		*/
		internal static string[] SplitArgumentsLine(string line)
		{
			return CommandLineToArgvW("echo " + line).Skip(1).ToArray();
		}

		/**
		 * C# equivalent of CommandLineToArgvW
		 * Translated from https://source.winehq.org/git/wine.git/blob/HEAD:/dlls/shcore/main.c#l264
		 */
		public static string[] CommandLineToArgvW(string cmdline)
		{
			if ((cmdline = cmdline.Trim()).Length == 0)
			{
				return new string[0];
			}
			int len = cmdline.Length;
			int argc = 0;
			int i = 0;
			char s = cmdline[i];
			char END = '\0';
			/* The first argument, the executable path, follows special rules */
			argc = 1;
			if (s == '"')
			{
				do
				{
					s = ++i < len ? cmdline[i] : END;
					if (s == '"')
						break;
				} while (s != END);
			}
			else
			{
				while (s != END && s != ' ' && s != '\t')
				{
					s = ++i < len ? cmdline[i] : END;
				}
			}
			/* skip to the first argument, if any */
			while (s == ' ' || s == '\t')
				s = ++i < len ? cmdline[i] : END;
			if (s != END)
				argc++;

			/* Analyze the remaining arguments */
			int qcount = 0; // quote count
			int bcount = 0; // backslash count
			while (i < len)
			{
				s = cmdline[i];
				if ((s == ' ' || s == '\t') && qcount == 0)
				{
					/* skip to the next argument and count it if any */
					do
					{
						s = ++i < len ? cmdline[i] : END;
					} while (s == ' ' || s == '\t');
					if (s != END)
						argc++;
					bcount = 0;
				}
				else if (s == '\\')
				{
					/* '\', count them */
					bcount++;
					s = ++i < len ? cmdline[i] : END;
				}
				else if (s == '"')
				{
					/* '"' */
					if ((bcount & 1) == 0)
						qcount++; /* unescaped '"' */
					s = ++i < len ? cmdline[i] : END;
					bcount = 0;
					/* consecutive quotes, see comment in copying code below */
					while (s == '"')
					{
						qcount++;
						s = ++i < len ? cmdline[i] : END;
					}
					qcount = qcount % 3;
					if (qcount == 2)
						qcount = 0;
				}
				else
				{
					/* a regular character */
					bcount = 0;
					s = ++i < len ? cmdline[i] : END;
				}
			}
			string[] argv = new string[argc];
			StringBuilder sb = new StringBuilder();
			i = 0;
			int j = 0;
			s = cmdline[i];
			if (s == '"')
			{
				do
				{
					s = ++i < len ? cmdline[i] : END;
					if (s == '"')
						break;
					else
						sb.Append(s);
				} while (s != END);
				argv[j++] = sb.ToString();
				sb.Clear();
			}
			else
			{
				while (s != END && s != ' ' && s != '\t')
				{
					sb.Append(s);
					s = ++i < len ? cmdline[i] : END;
				}
				argv[j++] = sb.ToString();
				sb.Clear();
			}
			while (s == ' ' || s == '\t')
				s = ++i < len ? cmdline[i] : END;
			if (i >= len)
				return argv;
			qcount = 0;
			bcount = 0;
			while (i < len)
			{
				if ((s == ' ' || s == '\t') && qcount == 0)
				{
					/* close the argument */
					argv[j++] = sb.ToString();
					sb.Clear();
					bcount = 0;
					/* skip to the next one and initialize it if any */
					do
					{
						s = ++i < len ? cmdline[i] : END;
					} while (s == ' ' || s == '\t');
				}
				else if (s == '\\')
				{
					sb.Append(s);
					s = ++i < len ? cmdline[i] : END;
					bcount++;
				}
				else if (s == '"')
				{
					if ((bcount & 1) == 0)
					{
						/* Preceded by an even number of '\', this is half that number of '\', plus a quote which we erase. */
						sb.Length -= bcount / 2;
						qcount++;
					}
					else
					{
						/* Preceded by an odd number of '\', this is half that number of '\' followed by a '"' */
						sb.Length = (sb.Length - 1) - bcount / 2 - 1;
						sb.Append('"');
					}
					s = ++i < len ? cmdline[i] : END;
					bcount = 0;
					/* Now count the number of consecutive quotes. Note that qcount
					 * already takes into account the opening quote if any, as well as
					 * the quote that lead us here.
					 */
					while (s == '"')
					{
						if (++qcount == 3)
						{
							sb.Append('"');
							qcount = 0;
						}
						s = ++i < len ? cmdline[i] : END;
					}
					if (qcount == 2)
						qcount = 0;
				}
				else
				{
					/* a regular character */
					sb.Append(s);
					s = ++i < len ? cmdline[i] : END;
					bcount = 0;
				}
			}
			if (sb.Length > 0)
			{
				argv[j++] = sb.ToString();
				sb.Clear();
			}
			return argv;
		}

		/**
		 * Windows native CommandLineToArgvW
		 * Copied from https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp#answer-749653
		 */
		/*
		[DllImport("shell32.dll", SetLastError = true)]
		static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);
		public static string[] CommandLineToArgvW(string commandLine)
		{
			int argc;
			IntPtr argv = CommandLineToArgvW(commandLine, out argc);
			if (argv == IntPtr.Zero)
				throw new System.ComponentModel.Win32Exception();
			try
			{
				string[] args = new string[argc];
				for (int i = 0; i < argc; i++)
				{
					IntPtr p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
					args[i] = Marshal.PtrToStringUni(p);
				}
				return args;
			}
			finally
			{
				Marshal.FreeHGlobal(argv);
			}
		}
		*/
	}
}
