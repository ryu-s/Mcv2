using LineLiveSitePlugin;
using Mcv.PluginV2;

namespace BouyomiPlugin;

class LineLiveMessageConverter : ISiteMessageConverter
{
    public (bool success, string? name, string? comment) Convert(ISiteMessage message, Options options)
    {
        var success = true;
        string? name = null;
        string? comment = null;
        if (message is ILineLiveMessage lineLiveMessage)
        {
            switch (lineLiveMessage.LineLiveMessageType)
            {
                case LineLiveMessageType.Connected:
                    if (options.IsLineLiveConnect)
                    {
                        name = null;
                        comment = (lineLiveMessage as ILineLiveConnected).Text;
                    }
                    break;
                case LineLiveMessageType.Disconnected:
                    if (options.IsLineLiveDisconnect)
                    {
                        name = null;
                        comment = (lineLiveMessage as ILineLiveDisconnected).Text;
                    }
                    break;
                case LineLiveMessageType.Comment:
                    if (options.IsLineLiveComment)
                    {
                        if (options.IsLineLiveCommentNickname)
                        {
                            name = (lineLiveMessage as ILineLiveComment).DisplayName;
                        }
                        comment = (lineLiveMessage as ILineLiveComment).Text;
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
