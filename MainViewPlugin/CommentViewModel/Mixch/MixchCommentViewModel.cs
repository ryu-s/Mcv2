using MixchSitePlugin;
using System.ComponentModel;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;
class MixchCommentViewModel : CommentViewModelBase, IMcvCommentViewModel, INotifyPropertyChanged
{
    public MixchCommentViewModel(IMixchMessage message, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
         : base(connName, options, user)
    {
        _nameItems = message.NameItems;
        MessageItems = message.MessageItems;
        Thumbnail = null;
        Id = message.Id;
        PostTime = message.PostTime.ToString("HH:mm:ss");
    }
    protected override SolidColorBrush CreateSiteForeground()
    {
        return new SolidColorBrush(_options.MixchForeColor);
    }

    protected override SolidColorBrush CreateSiteBackground()
    {
        return new SolidColorBrush(_options.MixchBackColor);
    }
}
