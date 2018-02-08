using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Commander.NET.Attributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class ParameterAttribute : Attribute
    {
		/// <summary>
		/// The names of this parameter.
		/// </summary>
		public string[] Names { get; private set; }

		/// <summary>
		/// If a Required option is missing when parsing, the parser will throw a ParameterException.
		/// <para>By default, a parameter is considered required when the default value of the corresponding field is null.</para>
		/// </summary>
		public bool? Required = null;

		/// <summary>
		/// A description of this parameter. Will be displayed when generating the usage string.
		/// </summary>
		public string Description;

		public bool Password = false;

		/// <summary>
		/// The names of this parameter, in any UNIX-like syntax:
		/// -e, e, --example, example
		/// </summary>
		/// <param name="names"></param>
		public ParameterAttribute(params string[] names)
		{
			Names = names.Select(n =>
			{
				if (Match(n, @"^-\w$") || Match(n, @"^--\w{2,}$"))
					return n;
				else if (Match(n, @"^\w$"))
					return "-" + n;
				else if (Match(n, @"^\w{2,}$"))
					return "--" + n;
				else
					throw new FormatException("Invalid parameter name: " + n);
			}).ToArray();
		}

		internal bool MatchesName(string name)
		{
			if (Names.Contains(name))
				return true;

			if (Match(name, @"^-\w{2,}$"))
			{
				// Multiple flags
				foreach (string singleCharacterName in Names.Where(n => n.Length == 2))
				{
					string flag = singleCharacterName.Substring(1);

					if (name.Contains(flag))
						return true;
				}
			}

			return false;
		}

		static bool Match(string input, string regex)
		{
			return Regex.Match(input, regex).Success;
		}
	}
}
