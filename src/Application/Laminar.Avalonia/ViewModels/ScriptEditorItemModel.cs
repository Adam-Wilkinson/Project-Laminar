using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using LaminarPoint = Laminar.Domain.ValueObjects.Point;

namespace Laminar.Avalonia.ViewModels;

public partial class ScriptEditorItemModel : ObservableObject
{
    public ScriptEditorItemModel(IConnection connectionModel)
    {
        CoreElement = connectionModel;
        IsSelectable = true;
        IsMovable = false;
    }

    public ScriptEditorItemModel(IWrappedNode nodeModel)
    {
        CoreElement = nodeModel;
        IsSelectable = true;
        IsMovable = true;
        nodeModel.Location.OnChanged += (_, changedArgs) =>
        {
            Position = new Point(changedArgs.NewValue.X, changedArgs.NewValue.Y);
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
    public partial Point Position { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public bool IsSelectable { get; }

    public bool IsMovable { get; }

    public object CoreElement { get; }
}