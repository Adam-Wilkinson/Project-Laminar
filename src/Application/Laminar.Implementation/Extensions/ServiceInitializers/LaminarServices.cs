using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.UserData;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Implementation.Base;
using Laminar.Implementation.Base.ActionSystem;
using Laminar.Implementation.Base.PluginLoading;
using Laminar.Implementation.Base.UserInterface;
using Laminar.Implementation.Scripting.NodeIO;
using Laminar.Implementation.Scripting.NodeWrapping;
using Laminar.Implementation.UserData;
using Laminar.Implementation.UserData.FileNavigation;
using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Extensions.ServiceInitializers;

public static class LaminarServices
{
    public static IServiceCollection AddLaminarServices(this IServiceCollection services) => services
            .AddSingleton<IPersistentDataManager, PersistentDataManager>()
            .AddSingleton<ISerializer, Serializer>()
            .AddSingleton<IUserActionManager, UserActionManager>()
            .AddSingleton<IDataInterfaceFactory, DataInterfaceFactory>()
            .AddSingleton<ITypeInfoStore, TypeInfoStore>()
            .AddSingleton<IPluginHostFactory, PluginHostFactory>()
            .AddSingleton<ILaminarStorageItemFactory, LaminarStorageItemFactory>()
            .AddUserInterfaceServices()
            .AddScriptingServices();
}