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
        var loadContext = new PluginLoadContext(pluginPath);
        var assembly = loadContext.LoadFromAssemblyPath(pluginPath);
        if (pluginPath.Contains("MainView"))
        {

        }
        foreach (Type type in assembly.GetExportedTypes())
        {
            if (pluginPath.Contains("MainView") && type.Name.Contains("PluginMain"))
            {

            }
            var a = typeof(IPlugin).IsAssignableFrom(type);
            var b = type.IsAbstract;
            if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsAbstract)
            {
                loadContext.Resolving += (sender, args) =>
                {

                    return null;
                };
                assembly.ModuleResolve += (sender, args) =>
                {
                    var assemblyName = new AssemblyName(args.Name);
                    //var assemblyPath = loadContext.ResolveUnmanagedDllToPath(assemblyName.Name + ".dll");
                    //if (assemblyPath != null)
                    //{
                    //    return loadContext.LoadFromAssemblyPath(assemblyPath);
                    //}
                    return null;
                };
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
