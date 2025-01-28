using System.ComponentModel;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;

class McvTwitchConnectedViewModel : CommentViewModelBase, IMcvCommentViewModel, INotifyPropertyChanged
{
    public McvTwitchConnectedViewModel(TwitchSitePlugin.ITwitchConnected _, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
       : base(connName, options, user)
    {
        MessageItems = Common.MessagePartFactory.CreateMessageItems("接続しました");
    }
    protected override SolidColorBrush CreateSiteForeground()
    {
        return new SolidColorBrush(_options.InfoForeColor);
    }
    protected override SolidColorBrush CreateSiteBackground()
    {
        return new SolidColorBrush(_options.InfoBackColor);
    }
}
