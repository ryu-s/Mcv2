using Mcv.PluginV2;
using NicoSitePlugin;

namespace BouyomiPlugin;

class NicoMessageConverter : ISiteMessageConverter
{
    public (bool success, string? name, string? comment) Convert(ISiteMessage message, Options options)
    {
        var success = true;
        string? name = null;
        string? comment = null;
        if (message is INicoMessage NicoMessage)
        {
            switch (NicoMessage.NicoMessageType)
            {
                case NicoMessageType.Connected:
                    if (options.IsNicoConnect)
                    {
                        name = null;
                        comment = (NicoMessage as INicoConnected).Text;
                    }
                    break;
                case NicoMessageType.Disconnected:
                    if (options.IsNicoDisconnect)
                    {
                        name = null;
                        comment = (NicoMessage as INicoDisconnected).Text;
                    }
                    break;
                case NicoMessageType.Comment:
                    if (options.IsNicoComment)
                    {
                        if (options.IsNicoCommentNickname)
                        {
                            name = (NicoMessage as INicoComment).UserName;
                        }
                        comment = (NicoMessage as INicoComment).Text;
                    }
                    break;
                case NicoMessageType.Item:
                    if (options.IsNicoItem)
                    {
                        if (options.IsNicoItemNickname)
                        {
                            //name = (NicoMessage as INicoItem).NameItems.ToText();
                        }
                        comment = (NicoMessage as INicoGift).Text;
                    }
                    break;
                case NicoMessageType.Ad:
                    if (options.IsNicoAd)
                    {
                        name = null;
                        comment = (NicoMessage as INicoAd).Text;
                    }
                    break;
                case NicoMessageType.Spi:
                    if (options.IsNicoSpi)
                    {
                        name = null;
                        comment = (NicoMessage as INicoSpi).Text;
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
