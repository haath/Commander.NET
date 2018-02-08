using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

using Commander.NET.Attributes;
using Commander.NET.Exceptions;

namespace Commander.NET
{
    public static class CommanderParser
    {

		public static T Parse<T>(string[] args) where T : new()
		{
			List<string> positionalArguments = new List<string>();

			for (int i = 0; i < args.Length; i++)
			{
				if (!args[i].StartsWith("-")
					&& (i == 0 || !args[i - 1].StartsWith("-")))
				{
					positionalArguments.Add(args[i]);
				}
			}

			T obj = new T();

			foreach (MemberInfo member in GetParameterMembers<T, ParameterAttribute>())
			{
				ParameterAttribute param = member.GetCustomAttribute<ParameterAttribute>();

				for (int i = 0; i < args.Length; i++)
				{
					if (param.MatchesName(args[i]))
					{
						if (GetType(member) == typeof(bool))
						{
							SetValue(obj, member, true);
						}
						else if (i < args.Length - 1 && !args[i + 1].StartsWith("-"))
						{
							SetValue(obj, member, args[i + 1]);
						}
					}
				}
			}

			return obj;
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
