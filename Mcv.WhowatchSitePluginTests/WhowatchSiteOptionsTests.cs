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
    class WhowatchSiteOptionsTests
    {
        [Test]
        public void DeepCloneTest()
        {
            var options = new WhowatchSiteOptions();
            options.LiveCheckIntervalSec = 555;
            var clone = options.Clone();
            options.LiveCheckIntervalSec = 777;
            Assert.That(clone.LiveCheckIntervalSec, Is.EqualTo(555));
        }
    }
}
