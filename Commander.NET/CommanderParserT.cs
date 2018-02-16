using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

using Commander.NET.Models;
using Commander.NET.Attributes;
using Commander.NET.Exceptions;
using Commander.NET.Interfaces;
using System.Text.RegularExpressions;

namespace Commander.NET
{
	public class CommanderParser<T> where T : new()
	{
		T defaultObject;
		string[] args;
		Separators separators = Commander.NET.Separators.Space;
		BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

		public CommanderParser()
		{
			defaultObject = new T();
		}

		public CommanderParser(params string[] args) : this()
		{
			Add(args);
		}

		#region options

		public CommanderParser<T> Add(params string[] args)
		{
			this.args = this.args.Concat(args);
			return this;
		}

		/// <summary>
		/// Add arguments by splitting up a line.
		/// Useful if you're getting commands from a file, or interactively through Console.ReadLine().
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		public CommanderParser<T> AddLine(string line)
		{
			string[] args = Utils.SplitArgumentsLine(line);
			return Add(args);
		}

		/// <summary>
		/// Set which key-value separators are considered valid.
		/// <para>By default, only the "--key value" format is considered valid.</para>
		/// </summary>
		/// <param name="separators"></param>
		/// <returns></returns>
		public CommanderParser<T> Separators(Separators separators)
		{
			this.separators = separators;
			return this;
		}

		/// <summary>
		/// Set the binding flags for getting a type's fields, properties and methods.
		/// <para>The default value is: BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static</para>
		/// </summary>
		/// <param name="bindingFlags"></param>
		/// <returns></returns>
		public CommanderParser<T> Bindings(BindingFlags bindingFlags)
		{
			this.bindingFlags = bindingFlags;
			return this;
		}

		#endregion

		#region parsing

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
			return Parse(new T());
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
			/*
			 * Parse arguments
			 */
			RawArguments<T> rawArgs = new RawArguments<T>(bindingFlags)
									.Parse(args, separators);

			/*
			 * Set named arguments
			 */
			foreach (MemberInfo member in Utils.GetParameterMembers<T, ParameterAttribute>(bindingFlags))
			{
				ParameterAttribute param = member.GetCustomAttribute<ParameterAttribute>();

				if (member.Type() == typeof(bool))
				{
					SetValue(
						obj,
						member,
						param.Keys.Any(name => rawArgs.GetBoolean(name))
						);
				}
				else
				{
					string matchingKey = rawArgs.GetMatchingKey(param.Keys);

					if (matchingKey != null)
					{
						SetValue(obj, member, param, matchingKey, rawArgs[matchingKey]);
					}
					else if (ParamRequired<T>(defaultObject, param, member))
					{
						// Required parameter missing
						throw new ParameterMissingException(param);
					}
				}
			}

			/*
			 * Set positional arguments
			 */
			foreach (MemberInfo member in Utils.GetParameterMembers<T, PositionalParameterAttribute>(bindingFlags))
			{
				PositionalParameterAttribute param = member.GetCustomAttribute<PositionalParameterAttribute>();

				if (param.Index < rawArgs.PositionalArguments)
				{
					SetValue(obj, member, param, param.Name, rawArgs[(int)param.Index]);
				}
				else if (ParamRequired<T>(defaultObject, param, member))
				{
					// Required parameter missing
					throw new ParameterMissingException(param);
				}
			}

			/*
			 * Set the positional arguments list
			 */
			foreach (MemberInfo member in Utils.GetParameterMembers<T, PositionalParameterListAttribute>(bindingFlags))
			{
				if (member.Type().IsArray)
				{
					SetValue(obj, member, rawArgs.GetPositionalArguments().ToArray());
				}
				else
				{
					SetValue(obj, member, rawArgs.GetPositionalArguments());
				}
			}

			/*
			 * Check if a command was entered
			 */
			if (rawArgs.Command != null)
			{
				string[] commandArgs = args.Skip(rawArgs.CommandIndex + 1).ToArray();
				MemberInfo commandMember = Utils.GetCommandWithName<T>(rawArgs.Command, bindingFlags);
				Type commandType = commandMember.Type();

				Type parserType = typeof(CommanderParser<>).MakeGenericType(commandType);
				object parser = Activator.CreateInstance(parserType, commandArgs);

				// Set options
				parserType.GetTypeInfo().GetMethod("Separators", new Type[] { typeof(Separators) })
				          .Invoke(parser, new object[] { separators });
				parserType.GetTypeInfo().GetMethod("Bindings", new Type[] { typeof(BindingFlags) })
						  .Invoke(parser, new object[] { bindingFlags });
				

				// Parse the command
				object command = parserType.GetTypeInfo().GetMethod("Parse", new Type[] { typeof(string[]) })
											.Invoke(parser, new object[] { commandArgs });

				// Set the command back to the object
				SetValue(obj, commandMember, command);

				// Call the command handlers
				if (typeof(ICommand).GetTypeInfo().IsAssignableFrom(commandType))
				{
					// First the handler on the command object, if it implements ICommand
					(command as ICommand).Execute(obj);
				}
				foreach (MethodInfo handler in Utils.GetMethods<T, CommandHandlerAttribute>(bindingFlags))
				{
					// Then check if the parent object has any handler for this command

					ParameterInfo[] handlerParams = handler.GetParameters();

					if (handlerParams.Length == 1 && handlerParams[0].ParameterType == commandType)
					{
						handler.Invoke(obj, new object[] { command });
					}
				}
			}
			else if (Utils.GetCommandNames<T>(bindingFlags).Count > 0)
			{
				// The are commands, but no command was passed
				throw new CommandMissingException();
			}

			return obj;
		}

		#endregion

		#region usage

		/// <summary>
		/// Generate the usage string based on the given type's attributes. 
		/// <para>Optional positional parameters will be shown in square brackets.</para>
		/// <para>Required options will have an asterix prefix.</para>
		/// </summary>
		/// <param name="executableName"></param>
		/// <param name="indentationSpaces"></param>
		/// <returns></returns>
		public string Usage(string executableName = null, int indentationSpaces = 4)
		{
			string executable = Environment.GetCommandLineArgs()[0];
			executableName = executableName ?? executable;
			string indentation = string.Join("", new int[indentationSpaces].Select(s => " "));

			StringBuilder usage = new StringBuilder();
			usage.AppendFormat("Usage: {0} [options] ", executableName);

			IOrderedEnumerable<MemberInfo> positionalParams = Utils.GetParameterMembers<T, PositionalParameterAttribute>(bindingFlags)
																	.OrderBy(member => member.GetCustomAttribute<PositionalParameterAttribute>().Index);
			IOrderedEnumerable<MemberInfo> optionParams = Utils.GetParameterMembers<T, ParameterAttribute>(bindingFlags)
																	.OrderBy(member => member.GetCustomAttribute<ParameterAttribute>().Names[0]);
			IOrderedEnumerable<MemberInfo> commands = Utils.GetParameterMembers<T, CommandAttribute>(bindingFlags)
																	.OrderBy(member => member.GetCustomAttribute<CommandAttribute>().Names[0]);

			foreach (MemberInfo member in positionalParams)
			{
				// Print example line
				PositionalParameterAttribute param = member.GetCustomAttribute<PositionalParameterAttribute>();

				if (ParamRequired<T>(defaultObject, param, member))
				{
					usage.AppendFormat("<{0}> ", param.Name);
				}
				else
				{
					usage.AppendFormat("[{0}] ", param.Name);
				}
			}

			if (commands.Count() > 0)
			{
				usage.Append("<command>");
			}

			usage.AppendLine();

			foreach (MemberInfo member in positionalParams)
			{
				PositionalParameterAttribute param = member.GetCustomAttribute<PositionalParameterAttribute>();
				// Print positional argument descriptions
				if (param.Description != null)
				{
					usage.AppendFormat("{0}{1}{2}{3}\n", indentation, param.Name, indentation, param.Description);
				}
			}

			if (commands.Count() > 0)
			{
				usage.AppendLine("Commands:");

				foreach (MemberInfo member in commands)
				{
					CommandAttribute param = member.GetCustomAttribute<CommandAttribute>();
					// Print command descriptions
					if (param.Description != null)
					{
						usage.AppendFormat("{0}{1}{2}{3}\n", indentation, param.Names[0], indentation, param.Description);
					}
					else
					{
						usage.AppendFormat("{0}{1}\n", indentation, param.Names[0]);
					}
				}
			}

			usage.AppendLine("Options:");

			foreach (MemberInfo member in optionParams)
			{
				ParameterAttribute param = member.GetCustomAttribute<ParameterAttribute>();
				object defaultValue = GetDefaultValue<T>(defaultObject, member);
				bool required = ParamRequired<T>(defaultObject, param, member);

				usage.Append(indentation).Append(required ? "* " : "  ");

				for (int i = 0; i < param.Names.Length; i++)
				{
					if (i != 0)
					{
						usage.Append(", ");
					}
					usage.Append(param.Names[i]);
				}
				usage.AppendLine();

				if (param.Description != null)
				{
					usage.AppendFormat("{0}{1}{2}\n", indentation, indentation, param.Description);
				}

				if (!required && defaultValue != null)
				{
					usage.AppendFormat("{0}{1}Default: {2}\n", indentation, indentation, defaultValue);
				}
			}

			return usage.ToString();
		}

#endregion

		#region Private


		#endregion

		#region Static
		static bool ParamRequired<T>(T defaultObj, CommanderAttribute param, MemberInfo member)
		{
			return param.Required == Required.Yes
				|| (param.Required == Required.Default && GetDefaultValue<T>(defaultObj, member) == null);
		}

		static void SetValue<T>(T obj, MemberInfo member, CommanderAttribute param, string name, string value)
		{
			if (param.Regex != null)
			{
				// We need to validate this value with a regex
				if (!value.Matches(param.Regex))
				{
					throw new ParameterMatchException(param, value);
				}
			}

			if (param.ValidateWith != null)
			{
				// We need to validate this value with a validator
				if (!typeof(IParameterValidator).GetTypeInfo().IsAssignableFrom(param.ValidateWith))
				{
					throw new ValidatorTypeException(param.ValidateWith);
				}

				IParameterValidator validator = (IParameterValidator)Activator.CreateInstance(param.ValidateWith);

				if (!validator.Validate(name, value))
				{
					throw new ParameterValidationException(validator);
				}
			}

			object convertedValue;

			if (param.FormatWith != null)
			{
				// We need to format this value
				if (!typeof(IParameterFormatter).GetTypeInfo().IsAssignableFrom(param.FormatWith))
				{
					throw new FormatterTypeException(param.FormatWith);
				}

				IParameterFormatter formatter = (IParameterFormatter)Activator.CreateInstance(param.FormatWith);

				convertedValue = formatter.Format(name, value);
			}
			else
			{
				// We need to try and convert the value ourselves
				try
				{
					convertedValue = ValueParse(member.Type(), value);
				}
				catch (FormatException)
				{
					// The value had to be parsed to a numerical type and failed
					throw new ParameterFormatException(param, value, member.GetType());
				}
			}

			SetValue(obj, member, convertedValue);
		}

		static void SetValue<T>(T obj, MemberInfo member, object value)
		{
			if (member is PropertyInfo)
			{
				(member as PropertyInfo).SetValue(obj, value);
			}
			else if (member is FieldInfo)
			{
				(member as FieldInfo).SetValue(obj, value);
			}
		}

		static object ValueParse(Type type, string value)
		{
			object convertedValue = value;

			if (type == typeof(int))
			{
				convertedValue = int.Parse(value);
			}
			else if (type == typeof(uint))
			{
				convertedValue = uint.Parse(value);
			}
			else if (type == typeof(long))
			{
				convertedValue = long.Parse(value);
			}
			else if (type == typeof(double))
			{
				convertedValue = double.Parse(value);
			}
			else if (type == typeof(float))
			{
				convertedValue = float.Parse(value);
			}
			else if (type.IsArray)
			{
				convertedValue = value.Split(',');
				return convertedValue;
				string[] values = value.Split(',');
				object[] converted = new object[values.Length];
				for (int i = 0; i < values.Length; i++)
				{
					converted[i] = ValueParse(type.GetElementType(), values[i]);
				}
				convertedValue = converted;
			}

			return convertedValue;
		}

		static object GetDefaultValue<T>(T obj, MemberInfo member)
		{
			if (member is PropertyInfo)
			{
				return (member as PropertyInfo).GetValue(obj);
			}
			else if (member is FieldInfo)
			{
				return (member as FieldInfo).GetValue(obj);
			}
			return null;
		}
		#endregion
	}
}
