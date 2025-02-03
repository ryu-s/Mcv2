using System.ComponentModel;
using System.Windows.Media;
using TwitchSitePlugin;

namespace Mcv.MainViewPlugin;
class McvTwitchCommentViewModel : CommentViewModelBase, IMcvCommentViewModel, INotifyPropertyChanged
{
    private readonly ITwitchMessage _message;
    public McvTwitchCommentViewModel(ITwitchComment comment, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
        : base(connName, options, user)
    {
        _message = comment;
        if (comment.DisplayName == comment.UserName)
        {
            _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.DisplayName);
        }
        else
        {
            _nameItems = Common.MessagePartFactory.CreateMessageItems($"{comment.DisplayName} ({comment.UserName})");
        }
        MessageItems = comment.CommentItems;
        Thumbnail = comment.UserIcon;
        Id = comment.Id.ToString();
        PostTime = comment.PostTime;

        connName.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(connName.BackColor):
                    RaisePropertyChanged(nameof(Background));
                    break;
                case nameof(connName.ForeColor):
                    RaisePropertyChanged(nameof(Foreground));
                    break;
            }
        };
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