using System;
using System.Runtime.CompilerServices;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Implementation.Base.PluginLoading;
using Laminar.PluginFramework;
using Laminar.PluginFramework.Registration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Extensions;

public static class LaminarInitializer
{
    public static void InitializeLaminar(this IServiceProvider serviceProvider, FrontendDependency frontendDependency, [CallerFilePath] string sourceFilePath = "")
    {
        LaminarFactory.ServiceProvider = serviceProvider;
        _ = new PluginLoader(sourceFilePath, frontendDependency, serviceProvider.GetService<IPluginHostFactory>()!);
        serviceProvider.GetRequiredService<ILogger<None>>().LogTrace("Laminar Initialized");
    }
}