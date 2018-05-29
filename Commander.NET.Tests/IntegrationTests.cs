using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace Commander.NET.Tests
{
	public delegate void AssertBasic(Basic b);

	[TestFixture]
	public class IntegrationTests
	{
		public static IEnumerable<object[]> BasicTestCases()
		{
			yield return
				new object[]
				{
					new string[]{ "asdf" },
					new AssertBasic(b =>
					{
						Assert.AreEqual(0, b.Row);
						Assert.AreEqual(null, b.Name);
					})
				};
			yield return
				new object[]
				{
					new string[]{ "-r", "5" },
					new AssertBasic(b =>
					{
						Assert.AreEqual(5, b.Row);
						Assert.AreEqual(null, b.Name);
					})
				};
			yield return
				new object[]
				{
					new string[]{ "-r", "5", "--name", "John" },
					new AssertBasic(b =>
					{
						Assert.AreEqual(5, b.Row);
						Assert.AreEqual("John", b.Name);
					})
				};
			yield return
				new object[]
				{
					new string[]{ "--name", "John" },
					new AssertBasic(b =>
					{
						Assert.AreEqual(0, b.Row);
						Assert.AreEqual("John", b.Name);
					})
				};
		}


		[Test, TestCaseSource("BasicTestCases")]
		public void Basic(string[] args, AssertBasic assert)
		{
			CommanderParser<Basic> p = new CommanderParser<Basic>();

			Basic b = p.Parse(args);

			assert(b);
		}
	}
}
