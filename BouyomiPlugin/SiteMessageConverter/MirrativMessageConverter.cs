using Mcv.PluginV2;
using MirrativSitePlugin;

namespace BouyomiPlugin;

class MirrativMessageConverter : ISiteMessageConverter
{
    public (bool success, string? name, string? comment) Convert(ISiteMessage message, Options options)
    {
        var success = true;
        string? name = null;
        string? comment = null;
        if (message is IMirrativMessage mirrativMessage)
        {
            switch (mirrativMessage.MirrativMessageType)
            {
                case MirrativMessageType.Connected:
                    if (options.IsMirrativConnect)
                    {
                        name = null;
                        comment = (mirrativMessage as IMirrativConnected).Text;
                    }
                    break;
                case MirrativMessageType.Disconnected:
                    if (options.IsMirrativDisconnect)
                    {
                        name = null;
                        comment = (mirrativMessage as IMirrativDisconnected).Text;
                    }
                    break;
                case MirrativMessageType.Comment:
                    if (options.IsMirrativComment)
                    {
                        if (options.IsMirrativCommentNickname)
                        {
                            name = (mirrativMessage as IMirrativComment).UserName;
                        }
                        comment = (mirrativMessage as IMirrativComment).Text;
                    }
                    break;
                case MirrativMessageType.JoinRoom:
                    if (options.IsMirrativJoinRoom)
                    {
                        name = null;
                        comment = (mirrativMessage as IMirrativJoinRoom).Text;
                    }
                    break;
                case MirrativMessageType.Item:
                    if (options.IsMirrativItem)
                    {
                        name = null;
                        comment = (mirrativMessage as IMirrativItem).Text;
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
