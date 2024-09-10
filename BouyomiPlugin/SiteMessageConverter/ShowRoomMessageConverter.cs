using Mcv.PluginV2;
using ShowRoomSitePlugin;

namespace BouyomiPlugin;

class ShowRoomMessageConverter : ISiteMessageConverter
{
    public (bool success, string? name, string? comment) Convert(ISiteMessage message, Options options)
    {
        var success = true;
        string? name = null;
        string? comment = null;
        if (message is IShowRoomMessage showroomMessage)
        {
            switch (showroomMessage.ShowRoomMessageType)
            {
                case ShowRoomMessageType.Comment:
                    if (options.IsShowRoomComment)
                    {
                        if (options.IsShowRoomCommentNickname)
                        {
                            name = (showroomMessage as IShowRoomComment).UserName;
                        }
                        comment = (showroomMessage as IShowRoomComment).Text;
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
