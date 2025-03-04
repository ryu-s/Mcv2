using System.ComponentModel;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;

class McvTwitchUserNoticedViewModel : CommentViewModelBase, IMcvCommentViewModel, INotifyPropertyChanged
{
    public McvTwitchUserNoticedViewModel(TwitchSitePlugin.ITwitchUserNotice notice, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
       : base(connName, options, user)
    {
        MessageItems = Common.MessagePartFactory.CreateMessageItems(notice.Message);
        Info = notice.MsgId;
    }
    protected override SolidColorBrush CreateSiteForeground()
    {
        return new SolidColorBrush(_options.TwitchForeColor);
    }

    protected override SolidColorBrush CreateSiteBackground()
    {
        return new SolidColorBrush(_options.TwitchBackColor);
    }
}
