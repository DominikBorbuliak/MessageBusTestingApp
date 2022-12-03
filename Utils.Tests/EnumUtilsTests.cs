using Microsoft.VisualStudio.TestTools.UnitTesting;
using Description = System.ComponentModel.DescriptionAttribute;

namespace Utils.Tests
{
	[TestClass()]
	public class EnumUtilsTests
	{
		[TestMethod()]
		public void GetDescriptionTest()
		{
			// Basic test cases
			Assert.AreEqual(string.Empty, TestEnumA.TypeA1.GetDescription());
			Assert.AreEqual(string.Empty, TestEnumA.TypeA2.GetDescription());
			Assert.AreEqual("TypeA3Description", TestEnumA.TypeA3.GetDescription());
			Assert.AreEqual("TypeA4Description", TestEnumA.TypeA4.GetDescription());

			// Advanced test cases
			Assert.AreEqual("TypeB1Description", ((TestEnumB) TestEnumA.TypeA1).GetDescription());
			Assert.AreEqual(null, ((TestEnumB) TestEnumA.TypeA2).GetDescription());
		}
	}

	/// <summary>
	/// Mocked enum for testing purposes
	/// </summary>
	public enum TestEnumA
	{
		TypeA1,

		[@Description("")]
		TypeA2,

		[@Description("TypeA3Description")]
		TypeA3,

		[@Description("TypeA4Description")]
		TypeA4
	}

	/// <summary>
	/// Mocked enum for testing purposes
	/// </summary>
	public enum TestEnumB
	{
		[@Description("TypeB1Description")]
		TypeB1
	}
}