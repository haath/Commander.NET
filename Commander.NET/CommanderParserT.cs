using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

using Commander.NET.Attributes;
using Commander.NET.Exceptions;
using System.Text.RegularExpressions;

namespace Commander.NET
{
	public class CommanderParser<T> where T : new()
	{
		T defaultObject;
		List<string> args;
		Separators separators = Commander.NET.Separators.Space;

		public CommanderParser()
		{
			args = new List<string>();
			defaultObject = new T();
		}

		public CommanderParser(params string[] args) : this()
		{
			Add(args);
		}

		public CommanderParser<T> Add(params string[] args)
		{
			this.args.AddRange(args);
			return this;
		}

		public CommanderParser<T> Separators(Separators separators)
		{
			this.separators = separators;
			return this;
		}

		/// <summary>
		/// Parse the given arguments - along with any other arguments added to this object - and serialize them into a new instance of a T object.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="separators"></param>
		/// <returns></returns>
		public T Parse(string[] args)
		{
			Add(args);
			return Parse();
		}

		/// <summary>
		/// Parse the arguments added to this object and serialize them into a new instance of a T object.
		/// </summary>
		/// <returns></returns>
		public T Parse()
		{
			return CommanderParser.Parse<T>(defaultObject, new T(), args.ToArray(), separators);
		}

		/// <summary>
		/// Parse the given arguments - along with any other arguments added to this object - and serialize them into an existing instance of a T object.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="args"></param>
		/// <param name="separators"></param>
		/// <returns></returns>
		public T Parse(T obj, string[] args)
		{
			Add(args);
			return Parse(obj);
		}

		/// <summary>
		/// Parse the arguments added to this object and serialize them into an existing instance of a T object.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="separators"></param>
		/// <returns></returns>
		public T Parse(T obj)
		{
			return CommanderParser.Parse<T>(defaultObject, obj, args.ToArray(), separators);
		}

		/// <summary>
		/// Generate the usage string based on the given type's attributes. 
		/// <para>Optional positional parameters will be shown in square brackets.</para>
		/// <para>Required options will have an asterix prefix.</para>
		/// </summary>
		/// <param name="executableName"></param>
		/// <param name="indentationSpaces"></param>
		/// <returns></returns>
		public string Usage(string executableName = "<exe>", int indentationSpaces = 4)
		{
			return CommanderParser.Usage<T>(defaultObject, executableName, indentationSpaces);
		}
	}
}
