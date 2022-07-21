using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Commander.NET.Attributes;

namespace Commander.NET.Models
{
	internal class RawArguments<T>
	{
		BindingFlags bindingFlags;
		HashSet<string> booleanKeys = new HashSet<string>();
		List<string> positionalArguments = new List<string>();
		List<string> flags = new List<string>();
		Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
		
		internal string Command { get; private set; }
		internal int CommandIndex { get; private set; }

		internal string this[int index]
		{
			get { return positionalArguments[index]; }
		}

		internal string this[string key]
		{
			get { return keyValuePairs[key]; }
		}

		internal int PositionalArguments
		{
			get { return positionalArguments.Count; }
		}

		internal RawArguments(BindingFlags bindingFlags)
		{
			this.bindingFlags = bindingFlags;
			foreach (MemberInfo member in Utils.GetParameterMembers<T, ParameterAttribute>(bindingFlags))
			{
				if (member.Type() == typeof(bool))
				{
					foreach (string booleanKey in member.GetCustomAttribute<ParameterAttribute>().Keys)
						booleanKeys.Add(booleanKey);
				}
			}
		}

		internal string GetMatchingKey(IEnumerable<string> keys)
		{
			return keys.FirstOrDefault(key => keyValuePairs.ContainsKey(key));
		}

		internal List<string> GetPositionalArguments()
		{
			return new List<string>(positionalArguments);
		}

		internal RawArguments<T> Parse(string[] args, Separators separators)
		{
			List<string> commands = Utils.GetCommandNames<T>(bindingFlags);

			for (int i = 0; i < args.Length; i++)
			{
				string arg = args[i];
				if (string.IsNullOrWhiteSpace(arg))
					continue;
				if ((arg.Matches(@"^-[a-zA-Z0-9_]=") || arg.Matches(@"^--[a-zA-Z0-9_-]{2,}=")) && separators.HasFlag(Separators.Equals))
				{
					string pair = arg.TrimStart('-');
					int pos = pair.IndexOf('=');
					string key = pair.Substring(0, pos);
					string value = pair.Substring(pos + 1);
					Console.WriteLine(key + "=" + value);

					TryAddKeyValuePair(key, value);
				}
				else if ((arg.Matches(@"^-[a-zA-Z0-9_]:") || arg.Matches(@"^--[a-zA-Z0-9_-]{2,}:")) && separators.HasFlag(Separators.Colon))
				{
					string pair = arg.TrimStart('-');
					int pos = pair.IndexOf(':');
					string key = pair.Substring(0, pos);
					string value = pair.Substring(pos + 1);

					TryAddKeyValuePair(key, value);
				}
				else if (arg.Matches(@"^-[a-zA-Z0-9_]$") || arg.Matches(@"^--[a-zA-Z0-9_-]{2,}$"))
				{
					string key = arg.TrimStart('-');

					int intTest;
					if (!booleanKeys.Contains(key) && i < args.Length - 1
						&& (!args[i + 1].StartsWith("-") || int.TryParse(args[i + 1], out intTest)))
					{
						TryAddKeyValuePair(key, args[i + 1]);
						i++;
					}
					else
					{
						flags.Add(key);
					}
				}
				else if (arg.Matches(@"^-[a-zA-Z0-9_]{2,}$"))
				{
					// Multiple flags
					flags.AddRange(
						arg.ToCharArray().Select(c => c.ToString())
						);
				}
				else
				{
					if (commands.Contains(arg))
					{
						// We caught a command name, stop parsing
						Command = arg;
						CommandIndex = i;

						return this;
					}
					else
					{
						// No commands with this name, add it to the positional arguments
						positionalArguments.Add(arg);
					}
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
