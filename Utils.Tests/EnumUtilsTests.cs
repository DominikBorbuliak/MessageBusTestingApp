﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using @Description = System.ComponentModel.DescriptionAttribute;

namespace Utils.Tests
{
	[TestClass()]
	public class EnumUtilsTests
	{
		[TestMethod()]
		public void GetConfigurationNameTest()
		{
			// Basic test cases
			Assert.AreEqual(string.Empty, TestEnumA.TypeA1.GetConfigurationName());
			Assert.AreEqual(string.Empty, TestEnumA.TypeA2.GetConfigurationName());
			Assert.AreEqual("TypeA3ConfigurationName", TestEnumA.TypeA3.GetConfigurationName());
			Assert.AreEqual("TypeA4ConfigurationName", TestEnumA.TypeA4.GetConfigurationName());

			// Advanced test cases
			Assert.AreEqual("TypeB1ConfigurationName", ((TestEnumB)TestEnumA.TypeA1).GetConfigurationName());
			Assert.ThrowsException<ArgumentException>(() => ((TestEnumB)TestEnumA.TypeA2).GetConfigurationName());
		}

		[TestMethod()]
		public void GetMenuDisplayNameTest()
		{
			// Basic test cases
			Assert.AreEqual(string.Empty, TestEnumA.TypeA1.GetMenuDisplayName());
			Assert.AreEqual(string.Empty, TestEnumA.TypeA2.GetMenuDisplayName());
			Assert.AreEqual("TypeA3MenuDisplayName", TestEnumA.TypeA3.GetMenuDisplayName());
			Assert.AreEqual("TypeA4MenuDisplayName", TestEnumA.TypeA4.GetMenuDisplayName());

			// Advanced test cases
			Assert.AreEqual("TypeB1MenuDisplayName", ((TestEnumB)TestEnumA.TypeA1).GetMenuDisplayName());
			Assert.ThrowsException<ArgumentException>(() => ((TestEnumB)TestEnumA.TypeA2).GetMenuDisplayName());
		}

		[TestMethod()]
		public void GetDescriptionTest()
		{
			// Basic test cases
			Assert.AreEqual(string.Empty, TestEnumA.TypeA1.GetDescription());
			Assert.AreEqual(string.Empty, TestEnumA.TypeA2.GetDescription());
			Assert.AreEqual("TypeA3Description", TestEnumA.TypeA3.GetDescription());
			Assert.AreEqual("TypeA4Description", TestEnumA.TypeA4.GetDescription());

			// Advanced test cases
			Assert.AreEqual("TypeB1Description", ((TestEnumB)TestEnumA.TypeA1).GetDescription());
			Assert.ThrowsException<ArgumentException>(() => ((TestEnumB)TestEnumA.TypeA2).GetDescription());
		}

		[TestMethod()]
		public void GetValueFromMenuDisplayNameTest()
		{
			var resultA = EnumUtils.GetValueFromMenuDisplayName<TestEnumA>("TypeA3MenuDisplayName");
			Assert.IsNotNull(resultA);
			Assert.AreEqual(TestEnumA.TypeA3, (TestEnumA)resultA);

			resultA = EnumUtils.GetValueFromMenuDisplayName<TestEnumA>(string.Empty);
			Assert.IsNotNull(resultA);
			Assert.AreEqual(TestEnumA.TypeA2, (TestEnumA)resultA);

			resultA = EnumUtils.GetValueFromMenuDisplayName<TestEnumA>("TypeANotExistingMenuDisplayName");
			Assert.IsNull(resultA);

			var resultB = EnumUtils.GetValueFromMenuDisplayName<TestEnumB>("TypeB1MenuDisplayName");
			Assert.IsNotNull(resultB);
			Assert.AreEqual(TestEnumB.TypeB1, (TestEnumB)resultB);

			resultB = EnumUtils.GetValueFromMenuDisplayName<TestEnumB>("TypeBNotExistingMenuDisplayName");
			Assert.IsNull(resultB);
		}
	}

	/// <summary>
	/// Mocked enum for testing purposes
	/// </summary>
	public enum TestEnumA
	{
		TypeA1,

		[ConfigurationName("")]
		[MenuDisplayName("")]
		[@Description("")]
		TypeA2,

		[ConfigurationName("TypeA3ConfigurationName")]
		[MenuDisplayName("TypeA3MenuDisplayName")]
		[@Description("TypeA3Description")]
		TypeA3,

		[ConfigurationName("TypeA4ConfigurationName")]
		[MenuDisplayName("TypeA4MenuDisplayName")]
		[@Description("TypeA4Description")]
		TypeA4
	}

	/// <summary>
	/// Mocked enum for testing purposes
	/// </summary>
	public enum TestEnumB
	{
		[ConfigurationName("TypeB1ConfigurationName")]
		[MenuDisplayName("TypeB1MenuDisplayName")]
		[@Description("TypeB1Description")]
		TypeB1
	}
}