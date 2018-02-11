using Commander.NET.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Exceptions
{
    public class ParameterMatchException : ParameterException
    {
		public readonly string Value;

		internal ParameterMatchException(CommanderAttribute attrib, string value) : base(attrib)
		{
			Value = value;
		}
		
		public override string Message
		{
			get
			{
				return string.Format("Parameter {0}: value \"{1}\" did not match the regular expression \"{2}\"", ParameterName, Value, attrib.Regex);
			}
		}
	}
}
