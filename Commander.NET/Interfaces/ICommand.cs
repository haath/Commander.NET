using System;
using System.Collections.Generic;
using System.Text;

namespace Commander.NET.Interfaces
{
    public interface ICommand
    {
		void Execute(object parent);
    }
}
