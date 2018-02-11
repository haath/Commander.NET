using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Commander.NET.Attributes
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate)]
	public class ParameterValidatorAttribute : Attribute
	{
		/// <summary>
		/// The names of this parameter.
		/// </summary>
		public string[] Names { get; private set; }

		internal IEnumerable<string> Keys
		{
			get { return Names.Select(name => name.TrimStart('-')); }
		}

		/// <summary>
		/// The parameter names that will be validated through this method, in any UNIX-like syntax:
		/// -e, e, --example, example
		/// </summary>
		/// <param name="names"></param>
		public ParameterValidatorAttribute(params string[] names)
		{
			Names = names.NormalizeParameterNames();
		}
	}
}
