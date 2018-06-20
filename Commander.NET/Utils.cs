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

			return args.Where(a => !string.IsNullOrWhiteSpace(a))
				       .ToArray();
		}
	}
}
