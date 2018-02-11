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

		#region Public

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
			string[] args = this.args.ToArray();
			HashSet<string> booleanKeys = new HashSet<string>();
			foreach (MemberInfo member in GetParameterMembers<T, ParameterAttribute>())
			{
				if (GetType(member) == typeof(bool))
				{
					foreach (string booleanKey in member.GetCustomAttribute<ParameterAttribute>().Keys)
						booleanKeys.Add(booleanKey);
				}
			}

			List<string> positionalArguments = new List<string>();
			List<string> flags = new List<string>();
			Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

			Action<string, string> TryAddKeyValuePair = (key, value) => { if (!keyValuePairs.ContainsKey(key)) keyValuePairs.Add(key, value); };

			Func<string, bool> GetBool = (key) =>
			{
				return flags.Contains(key) || keyValuePairs.ContainsKey(key);
			};

			/*
			 * Parse arguments
			 */
			for (int i = 0; i < args.Length; i++)
			{
				if ((Match(args[i], @"^-[a-zA-Z0-9_]=\w+$") || Match(args[i], @"^--[a-zA-Z0-9_]{2,}=\w+$")) && separators.HasFlag(NET.Separators.Equals)
					|| (Match(args[i], @"^-[a-zA-Z0-9_]:\w+$") || Match(args[i], @"^--[a-zA-Z0-9_]{2,}:\w+$")) && separators.HasFlag(NET.Separators.Colon))
				{
					string key = args[i].TrimStart('-').Split(':')[0].Split('=')[0];
					string value = args[i].Split(':').Last().Split('=').Last();

					TryAddKeyValuePair(key, value);
				}
				else if (Match(args[i], @"^-[a-zA-Z0-9_]$") || Match(args[i], @"^--[a-zA-Z0-9_]{2,}$"))
				{
					string key = args[i].TrimStart('-');

					if (!booleanKeys.Contains(key) && i < args.Length - 1 && !args[i + 1].StartsWith("-"))
					{
						TryAddKeyValuePair(key, args[i + 1]);
						i++;
					}
					else
					{
						flags.Add(key);
					}
				}
				else if (Match(args[i], @"^-[a-zA-Z0-9_]{2,}$"))
				{
					flags.AddRange(
						args[i].ToCharArray().Select(c => c.ToString())
						);
				}
				else
				{
					positionalArguments.Add(args[i]);
				}
			}

			/*
			 * Set named arguments
			 */
			foreach (MemberInfo member in GetParameterMembers<T, ParameterAttribute>())
			{
				ParameterAttribute param = member.GetCustomAttribute<ParameterAttribute>();

				if (GetType(member) == typeof(bool))
				{
					SetValue(
						obj,
						member,
						param.Keys.Any(name => GetBool(name))
						);
				}
				else
				{
					string value = param.Keys
						.Where(key => keyValuePairs.ContainsKey(key))
						.Select(key => keyValuePairs[key])
						.FirstOrDefault();
					if (value != null)
					{
						SetValue(obj, member, param, value);
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
			foreach (MemberInfo member in GetParameterMembers<T, PositionalParameterAttribute>())
			{
				PositionalParameterAttribute param = member.GetCustomAttribute<PositionalParameterAttribute>();

				if (param.Index < positionalArguments.Count)
				{
					SetValue(obj, member, param, positionalArguments[param.Index]);
				}
				else if (ParamRequired<T>(defaultObject, param, member))
				{
					// Required parameter missing
					throw new ParameterMissingException(param);
				}
			}

			foreach (MemberInfo member in GetParameterMembers<T, PositionalParameterListAttribute>())
			{
				if (GetType(member).IsArray)
				{
					SetValue(obj, member, positionalArguments.ToArray());
				}
				else
				{
					SetValue(obj, member, new List<string>(positionalArguments));
				}
			}

			return obj;
		}

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
			executableName = executableName ?? AppDomain.CurrentDomain.FriendlyName;
			string indentation = string.Join("", new int[indentationSpaces].Select(s => " "));

			StringBuilder usage = new StringBuilder();
			usage.AppendFormat("Usage: {0} [options] ", executableName);

			IOrderedEnumerable<MemberInfo> positionalParams = GetParameterMembers<T, PositionalParameterAttribute>()
																	.OrderBy(member => member.GetCustomAttribute<PositionalParameterAttribute>().Index);
			IOrderedEnumerable<MemberInfo> optionParams = GetParameterMembers<T, ParameterAttribute>()
																	.OrderBy(member => member.GetCustomAttribute<ParameterAttribute>().Names[0]);

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

			usage.AppendLine();

			foreach (MemberInfo member in positionalParams)
			{
				PositionalParameterAttribute param = member.GetCustomAttribute<PositionalParameterAttribute>();
				// Print positional argument descriptions
				if (param.Description != null)
				{
					usage.AppendFormat("{0}{1}: {2}\n", indentation, param.Name, param.Description);
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

		static bool Match(string input, string regex)
		{
			return Regex.Match(input, regex).Success;
		}

		static IEnumerable<MemberInfo> GetParameterMembers<T, Q>() where Q : Attribute
		{
			foreach (MemberInfo member in typeof(T).GetProperties())
			{
				if (member.GetCustomAttribute<Q>() != null)
					yield return member;
			}
			foreach (MemberInfo member in typeof(T).GetFields())
			{
				if (member.GetCustomAttribute<Q>() != null)
					yield return member;
			}
		}

		static void SetValue<T>(T obj, MemberInfo member, CommanderAttribute param, string value)
		{
			object convertedValue;
			try
			{
				convertedValue = ValueParse(GetType(member), value);
			}
			catch (FormatException)
			{
				// The value had to be parsed to a numerical type and failed
				throw new ParameterFormatException(param, value, member.GetType());
			}

			if (param.Regex != null)
			{
				// We need to validate this value with a regex
				if (!convertedValue.GetType().IsArray && !Match(value, param.Regex))
				{
					// If it's not an array, simple check the string value against the regex
					throw new ParameterMatchException(param, value);
				}
				else if (convertedValue.GetType().IsArray)
				{
					// If it's an array, check every single value against the regex
					if (value.Split(',').FirstOrDefault(val => !Match(val, param.Regex)) != null)
					{
						throw new ParameterMatchException(param, value);
					}
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

		static Type GetType(MemberInfo member)
		{
			if (member is PropertyInfo)
			{
				return (member as PropertyInfo).PropertyType;
			}
			else if (member is FieldInfo)
			{
				return (member as FieldInfo).FieldType;
			}
			return null;
		}
		#endregion
	}
}
