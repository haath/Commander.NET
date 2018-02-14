using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Interfaces
{
    public interface IParameterFormatter
    {
		object Format(string name, string value);
    }
}
