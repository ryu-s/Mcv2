using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhowatchSitePlugin;

namespace WhowatchSitePluginTests
{
    [TestFixture]
    class ToolsTests
    {
        [Test]
        public void IsValidUrlTest()
        {
            Assert.That(Tools.IsValidUrl("https://whowatch.tv/profile/w:kagawapro&test"), Is.True);
            Assert.That(Tools.IsValidUrl("https://whowatch.tv/profile/t:kagawapro&test"), Is.True);
            Assert.That(Tools.IsValidUrl("https://whowatch.tv/archives/6994269"), Is.True);
            Assert.That(Tools.IsValidUrl("https://whowatch.tv/archives/"), Is.False);
        }
        [Test]
        public void ExtractLiveIdFromInputTest()
        {
            Assert.That(Tools.ExtractLiveIdFromInput("https://whowatch.tv/viewer/7007970"), Is.EqualTo(7007970));
        }
        [TestCase("w:kagawapro", "https://whowatch.tv/profile/w:kagawapro")]
        [TestCase("t:kagawapro", "t:kagawapro")]
        [TestCase("t:shinya_yuunari", "t:shinya_yuunari")]
        [TestCase("t:test123", "t:test123")]
        [TestCase(null, "")]
        [TestCase("w:Abc", "w:Abc")]
        public void ExtractUserPathFromInputTest(string expected, string input)
        {
            Assert.That(Tools.ExtractUserPathFromInput(input), Is.EqualTo(expected));
        }

    }
}
