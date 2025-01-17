using System;
using System.Runtime.CompilerServices;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Implementation.Base.PluginLoading;
using Laminar.PluginFramework.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Extensions;

public static class LaminarInitializer
{
    public static void InitializeLaminar(this IServiceProvider serviceProvider, FrontendDependency frontendDependency, [CallerFilePath] string sourceFilePath = "")
    {
        PluginFramework.LaminarFactory.ServiceProvider = serviceProvider;
        _ = new PluginLoader(sourceFilePath, frontendDependency, serviceProvider.GetService<IPluginHostFactory>()!);
    }
}