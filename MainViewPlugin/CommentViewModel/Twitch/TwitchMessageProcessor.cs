using Mcv.PluginV2;
using TwitchSitePlugin;

namespace Mcv.MainViewPlugin;
class TwitchMessageProcessor : ILiveSiteMessageProcessor
{
    public IMcvCommentViewModel? CreateViewModel(ISiteMessage message, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    {
        IMcvCommentViewModel? vm = null;
        if (message is ITwitchMessage twitchMessage)
        {
            switch (twitchMessage)
            {
                case ITwitchComment twitchComment:
                    vm = new McvTwitchCommentViewModel(twitchComment, connName, options, user);
                    break;
                case ITwitchConnected twitchConnected:
                    vm = new McvTwitchConnectedViewModel(twitchConnected, connName, options, user);
                    break;
                case ITwitchDisconnected twitchDisconnected:
                    vm = new McvTwitchDisconnectedViewModel(twitchDisconnected, connName, options, user);
                    break;
                case ITwitchNotice twitchNotice:
                    vm = new McvTwitchNoticedViewModel(twitchNotice, connName, options, user);
                    break;
                default:
                    break;
            }
        }
        return vm;
    }

    public bool IsValidMessage(ISiteMessage message)
    {
        return message is ITwitchMessage _;
    }
}
