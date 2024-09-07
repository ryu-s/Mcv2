using System;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using System.Net;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Mcv.Core;
enum LogType
{
    Error,
    Debug,
}
class Log
{
    public string Message { get; }
    public LogType Type { get; }
    public DateTime LogDateTime { get; } = DateTime.Now;
    public Data? Data { get; }
    public Log(string message, LogType type, Data? data = null)
    {
        Message = message;
        Type = type;
        Data = data;
    }
    public string ToJson()
    {
        var data = Data is not null ? Data.ToJson() : "null";
        return $"{{\"message\":\"{Message}\",\"logtype\":\"{Type}\",\"datetime\":\"{LogDateTime:yyyy/MM/dd HH:mm:ss}\",\"data\":{data}}}";
    }
    public static Log? FromJson(string json)
    {
        var obj = JsonConvert.DeserializeObject<Log>(json);
        return obj;
    }
}
class Data
{
    public string DataTypeName { get; }
    public string Content { get; }
    public Data(byte[] bytes)
    {
        DataTypeName = "byte[]";
        Content = Convert.ToBase64String(bytes);
    }
    public Data(string s)
    {
        DataTypeName = "string";
        Content = s;
    }
    public Data(Exception ex)
    {
        DataTypeName = "Exception";
        Content = new Error(ex).ToJson();
    }
    public Data(Error error)
    {
        DataTypeName = "Exception";
        Content = error.ToJson();
    }
    public string ToJson()
    {
        return $"{{\"datatype\":\"{DataTypeName}\",\"content\":\"{Helper.EscapeForJson(Content)}\"}}";
    }
    public static Data? FromJson(string json)
    {
        dynamic? obj = JsonConvert.DeserializeObject(json);
        if (obj is null) return null;
        var typeName = (string)obj.datatype;
        switch (typeName)
        {
            case "byte[]":
                return new Data(Convert.FromBase64String((string)obj.content));
            case "string":
                return new Data((string)obj.content);
            case "Exception":
                {
                    var err = Error.FromJson((string)obj.content);
                    if (err is null) return null;
                    return new Data(err);
                }
            default:
                return null;
        }
    }
}
interface ICoreLogger
{
    void AddLog(string message, LogType type, Data? data = null, [CallerFilePath] string callerFile = "", [CallerLineNumber] int line = 0, [CallerMemberName] string callerName = "");
    public void AddLog(Exception ex, string message = "", [CallerFilePath] string callerFile = "", [CallerLineNumber] int line = 0, [CallerMemberName] string callerName = "");
    string GetLogs();
}
class LoggerTest : ICoreLogger
{
    readonly System.Collections.Concurrent.BlockingCollection<Log> _logs = [];
    public void AddLog(string message, LogType type, Data? data = null,
        [CallerFilePath] string callerFile = "", [CallerLineNumber] int line = 0, [CallerMemberName] string callerName = "")
    {
        _logs.Add(new Log($"{callerFile} line:{line} {callerName} \"{message}\"", type, data));
    }
    public void AddLog(Exception ex, string message = "",
            [CallerFilePath] string callerFile = "", [CallerLineNumber] int line = 0, [CallerMemberName] string callerName = "")
    {
        AddLog(message, LogType.Error, new Data(ex), callerFile, line, callerName);
    }
    public string GetLogs()
    {
        return string.Join(Environment.NewLine, _logs.Select(k => k.ToJson()));
    }
}
[Serializable]
public class Error
{
    public string Name { get; init; }
    public string Message { get; private set; }
    public string StackTrace { get; private set; }
    public string Timestamp { get; private set; }
    public Error[] InnerError { get; private set; }
    public Dictionary<string, string> Properties { get; private set; } = new Dictionary<string, string>();
    public Error()
    {
        Timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
    }
    public Error(Exception ex) : this()
    {
        Name = ex.GetType().FullName!;
        Message = ex.Message;
        StackTrace = ex.StackTrace!;
        SetProperties(ex);

        if (ex.InnerException != null)
        {
            InnerError = new Error[1];
            InnerError[0] = new Error(ex.InnerException);
        }
    }
    public Error(WebException ex) : this((Exception)ex)
    {
        Properties.Add(nameof(ex.Status), ex.Status.ToString());
        if (ex.Response is HttpWebResponse http)
        {
            Properties.Add(nameof(http.StatusCode), http.StatusCode.ToString());
            using (var sr = new System.IO.StreamReader(http.GetResponseStream()))
            {
                var s = sr.ReadToEnd();
                Properties.Add("Response", s.Replace("\"", "\\\""));
            }
        }
    }
    private void SetProperties(Exception ex)
    {
        try
        {
            var properties = ex.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    var get = property.GetGetMethod();
                    var name = property.Name;
                    var s = (string?)get?.Invoke(ex, null);
                    if (s is not null)
                    {
                        Properties.Add(name, s);
                    }
                }
            }
        }
        catch (Exception ex1)
        {
            Debug.WriteLine(ex1.Message);
        }
    }
    public Error(AggregateException ex) : this()
    {
        Name = ex.GetType().FullName ?? "";
        Message = ex.Message;
        StackTrace = ex.StackTrace ?? "";
        var innerCount = ex.InnerExceptions.Count;
        InnerError = new Error[innerCount];
        for (int i = 0; i < innerCount; i++)
        {
            InnerError[i] = new Error(ex.InnerExceptions[i]);
        }
    }
    public string ToJson()
    {
        var innererror = InnerError is not null ? "[" + string.Join(",", InnerError.Select(k => k.ToJson())) + "]" : "[]";
        var properties = Properties.Count > 0 ? "[" + string.Join(",", Properties.Select(k => $"{{\"key\":\"{k.Key}\",\"value\":\"{Helper.EscapeForJson(k.Value)}\"}}")) + "]" : "[]";
        return $"{{\"name\":\"{Helper.EscapeForJson(Name)}\", \"message\":\"{Helper.EscapeForJson(Message)}\", \"stacktrace\":\"{Helper.EscapeForJson(StackTrace)}\", \"timestamp\":\"{Timestamp}\", \"innererror\":{innererror}, \"properties\":{properties}}}";
    }
    public static Error? FromJson(string s)
    {
        dynamic? d = JsonConvert.DeserializeObject(s);
        if (d is null) return null;
        var innererror = d.innererror;
        var innerList = new List<Error>();
        foreach (dynamic ie in innererror)
        {
            var inner = Error.FromJson(ie.ToString());
            if (inner is not null)
            {
                innerList.Add(inner);
            }
        }

        var error = new Error
        {
            Name = d.name,
            Message = d.message,
            StackTrace = d.stacktrace,
            Timestamp = d.timestamp,
            InnerError = [.. innerList],
        };


        var properties = d.properties;
        foreach (dynamic kv in properties)
        {
            var key = (string)kv.key;
            var value = (string)kv.value;
            error.Properties.Add(key, value);
        }

        return error;
    }
}
static class Helper
{
    public static string EscapeForJson(string s)
    {
        var sb = new StringBuilder(s);
        sb.Replace("\\", "\\\\");
        sb.Replace("\r", "\\r");
        sb.Replace("\n", "\\n");
        sb.Replace("\t", "\\t");
        sb.Replace("\"", "\\\"");
        return sb.ToString();
    }
    public static string UnescapeForJson(string s)
    {
        var sb = new StringBuilder(s);
        sb.Replace("\\r", "\r");
        sb.Replace("\\n", "\n");
        sb.Replace("\\t", "\t");
        sb.Replace("\\\"", "\"");
        sb.Replace("\\\\", "\\");
        return sb.ToString();
    }
}
