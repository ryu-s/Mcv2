using Mcv.PluginV2;

namespace TwitchSitePlugin
{
    internal class TwitchNotice : MessageBase2, ITwitchNotice
    {
        public override SiteType SiteType { get; } = SiteType.Twitch;
        public TwitchMessageType TwitchMessageType { get; } = TwitchMessageType.Notice;
        public string Message { get; private set; }
        public TwitchNotice(TwitchSitePlugin.V2.InternalMessages.Notice notice) : base("")
        {
            Message = notice.Message;
        }
    }
    internal class TwitchUserNotice : MessageBase2, ITwitchUserNotice
    {
        public override SiteType SiteType { get; } = SiteType.Twitch;
        public TwitchMessageType TwitchMessageType { get; } = TwitchMessageType.UserNotice;
        public string Message { get; private set; }
        public string MsgId { get; private set; }
        public TwitchUserNotice(TwitchSitePlugin.V2.InternalMessages.UserNotice userNotice) : base(userNotice.Raw)
        {
            Message = userNotice.Message;
            MsgId = userNotice.MsgId;
        }
    }
}
