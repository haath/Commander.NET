using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Attributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class PositionalParameterAttribute : CommanderAttribute
	{
		public readonly int Index;

		public PositionalParameterAttribute(int index)
		{
			Index = index;
		}
    }
}
