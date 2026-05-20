using System.Xml.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Avalonia.InitializationTargets;

public class DataInterfaceTemplate(TopLevel topLevel, IDataInterfaceFactory dataInterfaceFactory) : IDataTemplate, IBeforeApplicationBuiltTarget
{
    public Control? Build(object? param)
    {
        if (param is not IInterfaceData interfaceData) return null;
        
        var result = dataInterfaceFactory.GetDataInterface<Control>(interfaceData);
        
        // // Caching the value is required because sometimes Avalonia data coercion changes the value before data validation is fully initialized
        // var valueCache = result.InterfaceData.Value;
        // result.InterfaceFrontend.DataContext = result.InterfaceData;
        // if (result.InterfaceData.IsUserEditable)
        // {
        //     result.InterfaceData.Value = valueCache;
        // }

        return new Decorator
        {
            [!Decorator.ChildProperty] = CompiledBinding.Create((IDataInterface<Control> x) => x.InterfaceFrontend, source: result),
            [!StyledElement.DataContextProperty] = result.InterfaceData.AsStaticBinding()
        };
    }

    public bool Match(object? data) => data is IInterfaceData;
    
    public void BeforeApplicationBuiltInitialization()
    {
        topLevel.DataTemplates.Add(this);
    }
}