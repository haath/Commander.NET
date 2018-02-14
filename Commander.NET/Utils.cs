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
		internal static IEnumerable<MemberInfo> GetParameterMembers<T, Q>() where Q : Attribute
		{
			foreach (MemberInfo member in typeof(T).GetProperties())
			{
				if (member.GetCustomAttribute<Q>() != null)
					yield return member;
			}
			foreach (MemberInfo member in typeof(T).GetFields())
			{
				if (member.GetCustomAttribute<Q>() != null)
					yield return member;
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
				if (n.Matches(@"^-[a-zA-Z0-9_]$") || n.Matches(@"^--[a-zA-Z0-9_]{2,}$"))
					return n;
				else if (n.Matches(@"^[a-zA-Z0-9_]$"))
					return "-" + n;
				else if (n.Matches(@"^[a-zA-Z0-9_]{2,}$"))
					return "--" + n;
				else
					throw new FormatException("Invalid parameter name: " + n);
			})
			.OrderBy(n => n)
			.ToArray();
		}

		internal static List<string> GetCommandNames<T>()
		{
			List<string> names = new List<string>();
			foreach (MemberInfo member in GetParameterMembers<T, CommandAttribute>())
			{
				CommandAttribute cmd = member.GetCustomAttribute<CommandAttribute>();

				names.AddRange(cmd.Names);
			}
			return names;
		}

		internal static MemberInfo GetCommandWithName<T>(string name)
		{
			foreach (MemberInfo member in GetParameterMembers<T, CommandAttribute>())
			{
				CommandAttribute cmd = member.GetCustomAttribute<CommandAttribute>();

				if (cmd.Names.Contains(name))
					return member;
			}
			return null;
		}

		internal static string[] SplitArgumentsLine(string line)
		{
			List<string> args = new List<string>();
			StringBuilder quotesArg = null;
			string curQuote = null;
			foreach (string part in line.Split(' '))
			{
				if (quotesArg == null)
				{
					if ((part.StartsWith("'") && part.EndsWith("'"))
						|| (part.StartsWith("\"") && part.EndsWith("\"")))
					{
						args.Add(part.Substring(1, part.Length - 2));
					}
					if (part.StartsWith("'") || part.StartsWith("\""))
					{
						quotesArg = new StringBuilder(part.Substring(1));
						curQuote = part.StartsWith("'") ? "'" : "\"";
					}
					else
					{
						args.Add(part);
					}
				}
				else
				{
					string value = part.EndsWith(curQuote) ? part.Substring(0, part.Length - 1) : part;

					quotesArg.Append(" ").Append(value);

					if (part.EndsWith(curQuote))
					{
						args.Add(quotesArg.ToString());
						quotesArg = null;
					}
				}
			}
			return args.ToArray();
		}
	}
}
