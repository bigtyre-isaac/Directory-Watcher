using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass()]
    public class StringExtensionsTests
    {
        [TestMethod()]
        public void TrimStartExactMatch()
        {
            Assert.AreEqual(StringExtensions.TrimStart("unusualFrog", "unusual"), "Frog");
        }

        [TestMethod()]
        public void TrimStartNonmatch()
        {
            Assert.AreEqual(StringExtensions.TrimStart("unusualFrog", "common"), "unusualFrog");
        }

        [TestMethod()]
        public void TrimStartCaseNonMatch()
        {
            Assert.AreEqual(StringExtensions.TrimStart("unusualFrog", "Unusual"), "unusualFrog");
        }

        [TestMethod()]
        public void TrimStartIgnoreCase()
        {
            Assert.AreEqual(StringExtensions.TrimStart("unusualFrog", "Unusual", StringComparison.OrdinalIgnoreCase), "Frog");
        }
    }
}