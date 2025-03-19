namespace Mcv.PluginV2.Messages;

public interface IMessage
{
    /// <summary>
    /// メッセージのJSON表現
    /// </summary>
    string Raw { get; }
}
/// <summary>
/// Coreに対して状態の変更を求める
/// </summary>
public interface ISetMessageToCoreV2 : ISetMessageV2;
/// <summary>
/// Pluginに対して状態の変更を求める
/// </summary>
public interface ISetMessageToPluginV2 : ISetMessageV2;
/// <summary>
/// 状態の変更があった場合の通知
/// </summary>
public interface INotifyMessageV2 : IMessage;
/// <summary>
/// 現在の状態を取得する要求
/// </summary>
public interface IGetMessageToCoreV2 : IGetMessageV2;
/// <summary>
/// Getメッセージに対する返答
/// </summary>
public interface IReplyMessageToCoreV2 : IReplyMessageV2;

public interface ISetMessageV2 : IMessage;
public interface IGetMessageV2 : IMessage;
public interface IReplyMessageV2 : IMessage;
/// <summary>
/// 現在の状態を取得する要求
/// </summary>
public interface IGetMessageToPluginV2 : IGetMessageV2;
/// <summary>
/// Getメッセージに対する返答
/// </summary>
public interface IReplyMessageToPluginV2 : IReplyMessageV2;
