using Mcv.PluginV2;
using TwitchSitePlugin;

namespace BouyomiPlugin;

class TwitchMessageConverter : ISiteMessageConverter
{
    public (bool success, string? name, string? comment) Convert(ISiteMessage message, Options options)
    {
        var success = true;
        string? name = null;
        string? comment = null;
        if (message is ITwitchMessage twitchMessage)
        {
            switch (twitchMessage.TwitchMessageType)
            {
                case TwitchMessageType.Connected:
                    if (options.IsTwitchConnect)
                    {
                        name = null;
                        comment = (twitchMessage as ITwitchConnected).Text;
                    }
                    break;
                case TwitchMessageType.Disconnected:
                    if (options.IsTwitchDisconnect)
                    {
                        name = null;
                        comment = (twitchMessage as ITwitchDisconnected).Text;
                    }
                    break;
                case TwitchMessageType.Comment:
                    if (options.IsTwitchComment)
                    {
                        if (options.IsTwitchCommentNickname)
                        {
                            name = (twitchMessage as ITwitchComment).DisplayName;
                        }
                        comment = (twitchMessage as ITwitchComment).CommentItems.ToText();
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
