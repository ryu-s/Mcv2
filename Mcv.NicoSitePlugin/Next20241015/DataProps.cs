using Newtonsoft.Json;
using NicoSitePlugin;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mcv.NicoSitePlugin.Next20241015;

class DataProps
{
    public string Title { get; private set; }
    public string ProviderType { get; private set; }
    public long OpenTime { get; private set; }
    public long BeginTime { get; private set; }
    public long VposBaseTime { get; private set; }
    public long EndTime { get; private set; }
    public long ScheduledEndTime { get; private set; }
    public string Status { get; private set; }
    public string WebsocketUrl { get; private set; }
    public string UserId { get; private set; }
    public bool IsLoggedIn { get; private set; }
    public static async Task<DataProps?> GetDataProps(IDataSource _server, string vid, CookieContainer cc)
    {
        var url = "https://live.nicovideo.jp/watch/" + vid;
        var livePagehtml = await _server.GetAsync(url, cc);
        var match = Regex.Match(livePagehtml, "<script [^>]+ data-props=\"([^>]+)\"></script>");
        if (!match.Success) return null;
        var pre = match.Groups[1].Value;
        var dataPropsJson = pre.Replace("&quot;", "\"");
        var dataProps = DataProps.Parse(dataPropsJson);
        return dataProps;
    }
    public static DataProps Parse(string json)
    {
        dynamic? d = JsonConvert.DeserializeObject(json);
        if (d is null)
        {
            throw new InternalMessage.ParseException();
        }
        return new DataProps
        {
            Title = d.program.title,
            ProviderType = d.program.providerType,
            OpenTime = d.program.openTime,
            BeginTime = d.program.beginTime,
            VposBaseTime = d.program.vposBaseTime,
            EndTime = d.program.endTime,
            ScheduledEndTime = d.program.scheduledEndTime,
            Status = d.program.status,
            WebsocketUrl = d.site.relive.webSocketUrl,
            UserId = d.user.id,
            IsLoggedIn = d.user.isLoggedIn,
        };
    }
}
