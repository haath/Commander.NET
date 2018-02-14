using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Exceptions
{
    public class FormatterTypeException : Exception
	{
		public readonly Type Type;

		internal FormatterTypeException(Type type)
		{
			Type = type;
		}

		public override string Message
		{
			get
			{
				return string.Format("Type {0} does not implement IParameterFormatter", Type.ToString());
			}
		}
	}
}
