namespace Mcv.PluginV2.Messages;

public class ErrorOccurred : IReplyMessageToPluginV2
{
    public Exception? Exception { get; }
    public string Raw => $"{{\"type\":\"reply\",\"reply\":\"error\",\"error\":{System.Text.Json.JsonSerializer.Serialize(Exception)}}}";
    public ErrorOccurred()
    {

    }
    public ErrorOccurred(Exception ex)
    {
        Exception = ex;
    }
}
