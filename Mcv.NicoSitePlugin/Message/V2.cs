using Mcv.PluginV2;
using System;

namespace Mcv.NicoSitePlugin.MessageV2;
class NicoComment : INicoComment
{
    public required string Content { get; set; }
    public required string UserName { get; set; }
    public required int Vpos { get; set; }
    public required int No { get; set; }
    public required string UserId { get; set; }
    public required DateTime DateTime { get; set; }
    public NicoMessageType NicoMessageType { get; } = NicoMessageType.Comment;
}
class NicoDisconnected : INicoDisconnected
{
    public NicoMessageType NicoMessageType { get; } = NicoMessageType.Disconnected;
    public string Text { get; } = "切断しました";
}
class NicoGift : INicoGift
{
    public required string ItemId { get; set; }
    public long? UserId { get; set; }
    public required string UserName { get; set; }
    public string? Message { get; set; }

    public NicoMessageType NicoMessageType { get; } = NicoMessageType.Gift;
    public required string ItemName { get; set; }
    public required string Content { get; set; }
    public required DateTime DateTime { get; set; }
}
class NicoSimpleNotification : INicoSimpleNotification
{
    public SimpleNotificationType SimpleNotificationType { get; }
    public string Content { get; }
    public NicoMessageType NicoMessageType { get; } = NicoMessageType.SimpleNotification;
    public DateTime DateTime { get; }
    public NicoSimpleNotification(InternalMessage.SimpleNotification low, DateTime dateTime)
    {
        if (low.Cruise is string cruise)
        {
            SimpleNotificationType = SimpleNotificationType.Cruise;
            Content = cruise;
        }
        else if (low.Ichiba is string ichiba)
        {
            SimpleNotificationType = SimpleNotificationType.Ichiba;
            Content = ichiba;
        }
        else if (low.Emotion is string emotion)
        {
            SimpleNotificationType = SimpleNotificationType.Emotion;
            Content = emotion;
        }
        else if (low.Quote is string quote)
        {
            SimpleNotificationType = SimpleNotificationType.Quote;
            Content = quote;
        }
        else if (low.ProgramExtended is string programExtended)
        {
            SimpleNotificationType = SimpleNotificationType.ProgramExtended;
            Content = programExtended;
        }
        else if (low.RankingIn is string rankingIn)
        {
            SimpleNotificationType = SimpleNotificationType.RankingIn;
            Content = rankingIn;
        }
        else if (low.RankingUpdated is string rankingUpdated)
        {
            SimpleNotificationType = SimpleNotificationType.RankingUpdated;
            Content = rankingUpdated;
        }
        else if (low.Visited is string visited)
        {
            SimpleNotificationType = SimpleNotificationType.Visited;
            Content = visited;
        }
        else
        {
            SimpleNotificationType = SimpleNotificationType.Unknown;
            Content = "";
        }

        DateTime = dateTime;
    }
}