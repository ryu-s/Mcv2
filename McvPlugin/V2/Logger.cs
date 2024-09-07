namespace Mcv.PluginV2;
[Obsolete("共通化する必要性が無くなったため")]
public interface ILogger
{
    string GetExceptions();
    void LogException(Exception ex, string message = "", string detail = "");
}
