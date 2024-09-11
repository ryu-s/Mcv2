using Mcv.PluginV2;
namespace CommentGeneratorPlugin;

class Options : DynamicOptionsBase
{
    public bool IsEnabled { get { return GetValue(); } set { SetValue(value); } }
    public string HcgSettingFilePath { get { return GetValue(); } set { SetValue(value); } }
    public bool IsMirrativeJoin { get { return GetValue(); } set { SetValue(value); } }
    protected override void Init()
    {
        Dict.Add(nameof(IsEnabled), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
        Dict.Add(nameof(HcgSettingFilePath), new Item { DefaultValue = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"hcg\setting.xml"), Predicate = s => true, Serializer = s => s, Deserializer = s => s });
        Dict.Add(nameof(IsMirrativeJoin), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
    }
    public void Set(Options options)
    {
        var props = typeof(Options).GetProperties();
        foreach (var prop in props)
        {
            if (prop.CanRead && prop.CanWrite)
            {
                var item = Dict[prop.Name];
                var newVal = prop.GetValue(options);
                if (newVal is not null && item.Predicate(newVal))
                {
                    item.Value = newVal;
                    RaisePropertyChanged(prop.Name);
                }
            }
        }
    }
}
