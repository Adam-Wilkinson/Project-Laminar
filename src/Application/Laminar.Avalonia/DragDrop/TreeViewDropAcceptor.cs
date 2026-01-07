using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Core;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using Laminar.Domain.Extensions;

namespace Laminar.Avalonia.DragDrop;

public class TreeViewDropAcceptor : DropAcceptor<TreeView>
{
    protected override IEnumerable<Receptacle> GetReceptacles(TreeView visual)
    {
        if (visual.ItemsPanelRoot is not { } visualPanel) yield break;

        var rootTreeViewItems = visualPanel.GetVisualChildren().Where(x => x is TreeViewItem).Cast<TreeViewItem>();
        
        foreach (TreeViewItem child in Flatten(rootTreeViewItems))
        {
            yield return new Receptacle(new RectangleGeometry(child.Bounds), null);
        }
    }

    protected override IPen? DebugReceptaclePen { get; set; } = new Pen(Brushes.Blue, 2);
    
    private IEnumerable<TreeViewItem> Flatten(IEnumerable<TreeViewItem> items)
    {
        return items.SelectMany(x => x.GetLogicalChildren().Where(y => y is TreeViewItem).Cast<TreeViewItem>()).Concat(items);
    }
}