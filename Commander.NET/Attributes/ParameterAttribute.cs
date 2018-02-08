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
			Func<string, string, bool> Match = (x, y) => Regex.Match(x, y).Success;

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
    }
}
