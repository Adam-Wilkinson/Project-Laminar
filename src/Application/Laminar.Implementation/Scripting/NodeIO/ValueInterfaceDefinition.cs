using System;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeIO;

internal class ValueInterfaceDefinition<T>(ITypeInfoStore typeInfoStore, IUserInterfaceProvider uiProvider)
    : IValueInterfaceDefinition
{
    public Type? ValueType => typeof(T);

    public bool IsUserEditable { get; set; }

    public IUserInterfaceDefinition? Editor { get; set; }

    public IUserInterfaceDefinition? Viewer { get; set; }

    public IUserInterfaceDefinition? GetCurrentDefinition()
    {
        if (IsUserEditable
            && Editor is not null
            && uiProvider.InterfaceImplemented(Editor))
        {
            return Editor;
        }

        if (!IsUserEditable
            && Viewer is not null
            && uiProvider.InterfaceImplemented(Viewer))
        {
            return Viewer;
        }

        if (IsUserEditable
            && ValueType is not null
            && typeInfoStore.GetTypeInfoOrBlank(ValueType).EditorDefinition is IUserInterfaceDefinition editorDefinition
            && uiProvider.InterfaceImplemented(editorDefinition))
        {
            return editorDefinition;
        }

        if (!IsUserEditable
            && ValueType is not null
            && typeInfoStore.GetTypeInfoOrBlank(ValueType).ViewerDefinition is IUserInterfaceDefinition viewerDefinition
            && uiProvider.InterfaceImplemented(viewerDefinition))
        {
            return viewerDefinition;
        }

        return null;
    }
}
