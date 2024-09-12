using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using LineLiveSitePlugin;
using Moq;

namespace LineLiveSitePluginTests
{
    [TestFixture]
    class ApiTests
    {
        [Test]
        public async Task GetPromptyStatsTest()
        {
            var data = TestHelper.GetSampleData("PromptyStats.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(data));
            var server = serverMock.Object;
            var res = await Api.GetPromptyStats(server, "", "");
            Assert.That(res.LiveStatus, Is.EqualTo("LIVE"));
            Assert.That(res.Status, Is.EqualTo(200));
            Assert.That(res.ApiStatusCode, Is.EqualTo(200));
        }
    }
}
