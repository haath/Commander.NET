using Commander.NET.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Exceptions
{
	public abstract class ParameterException : Exception
	{
		protected CommanderAttribute attrib;

		public string ParameterName
		{
			get
			{
				return (attrib is ParameterAttribute) ?
					(attrib as ParameterAttribute).Names[0] : (attrib as PositionalParameterAttribute).Name;
			}
		}

		internal ParameterException(CommanderAttribute attrib)
		{
			this.attrib = attrib;
		}
	}
}
