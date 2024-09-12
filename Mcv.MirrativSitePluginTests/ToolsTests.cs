using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrativSitePluginTests
{
    [TestFixture]
    class ToolsTests
    {
        [Test]
        public void ExtractLiveIdTest()
        {
            Assert.That(MirrativSitePlugin.Tools.ExtractLiveId("https://www.mirrativ.com/live/sUbSZSYyTAOYJtLd0WamoQ?test"), Is.EqualTo("sUbSZSYyTAOYJtLd0WamoQ"));
            Assert.That(MirrativSitePlugin.Tools.ExtractLiveId("https://www.mirrativ.com/broadcast/sUbSZSYyTAOYJtLd0WamoQ?test"), Is.EqualTo("sUbSZSYyTAOYJtLd0WamoQ"));
        }
        [Test]
        public void ExtractUserIdTest()
        {
            Assert.That(MirrativSitePlugin.Tools.ExtractUserId("https://www.mirrativ.com/user/1091674"), Is.EqualTo("1091674"));
            Assert.That(MirrativSitePlugin.Tools.ExtractUserId("abc"), Is.Null);
        }
        [Test]
        public void KeyValue2DictTest()
        {
            var str = "{\"key1\":\"value1\",\"key2\":0,\"key3\":\"value3\"}";
            var dict = MirrativSitePlugin.Tools.KeyValue2Dict(str);
            Assert.That(dict.Count, Is.EqualTo(3));
            Assert.That(dict.ContainsKey("key1"), Is.True);
            Assert.That(dict["key1"], Is.EqualTo("value1"));
            Assert.That(dict.ContainsKey("key2"), Is.True);
            Assert.That(dict["key2"], Is.EqualTo("0"));
            Assert.That(dict.ContainsKey("key3"), Is.True);
            Assert.That(dict["key3"], Is.EqualTo("value3"));
        }
    }
}
