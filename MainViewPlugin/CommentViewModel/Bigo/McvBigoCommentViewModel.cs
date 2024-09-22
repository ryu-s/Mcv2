using BigoSitePlugin;
using Mcv.PluginV2;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;

class McvBigoCommentViewModel : CommentViewModelBase, INotifyPropertyChanged
{
    public McvBigoCommentViewModel(IBigoComment comment, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
       : base(connName, options, user)
    {
        MessageItems = Common.MessagePartFactory.CreateMessageItems(comment.Message);
        _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.Name);
        //Id = comment.Id;
        PostTime = comment.PostedAt.ToString("HH:mm:ss");
    }
    protected override SolidColorBrush CreateSiteForeground()
    {
        return new SolidColorBrush(_options.BigoForeColor);
    }

    protected override SolidColorBrush CreateSiteBackground()
    {
        return new SolidColorBrush(_options.BigoBackColor);
    }
}
class McvBigoGiftViewModel : CommentViewModelBase, INotifyPropertyChanged
{
    public McvBigoGiftViewModel(IBigoGift comment, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
       : base(connName, options, user)
    {
        MessageItems = new List<IMessagePart>
            {
                new MessageImage
                {
                     Alt = comment.GiftName,
                      Height=40,
                       Width=40,
                        Url = comment.GiftImgUrl
                },
                Common.MessagePartFactory.CreateMessageText($"×{comment.GiftCount}")
            };
        _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.Username);
        //Id = comment.Id;
        //PostTime = comment.PostedAt.ToString("HH:mm:ss");
    }
    protected override SolidColorBrush CreateSiteForeground()
    {
        return new SolidColorBrush(_options.BigoForeColor);
    }

    protected override SolidColorBrush CreateSiteBackground()
    {
        return new SolidColorBrush(_options.BigoBackColor);
    }
}
