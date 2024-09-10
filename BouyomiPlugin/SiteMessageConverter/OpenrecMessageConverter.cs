using Mcv.PluginV2;
using OpenrecSitePlugin;

namespace BouyomiPlugin;

class OpenrecMessageConverter : ISiteMessageConverter
{
    public (bool success, string? name, string? comment) Convert(ISiteMessage message, Options options)
    {
        var success = true;
        string? name = null;
        string? comment = null;
        if (message is IOpenrecMessage openrecMessage)
        {
            switch (openrecMessage.OpenrecMessageType)
            {
                case OpenrecMessageType.Connected:
                    if (options.IsOpenrecConnect)
                    {
                        name = null;
                        comment = (openrecMessage as IOpenrecConnected).Text;
                    }
                    break;
                case OpenrecMessageType.Disconnected:
                    if (options.IsOpenrecDisconnect)
                    {
                        name = null;
                        comment = (openrecMessage as IOpenrecDisconnected).Text;
                    }
                    break;
                case OpenrecMessageType.Comment:
                    if (options.IsOpenrecComment)
                    {
                        if (options.IsOpenrecCommentNickname)
                        {
                            name = (openrecMessage as IOpenrecComment).NameItems.ToText();
                        }
                        comment = (openrecMessage as IOpenrecComment).MessageItems.ToText();
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
