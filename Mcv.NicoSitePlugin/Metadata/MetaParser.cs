using Newtonsoft.Json;

namespace NicoSitePlugin.Metadata
{
    static class MetaParser
    {
        public static IMetaMessage Parse(string raw)
        {
            dynamic? d = JsonConvert.DeserializeObject(raw);
            IMetaMessage ret;
            var type = (string)d.type;
            switch (type)
            {
                case "serverTime":
                    ret = new ServerTime(raw);
                    break;
                case "ping":
                    ret = new Ping();
                    break;
                case "disconnect":
                    var reason = (string)d.data.reason;
                    ret = new Disconnect(reason);
                    break;
                case "room":
                    ret = new Room(raw);
                    break;
                case "seat":
                    ret = new Seat(raw);
                    break;
                case "statistics":
                    ret = new Statistics(raw);
                    break;
                case "stream":
                    ret = new IgnoredMessage(raw);
                    break;
                case "postCommentResult":
                    ret = new PostCommentResult(raw);
                    break;
                case "schedule":
                    ret = new IgnoredMessage(raw);
                    break;
                case "tagUpdated":
                    ret = new IgnoredMessage(raw);
                    break;
                case "messageServer":
                    ret = MessageServer.CreateMessage(raw);
                    break;
                default:
                    //{"type":"startWatching","data":{"stream":{"quality":"abr","protocol":"hls+fmp4","latency":"low","chasePlay":false},"room":{"protocol":"webSocket","commentable":true},"reconnect":false}}
                    //{"type":"serverTime","data":{"currentMs":"2024-08-19T15:09:53.368+09:00"}}
                    //{"type":"seat","data":{"keepIntervalSec":30}}
                    //{"type":"stream","data":{"uri":"https://vodedge793.dmc.nico/hlslive/ht2_nicolive/nicolive-production-pg77767019332165_70ebb57cc5ad60c2197fd2808773945d1a218ac34e31be7474e5a499510fb836/master.m3u8?ht2_nicolive=2297426.8e0cfxql15_sigbsh_2mcjpq2byply","syncUri":"https://vodedge793.dmc.nico/hlslive/ht2_nicolive/nicolive-production-pg77767019332165_70ebb57cc5ad60c2197fd2808773945d1a218ac34e31be7474e5a499510fb836/stream_sync.json?ht2_nicolive=2297426.8e0cfxql15_sigbsh_2mcjpq2byply","quality":"abr","availableQualities":["abr","super_high","high","normal","low","super_low","audio_high","audio_only"],"protocol":"hls"}}
                    //{"type":"schedule","data":{"begin":"2024-08-19T14:22:41+09:00","end":"2024-08-19T15:22:41+09:00"}}
                    //{"type":"messageServer","data":{"viewUri":"https://mpn.live.nicovideo.jp/api/view/v4/BBy_WxuVsANvRXv1qoIU8hpTzuK_YOxWoR9aV2NFH8Z74c-KQpr9fs0Q1wYb4IEGWi5W_7ecwrBa8dy0CfQ","vposBaseTime":"2024-08-19T14:22:36+09:00","hashedUserId":"a:xL9BhMPO6WMx4j69"}}
                    //{"type":"statistics","data":{"viewers":448,"comments":257}}
                    //{"type":"getAkashic","data":{"chasePlay":false}}
                    //{"type":"akashic","data":{"playId":"94284804","contentUrl":"https://ak.cdn.nimg.jp/coe/contents/aufeiR7C/nicocas/4.2.2.0/content.json","logServerUrl":"wss://msg02.akashic.coe.nicovideo.jp/4003/","status":"ready","token":"f06ac60c8c5f9e42d3117b1546203246f46094ceb7b8bae8938699194aea294c","playerId":"2297426"}}
                    //{"type":"ping"}
                    //{"type":"pong"}
                    //{"type":"keepSeat"}
                    //{"type":"reconnect","data":{"audienceToken":"77767019332165_2297426_1724134606_5e7f223c50ddb5c92981ee732ecd20818887b21f","waitTimeSec":1}}
                    ret = new UnknownMessage(raw);
                    break;
            }
            return ret;
        }
    }
}