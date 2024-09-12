using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TwicasSitePlugin;
using YouTubeLiveSitePluginTests;

namespace TwicasSitePluginTests
{
    [TestFixture]
    class ApiTests
    {
        [Test]
        public async Task Test()
        {
            var data = TestHelper.GetSampleData("ListAll.txt");
            var serverMock = new Mock<IDataServer>();
            serverMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CookieContainer>())).Returns(Task.FromResult(data));
            var server = serverMock.Object;
            var (comments, raw) = await API.GetListAll(server, "", 0, 0, 0, 0, new System.Net.CookieContainer());
            Assert.That(comments.Length, Is.EqualTo(20));
            var c = comments[0];
            Assert.That(c.Type, Is.EqualTo("comment"));
            Assert.That(c.CreatedAt, Is.EqualTo(1583166022000));
            Assert.That(c.HtmlMessage, Is.Null);
            Assert.That(c.Id, Is.EqualTo(17919844422));
            Assert.That(c.Message, Is.EqualTo("次いでによっちゃんも残して死なないで"));
            Assert.That(c.Author.Name, Is.EqualTo("エコハ@🍃"));
        }
    }
}
