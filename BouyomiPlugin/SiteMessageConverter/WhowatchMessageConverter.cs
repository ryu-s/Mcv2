using Mcv.PluginV2;
using WhowatchSitePlugin;

namespace BouyomiPlugin;

class WhowatchMessageConverter : ISiteMessageConverter
{
    public (bool success, string? name, string? comment) Convert(ISiteMessage message, Options options)
    {
        var success = true;
        string? name = null;
        string? comment = null;
        if (message is IWhowatchMessage whowatchMessage)
        {
            switch (whowatchMessage.WhowatchMessageType)
            {
                case WhowatchMessageType.Connected:
                    if (options.IsWhowatchConnect)
                    {
                        name = null;
                        comment = (whowatchMessage as IWhowatchConnected).Text;
                    }
                    break;
                case WhowatchMessageType.Disconnected:
                    if (options.IsWhowatchDisconnect)
                    {
                        name = null;
                        comment = (whowatchMessage as IWhowatchDisconnected).Text;
                    }
                    break;
                case WhowatchMessageType.Comment:
                    if (options.IsWhowatchComment)
                    {
                        if (options.IsWhowatchCommentNickname)
                        {
                            name = (whowatchMessage as IWhowatchComment).UserName;
                        }
                        comment = (whowatchMessage as IWhowatchComment).Comment;
                    }
                    break;
                case WhowatchMessageType.Item:
                    if (options.IsWhowatchItem)
                    {
                        if (options.IsWhowatchItemNickname)
                        {
                            name = (whowatchMessage as IWhowatchItem).UserName;
                        }
                        comment = (whowatchMessage as IWhowatchItem).Comment;
                    }
                    break;
                default:
                    success = false;
                    break;
            }
        }
        else
        {
            success = false;
        }
        return (success, name, comment);
    }
}
