using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Commander.NET;
using Commander.NET.Attributes;
using Commander.NET.Exceptions;

namespace Tests
{
	class Options
	{
		[Parameter("-i", Description = "The ID")]
		public int ID = 42;
		
		[Parameter("-n", "--name", Description = "The name.")]
		public string Name;

		[PositionalParameter(0, "operation", Description = "The operation to perform.")]
		public string Operation;

		[PositionalParameter(1, "target", Description = "The host to connect to.")]
		public string Host = "127.0.0.1";

		[Parameter("-s", "--stuff", Description = "some stuff", Required = Required.No)]
		public string[] Stuff;

		[Parameter("-h", "--help", Description = "Print this message and exit.")]
		public bool Help;

		[Parameter("-r", Required = Required.Yes)]
		public int RequiredParameter;

		[Parameter("--lorem", Required = Required.No)]
		public string NotRequiredParameter;

		[Parameter("--ipsum")]	// Required.Default
		public string ThisOneIsRequired;

		[Parameter("--dolor")]  // Required.Default
		public string ThisOneIsNotRequired = "Because it has a default value";

		public override string ToString()
		{
			string s = ID + " " + Name + " " + Help + " ";
			foreach (string st in Stuff)
			{
				s += "," + st;
			}
			s += "\n" + Host + " " + Operation;
			return s;
		}
	}

	class Program
	{

		static void Main(string[] argc)
		{
			string[] args = { "-i", "123", "--name", "john", "--test:asdf", "--test=asdf" };

			Console.WriteLine(Regex.Match("shit", @"bacon|onion|tomato").Success);
			Console.WriteLine(CommanderParser.Usage<Options>());

			try
			{
				Options opts = CommanderParser.Parse<Options>(args, Separators.Space);
			}
			catch (ParameterMissingException ex)
			{
				// A required parameter was missing
				Console.WriteLine("Missing parameter: " + ex.ParameterName);
			}
			catch (ParameterFormatException ex)
			{
				/*
				 *	A string-parsing method raised a FormatException
				 *	ex.ParameterName
				 *	ex.Value
				 *	ex.RequiredType
				 */
				Console.WriteLine(ex.Message);
			}
		}
	}
}
