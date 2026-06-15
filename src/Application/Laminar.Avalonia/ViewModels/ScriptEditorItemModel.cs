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
        Left = nodeModel.Location.Value.X;
        Top = nodeModel.Location.Value.Y;
        
        nodeModel.Location.OnChanged += (_, changedArgs) =>
        {
            Left = changedArgs.NewValue.X;
            Top = changedArgs.NewValue.Y;
        };

        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(Left) or nameof(Top))
            {
                nodeModel.Location.Value = new LaminarPoint { X = Left, Y = Top };
            }
        };
    }

    [ObservableProperty]
    public partial double Left { get; set; }

    [ObservableProperty]
    public partial double Top { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public bool IsSelectable { get; }

    public bool IsMovable { get; }

    public int LayerIndex { get; }

    public object CoreElement { get; }
}