using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mcv.PluginV2.Messages
{
    public class MessageParser
    {
        private static ISetMessageToCoreV2 ParseSetMessage(dynamic d)
        {
            var set = (string)d.set;
            switch (set)
            {
                case "add_connection":
                    {
                        //{"type":"set", "set":"add_connection"}
                        return new RequestAddConnection();
                    }
                case "remove_connection":
                    {
                        //{"type":"set", "set":"remove_connection", "conn_id":"292255B2-8E9F-4289-A926-F7B302D8A3FC"}
                        var rawConnId = (string)d.conn_id;
                        if (Guid.TryParse(rawConnId, out var guid))
                        {
                            var connId = new ConnectionId(guid);
                            return new RequestRemoveConnection(connId);
                        }
                    }
                    break;
                case "plugin_hello":
                    {
                        //{"type":"set", "set":"plugin_hello", "plugin_id":"...", "plugin_name":"...", "plugin_role":[...]}
                        var rawPluginId = (string)d.plugin_id;
                        var pluginName = (string)d.plugin_name;
                        var pluginRole = new List<string>();
                        foreach (var role in d.plugin_role)
                        {
                            pluginRole.Add((string)role);
                        }
                        if (Guid.TryParse(rawPluginId, out var guid))
                        {
                            var pluginId = new PluginId(guid);
                            return new SetPluginHello(pluginId, pluginName, pluginRole);
                        }
                    }
                    break;
                case "close_app":
                    {
                        //{"type":"set", "set":"close_app"}
                        return new SetCloseApp();
                    }
                case "savepluginoptions":
                    {
                        //{"type":"set", "set":"savepluginoptions", "raw_options":"..."}
                        var filename = (string)d.filename;
                        var rawOptions = (string)d.raw_options;
                        return new RequestSavePluginOptions(filename, rawOptions);
                    }
                case "direct_message":
                    {
                        //{"type":"set", "set":"direct_message", "target":"...", "message":{...}}
                        var rawTarget = (string)d.target;
                        var rawMessage = (string)d.message.ToString();
                        if (Guid.TryParse(rawTarget, out var guid))
                        {
                            var targetId = new PluginId(guid);
                            // 内部メッセージを解析する必要があります
                            var innerMessage = Parse(rawMessage);
                            if (innerMessage is ISetMessageToPluginV2 setMessage)
                            {
                                return new SetDirectMessage(targetId, setMessage);
                            }
                        }
                    }
                    break;
                case "update":
                    {
                        //{"type":"set", "set":"update", "update_to":"...", "url":"..."}
                        var updateTo = (string)d.update_to;
                        var url = (string)d.url;
                        return new RequestUpdate(updateTo, url);
                    }
                case "showsettingspanel":
                    {
                        //{"type":"set", "set":"showsettingspanel", "plugin_id":"..."}
                        var rawPluginId = (string)d.plugin_id;
                        if (Guid.TryParse(rawPluginId, out var guid))
                        {
                            var pluginId = new PluginId(guid);
                            return new RequestShowSettingsPanel(pluginId);
                        }
                    }
                    break;
                case "exception":
                    {
                        //{"type":"set", "set":"exception", "message":"...", "details":"..."}
                        var message = (string)d.message;
                        var details = (string)d.details;
                        return new SetException(null, message, details);
                    }
            }
            throw new Exception($"Unknown set message: {set}");
        }

        private static IGetMessageV2 ParseGetMessage(dynamic d)
        {
            var get = (string)d.get;
            switch (get)
            {
                case "appname":
                    return new GetAppName();
                case "appversion":
                    return new GetAppVersion();
                case "appsolutionconfiguration":
                    return new GetAppSolutionConfiguration();
                case "browserprofiles":
                    return new GetBrowserProfiles();
                case "connectionstatus":
                    {
                        //{"type":"get", "get":"connectionstatus", "conn_id":"..."}
                        var rawConnId = (string)d.conn_id;
                        if (Guid.TryParse(rawConnId, out var guid))
                        {
                            var connId = new ConnectionId(guid);
                            return new GetConnectionStatus(connId);
                        }
                    }
                    break;
                case "cookies":
                    {
                        //{"type":"get", "get":"cookies", "browser_profile_id":"...", "domain":"..."}
                        var rawBrowserProfileId = (string)d.browser_profile_id;
                        var domain = (string)d.domain;
                        if (Guid.TryParse(rawBrowserProfileId, out var guid))
                        {
                            var browserProfileId = new BrowserProfileId(guid);
                            return new GetCookies(browserProfileId, domain);
                        }
                    }
                    break;
                case "direct_message":
                    {
                        //{"type":"get", "get":"direct_message", "target":"...", "message":{...}}
                        var rawTarget = (string)d.target;
                        var rawMessage = (string)d.message.ToString();
                        if (Guid.TryParse(rawTarget, out var guid))
                        {
                            var targetId = new PluginId(guid);
                            // 内部メッセージを解析する必要があります
                            var innerMessage = Parse(rawMessage);
                            if (innerMessage is IGetMessageToPluginV2 getMessage)
                            {
                                return new GetDirectMessage(targetId, getMessage);
                            }
                        }
                    }
                    break;
                case "ifupdateexists":
                    return new GetIfUpdateExists();
                case "loadpluginoptions":
                    {
                        var pluginName = "";
                        return new RequestLoadPluginOptions(pluginName);
                    }
                case "legacyoptions":
                    return new GetLegacyOptions();
                case "pluginsettingsdirpath":
                    {
                        //{"type":"get", "get":"pluginsettingsdirpath", "filepath":"..."}
                        var filepath = (string)d.filepath;
                        return new GetPluginSettingsDirPath(filepath);
                    }
                case "useragent":
                    return new GetUserAgent();
                case "isvalidsiteurl":
                    {
                        //{"type":"get", "get":"isvalidsiteurl", "url":"..."}
                        var url = (string)d.url;
                        return new GetIsValidSiteUrl(url);
                    }
                case "settingspanel":
                    return new GetSettingsPanel();
                case "sitedomain":
                    {
                        //{"type":"get", "get":"sitedomain", "connection_id":"..."}
                        var rawConnId = (string)d.connection_id;
                        if (Guid.TryParse(rawConnId, out var guid))
                        {
                            var connId = new ConnectionId(guid);
                            return new GetSiteDomain(connId);
                        }
                    }
                    break;
                case "siteplugindisplayname":
                    return new GetSitePluginDisplayName();
            }
            throw new Exception($"Unknown get message: {get}");
        }

        private static INotifyMessageV2 ParseNotifyMessage(dynamic d)
        {
            var notify = (string)d.notify;
            switch (notify)
            {
                case "plugin_added":
                    {
                        //{"type":"notify", "notify":"plugin_added", "plugin_id":"...", "plugin_name":"...", "plugin_role":[...]}
                        var rawPluginId = (string)d.plugin_id;
                        var pluginName = (string)d.plugin_name;
                        var pluginRole = new List<string>();
                        foreach (var role in d.plugin_role)
                        {
                            pluginRole.Add((string)role);
                        }
                        if (Guid.TryParse(rawPluginId, out var guid))
                        {
                            var pluginId = new PluginId(guid);
                            return new NotifyPluginAdded(pluginId, pluginName, pluginRole);
                        }
                    }
                    break;
                case "connection_added":
                    {
                        //{"type":"notify", "notify":"connection_added", "connection_status":{...}}
                        // ここでConnection Statusを解析する必要がありますが、
                        // 詳細な実装はIConnectionStatusの実装に依存します
                        throw new NotImplementedException("Connection status parsing not implemented");
                    }
                case "download_progress":
                    {
                        //{"type":"notify", "notify":"download_progress", "download_progress":{...}}
                        throw new NotImplementedException("Download progress parsing not implemented");
                    }
                case "site_connected":
                    {
                        //{"type":"notify", "notify":"site_connected", "conn_id":"..."}
                        var rawConnId = (string)d.conn_id;
                        if (Guid.TryParse(rawConnId, out var guid))
                        {
                            var connId = new ConnectionId(guid);
                            return new NotifySiteConnected(connId);
                        }
                    }
                    break;
                case "site_disconnected":
                    {
                        //{"type":"notify", "notify":"site_disconnected", "conn_id":"..."}
                        var rawConnId = (string)d.conn_id;
                        if (Guid.TryParse(rawConnId, out var guid))
                        {
                            var connId = new ConnectionId(guid);
                            return new NotifySiteDisconnected(connId);
                        }
                    }
                    break;
            }
            throw new Exception($"Unknown notify message: {notify}");
        }
        private static IReplyMessageV2 ParseReplyMessage(dynamic d)
        {
            var reply = (string)d.reply;
            switch (reply)
            {
                case "direct_message":
                    {
                        //{"type":"reply", "reply":"direct_message", "message":{...}}
                        var rawMessage = (string)d.message.ToString();
                        // 内部メッセージを解析する必要があります
                        var innerMessage = Parse(rawMessage);
                        if (innerMessage is IReplyMessageToPluginV2 replyMessage)
                        {
                            return new ReplyDirectMessage(replyMessage);
                        }
                    }
                    break;
                case "plugin_notfound":
                    return new ReplyPluginNotfound();
                case "pluginoptions":
                    {
                        //{"type":"reply", "reply":"pluginoptions", "raw_options":"..."}
                        if (d.ContainsKey("raw_options") && d.raw_options != null && d.raw_options.Type != JTokenType.Null)
                        {
                            var rawOptions = (string)d.raw_options;
                            return new ReplyPluginOptions(rawOptions);
                        }
                        return new ReplyPluginOptions(null);
                    }
                case "useragent":
                    {
                        //{"type":"reply", "reply":"useragent", "useragent":"..."}
                        var userAgent = (string)d.useragent;
                        return new ReplyUserAgent(userAgent);
                    }
                case "legacyoptions":
                    {
                        //{"type":"reply", "reply":"legacyoptions", "raw_options":"..."}
                        var rawOptions = (string)d.raw_options;
                        return new ReplyLegacyOptions(rawOptions);
                    }
                case "error":
                    {
                        //{"type":"reply", "reply":"error", "error":{...}}
                        if (d.ContainsKey("error") && d.error != null && d.error.Type != JTokenType.Null)
                        {
                            var errorMessage = (string)d.error.Message;
                            var ex = new Exception(errorMessage);
                            return new ErrorOccurred(ex);
                        }
                        return new ErrorOccurred();
                    }
                case "ifupdateexists":
                    {
                        //{"type":"reply", "reply":"ifupdateexists", "update_exists":true, "url":"...", "current":"...", "latest":"..."}
                        var updateExists = (bool)d.update_exists;
                        var url = (string)d.url;
                        var current = (string)d.current;
                        var latest = (string)d.latest;
                        return new ReplyIfUpdateExists(updateExists, url, current, latest);
                    }
                case "ifupdateexists_error":
                    return new ReplyIfUpdateExistsError();
            }
            throw new Exception($"Unknown reply message: {reply}");
        }

        public static IMessage Parse(string raw)
        {
            //{"type":"req","req":"add_connection"}
            //{"type":"notify","notify":"connection_added"}
            //

            dynamic? d = null;
            try
            {
                d = JsonConvert.DeserializeObject(raw);
            }
            catch (Exception)
            {
                return new UnknownMessage(raw);
            }
            if (d is null)
            {
                return new UnknownMessage(raw);
            }
            if (!d.ContainsKey("type"))
            {
                return new UnknownMessage(raw);
            }
            try
            {
                var type = (string)d.type;
                if (type == "set" && d.ContainsKey("set"))
                {
                    return ParseSetMessage(d);
                }
                else if (type == "notify" && d.ContainsKey("notify"))
                {
                    return ParseNotifyMessage(d);
                }
                else if (type == "get" && d.ContainsKey("get"))
                {
                    return ParseGetMessage(d);
                }
                else if (type == "reply" && d.ContainsKey("reply"))
                {
                    return ParseReplyMessage(d);
                }
            }
            catch (Exception ex)
            {
                return new UnknownMessage(raw);
            }
            return new UnknownMessage(raw);
        }
    }
}
