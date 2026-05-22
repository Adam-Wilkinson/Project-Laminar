using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;

using AvaloniaPoint = Avalonia.Point;
using LaminarPoint = Laminar.Domain.ValueObjects.Point;

namespace Laminar.Avalonia.ViewModels;

public partial class ScriptEditorItemModel : ObservableObject
{
    public ScriptEditorItemModel(IConnection connectionModel)
    {
        CoreElement = connectionModel;
        IsSelectable = true;
        IsMovable = false;
        LayerIndex = 0;
    }

    public ScriptEditorItemModel(IWrappedNode nodeModel)
    {
        CoreElement = nodeModel;
        IsSelectable = true;
        IsMovable = true;
        LayerIndex = 10;
        
        nodeModel.Location.OnChanged += (_, changedArgs) =>
        {
            Position = new AvaloniaPoint(changedArgs.NewValue.X, changedArgs.NewValue.Y);
        };

        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Position))
            {
                nodeModel.Location.Value = new LaminarPoint { X = Position.X, Y = Position.Y };
            }
        };
    }

    [ObservableProperty]
    public partial AvaloniaPoint Position { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public bool IsSelectable { get; }

    public bool IsMovable { get; }

    public int LayerIndex { get; }

    public object CoreElement { get; }
}