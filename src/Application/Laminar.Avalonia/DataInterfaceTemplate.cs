using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Laminar.Avalonia.InitializationTargets;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Avalonia;

public class DataInterfaceTemplate(TopLevel topLevel, IDataInterfaceFactory dataInterfaceFactory) : IDataTemplate, IBeforeApplicationBuiltTarget
{
    public Control? Build(object? param)
    {
        if (param is not IInterfaceData interfaceData) return null;
        
        var result = dataInterfaceFactory.GetDataInterface<Control>(interfaceData);
        result.InterfaceFrontend.DataContext = result.InterfaceData;
        return result.InterfaceFrontend;
    }

    public bool Match(object? data) => data is IInterfaceData;
    
    public void BeforeApplicationBuiltInitialization()
    {
        topLevel.DataTemplates.Add(this);
    }
}