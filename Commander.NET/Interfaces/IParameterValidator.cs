using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Interfaces
{
    public interface IParameterValidator
    {
		bool Validate(string name, string value);
    }
}
