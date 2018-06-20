using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Commander.NET.Tests
{
	[TestFixture]
	public class UtilsTests
	{
		public static IEnumerable<object[]> ParseTestCases()
		{
			yield return
				new object[]
				{
					"--name John&Doe pos",
					new string[]{ "--name", "John&Doe", "pos" }
				};
			yield return
				new object[]
				{
					"\"JohnDoe\"",
					new string[]{ "JohnDoe" }
				};
			yield return
				new object[]
				{
					"--name \"JohnDoe\"",
					new string[]{ "--name", "JohnDoe" }
				};
			yield return
				new object[]
				{
					"pos --name \"JohnDoe\"",
					new string[]{ "pos", "--name", "JohnDoe" }
				};
			yield return
				new object[]
				{
					"--name \"JohnDoe\" pos",
					new string[]{  "--name", "JohnDoe", "pos" }
				};
			yield return
				new object[]
				{
					"--name \"JohnDoe\" ' pos ' 123",
					new string[]{  "--name", "JohnDoe", " pos ", "123" }
				};
			yield return
				new object[]
				{
					"--name \"John Doe\" ' pos ' 123",
					new string[]{  "--name", "John Doe", " pos ", "123" }
				};
			yield return
				new object[]
				{
					"--name \"John&Doe\" ' pos ' 123",
					new string[]{  "--name", "John&Doe", " pos ", "123" }
				};
			yield return
				new object[]
				{
					"--name \"John&'Doe'\" ' pos ' 123",
					new string[]{  "--name", "John&'Doe'", " pos ", "123" }
				};
			yield return
				new object[]
				{
					"--name \"John& 'Doe ' \" ' pos ' 123",
					new string[]{  "--name", "John& 'Doe ' ", " pos ", "123" }
				};
		}

		[Test, TestCaseSource("ParseTestCases")]
		public void Parse(string line, string[] exp)
		{
			string[] args = Utils.SplitArgumentsLine(line);

			CollectionAssert.AreEqual(exp, args);
		}
	}
}
