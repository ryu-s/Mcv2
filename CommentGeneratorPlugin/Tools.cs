using Mcv.PluginV2;

namespace CommentGeneratorPlugin;

public static class Tools
{
    public static (string? name, string? comment) GetData(ISiteMessage message)
    {
        throw new NotImplementedException();
    }
    public static string GetSiteName(ISiteMessage message)
    {
        //各サイトのサービス名
        //YouTubeLive:youtubelive
        //ニコ生:nicolive
        //Twitch:twitch
        //Twicas:twicas
        //ふわっち:whowatch
        //OPENREC:openrec
        //Mirrativ:mirrativ
        //LINELIVE:linelive
        //SHOWROOM:showroom
        //BIGO:bigo

        string siteName;
        switch (message)
        {
            case Mcv.YouTubeLiveSitePlugin.IYouTubeLiveMessage _:
                siteName = "youtubelive";
                break;
            case NicoSitePlugin.INicoMessage _:
                siteName = "nicolive";
                break;
            case TwitchSitePlugin.ITwitchMessage _:
                siteName = "twitch";
                break;
            case TwicasSitePlugin.ITwicasMessage _:
                siteName = "twicas";
                break;
            case WhowatchSitePlugin.IWhowatchMessage _:
                siteName = "whowatch";
                break;
            case OpenrecSitePlugin.IOpenrecMessage _:
                siteName = "openrec";
                break;
            case MirrativSitePlugin.IMirrativMessage _:
                siteName = "mirrativ";
                break;
            case ShowRoomSitePlugin.IShowRoomMessage _:
                siteName = "showroom";
                break;
            case BigoSitePlugin.IBigoMessage _:
                siteName = "bigo";
                break;
            default:
                siteName = "unknown";
                break;
        }
        return siteName;
    }
}
