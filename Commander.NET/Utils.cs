using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Commander.NET
{
    internal static class Utils
	{
		internal static IEnumerable<MemberInfo> GetParameterMembers<T, Q>() where Q : Attribute
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

		internal static Type Type(this MemberInfo member)
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

		internal static bool Matches(this string input, string regex)
		{
			return Regex.Match(input, regex).Success;
		}

		internal static T[] Concat<T>(this T[] x, T[] y)
		{
			if (x == null) return y;
			if (y == null) return x;
			int oldLen = x.Length;
			Array.Resize<T>(ref x, x.Length + y.Length);
			Array.Copy(y, 0, x, oldLen, y.Length);
			return x;
		}
	}
}
