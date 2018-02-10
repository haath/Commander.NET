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
			return Usage<T>(new T(), executableName, indentationSpaces);
		}

		internal static string Usage<T>(T defaultObj, string executableName = null, int indentationSpaces = 4)
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

				if (ParamRequired<T>(defaultObj, param, member))
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
				object defaultValue = GetDefaultValue<T>(defaultObj, member);
				bool required = ParamRequired<T>(defaultObj, param, member);

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

		/// <summary>
		/// Parse the given arguments and serialize them into a new instance of a T object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="args"></param>
		/// <returns></returns>
		public static T Parse<T>(params string[] args) where T : new()
		{
			return Parse<T>(new T(), new T(), args);
		}

		/// <summary>
		/// Parse the given arguments and serialize them into an existing instance of a T object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static T Parse<T>(T obj, params string[] args) where T : new()
		{
			return Parse<T>(new T(), obj, args);
		}

		internal static T Parse<T>(T defaultObj, T obj, params string[] args)
		{
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

			Func<string, bool> GetBool = (key) =>
			{
				return flags.Contains(key) || keyValuePairs.ContainsKey(key);
			};

			/*
			 * Parse arguments
			 */
			for (int i = 0; i < args.Length; i++)
			{
				if (Match(args[i], @"^-\w$") || Match(args[i], @"^--\w{2,}$"))
				{
					string key = args[i].TrimStart('-');

					if (!booleanKeys.Contains(key) && i < args.Length - 1 && !args[i + 1].StartsWith("-"))
					{
						keyValuePairs.Add(key, args[i + 1]);
						i++;
					}
					else
					{
						flags.Add(key);
					}
				}
				else if (Match(args[i], @"^-\w{2,}$"))
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
					else if (ParamRequired<T>(defaultObj, param, member))
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
				else if (ParamRequired<T>(defaultObj, param, member))
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
					SetValue(obj, member, positionalArguments);
				}
			}

			return obj;
		}

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
			try
			{
				object convertedValue = ValueParse(GetType(member), value);

				SetValue(obj, member, convertedValue);
			}
			catch (FormatException)
			{
				throw new ParameterFormatException(param, value, member.GetType());
			}
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
	}
}
