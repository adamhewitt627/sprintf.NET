using Microsoft.VisualStudio.TestTools.UnitTesting;
using static SprintfNET.StringFormatter;

namespace SprintfNET.Tests
{
    [TestClass]
    public class IntegerTests
    {
        [DataTestMethod]
        [DataRow(1, "1")]
        [DataRow(42, "42")]
        [DataRow(-81, "-81")]
        [DataRow(int.MaxValue, "2147483647")]
        [DataRow(int.MinValue, "-2147483648")]
        public void Format(int value, string expected) 
            => Assert.AreEqual(expected, PrintF("%d", value));

        [DataTestMethod]
        [DataRow(1, 3, "  1")]
        [DataRow(42, -4, "42  ")]
        [DataRow(-81, -8, "-81     ")]
        [DataRow(-3, 5, "   -3")]
        [DataRow(int.MaxValue, 2, "2147483647")]
        [DataRow(int.MinValue, 4, "-2147483648")]
        public void Padding(int value, int padding, string expected) 
            => Assert.AreEqual(expected, PrintF($"%{padding}d", value));

        [DataTestMethod]
        [DataRow(1, 3, "001")]
        [DataRow(-3, 5, "-0003")]
        [DataRow(int.MaxValue, 2, "2147483647")]
        [DataRow(int.MinValue, 4, "-2147483648")]
        public void ZeroPadding(int value, int padding, string expected)
            => Assert.AreEqual(expected, PrintF($"%0{padding}d", value));

        [DataTestMethod]
        [DataRow(1, "1")]
        [DataRow(42, "42")]
        [DataRow(-81, "-81")]
        [DataRow(int.MaxValue, "2147483647")]
        [DataRow(int.MinValue, "-2147483648")]
        public void NSStyleFormat(int value, string expected)
            => Assert.AreEqual(expected, PrintF("%@", value));

    }
}
