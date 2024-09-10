using Mcv.PluginV2;
using MixchSitePlugin;

namespace BouyomiPlugin;

class MixchMessageConverter : ISiteMessageConverter
{
    public (bool success, string? name, string? comment) Convert(ISiteMessage message, Options options)
    {
        var success = true;
        string? name = null;
        string? comment = null;
        if (message is IMixchMessage mixchMessage)
        {
            switch (mixchMessage.MixchMessageType)
            {
                case MixchMessageType.Comment:
                    if (options.IsMixchComment && (!options.IsMixchCommentOnlyFirst || mixchMessage.IsFirstComment))
                    {
                        if (options.IsMixchCommentNickname)
                        {
                            name = mixchMessage.NameItems.ToText();
                        }
                        comment = mixchMessage.MessageItems.ToText();
                    }
                    break;
                case MixchMessageType.SuperComment:
                case MixchMessageType.Stamp:
                case MixchMessageType.PoiPoi:
                case MixchMessageType.Item:
                case MixchMessageType.CoinBox:
                    if (options.IsMixchItem)
                    {
                        if (options.IsMixchItemNickname)
                        {
                            name = mixchMessage.NameItems.ToText();
                        }
                        comment = mixchMessage.MessageItems.ToText();
                    }
                    break;
                case MixchMessageType.Share:
                case MixchMessageType.EnterNewbie:
                case MixchMessageType.EnterLevel:
                case MixchMessageType.Follow:
                case MixchMessageType.EnterFanclub:
                    if (options.IsMixchSystem)
                    {
                        comment = mixchMessage.MessageItems.ToText();
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
