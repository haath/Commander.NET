using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Exceptions
{
    public class ValidatorTypeException : Exception
    {
		public readonly Type Type;

		internal ValidatorTypeException(Type type)
		{
			Type = type;
		}

		public override string Message
		{
			get
			{
				return string.Format("Type {0} does not implement IParameterValidator", Type.ToString());
			}
		}
	}
}
