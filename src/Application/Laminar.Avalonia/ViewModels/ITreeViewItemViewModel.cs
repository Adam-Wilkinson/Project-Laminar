using Avalonia.Media;

namespace Laminar.Avalonia.ViewModels;

public interface ITreeViewItemViewModel
{
    public bool IsExpanded { get; set; }

    public Geometry? IconGeometry { get; }
}