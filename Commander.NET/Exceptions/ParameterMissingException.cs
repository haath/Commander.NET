using System;
using System.Collections.Generic;
using System.Text;

using Commander.NET.Attributes;

namespace Commander.NET.Exceptions
{
    public class ParameterMissingException : ParameterException
	{
		internal ParameterMissingException(CommanderAttribute attrib) : base(attrib)
		{

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
