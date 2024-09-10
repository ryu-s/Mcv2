using BigoSitePlugin;
using Mcv.PluginV2;

namespace BouyomiPlugin;

class BigoMessageConverter : ISiteMessageConverter
{
    public (bool success, string? name, string? comment) Convert(ISiteMessage message, Options options)
    {
        var success = true;
        string? name = null;
        string? comment = null;
        if (message is IBigoMessage bigoMessage)
        {
            switch (bigoMessage.BigoMessageType)
            {
                case BigoMessageType.Comment:
                    if (options.IsBigoLiveComment)
                    {
                        if (options.IsBigoLiveCommentNickname)
                        {
                            name = (bigoMessage as IBigoComment).Name;
                        }
                        comment = (bigoMessage as IBigoComment).Message;
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
