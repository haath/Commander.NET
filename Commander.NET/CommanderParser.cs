using Commander.NET.Attributes;
using Commander.NET.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Commander.NET
{
	public static class CommanderParser
	{
		/// <summary>
		/// Generate the usage string based on the given type's attributes. 
		/// <para>Optional positional parameters will be shown in square brackets.</para>
		/// <para>Required options will have an asterix prefix.</para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="executableName">The name of the executable that should be shown on the sample usage line. 
		/// By default the name of the actual executable will be used from System.AppDomain.CurrentDomain.FriendlyName.</param>
		/// <param name="indentationSpaces"></param>
		/// <returns></returns>
		public static string Usage<T>(string executableName = null, int indentationSpaces = 4) where T : new()
		{
			return new CommanderParser<T>().Usage(executableName, indentationSpaces);
		}

		/// <summary>
		/// Parse the given arguments and serialize them into a new instance of a T object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="args"></param>
		/// <param name="separators"></param>
		/// <returns></returns>
		public static T Parse<T>(string[] args, Separators separators = Separators.Space) where T : new()
		{
			return new CommanderParser<T>(args)
							.Separators(separators)
							.Parse();
		}

		/// <summary>
		/// Parse the given arguments and serialize them into an existing instance of a T object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="args"></param>
		/// <param name="separators"></param>
		/// <returns></returns>
		public static T Parse<T>(T obj, string[] args, Separators separators = Separators.Space) where T : new()
		{
			return new CommanderParser<T>(args)
							.Separators(separators)
							.Parse(obj);
		}

	}

	[Flags]
	public enum Separators
	{
		/// <summary>
		/// Any separators will be considered
		/// </summary>
		All		=	0xFF,

		/// <summary>
		/// --key value
		/// </summary>
		Space	=	1 << 0,

		/// <summary>
		/// --key=value
		/// </summary>
		Equals	=	1 << 1,

		/// <summary>
		/// --key:value
		/// </summary>
		Colon	=	1 << 2
	}
}
