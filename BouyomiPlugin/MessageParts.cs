

using Mcv.PluginV2;

namespace BouyomiPlugin;

static class MessageParts
{
    public static string ToTextWithImageAlt(this IEnumerable<IMessagePart> parts)
    {
        string s = "";
        if (parts != null)
        {
            foreach (var part in parts)
            {
                if (part is IMessageText text)
                {
                    s += text;
                }
                else if (part is IMessageImage image)
                {
                    s += image.Alt;
                }
                else if (part is IMessageRemoteSvg remoteSvg)
                {
                    s += remoteSvg.Alt;
                }
                else
                {

                }
            }
        }
        return s;
    }
}
