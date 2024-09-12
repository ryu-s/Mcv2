using NUnit.Framework;
using Mcv.MainViewPlugin;
using System.Windows.Media;

namespace MainViewPluginTests
{

    [TestFixture]
    public class Class1
    {
        [Test]
        public void Test()
        {
            var options = new DynamicOptionsTest()
            {
                BackColor = Colors.Black,
            };
            var aa = new DynamicOptionsTest()
            {
                BackColor = Colors.Blue,
            };
            options.Set(aa);

            Assert.That(options.BackColor, Is.EqualTo(Colors.Blue));
        }
    }
}