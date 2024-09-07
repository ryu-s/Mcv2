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
        Assert.AreEqual(data.DataTypeName, restoredData?.DataTypeName);
        Assert.AreEqual(data.Content, restoredData?.Content);
    }
}