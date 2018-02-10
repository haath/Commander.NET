using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Commander.NET.Attributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class ParameterAttribute : CommanderAttribute
	{
		/// <summary>
		/// The names of this parameter.
		/// </summary>
		public string[] Names { get; private set; }

		public bool Password = false;

		internal IEnumerable<string> Keys
		{
			get { return Names.Select(name => name.TrimStart('-')); }
		}

		/// <summary>
		/// The names of this parameter, in any UNIX-like syntax:
		/// -e, e, --example, example
		/// </summary>
		/// <param name="names"></param>
		public ParameterAttribute(params string[] names)
		{
			Names = names.Select(n =>
			{
				if (Match(n, @"^-[a-zA-Z0-9_]$") || Match(n, @"^--[a-zA-Z0-9_]{2,}$"))
					return n;
				else if (Match(n, @"^[a-zA-Z0-9_]$"))
					return "-" + n;
				else if (Match(n, @"^[a-zA-Z0-9_]{2,}$"))
					return "--" + n;
				else
					throw new FormatException("Invalid parameter name: " + n);
			}).OrderBy(n => n)
			.ToArray();
		}

		static bool Match(string input, string regex)
		{
			return Regex.Match(input, regex).Success;
		}
	}
}
