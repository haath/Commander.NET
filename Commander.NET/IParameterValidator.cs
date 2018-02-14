using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET
{
    public interface IParameterValidator
    {
		bool Validate(string name, string value);
    }
}
