using Mcv.PluginV2;
using Mcv.NicoSitePlugin.MessageV2;

namespace Mcv.MainViewPlugin;

class NicoLiveMessageProcessorV2 : ILiveSiteMessageProcessor
{
    public IMcvCommentViewModel? CreateViewModel(ISiteMessage message, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    {
        IMcvCommentViewModel? vm = null;
        if (message is INicoMessage nicoMessage)
        {
            switch (nicoMessage)
            {
                case INicoComment nicoComment:
                    vm = new McvNicoCommentViewModelV2(nicoComment, connName, options, user);
                    break;
                case INicoGift nicoGift:
                    vm = new McvNicoCommentViewModelV2(nicoGift, connName, options, user);
                    break;
                case INicoSimpleNotification nicoSimple:
                    vm = new McvNicoSimpleNotificationCommentViewModelV2(nicoSimple, connName, options, user);
                    break;
                case INicoConnected nicoConnected:
                    vm = new McvNicoCommentViewModelV2(nicoConnected, connName, options, user);
                    break;
                case INicoDisconnected nicoDisconnected:
                    vm = new McvNicoCommentViewModelV2(nicoDisconnected, connName, options, user);
                    break;
                default:
                    break;
            }
        }
        return vm;
    }

    public bool IsValidMessage(ISiteMessage message)
    {
        return message is INicoMessage _;
    }
}
