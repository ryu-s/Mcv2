using Mcv.PluginV2;
using System.Collections.Generic;

namespace Mcv.Core.PluginLoader;

interface IPluginLoader
{
    List<IPlugin> LoadPlugins(string pluginsDir);
}
