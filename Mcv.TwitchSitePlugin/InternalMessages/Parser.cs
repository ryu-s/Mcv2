using Mcv.PluginV2.AutoReconnection;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TwitchSitePlugin.V2.InternalMessages;
interface ITwitchInternalMessage { }
class Ping : ITwitchInternalMessage
{
    public static Ping? Parse()
    {
        return new Ping();
    }
}
class PrivMsg : ITwitchInternalMessage
{
    public string? Name { get; private set; }
    public string? Id { get; private set; }
    public string? UserId { get; private set; }
    public string? DisplayName { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string Message { get; private set; }
    public string? Emotes { get; private set; }
    public string? Username { get; private set; }
    public string? Badges { get; private set; }
    private PrivMsg(string? name, string? id, string? userId, string? displayName, DateTime? sentAt, string message, string? emotes, string? username, string? badges)
    {
        Name = name;
        Id = id;
        UserId = userId;
        DisplayName = displayName;
        SentAt = sentAt;
        Message = message;
        Emotes = emotes;
        Username = username;
        Badges = badges;
    }

    public static PrivMsg? Parse(Result result)
    {
        var username = result.Tags.GetValueOrDefault("username");
        var login = result.Tags.GetValueOrDefault("login");
        var name = username ?? login ?? result.Prefix.Split('!')[0];
        var badges = result.Tags.GetValueOrDefault("badges");
        if (!string.IsNullOrEmpty(badges))
        {
            //"subscriber/0"
            //"subscriber/3"
            //"founder/0,hype-train/2"ファウンダー　ハイプトレインの元車掌
            //"subscriber/0,bits/100"
            //"subscriber/0,twitch-recap-2023/1"
            //"founder/0,superultracombo-2023/1"
            //"subscriber/0,turbo/1"
            //"subscriber/0,premium/1"
        }
        var emotes = result.Tags.GetValueOrDefault("emotes");
        var id = result.Tags.GetValueOrDefault("id");
        var userId = result.Tags.GetValueOrDefault("user-id");
        var displayName = result.Tags.GetValueOrDefault("display-name");
        DateTime? sentAt = null;
        if (result.Tags.TryGetValue("tmi-sent-ts", out var ts))
        {
            var unix = new DateTime(1970, 1, 1).AddMilliseconds(long.Parse(ts));
            sentAt = unix.ToLocalTime();
        }
        return new PrivMsg(name, id, userId, displayName, sentAt, result.Params[1], emotes, name, badges);
    }
}
class Notice : ITwitchInternalMessage
{
    public string MsgId { get; private set; }
    public string Message { get; private set; }
    private Notice(string msgId, string message)
    {
        Message = message;
    }
    public static Notice? Parse(Result result)
    {
        var msgId = result.Tags.GetValueOrDefault("msg-id");
        if (msgId is null) return null;
        return new Notice(msgId, result.Params[1]);
    }
}
class GlobalUserState : ITwitchInternalMessage
{
    public Dictionary<string, string> Tags { get; private set; }
    public List<string> Params { get; private set; }
    private GlobalUserState(Dictionary<string, string> tags, List<string> param)
    {
        Tags = tags;
        Params = param;
    }
    public static GlobalUserState? Parse(Result result)
    {
        return new GlobalUserState(result.Tags, result.Params);
    }
}

class UserState : ITwitchInternalMessage
{
    public static UserState? Parse(Result result)
    {
        return null;
    }
}
class UserNotice : ITwitchInternalMessage
{
    public string MsgId { get; private set; }
    public string Message { get; private set; }
    public string UserId { get; private set; }
    public string Raw { get; private set; }
    public UserNotice(string msgId, string message, string userId, string raw)
    {
        MsgId = msgId;
        Message = message;
        UserId = userId;
        Raw = raw;
    }
    private static void WriteLog(string message, string raw, string type)
    {
        using var sw = new StreamWriter(type + ".txt", true);
        sw.WriteLine(message);
        sw.WriteLine(raw);
        sw.WriteLine("========");
    }
    public static UserNotice? Parse(Result result)
    {
        var msgId = result.Tags.GetValueOrDefault("msg-id");
        if (msgId is null) return null;
        string message;
        switch (msgId)
        {
            case "subgift":
                //@badge-info=;badges=;color=;display-name=AnAnonymousGifter;emotes=;flags=;id=a307a7d0-d8ca-43f3-bfcf-f3052377062a;login=ananonymousgifter;mod=0;msg-id=subgift;msg-param-fun-string=FunStringThree;msg-param-gift-months=1;msg-param-months=10;msg-param-origin-id=18127844319340756991;msg-param-recipient-display-name=くーるみんと26;msg-param-recipient-id=419010060;msg-param-recipient-user-name=coolmint6626;msg-param-sub-plan-name=Channel\sSubscription\s(sasatikk);msg-param-sub-plan=1000;room-id=67519684;subscriber=0;system-msg=An\sanonymous\suser\sgifted\sa\sTier\s1\ssub\sto\sくーるみんと26!\s;tmi-sent-ts=1738573181437;user-id=274598607;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #sasatikk
                {
                    message = result.Tags.GetValueOrDefault("system-msg")!.Replace("\\s", " ");
                    WriteLog(message, result.Raw, msgId);
                }
                break;
            case "resub":
                //@badge-info=subscriber/6;badges=subscriber/6,destiny-2-final-shape-raid-race/1;color=#DAA520;display-name=hannibal092;emotes=;flags=;id=dcea2657-3dc0-4c30-a786-f9fa13a7eb77;login=hannibal092;mod=0;msg-id=resub;msg-param-cumulative-months=6;msg-param-months=0;msg-param-multimonth-duration=1;msg-param-multimonth-tenure=0;msg-param-should-share-streak=0;msg-param-sub-plan-name=Channel\sSubscription\s(sasatikk);msg-param-sub-plan=Prime;msg-param-was-gifted=false;room-id=67519684;subscriber=1;system-msg=hannibal092\ssubscribed\swith\sPrime.\sThey've\ssubscribed\sfor\s6\smonths!;tmi-sent-ts=1738573212186;user-id=438240057;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #sasatikk
                //system-msg=カピターノ\ssubscribed\swith\sPrime.\sThey've\ssubscribed\sfor\s13\smonths,\scurrently\son\sa\s1\smonth\sstreak!
                //@badge-info=subscriber/15;badges=subscriber/12,subtember-2024/1;color=#5381C9;display-name=ちき_ちき;emotes=;flags=;id=5241498d-a361-40fa-9172-d766f582df01;login=gungun_grt;mod=0;msg-id=resub;msg-param-cumulative-months=15;msg-param-months=0;msg-param-multimonth-duration=6;msg-param-multimonth-tenure=1;msg-param-should-share-streak=1;msg-param-streak-months=15;msg-param-sub-plan-name=Channel\sSubscription\s(sirry_twich);msg-param-sub-plan=1000;msg-param-was-gifted=false;room-id=446699865;subscriber=1;system-msg=ちき_ちき\ssubscribed\sat\sTier\s1.\sThey've\ssubscribed\sfor\s15\smonths,\scurrently\son\sa\s15\smonth\sstreak!;tmi-sent-ts=1738582505040;user-id=417966118;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #sirry_twich
                //ちき_ちき (gungun_grt)さんがティア1でサブスクしました。累計で15ヶ月サブスクしています。現在、15ヶ月連続でサブスクしています!
                //@badge-info=subscriber/3;badges=subscriber/3;color=#1E90FF;display-name=うじぃー;emotes=;flags=;id=b54ab831-b36f-47c1-884c-4c3e094edbb4;login=uj1_angler;mod=0;msg-id=resub;msg-param-cumulative-months=3;msg-param-months=0;msg-param-multimonth-duration=1;msg-param-multimonth-tenure=1;msg-param-should-share-streak=0;msg-param-sub-plan-name=Channel\sSubscription\s(sirry_twich);msg-param-sub-plan=1000;msg-param-was-gifted=false;room-id=446699865;subscriber=1;system-msg=うじぃー\ssubscribed\sat\sTier\s1.\sThey've\ssubscribed\sfor\s3\smonths!;tmi-sent-ts=1738583236499;user-id=557834849;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #sirry_twich
                //@badge-info=subscriber/9;badges=subscriber/9,twitch-recap-2023/1;color=#008000;display-name=el_nikao1;emotes=;flags=;id=0aa3249c-e97b-4c11-9fd4-f21349a4bcf3;login=el_nikao1;mod=0;msg-id=resub;msg-param-cumulative-months=9;msg-param-months=0;msg-param-multimonth-duration=1;msg-param-multimonth-tenure=0;msg-param-should-share-streak=1;msg-param-streak-months=1;msg-param-sub-plan-name=Channel\sSubscription\s(luquet4);msg-param-sub-plan=Prime;msg-param-was-gifted=false;room-id=267635380;subscriber=1;system-msg=el_nikao1\ssubscribed\swith\sPrime.\sThey've\ssubscribed\sfor\s9\smonths,\scurrently\son\sa\s1\smonth\sstreak!;tmi-sent-ts=1738635803159;user-id=433578524;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #luquet4 :tamo Junto @LuquEt4  tu é foda meu mano parabéns pelo trampo, sucesso red quer beijar.
                //@badge-info=subscriber/21;badges=subscriber/18,subtember-2024/1;color=#FF0000;display-name=god_resson;emotes=;flags=;id=33df416a-abbf-434e-8c5a-3239f693a605;login=god_resson;mod=0;msg-id=resub;msg-param-cumulative-months=21;msg-param-months=0;msg-param-multimonth-duration=5;msg-param-multimonth-tenure=4;msg-param-should-share-streak=0;msg-param-sub-plan-name=Channel\sSubscription\s(luquet4);msg-param-sub-plan=1000;msg-param-was-gifted=false;room-id=267635380;subscriber=1;system-msg=god_resson\ssubscribed\sat\sTier\s1.\sThey've\ssubscribed\sfor\s21\smonths!;tmi-sent-ts=1738636306450;user-id=744169827;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #luquet4 :Chegando agora oque tá acontecendo?
                //@badge-info=subscriber/4;badges=subscriber/3,premium/1;color=;display-name=voguishprotrem_7;emotes=;flags=;id=85d80b9a-823d-40b5-b3f3-f17964391b0b;login=voguishprotrem_7;mod=0;msg-id=resub;msg-param-cumulative-months=4;msg-param-months=0;msg-param-multimonth-duration=1;msg-param-multimonth-tenure=0;msg-param-should-share-streak=0;msg-param-sub-plan-name=Channel\sSubscription\s(luquet4);msg-param-sub-plan=Prime;msg-param-was-gifted=false;room-id=267635380;subscriber=1;system-msg=voguishprotrem_7\ssubscribed\swith\sPrime.\sThey've\ssubscribed\sfor\s4\smonths!;tmi-sent-ts=1738636431750;user-id=684483498;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #luquet4 :fe gago
                //@badge-info=subscriber/22;badges=subscriber/18;color=#FF0000;display-name=ryan_rp;emotes=;flags=;id=8025bb7b-3d89-479a-875b-9f7ab11fbc0a;login=ryan_rp;mod=0;msg-id=resub;msg-param-cumulative-months=22;msg-param-months=0;msg-param-multimonth-duration=4;msg-param-multimonth-tenure=3;msg-param-should-share-streak=1;msg-param-streak-months=21;msg-param-sub-plan-name=Channel\sSubscription\s(luquet4);msg-param-sub-plan=1000;msg-param-was-gifted=false;room-id=267635380;subscriber=1;system-msg=ryan_rp\ssubscribed\sat\sTier\s1.\sThey've\ssubscribed\sfor\s22\smonths,\scurrently\son\sa\s21\smonth\sstreak!;tmi-sent-ts=1738638567383;user-id=37584369;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #luquet4
                //aruka0722さんがティア1でサブスクしました。累計で15ヶ月サブスクしています！
                //@badge-info=subscriber/15;badges=subscriber/0;color=;display-name=aruka0722;emotes=;flags=;id=5560bb7c-50cf-43bc-970a-b96abe7ae812;login=aruka0722;mod=0;msg-id=resub;msg-param-cumulative-months=15;msg-param-months=0;msg-param-multimonth-duration=15;msg-param-multimonth-tenure=14;msg-param-should-share-streak=0;msg-param-sub-plan-name=ごっちゃんマイキー応援団;msg-param-sub-plan=1000;msg-param-was-gifted=false;room-id=818506560;subscriber=1;system-msg=aruka0722\ssubscribed\sat\sTier\s1.\sThey've\ssubscribed\sfor\s15\smonths!;tmi-sent-ts=1739655142399;user-id=828034099;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #gocchanmikey

                {
                    var subPlan = result.Tags.GetValueOrDefault("msg-param-sub-plan")!;
                    var subPlanName = result.Tags.GetValueOrDefault("msg-param-sub-plan-name")!;
                    var months = result.Tags.GetValueOrDefault("msg-param-months")!;
                    var isPrime = subPlan.Contains("Prime");
                    var cumulativeMonths = result.Tags.GetValueOrDefault("msg-param-cumulative-months")!;
                    var shouldShareStreak = result.Tags.GetValueOrDefault("msg-param-should-share-streak") == "1";
                    if (shouldShareStreak)
                    {

                    }
                    if (!isPrime && subPlan != "1000")
                    {

                    }
                    var streakMonths = result.Tags.GetValueOrDefault("msg-param-streak-months")!;
                    var wasGifted = result.Tags.GetValueOrDefault("msg-param-was-gifted") == "true";



                    var displayName = result.Tags.GetValueOrDefault("display-name")!;
                    var viewerCount = result.Tags.GetValueOrDefault("msg-param-viewerCount")!;
                    var login = result.Tags.GetValueOrDefault("login")!;
                    var userId2 = result.Tags.GetValueOrDefault("user-id")!;
                    var id = result.Tags.GetValueOrDefault("id")!;
                    var tmiSentTs = result.Tags.GetValueOrDefault("tmi-sent-ts")!;
                    if (isPrime)
                    {

                    }
                    else if (shouldShareStreak)
                    {
                    }
                    else
                    {

                    }
                    message = result.Tags.GetValueOrDefault("system-msg")!.Replace("\\s", " ");
                    WriteLog(message, result.Raw, msgId);
                }
                break;
            case "submysterygift":
                //@badge-info=subscriber/1;badges=subscriber/0,premium/1;color=#1E90FF;display-name=さあき小次郎;emotes=;flags=;id=cc3dd3d9-1a72-48a9-9dbb-4e7abb5f5ec7;login=kozirousasaki;mod=0;msg-id=submysterygift;msg-param-community-gift-id=5273500000675486452;msg-param-mass-gift-count=1;msg-param-origin-id=5273500000675486452;msg-param-sender-count=1;msg-param-sub-plan=1000;room-id=67519684;subscriber=1;system-msg=さあき小次郎\sis\sgifting\s1\sTier\s1\sSubs\sto\ssasatikk's\scommunity!\sThey've\sgifted\sa\stotal\sof\s1\sin\sthe\schannel!;tmi-sent-ts=1738573694484;user-id=641890877;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #sasatikk
                //@badge-info=;badges=;color=#DAA520;display-name=田口の中の田口;emotes=;flags=;id=ce2be17b-f150-4082-aa3a-c6bdf380babb;login=taguti42;mod=0;msg-id=submysterygift;msg-param-community-gift-id=13906984915523557854;msg-param-goal-contribution-type=NEW_SUB_POINTS;msg-param-goal-current-contributions=37;msg-param-goal-target-contributions=100;msg-param-goal-user-contributions=1;msg-param-mass-gift-count=1;msg-param-origin-id=13906984915523557854;msg-param-sender-count=1;msg-param-sub-plan=1000;room-id=446699865;subscriber=0;system-msg=田口の中の田口\sis\sgifting\s1\sTier\s1\sSubs\sto\sSirry_Twich's\scommunity!\sThey've\sgifted\sa\stotal\sof\s1\sin\sthe\schannel!;tmi-sent-ts=1738582304326;user-id=861484799;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #sirry_twich
                {
                    message = result.Tags.GetValueOrDefault("system-msg")!.Replace("\\s", " ");
                    WriteLog(message, result.Raw, msgId);
                }
                break;
            case "sub":
                //@badge-info=subscriber/1;badges=subscriber/0,premium/1;color=;display-name=ledon0527;emotes=;flags=;id=d5138cc7-d584-4dfc-a87f-25bfe86cb662;login=ledon0527;mod=0;msg-id=sub;msg-param-cumulative-months=1;msg-param-months=0;msg-param-multimonth-duration=1;msg-param-multimonth-tenure=0;msg-param-should-share-streak=0;msg-param-sub-plan-name=Channel\sSubscription\s(sasatikk);msg-param-sub-plan=Prime;msg-param-was-gifted=false;room-id=67519684;subscriber=1;system-msg=ledon0527\ssubscribed\swith\sPrime.;tmi-sent-ts=1738579013408;user-id=1247263177;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #sasatikk
                //@badge-info=subscriber/1;badges=subscriber/0;color=;display-name=sasa_w;emotes=;flags=;id=2d955e52-4f9b-48a5-af92-2474cc51653b;login=sasa_w;mod=0;msg-id=sub;msg-param-cumulative-months=1;msg-param-goal-contribution-type=SUB_POINTS;msg-param-goal-current-contributions=1115;msg-param-goal-target-contributions=1400;msg-param-goal-user-contributions=1;msg-param-months=0;msg-param-multimonth-duration=1;msg-param-multimonth-tenure=0;msg-param-should-share-streak=0;msg-param-sub-plan-name=Subscription\s(kyoyu_shiroya);msg-param-sub-plan=1000;msg-param-was-gifted=false;room-id=1016942766;subscriber=1;system-msg=sasa_w\ssubscribed\sat\sTier\s1.;tmi-sent-ts=1738583626935;user-id=261539416;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #kyoyu_shiroya
                {
                    message = result.Tags.GetValueOrDefault("system-msg")!.Replace("\\s", " ");
                    WriteLog(message, result.Raw, msgId);
                }
                break;
            case "viewermilestone":
                //@badge-info=subscriber/5;badges=subscriber/3;color=;display-name=あらん_らんちゃー;emotes=;flags=;id=e7d13d8f-61ea-4fac-8b57-64bb10551ce8;login=allan_launcher;mod=0;msg-id=viewermilestone;msg-param-category=watch-streak;msg-param-copoReward=450;msg-param-id=7b6ac90d-3b15-4fb4-bc16-7f29276f2960;msg-param-value=70;room-id=271117427;subscriber=1;system-msg=あらん_らんちゃー\swatched\s70\sconsecutive\sstreams\sthis\smonth\sand\ssparked\sa\swatch\sstreak!;tmi-sent-ts=1738576125481;user-id=990832253;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #m2_kento_heluan
                {
                    message = result.Tags.GetValueOrDefault("system-msg")!.Replace("\\s", " ");
                    WriteLog(message, result.Raw, msgId);
                }
                break;
            case "primepaidupgrade":
                //@badge-info=subscriber/10;badges=subscriber/9,premium/1;color=;display-name=薄毛のアン;emotes=;flags=;id=c38c4c42-8445-42e6-bba0-acc5df20cd8e;login=hicya1125;mod=0;msg-id=primepaidupgrade;msg-param-sub-plan=1000;room-id=67519684;subscriber=1;system-msg=薄毛のアン\sconverted\sfrom\sa\sPrime\ssub\sto\sa\sTier\s1\ssub!;tmi-sent-ts=1738576151209;user-id=265262896;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #sasatikk
                {
                    message = result.Tags.GetValueOrDefault("system-msg")!.Replace("\\s", " ");
                    WriteLog(message, result.Raw, msgId);
                }
                break;
            case "announcement":
                //@badge-info=;badges=moderator/1,partner/1;color=#5B99FF;display-name=StreamElements;emotes=;flags=;id=a709fbea-5e93-4cf2-88ea-b9d32444b210;login=streamelements;mod=1;msg-id=announcement;msg-param-color=GREEN;room-id=267635380;subscriber=0;system-msg=;tmi-sent-ts=1738635416306;user-id=100135110;user-type=mod;vip=0 :tmi.twitch.tv USERNOTICE #luquet4 :Siga no instagram: instagram.com/luquet4
                //@badge-info=;badges=moderator/1,partner/1;color=#5B99FF;display-name=StreamElements;emotes=;flags=;id=cdef24da-6413-4fad-87e2-362424e9b939;login=streamelements;mod=1;msg-id=announcement;msg-param-color=GREEN;room-id=267635380;subscriber=0;system-msg=;tmi-sent-ts=1738636406315;user-id=100135110;user-type=mod;vip=0 :tmi.twitch.tv USERNOTICE #luquet4 :Siga no instagram: instagram.com/luquet4
                {
                    message = "";
                    WriteLog(message, result.Raw, msgId);
                }
                break;
            case "raid":
                {
                    //ボイラは83人のパーティーとRaid中。
                    //@badge-info=;badges=partner/1;color=#1E90FF;display-name=ボイラ;emotes=;flags=;id=78a32596-4af2-4e4c-bb39-ed3d4c96be22;login=boiraischerry;mod=0;msg-id=raid;msg-param-displayName=ボイラ;msg-param-login=boiraischerry;msg-param-profileImageURL=https://static-cdn.jtvnw.net/jtv_user_pictures/870a9860-eb62-4587-b3eb-67068e6b4964-profile_image-%s.png;msg-param-viewerCount=83;room-id=759251079;subscriber=0;system-msg=83\sraiders\sfrom\sボイラ\shave\sjoined!;tmi-sent-ts=1739292545316;user-id=651640541;user-type=;vip=0 :tmi.twitch.tv USERNOTICE #yucham_yuchamero
                    var displayName = result.Tags.GetValueOrDefault("msg-param-displayName")!;
                    var viewerCount = result.Tags.GetValueOrDefault("msg-param-viewerCount")!;
                    var login = result.Tags.GetValueOrDefault("login")!;
                    var userId2 = result.Tags.GetValueOrDefault("user-id")!;
                    var id = result.Tags.GetValueOrDefault("id")!;
                    var tmiSentTs = result.Tags.GetValueOrDefault("tmi-sent-ts")!;
                    message = $"{displayName}は{viewerCount}人のパーティーとRaid中。";
                    WriteLog(message, result.Raw, msgId);
                }
                break;
            default:
                {
                    message = "";
                    WriteLog(message, result.Raw, msgId);
                }
                break;
        }
        var systemMessage = result.Tags.GetValueOrDefault("system-msg")!;
        var userId = result.Tags.GetValueOrDefault("user-id")!;

        return new UserNotice(msgId, message, userId, result.Raw);
    }
}
class RoomState : ITwitchInternalMessage
{
    private RoomState()
    {

    }
    public static RoomState? Parse(Result result)
    {
        return new RoomState();
    }
}
class IgnoredMessage : ITwitchInternalMessage
{
    public string Raw { get; private set; }
    private IgnoredMessage(string raw)
    {
        Raw = raw;
    }
    public static IgnoredMessage? Parse(Result result)
    {
        return new IgnoredMessage(result.Raw);
    }
}
class UnknownMessage : ITwitchInternalMessage
{
    public string Raw { get; private set; }
    private UnknownMessage(string raw)
    {
        Raw = raw;
    }
    internal static UnknownMessage Parse(Result result)
    {
        return new UnknownMessage(result.Raw);
    }
}
static class Parser
{
    public static ITwitchInternalMessage Parse(Result result)
    {
        ITwitchInternalMessage? ret = null;
        switch (result.Command)
        {
            case "CLEARCHAT":
                //"@ban-duration=10;room-id=37402112;target-msg-id=4830aaeb-1610-47b1-911e-9da2637816c5;target-user-id=87037096;tmi-sent-ts=1567069654595 :tmi.twitch.tv CLEARCHAT #shroud :derzackenausderkrone"
                ret = IgnoredMessage.Parse(result);
                break;
            case "CLEARMSG":
                //"@login=kale9222;room-id=;target-msg-id=759454c4-d09f-4fed-a8d6-3a20335995ec;tmi-sent-ts=1567075260054 :tmi.twitch.tv CLEARMSG #shroud :stop playing this game man :D"
                //@login=talison010;room-id=267635380;target-msg-id=bca20aa4-3aa5-4ed3-9057-c985d9c6e679;tmi-sent-ts=1738637513326 :tmi.twitch.tv CLEARMSG #luquet4 :Cadê o luan
                ret = IgnoredMessage.Parse(result);
                break;
            case "PING":
                ret = Ping.Parse();
                break;
            case "GLOBALUSERSTATE":
                ret = GlobalUserState.Parse(result);
                break;
            case "HOSTTARGET":
                //:tmi.twitch.tv HOSTTARGET #evo6 :evo 4922
                break;
            case "USERSTATE":
                ret = UserState.Parse(result);
                break;
            case "USERNOTICE":
                //"@badge-info=subscriber/11;badges=subscriber/6,bits/100;color=#FF00FF;display-name=Kosnes;emotes=205480:0-10;flags=;id=b0dbd1a7-86fe-4f54-9d4b-1cdd47a49628;login=kosnes;mod=0;msg-id=resub;msg-param-cumulative-months=11;msg-param-months=0;msg-param-should-share-streak=1;msg-param-streak-months=11;msg-param-sub-plan-name=Channel\\sSubscription\\s(meclipse);msg-param-sub-plan=Prime;room-id=37402112;subscriber=1;system-msg=Kosnes\\ssubscribed\\swith\\sTwitch\\sPrime.\\sThey've\\ssubscribed\\sfor\\s11\\smonths,\\scurrently\\son\\sa\\s11\\smonth\\sstreak!;tmi-sent-ts=1567069704460;user-id=42814323;user-type= :tmi.twitch.tv USERNOTICE #shroud :shroud4Head"
                ret = UserNotice.Parse(result);
                break;
            case "ROOMSTATE":
                //"@emote-only=0;followers-only=10;r9k=0;rituals=0;room-id=37402112;slow=5;subs-only=0 :tmi.twitch.tv ROOMSTATE #shroud"
                ret = RoomState.Parse(result);
                break;
            case "PRIVMSG":
                {
                    //useridが含まれていないPRIVMSGを確認。ホスティングされたことを伝える運営コメント
                    //:jtv!jtv@jtv.tmi.twitch.tv PRIVMSG 3lis_game :GamesFan34260 is now hosting you.

                    ret = PrivMsg.Parse(result);
                    //var cvm = new TwitchCommentViewModel(_options, _siteOptions, commentData, isFirstComment, this, user);
                    //CommentReceived?.Invoke(this, cvm);
                }
                break;
            case "NOTICE":
                //@msg-id=msg_channel_suspended :tmi.twitch.tv NOTICE #videos :This channel has been suspended.
                //@msg-id=msg_requires_verified_phone_number :tmi.twitch.tv NOTICE #ksonsouchou :A verified phone number is required to chat in this channel. Please visit https://www.twitch.tv/settings/security to verify your phone number.
                //@msg-id=subs_off :tmi.twitch.tv NOTICE #jltomy :This room is no longer in subscribers-only mode.
                ret = Notice.Parse(result);
                break;
            case "CAP":
                ret = IgnoredMessage.Parse(result);
                break;
            case "JOIN":
                //":kv501k!kv501k@kv501k.tmi.twitch.tv JOIN #shroud"
                ret = IgnoredMessage.Parse(result);
                break;
            case "001":
                //":tmi.twitch.tv 001 kv501k :Welcome, GLHF!"
                ret = IgnoredMessage.Parse(result);
                break;
            case "002":
                ret = IgnoredMessage.Parse(result);
                break;
            case "003":
                //":tmi.twitch.tv 003 kv501k :This server is rather new"
                ret = IgnoredMessage.Parse(result);
                break;
            case "004":
                //":tmi.twitch.tv 004 kv501k :-"
                ret = IgnoredMessage.Parse(result);
                break;
            case "353":
                //":kv501k.tmi.twitch.tv 353 kv501k = #shroud :kv501k"
                ret = IgnoredMessage.Parse(result);
                break;
            case "366":
                //":kv501k.tmi.twitch.tv 366 kv501k #shroud :End of /NAMES list"
                ret = IgnoredMessage.Parse(result);
                break;
            case "372":
                //":tmi.twitch.tv 372 kv501k :You are in a maze of twisty passages, all alike."
                ret = IgnoredMessage.Parse(result);
                break;
            case "375":
                //":tmi.twitch.tv 375 kv501k :-"
                ret = IgnoredMessage.Parse(result);
                break;
            case "376":
                //":tmi.twitch.tv 376 kv501k :>"
                ret = IgnoredMessage.Parse(result);
                break;
            default:
                break;
        }
        return ret ?? UnknownMessage.Parse(result);
    }
}
