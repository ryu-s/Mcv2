using Mcv.PluginV2;
using System;

namespace Mcv.Core
{
    class McvCoreOptions : DynamicOptionsBase, IMcvCoreOptions
    {
        public string PluginDir { get => GetValue(); set => SetValue(value); }
        public string SettingsDirPath { get => GetValue(); set => SetValue(value); }
        public LogType LogLevel { get => GetValue(); set => SetValue(value); }

        protected override void Init()
        {
            Dict.Add(nameof(PluginDir), new Item { DefaultValue = "plugins", Predicate = c => true, Serializer = c => c, Deserializer = s => s });
            Dict.Add(nameof(SettingsDirPath), new Item { DefaultValue = "settings", Predicate = c => true, Serializer = c => c, Deserializer = s => s });
#if DEBUG
            Dict.Add(nameof(LogLevel), new Item { DefaultValue = LogType.Debug, Predicate = c => true, Serializer = c => c.ToString(), Deserializer = s => Enum.Parse<LogType>(s) });
#else
            Dict.Add(nameof(LogLevel), new Item { DefaultValue = LogType.Error, Predicate = c => true, Serializer = c => c.ToString(), Deserializer = s => Enum.Parse<LogType>(s) });
#endif
        }
    }

}
