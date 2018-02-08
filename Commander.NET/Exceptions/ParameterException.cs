using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Exceptions
{
    public class ParameterException : Exception
    {
		string[] parameterNames;

		public string ParameterName
		{
			get { return parameterNames[0]; }
		}

		internal ParameterException(string[] parameterNames)
		{
			this.parameterNames = parameterNames;
		}

		public override string Message
		{
			get
			{
				StringBuilder msg = new StringBuilder("Parameter missing: ");
				msg.Append(ParameterName);
				return msg.ToString();
			}
		}
	}
}
