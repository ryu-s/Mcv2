using System.ComponentModel;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;

class McvTwitchNoticedViewModel : CommentViewModelBase, IMcvCommentViewModel, INotifyPropertyChanged
{
    public McvTwitchNoticedViewModel(TwitchSitePlugin.ITwitchNotice notice, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
       : base(connName, options, user)
    {
        MessageItems = Common.MessagePartFactory.CreateMessageItems(notice.Message);
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
