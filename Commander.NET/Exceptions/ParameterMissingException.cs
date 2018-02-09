using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Exceptions
{
    public class ParameterMissingException : Exception
    {
		string parameterName;

		internal ParameterMissingException(string parameterName)
		{
			this.parameterName = parameterName;
		}

		public override string Message
		{
			get
			{
				StringBuilder msg = new StringBuilder("Parameter missing: ");
				msg.Append(parameterName);
				return msg.ToString();
			}
		}
	}
}
