namespace Mcv.PluginV2.Messages;

public class ErrorOccurred : IReplyMessageToPluginV2
{
    public Exception? Exception { get; }
    public string Raw => "";
    public ErrorOccurred()
    {

    }
    public ErrorOccurred(Exception ex)
    {
        Exception = ex;
    }
}
