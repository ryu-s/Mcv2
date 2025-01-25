using Newtonsoft.Json;

namespace NicoSitePlugin.Metadata
{
    class MessageServer : IMetaMessage
    {
        public string ViewUri { get; }
        public string VposBaseTime { get; }
        public string HashedUserId { get; }
        public string Raw { get; }
        private MessageServer(string raw, string viewUri, string vposBaseTime, string hashedUserId)
        {
            Raw = raw;
            ViewUri = viewUri;
            VposBaseTime = vposBaseTime;
            HashedUserId = hashedUserId;
        }

        public static MessageServer CreateMessage(string raw)
        {
            dynamic? d = JsonConvert.DeserializeObject(raw);
            var viewUri = (string)d.data.viewUri;
            var vposBaseTime = (string)d.data.vposBaseTime;
            var hashedUserId = (string)d.data.hashedUserId;
            return new MessageServer(raw, viewUri, vposBaseTime, hashedUserId);
        }
    }
}