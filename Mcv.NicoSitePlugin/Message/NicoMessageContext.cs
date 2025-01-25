using Mcv.PluginV2;
using System.Collections.Generic;
using MessageV2 = Mcv.NicoSitePlugin.MessageV2;
namespace NicoSitePlugin
{
    internal class NicoMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? NewNickname { get; }
        public bool IsInitialComment { get; }
        public string? UserId { get; }
        public IEnumerable<IMessagePart>? UsernameItems { get; }

        public NicoMessageContext(MessageV2.INicoMessage message, string? userId, string? newNickname, bool isInitialComment, string? userName)
        {
            Message = message;
            UserId = userId;
            NewNickname = newNickname;
            IsInitialComment = isInitialComment;
            if (userName is not null)
            {
                UsernameItems = MessagePartFactory.CreateMessageItems(userName);
            }
        }
    }
}
