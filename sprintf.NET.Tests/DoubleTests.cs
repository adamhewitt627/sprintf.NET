using Microsoft.VisualStudio.TestTools.UnitTesting;
using static SprintfNET.StringFormatter;

namespace SprintfNET.Tests
{
    [TestClass]
    public class DoubleTests
    {
        [DataTestMethod]
        [DataRow(5, "%lf", "")]
        [DataRow(1.2, "%.1lf", "1.2")]
        public void Format(double value, string format, string expected)
            => Assert.AreEqual(expected, PrintF(format, value));

    }
}