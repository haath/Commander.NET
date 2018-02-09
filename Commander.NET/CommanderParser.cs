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
    public static class CommanderParser
    {

		public static T Parse<T>(string[] args) where T : new()
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

			T obj = new T();

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
						SetValue(obj, member, value);
					}
				}
			}

			foreach (MemberInfo member in GetParameterMembers<T, PositionalParameterAttribute>())
			{
				PositionalParameterAttribute param = member.GetCustomAttribute<PositionalParameterAttribute>();

				if (param.Index < positionalArguments.Count)
				{
					SetValue(obj, member, positionalArguments[param.Index]);
				}
			}

			return obj;
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

		static void SetValue<T>(T obj, MemberInfo member, string value)
		{
			object convertedValue = ValueParse(GetType(member), value);

			SetValue(obj, member, convertedValue);
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

		static object GetDefaultValue<T>(MemberInfo member) where T : new()
		{
			if (member is PropertyInfo)
			{
				return (member as PropertyInfo).GetValue(new T());
			}
			else if (member is FieldInfo)
			{
				return (member as FieldInfo).GetValue(new T());
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
