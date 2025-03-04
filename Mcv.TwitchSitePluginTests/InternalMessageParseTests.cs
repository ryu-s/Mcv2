using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchSitePlugin;

namespace Mcv.TwitchSitePluginTests;
[TestFixture]
internal class InternalMessageParseTests
{
    [Test]
    public void TestTwitchNotice()
    {
        var result = Tools.Parse("@badge-info=subscriber/3;badges=subscriber/3;color=#1E90FF;display-name=うじぃー;emotes=;flags=;id=b54ab831-b36f-47c1-884c-4c3e094edbb4;login=uj1_angler;mod=0;msg-id=resub;msg-param-cumulative-months=3;msg-param-months=0;msg-param-multimonth-duration=1;msg-param-multimonth-tenure=1;msg-param-should-share-streak=0;msg-param-sub-plan-name=Channel\\sSubscription\\s(sirry_twich);msg-param-sub-plan=1000;msg-param-was-gifted=false;room-id=446699865;subscriber=1;system-msg=うじぃー\\ssubscribed\\sat\\sTier\\s1.\\sThey've\\ssubscribed\\sfor\\s3\\smonths!;tmi-sent-ts=1738583236499;user-id=557834849;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #sirry_twich");
        var notice = TwitchSitePlugin.V2.InternalMessages.UserNotice.Parse(result);
        Assert.That(notice.Message, Is.EqualTo("うじぃー subscribed at Tier 1. They've subscribed for 3 months!"));

    }
    [Test]
    public void Abc()
    {
        var result = Tools.Parse("@badge-info=subscriber/15;badges=subscriber/0;color=;display-name=abc0722;emotes=;flags=;id=5560bb7c-50cf-43bc-970a-b96abe7ae812;login=abc0722;mod=0;msg-id=resub;msg-param-cumulative-months=15;msg-param-months=0;msg-param-multimonth-duration=15;msg-param-multimonth-tenure=14;msg-param-should-share-streak=0;msg-param-sub-plan-name=GGGG応援団;msg-param-sub-plan=1000;msg-param-was-gifted=false;room-id=818506560;subscriber=1;system-msg=abc0722\\ssubscribed\\sat\\sTier\\s1.\\sThey've\\ssubscribed\\sfor\\s15\\smonths!;tmi-sent-ts=1739655142399;user-id=828034099;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #ggggpppp");
        var notice = TwitchSitePlugin.V2.InternalMessages.UserNotice.Parse(result);
        Assert.That(notice.Message, Is.EqualTo("abc0722さんがティア1でサブスクしました。累計で15ヶ月サブスクしています！"));
    }
}
