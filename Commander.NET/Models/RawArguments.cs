using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Commander.NET.Attributes;

namespace Commander.NET.Models
{
    internal class RawArguments
	{
		HashSet<string> booleanKeys = new HashSet<string>();
		List<string> positionalArguments = new List<string>();
		List<string> flags = new List<string>();
		Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

		internal string this[int index]
		{
			get { return positionalArguments[index]; }
		}

		internal int PositionalArguments
		{
			get { return positionalArguments.Count; }
		}

		internal string GetValue(IEnumerable<string> keys)
		{
			return keys.Where(key => keyValuePairs.ContainsKey(key))
						.Select(key => keyValuePairs[key])
						.FirstOrDefault();
		}

		internal List<string> GetPositionalArguments()
		{
			return new List<string>(positionalArguments);
		}

		internal RawArguments Parse(string[] args, Separators separators)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if ((args[i].Matches(@"^-[a-zA-Z0-9_]=\w+$") || args[i].Matches(@"^--[a-zA-Z0-9_]{2,}=\w+$")) && separators.HasFlag(Separators.Equals)
					|| (args[i].Matches(@"^-[a-zA-Z0-9_]:\w+$") || args[i].Matches(@"^--[a-zA-Z0-9_]{2,}:\w+$")) && separators.HasFlag(Separators.Colon))
				{
					string key = args[i].TrimStart('-').Split(':')[0].Split('=')[0];
					string value = args[i].Split(':').Last().Split('=').Last();

					TryAddKeyValuePair(key, value);
				}
				else if (args[i].Matches(@"^-[a-zA-Z0-9_]$") || args[i].Matches(@"^--[a-zA-Z0-9_]{2,}$"))
				{
					string key = args[i].TrimStart('-');

					if (!booleanKeys.Contains(key) && i < args.Length - 1 && !args[i + 1].StartsWith("-"))
					{
						TryAddKeyValuePair(key, args[i + 1]);
						i++;
					}
					else
					{
						flags.Add(key);
					}
				}
				else if (args[i].Matches(@"^-[a-zA-Z0-9_]{2,}$"))
				{
					flags.AddRange(
						args[i].ToCharArray().Select(c => c.ToString())
						);
				}
				else
				{
					positionalArguments.Add(args[i]);
				}
			}
			return this;
		}

		internal RawArguments AddBooleanKeys<T>()
		{
			foreach (MemberInfo member in Utils.GetParameterMembers<T, ParameterAttribute>())
			{
				if (member.Type() == typeof(bool))
				{
					foreach (string booleanKey in member.GetCustomAttribute<ParameterAttribute>().Keys)
						booleanKeys.Add(booleanKey);
				}
			}
			return this;
		}

		internal bool GetBoolean(string key)
		{
			return flags.Contains(key) || keyValuePairs.ContainsKey(key);
		}

		void TryAddKeyValuePair(string key, string value)
		{
			if (!keyValuePairs.ContainsKey(key))
				keyValuePairs.Add(key, value);
		}
	}
}
