using Microsoft.VisualStudio.TestTools.UnitTesting;
using static SprintfNET.StringFormatter;

namespace SprintfNET.Tests
{
    [TestClass]
    public class LongTests
    {
        private const string MaxValue = "9223372036854775807";
        private const string MinValue = "-9223372036854775808";

        [DataTestMethod]
        [DataRow(1, "1")]
        [DataRow(42, "42")]
        [DataRow(-81, "-81")]
        [DataRow(long.MaxValue, MaxValue)]
        [DataRow(long.MinValue, MinValue)]
        public void Format(long value, string expected) 
            => Assert.AreEqual(expected, PrintF("%lld", value));

        [DataTestMethod]
        [DataRow(1, 3, "  1")]
        [DataRow(42, -4, "42  ")]
        [DataRow(-81, -8, "-81     ")]
        [DataRow(-3, 5, "   -3")]
        [DataRow(long.MaxValue, 2, MaxValue)]
        [DataRow(long.MinValue, 4, MinValue)]
        public void Padding(long value, int padding, string expected) 
            => Assert.AreEqual(expected, PrintF($"%{padding}lld", value));

        [DataTestMethod]
        [DataRow(1, 3, "001")]
        [DataRow(-3, 5, "-0003")]
        [DataRow(long.MaxValue, 2, MaxValue)]
        [DataRow(long.MinValue, 4, MinValue)]
        public void ZeroPadding(long value, int padding, string expected)
            => Assert.AreEqual(expected, PrintF($"%0{padding}lli", value));

        [DataTestMethod]
        [DataRow(1, "1")]
        [DataRow(42, "42")]
        [DataRow(-81, "-81")]
        [DataRow(long.MaxValue, MaxValue)]
        [DataRow(long.MinValue, MinValue)]
        public void NSStyleFormat(long value, string expected)
            => Assert.AreEqual(expected, PrintF("%@", value));

    }
}
