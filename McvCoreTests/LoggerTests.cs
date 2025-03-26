using Mcv.Core;

namespace McvCoreTests;

public class LoggerTests
{
    [Test]
    public void RestoreDataTest()
    {
        var ex = new Exception("Test", new Exception("inner"));
        var data = new Data(ex);
        var json = data.ToJson();
        var restoredData = Data.FromJson(json);
        Assert.That(data.DataTypeName, Is.EqualTo(restoredData?.DataTypeName));
        Assert.That(data.Content, Is.EqualTo(restoredData?.Content));
    }
}