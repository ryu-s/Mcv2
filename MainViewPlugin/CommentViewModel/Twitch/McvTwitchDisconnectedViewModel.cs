using System.ComponentModel;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;

class McvTwitchDisconnectedViewModel : CommentViewModelBase, IMcvCommentViewModel, INotifyPropertyChanged
{
    public McvTwitchDisconnectedViewModel(TwitchSitePlugin.ITwitchDisconnected _, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
       : base(connName, options, user)
    {
        MessageItems = Common.MessagePartFactory.CreateMessageItems("切断しました");
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
