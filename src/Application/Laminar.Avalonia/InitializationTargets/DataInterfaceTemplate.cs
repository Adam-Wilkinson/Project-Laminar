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
        var valueCache = interfaceData.Value;
        var returnValue = new Decorator
        {
            [!Decorator.ChildProperty] = CompiledBinding.Create((IDataInterface<Control> x) => x.InterfaceFrontend, source: result),
            [!StyledElement.DataContextProperty] = CompiledBinding.Create((IDataInterface<Control> x) => x.InterfaceData, source: result),
        };

        result.InterfaceData?.Value = valueCache;
        
        return returnValue;
    }

    public bool Match(object? data) => data is IInterfaceData;
    
    public void BeforeApplicationBuiltInitialization()
    {
        topLevel.DataTemplates.Add(this);
    }
}