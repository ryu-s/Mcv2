using Mcv.PluginV2;

namespace TwitchSitePlugin
{
    class TwitchUnknownMessage : MessageBase2, ITwitchMessage
    {
        public override SiteType SiteType { get; } = SiteType.Twitch;
        public TwitchMessageType TwitchMessageType { get; } = TwitchMessageType.Unknown;
        public TwitchUnknownMessage(string raw) : base(raw) { }
    }
}
