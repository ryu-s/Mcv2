namespace Mcv.PluginV2;

public class MessageSvgData : IMessageSvg
{
    public int? Width { get; set; }
    public int? Height { get; set; }
    public required string Data { get; set; }
    public string? Alt { get; set; }
}
