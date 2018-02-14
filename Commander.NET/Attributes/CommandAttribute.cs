using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Attributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class CommandAttribute : Attribute
    {
		/// <summary>
		/// The names of this command.
		/// </summary>
		public string[] Names { get; private set; }

		public string Description;

		public CommandAttribute(params string[] names)
		{
			Names = names;
		}
	}
}
