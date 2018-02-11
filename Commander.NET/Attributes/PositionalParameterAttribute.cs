using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Attributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class PositionalParameterAttribute : CommanderAttribute
	{
		public readonly uint Index;
		public readonly string Name;

		public PositionalParameterAttribute(uint index, string name)
		{
			Index = index;
			Name = name;
		}
	}
}
