using Mcv.PluginV2;
using System;

namespace Mcv.NicoSitePlugin.MessageV2;
public enum NicoMessageType
{
    Unknown,
    Comment,
    Connected,
    Disconnected,
    Gift,
    SimpleNotification,
}
public interface INicoMessage : ISiteMessage
{
    NicoMessageType NicoMessageType { get; }
}
public interface INicoConnected : INicoMessage
{
    string Text { get; }
}
public interface INicoDisconnected : INicoMessage
{
    string Text { get; }
}
public interface INicoComment : INicoMessage
{
    string Content { get; }
    string UserName { get; }
    int Vpos { get; }
    int No { get; }
    string UserId { get; }
    DateTime DateTime { get; }
}
public enum SimpleNotificationType
{
    Unknown,
    Ichiba,
    Quote,
    Emotion,
    Cruise,
    ProgramExtended,
    RankingIn,
    RankingUpdated,
    Visited,
}
public interface INicoSimpleNotification : INicoMessage
{
    SimpleNotificationType SimpleNotificationType { get; }
    string Content { get; }
    DateTime DateTime { get; }
}
public interface INicoGift : INicoMessage
{
    string ItemId { get; }
    long? UserId { get; }
    string UserName { get; }
    string? Message { get; }
    string ItemName { get; }
    string Content { get; }
}
public interface INicoAd : INicoMessage
{

}