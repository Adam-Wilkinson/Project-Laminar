using System;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Base;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeIO;

internal class ValueInterfaceDefinition<T> : IValueInterfaceDefinition
{
    readonly ITypeInfoStore _typeInfoStore;
    readonly IUserInterfaceProvider _uiProvider;

    public ValueInterfaceDefinition(ITypeInfoStore typeInfoStore, IUserInterfaceProvider uiProvider)
    {
        _typeInfoStore = typeInfoStore;
        _uiProvider = uiProvider;
    }

    public Type? ValueType => typeof(T);

    public bool IsUserEditable { get; set; }

    public IUserInterfaceDefinition? Editor { get; set; }

    public IUserInterfaceDefinition? Viewer { get; set; }

    public IUserInterfaceDefinition? GetCurrentDefinition()
    {
        if (IsUserEditable
            && Editor is not null
            && _uiProvider.InterfaceImplemented(Editor))
        {
            return Editor;
        }

        if (!IsUserEditable
            && Viewer is not null
            && _uiProvider.InterfaceImplemented(Viewer))
        {
            return Viewer;
        }

        if (IsUserEditable
            && ValueType is not null
            && _typeInfoStore.GetTypeInfoOrBlank(ValueType).EditorDefinition is IUserInterfaceDefinition editorDefinition
            && _uiProvider.InterfaceImplemented(editorDefinition))
        {
            return editorDefinition;
        }

        if (!IsUserEditable
            && ValueType is not null
            && _typeInfoStore.GetTypeInfoOrBlank(ValueType).ViewerDefinition is IUserInterfaceDefinition viewerDefinition
            && _uiProvider.InterfaceImplemented(viewerDefinition))
        {
            return viewerDefinition;
        }

        return null;
    }
}
