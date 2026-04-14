using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using Avalonia.VisualTree;
using Laminar.Avalonia.InitializationTargets;

namespace Laminar.Avalonia.ToolSystem;

public class ToolKeyBindManager(TopLevel defaultTopLevel) : IAfterApplicationBuiltTarget
{
    private readonly List<KeyBinding> _keyBindings = [];
    private readonly TopLevel _defaultToplevel = defaultTopLevel;
    
    private TopLevel _focusedTopLevel = defaultTopLevel;
    private Visual? _visualUnderCursor;
    private bool _keyChordHandled = false;
    private Tool? _rootTool;
    
    public void OnApplicationBuilt()
    {
        _defaultToplevel.Resources.GetResourceObservable(Tool.ToolRootKey).Subscribe(new AnonymousObserver<object?>(
            toolRoot =>
            {
                if (toolRoot is Tool rootToolTemplate && rootToolTemplate != _rootTool)
                {
                    _rootTool = rootToolTemplate;
                    _keyBindings.Clear();
                    BindTool(rootToolTemplate);
                }
            }));
        
        HookTopLevel(_focusedTopLevel);
    }

    private void HookTopLevel(TopLevel focusedTopLevel)
    {
        _focusedTopLevel = focusedTopLevel;
        
        focusedTopLevel.LostFocus += FocusedTopLevelLostFocus;
        
        focusedTopLevel.PointerMoved += (_, args) =>
        {
            _visualUnderCursor = focusedTopLevel.GetVisualAt(args.GetPosition(focusedTopLevel));
        };
        
        // Hack to stop Avalonia from focusing the menu when keybindings involving the Alt key are released.
        // If the bug is fixed, remove this.
        focusedTopLevel.AddHandler(InputElement.KeyUpEvent, (_, e) =>
        {
            if (_keyChordHandled) e.Handled = true;
            if (e.KeyModifiers == KeyModifiers.None) _keyChordHandled = false;
        }, RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
        
        focusedTopLevel.KeyBindings.AddRange(_keyBindings);
    }

    private void FocusedTopLevelLostFocus(object? sender, EventArgs args)
    {
        var newTopLevel = GetFocusedTopLevel();
        if (newTopLevel == _focusedTopLevel)
        {
            return;
        }
        
        _focusedTopLevel.LostFocus -= FocusedTopLevelLostFocus;
        _focusedTopLevel = newTopLevel;
        HookTopLevel(newTopLevel);
    }

    private TopLevel GetFocusedTopLevel()
    {
        if (_focusedTopLevel.IsKeyboardFocusWithin)
        {
            return _focusedTopLevel;
        }

        if (_focusedTopLevel.FocusManager?.GetFocusedElement() is not Visual focusedVisual)
        {
            return _defaultToplevel;
        }
        
        if (TopLevel.GetTopLevel(focusedVisual) is not { } newTopLevel)
        {
            return _defaultToplevel;
        }

        return newTopLevel;
    }
    
    private void BindTool(Tool tool)
    {
        _keyBindings.Add(new KeyBinding
        {
            [!KeyBinding.GestureProperty] = tool[!Tool.GestureProperty],
            Command = new ExecuteToolAtCursor(tool, this),
        });
        
        tool.DefaultPopupTarget ??= _defaultToplevel;
        
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
            if (keyBindManager._focusedTopLevel.FocusManager?.GetFocusedElement() is TextBox)
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
                keyBindManager._keyChordHandled = true;
                return;
            }

            if (TryBuildTool(out var toolInstance, parameter)
                && toolInstance is not null
                && toolInstance.Command.CanExecute(parameter))
            {
                toolInstance.Command.Execute(parameter);
                keyBindManager._keyChordHandled = true;
            }
        }

        public event EventHandler? CanExecuteChanged { add { } remove { } }

        private bool TryBuildTool(out ToolInstance? instance, object? parameter)
        {
            var allVisualsUnderCursor = keyBindManager._visualUnderCursor?.GetVisualAncestors() ?? [];
            foreach (var visual in allVisualsUnderCursor)
            {
                if (TryBuildToolFromTarget(visual) is { } hoveredVisualTool && hoveredVisualTool.Command.CanExecute(parameter))
                {
                    instance = hoveredVisualTool;
                    return true;
                }
            }
            
            if (TryBuildToolFromTarget(keyBindManager._focusedTopLevel.FocusManager?.GetFocusedElement()) is { } focusedElementTool && focusedElementTool.Command.CanExecute(parameter))
            {
                instance = focusedElementTool;
                return true;
            }
            
            instance = null;
            return false;
        }

        private ToolInstance? TryBuildToolFromTarget(object? target) => target switch
        {
            not null when tool.Build(target) is { } instance => instance,
            ContentPresenter contentPresenter when tool.Build(contentPresenter.Content) is { } instance => instance,
            ContentControl contentControl when tool.Build(contentControl.Content) is { } instance => instance,
            StyledElement styledElement when tool.Build(styledElement.DataContext) is { } instance => instance,
            _ => null,
        };
    }
}