using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Attributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class PositionalParameterListAttribute : Attribute
	{
	}
}
