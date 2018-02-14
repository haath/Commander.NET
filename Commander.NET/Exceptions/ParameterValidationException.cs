using System;
using System.Collections.Generic;
using System.Text;

using Commander.NET.Interfaces;

namespace Commander.NET.Exceptions
{
    public class ParameterValidationException : Exception
    {
		/// <summary>
		/// The IParameterValidator instance whose Validate() method returned false, thus raising this exception.
		/// </summary>
		public readonly IParameterValidator Validator;

		internal ParameterValidationException(IParameterValidator validator)
		{
			Validator = validator;
		}
    }
}
