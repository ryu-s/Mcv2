using Mcv.PluginV2;
using TwicasSitePlugin;

namespace BouyomiPlugin;

class TwicasMessageConverter : ISiteMessageConverter
{
    public (bool success, string? name, string? comment) Convert(ISiteMessage message, Options options)
    {
        var success = true;
        string? name = null;
        string? comment = null;
        if (message is ITwicasMessage twicasMessage)
        {
            switch (twicasMessage.TwicasMessageType)
            {
                case TwicasMessageType.Connected:
                    if (options.IsTwicasConnect)
                    {
                        name = null;
                        comment = (twicasMessage as ITwicasConnected).Text;
                    }
                    break;
                case TwicasMessageType.Disconnected:
                    if (options.IsTwicasDisconnect)
                    {
                        name = null;
                        comment = (twicasMessage as ITwicasDisconnected).Text;
                    }
                    break;
                case TwicasMessageType.Comment:
                    if (options.IsTwicasComment)
                    {
                        if (options.IsTwicasCommentNickname)
                        {
                            name = (twicasMessage as ITwicasComment).UserName;
                        }
                        comment = (twicasMessage as ITwicasComment).CommentItems.ToText();
                    }
                    break;
                case TwicasMessageType.Item:
                    if (options.IsTwicasItem)
                    {
                        if (options.IsTwicasItemNickname)
                        {
                            name = (twicasMessage as ITwicasItem).UserName;
                        }
                        comment = (twicasMessage as ITwicasItem).CommentItems.ToTextWithImageAlt();
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
