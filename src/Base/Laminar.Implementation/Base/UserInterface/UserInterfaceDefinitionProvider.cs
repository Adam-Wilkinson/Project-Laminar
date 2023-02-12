using Laminar.Contracts.Base;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Base.UserInterface;

internal class UserInterfaceDefinitionProvider : IUserInterfaceDefinitionFinder
{
    readonly static IUserInterfaceDefinition _default = new DefaultViewer();

    readonly ITypeInfoStore _typeInfoStore;
    readonly IUserInterfaceProvider _uiProvider;

    public UserInterfaceDefinitionProvider(ITypeInfoStore typeInfoStore, IUserInterfaceProvider uiProvider)
    {
        _typeInfoStore = typeInfoStore;
        _uiProvider = uiProvider;
    }

    public IUserInterfaceDefinition? GetCurrentDefinitionOf(ValueInterfaceDefinition valueInfo)
    {
        if (valueInfo.IsUserEditable 
            && valueInfo.Editor is not null 
            && _uiProvider.InterfaceImplemented(valueInfo.Editor))
        {
            return valueInfo.Editor;
        }

        if (!valueInfo.IsUserEditable 
            && valueInfo.Viewer is not null 
            && _uiProvider.InterfaceImplemented(valueInfo.Viewer))
        {
            return valueInfo.Viewer;
        }

        if (valueInfo.IsUserEditable 
            && valueInfo.ValueType is not null 
            && _typeInfoStore.GetTypeInfoOrBlank(valueInfo.ValueType).EditorDefinition is IUserInterfaceDefinition editorDefinition 
            && _uiProvider.InterfaceImplemented(editorDefinition))
        {
            return editorDefinition;
        }

        if (!valueInfo.IsUserEditable 
            && valueInfo.ValueType is not null 
            && _typeInfoStore.GetTypeInfoOrBlank(valueInfo.ValueType).ViewerDefinition is IUserInterfaceDefinition viewerDefinition 
            && _uiProvider.InterfaceImplemented(viewerDefinition))
        {
            return viewerDefinition;
        }

        return null;
    }
}
