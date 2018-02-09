using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Exceptions
{
    public class ParameterFormatException : Exception
    {
		public readonly string ParameterName;
		public readonly string Value;
		public readonly Type RequiredType;

		internal ParameterFormatException(string parameterName, string value, Type requiredType)
		{
			ParameterName = parameterName;
			Value = value;
			RequiredType = requiredType;
		}

		public override string Message
		{
			get
			{
				return string.Format("Formatting error for parameter {0}. Unable to cast value \"{1}\" to type {2}", ParameterName, Value, RequiredType.ToString());
			}
		}
	}
}
