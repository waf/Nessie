using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nessie.Tests.Utilities
{
    public static class AssertHelper
    {
        /// <summary>
        /// AreEqual that normalizes newlines. 
        /// Useful because our tests run on Windows, Linux, and Mac OS, and pandoc outputs
        /// platform-specific newlines.
        /// </summary>
        public static void AreEqualIgnoringNewLines(string expected, string actual)
        {
            expected = expected.Replace("\r\n", "").Replace("\n", "");
            actual = actual.Replace("\r\n", "").Replace("\n", "");
            Assert.AreEqual(expected, actual);
        }
    }
}