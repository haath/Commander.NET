using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Commander.NET;
using Commander.NET.Attributes;
using Commander.NET.Exceptions;
using Commander.NET.Interfaces;

namespace Tests
{
	public class PositiveInteger : IParameterValidator, IParameterFormatter
	{
		object IParameterFormatter.Format(string name, string value)
		{
			return Encoding.ASCII.GetBytes(value);
		}

		bool IParameterValidator.Validate(string name, string value)
		{
			int intVal;
			return int.TryParse(value, out intVal) && intVal > 0;
		}
	}

	class Options
	{
		[Parameter("-i", Description = "The ID")]
		public int ID = 42;
		
		[Parameter("-n", "--name", Description = "The name.", Regex = "^john|mary$")]
		public string Name;

		[Parameter("-h", "--help", Description = "Print this message and exit.")]
		public bool Help;

		[Parameter("-t")]
		public bool Test;

		[Parameter("-a", "--age", ValidateWith = typeof(PositiveInteger))]
		public int Age;

		[Parameter("--ascii", FormatWith = typeof(PositiveInteger))]
		public byte[] ascii;

		public override string ToString()
		{
			string s = ID + " " + Name + " " + Help + " " + Test + " " + Age + " " + ascii.Length;
			return s;
		}
	}

	class Program
	{

		static void Main(string[] argc)
		{
			string[] args = { "-i", "123", "--name", "john", "-a", "5", "--ascii", "penis" };

			Console.WriteLine(Regex.Match("shit", @"bacon|onion|tomato").Success);
			Console.WriteLine(CommanderParser.Usage<Options>());

			try
			{
				Options opts = CommanderParser.Parse<Options>(args, Separators.Space);
				Console.WriteLine(opts.ToString());
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
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
