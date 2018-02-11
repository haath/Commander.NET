using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Attributes
{
	public abstract class CommanderAttribute : Attribute
	{
		/// <summary>
		/// If a Required option is missing when parsing, the parser will throw a ParameterMissingException.
		/// <para>By default, a parameter is considered required when the default value of the corresponding field is null.</para>
		/// </summary>
		public Required Required = Required.Default;

		/// <summary>
		/// A description of this parameter. Will be displayed when generating the usage string.
		/// </summary>
		public string Description;

		/// <summary>
		/// The regular expression that the values of this parameter need to match.
		/// If the passed value does not match, the parser will raise a ParameterMatchException.
		/// </summary>
		public string Regex;
	}

	public enum Required
	{
		/// <summary>
		/// It is considered required when the default value of the corresponding field is null.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Required.
		/// </summary>
		Yes = 1,

		/// <summary>
		/// Not required.
		/// </summary>
		No = 2
	}
}
