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
					new string[]{ "" },
					new AssertBasic(b =>
					{
						Assert.AreEqual(0, b.Row);
						Assert.AreEqual(null, b.Name);
						Assert.IsFalse(b.Flag);
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
						Assert.IsFalse(b.Flag);
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
						Assert.IsFalse(b.Flag);
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
						Assert.IsFalse(b.Flag);
					})
				};
			yield return
				new object[]
				{
					new string[]{ "--name", "John&Doe" },
					new AssertBasic(b =>
					{
						Assert.AreEqual(0, b.Row);
						Assert.AreEqual("John&Doe", b.Name);
						Assert.IsFalse(b.Flag);
					})
				};
			yield return
				new object[]
				{
					new string[]{ "--name", "John&Doe", "0.34" },
					new AssertBasic(b =>
					{
						Assert.AreEqual(0, b.Row);
						Assert.AreEqual("John&Doe", b.Name);
						Assert.AreEqual(0.34, b.Positional);
						Assert.IsFalse(b.Flag);
					})
				};
			yield return
				new object[]
				{
					new string[]{ "--name", "John&Doe", "0.34", "-r", "12" },
					new AssertBasic(b =>
					{
						Assert.AreEqual(12, b.Row);
						Assert.AreEqual("John&Doe", b.Name);
						Assert.AreEqual(0.34, b.Positional);
						Assert.IsFalse(b.Flag);
					})
				};
			yield return
				new object[]
				{
					new string[]{ "--extra-args", "someValue" },
					new AssertBasic(b =>
					{
						Assert.AreEqual("someValue", b.ExtraArg);
						Assert.IsFalse(b.Flag);
					})
				};
			yield return
				new object[]
				{
					new string[]{ "--extra-args", "someValue", "-f" },
					new AssertBasic(b =>
					{
						Assert.AreEqual("someValue", b.ExtraArg);
						Assert.IsTrue(b.Flag);
					})
				};
			yield return
				new object[]
				{
					new string[]{ "--name", "John&Doe", "0.34", "--for-sure", "-r", "12" },
					new AssertBasic(b =>
					{
						Assert.AreEqual(12, b.Row);
						Assert.AreEqual("John&Doe", b.Name);
						Assert.AreEqual(0.34, b.Positional);
						Assert.IsTrue(b.Flag);
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
