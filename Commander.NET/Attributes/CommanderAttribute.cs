using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Attributes
{
    public class CommanderAttribute : Attribute
	{
		/// <summary>
		/// If a Required option is missing when parsing, the parser will throw a ParameterException.
		/// <para>By default, a parameter is considered required when the default value of the corresponding field is null.</para>
		/// </summary>
		public bool? Required = null;

		/// <summary>
		/// A description of this parameter. Will be displayed when generating the usage string.
		/// </summary>
		public string Description;
	}
}
