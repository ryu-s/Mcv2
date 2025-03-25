using Mcv.PluginV2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Mcv.Core.PluginLoader;

class PluginLoaderV2 : IPluginLoader
{
    private readonly ICoreLogger _logger;

    private static bool IsValidPluginFileName(string filePath)
    {
        return filePath.EndsWith("Plugin.dll") && filePath is not "Plugin.dll" && filePath is not "McvPlugin.dll";
    }

    private static IPlugin? LoadPlugin(string pluginPath)
    {
        //プラグインを異なるAssemblyLoadContextに読み込みたい。
        //各プラグインのPluginMainクラスがIPluginを実装しているが、Pluginを読み込む処理で
        //typeof(IPlugin).IsAssignableFrom(type)がfalseになってしまい意図した処理ができない。
        //そこで、一時的に全てのプラグインをDefaultのAssemblyLoadContextに読み込むようにしている。
        //var loadContext = new PluginLoadContext(pluginPath);
        var loadContext = PluginLoadContext.Default;
        var assembly = loadContext.LoadFromAssemblyPath(pluginPath);
        foreach (Type type in assembly.GetExportedTypes())
        {
            if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsAbstract)
            {
                return Activator.CreateInstance(type) as IPlugin;
            }
        }
        return null;
    }

    public List<IPlugin> LoadPlugins(string pluginsDir)
    {
        var pluginDirs = Directory.GetDirectories(pluginsDir);
        var plugins = new List<IPlugin>();

        foreach (var pluginDir in pluginDirs)
        {
            var files = Directory.GetFiles(pluginDir).Where(s => IsValidPluginFileName(s));
            foreach (var filePath in files)
            {
                try
                {
                    var plugin = LoadPlugin(filePath);
                    if (plugin != null)
                    {
                        plugins.Add(plugin);
                    }
                }
                catch (Exception ex)
                {
                    _logger.AddLog(ex);
                }
            }
        }
        return plugins;
    }
    public PluginLoaderV2(ICoreLogger logger)
    {
        _logger = logger;
    }
}
class PluginLoadContext : AssemblyLoadContext
{
    private AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}
