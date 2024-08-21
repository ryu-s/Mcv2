using Mcv.PluginV2;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
namespace NicoSitePlugin
{
    public class DataSource : ServerBase, IDataSource
    {
        private readonly string _userAgent;

        public async Task<string> GetAsync(string url, CookieContainer cc)
        {
            var result = await GetInternalAsync(new HttpOptions
            {
                Url = url,
                Cc = cc,
                UserAgent = _userAgent,
            }, false);
            var str = await result.Content.ReadAsStringAsync();
            return str;
        }

        public Task<string> GetAsync(string url)
        {
            return GetAsync(url, null);
        }

        public async Task<byte[]> GetBytesAsync(string url)
        {
            var headers = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Referer", "https://live.nicovideo.jp/" },
                   { "Origin", "https://live.nicovideo.jp" },
                {"priority","u=1, i" },
            };
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36";
            var options = new HttpOptions
            {
                Url = url,
                UserAgent = userAgent,
                Headers = headers,
            };
            var message = await GetInternalAsync(options);
            return await message.Content.ReadAsByteArrayAsync();
        }

        public DataSource(string userAgent)
        {
            _userAgent = userAgent;
        }
    }
}
