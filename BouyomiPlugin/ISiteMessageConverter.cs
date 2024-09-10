using Mcv.PluginV2;

namespace BouyomiPlugin;

interface ISiteMessageConverter
{
    (bool success, string? name, string? comment) Convert(ISiteMessage message, Options options);
}
