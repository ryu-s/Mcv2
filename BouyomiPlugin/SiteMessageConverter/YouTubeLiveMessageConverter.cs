using Mcv.PluginV2;
using Mcv.YouTubeLiveSitePlugin;

namespace BouyomiPlugin;

class YouTubeLiveMessageConverter : ISiteMessageConverter
{
    public (bool success, string? name, string? comment) Convert(ISiteMessage message, Options options)
    {
        var success = true;
        string? name = null;
        string? comment = null;
        if (message is IYouTubeLiveMessage youTubeLiveMessage)
        {
            switch (youTubeLiveMessage.YouTubeLiveMessageType)
            {
                case YouTubeLiveMessageType.Connected:
                    if (options.IsYouTubeLiveConnect)
                    {
                        name = null;
                        comment = (youTubeLiveMessage as IYouTubeLiveConnected)?.Text;
                    }
                    break;
                case YouTubeLiveMessageType.Disconnected:
                    if (options.IsYouTubeLiveDisconnect)
                    {
                        name = null;
                        comment = (youTubeLiveMessage as IYouTubeLiveDisconnected)?.Text;
                    }
                    break;
                case YouTubeLiveMessageType.Comment:
                    if (options.IsYouTubeLiveComment)
                    {
                        if (options.IsYouTubeLiveCommentNickname)
                        {
                            name = (youTubeLiveMessage as IYouTubeLiveComment)?.NameItems.ToText();
                        }
                        if (options.IsYouTubeLiveCommentStamp)
                        {
                            comment = (youTubeLiveMessage as IYouTubeLiveComment)?.CommentItems.ToTextWithImageAlt();
                        }
                        else
                        {
                            comment = (youTubeLiveMessage as IYouTubeLiveComment)?.CommentItems.ToText();
                        }
                    }
                    break;
                case YouTubeLiveMessageType.Superchat:
                    if (options.IsYouTubeLiveSuperchat)
                    {
                        var superChat = youTubeLiveMessage as IYouTubeLiveSuperchat;
                        if (options.IsYouTubeLiveSuperchatNickname)
                        {
                            name = superChat?.NameItems.ToText();
                        }
                        //TODO:superchat中のスタンプも読ませるべきでは？
                        var text = superChat?.CommentItems.ToText();
                        var amount = superChat?.PurchaseAmount;
                        comment = amount + Environment.NewLine + text;
                    }
                    break;
                case YouTubeLiveMessageType.Membership:
                    if (options.IsYouTubeLiveMembership)
                    {
                        var membership = youTubeLiveMessage as IYouTubeLiveMembership;
                        if (options.IsYouTubeLiveMembershipNickname)
                        {
                            name = membership?.NameItems.ToText();
                        }
                        comment = membership?.CommentItems.ToText();
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
