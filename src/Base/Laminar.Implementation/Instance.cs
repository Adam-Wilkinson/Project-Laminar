using System;
using System.Threading;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Laminar.PluginFramework.Registration;
using Laminar.Implementation.Base.PluginLoading;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Implementation.Extensions.ServiceInitializers;

namespace Laminar.Implementation;

public class Instance
{
    private PluginLoader _pluginLoader;

    public Instance(SynchronizationContext uiContext, FrontendDependency supportedDependencies, [CallerFilePath] string path = "")
    {
        _pluginLoader = new PluginLoader(path, supportedDependencies, ServiceProvider.GetService<IPluginHostFactory>());

        // _isLoading = true;
        //foreach (var serializedScript in UserData.LoadAllFromFolder<ISerializedObject<IAdvancedScript>>("Scripts", "las"))
        //{
        //    AllAdvancedScripts.Add(Serializer.Deserialize(serializedScript, null));
        //}
        // _isLoading = false;
    }

    public IServiceProvider ServiceProvider { get; } = new ServiceCollection()
        .AddScriptingServices()
        .AddUserInterfaceServices()
        .AddPluginServices()
        .AddEnvironmentServices()
        .BuildServiceProvider();
}
