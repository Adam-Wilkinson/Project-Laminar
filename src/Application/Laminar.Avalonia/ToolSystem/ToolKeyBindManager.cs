using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using Laminar.Avalonia.InitializationTargets;

namespace Laminar.Avalonia.ToolSystem;

public class ToolKeyBindManager(TopLevel defaultTopLevel) : IAfterApplicationBuiltTarget
{
    private static KeyEventArgs? _mostRecentlyHandledKeyEvent;
    private static readonly List<KeyBinding> AllKeybinds = [];
    private InputElement? _elementUnderCursor;
    private Tool? _rootTool;
    
    private IFocusManager FocusManager => defaultTopLevel.FocusManager;
    
    static ToolKeyBindManager()
    {
        InputElement.KeyDownEvent.AddClassHandler<InputElement>((_, e) =>
        {
            if (e.Equals(_mostRecentlyHandledKeyEvent)) return;
            _mostRecentlyHandledKeyEvent = e;
            foreach (var keybind in AllKeybinds.Where(keybind => keybind.Gesture.Matches(e)))
            {
                keybind.TryHandle(e);
            }
        });
    }
    
    public void OnApplicationBuilt()
    {
        defaultTopLevel.Resources.GetResourceObservable(Tool.ToolRootKey).Subscribe(new AnonymousObserver<object?>(
            toolRoot =>
            {
                if (toolRoot is Tool rootToolTemplate && rootToolTemplate != _rootTool)
                {
                    _rootTool = rootToolTemplate;
                    AllKeybinds.Clear();
                    BindTool(rootToolTemplate);
                }
            }));
        
        defaultTopLevel.PointerMoved += (_, args) =>
        {
            _elementUnderCursor = defaultTopLevel.InputHitTest(args.GetPosition(defaultTopLevel)) as InputElement;
        };
    }
    
    private void BindTool(Tool tool)
    {
        AllKeybinds.Add(new KeyBinding
        {
            [!KeyBinding.GestureProperty] = tool[!Tool.GestureProperty],
            Command = new ExecuteToolAtCursor(tool, this),
        });
        
        tool.DefaultPopupTarget ??= defaultTopLevel;
        
        if (tool.ChildTools is not null)
        {
            foreach (var childTool in tool.ChildTools)
            {
                BindTool(childTool);
            }
        }
    }

    private class ExecuteToolAtCursor(Tool tool, ToolKeyBindManager keyBindManager) : ICommand
    {
        private ToolInstance? _toolInstanceCache;

        public bool CanExecute(object? parameter)
        {
            if (keyBindManager.FocusManager.GetFocusedElement() is TextBox)
            {
                return false;
            }
            
            parameter ??= tool.CommandParameter;
            return TryBuildTool(out _toolInstanceCache, parameter); 
        }

        public void Execute(object? parameter)
        {
            parameter ??= tool.CommandParameter;
            if (_toolInstanceCache is not null && _toolInstanceCache.Command.CanExecute(parameter))
            {
                _toolInstanceCache.Command.Execute(parameter);
                _toolInstanceCache = null;
                return;
            }

            if (TryBuildTool(out var toolInstance, parameter)
                && toolInstance is not null
                && toolInstance.Command.CanExecute(parameter))
            {
                toolInstance.Command.Execute(parameter);
            }
        }

        public event EventHandler? CanExecuteChanged { add { } remove { } }

        private bool TryBuildTool(out ToolInstance? instance, object? parameter)
        {
            var allVisualsUnderCursor = keyBindManager._elementUnderCursor?.GetVisualAncestors() ?? [];
            foreach (var visual in allVisualsUnderCursor)
            {
                if (tool.TryFindTargetAndBuild(visual) is { } hoveredVisualTool && hoveredVisualTool.Command.CanExecute(parameter))
                {
                    instance = hoveredVisualTool;
                    return true;
                }
            }
            
            if (tool.TryFindTargetAndBuild(keyBindManager.FocusManager.GetFocusedElement()) is { } focusedElementTool && focusedElementTool.Command.CanExecute(parameter))
            {
                instance = focusedElementTool;
                return true;
            }
            
            instance = null;
            return false;
        }
    }
}