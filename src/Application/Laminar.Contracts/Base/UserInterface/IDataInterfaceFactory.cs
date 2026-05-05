using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Contracts.Base.UserInterface;

public interface IDataInterfaceFactory
{
    public void RegisterInterface<TInterfaceDefinition, TValue, TInterface>()
        where TInterfaceDefinition : IUserInterfaceDefinition, new()
        where TValue : notnull
        where TInterface : class, new();

    public IDataInterface<TFrontend> GetDataInterface<TFrontend>(IInterfaceData interfaceData)
        where TFrontend : class, new();

    void RegisterInterfaceFactory<TInterfaceDefinition, TValue, TInterface>(Func<TInterface> factory)
        where TInterfaceDefinition : IUserInterfaceDefinition, new()
        where TValue : notnull
        where TInterface : class;
}