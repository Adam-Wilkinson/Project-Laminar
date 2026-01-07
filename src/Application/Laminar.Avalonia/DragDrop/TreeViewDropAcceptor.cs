using System;
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

public class TreeViewDropAcceptor : DropAcceptor<TreeViewItem>
{
    private readonly StackPanelDropAcceptor _stackPanelDropAcceptor = new();
    
    protected override IEnumerable<Receptacle> GetReceptacles(TreeViewItem treeViewItem)
    {
        if (treeViewItem.ItemsPanelRoot is StackPanel stackPanel)
        {
            return _stackPanelDropAcceptor.Receptacles(stackPanel).Skip(1);
        }

        return [];
    }

    protected override IPen? DebugReceptaclePen => new Pen(new SolidColorBrush(new HslColor(1, Random.Shared.NextSingle() * 360, 1, 0.5).ToRgb()), 1.5);
    
    private IEnumerable<TreeViewItem> Flatten(IEnumerable<TreeViewItem> items)
    {
        return items.SelectMany(x => x.GetLogicalChildren().Where(y => y is TreeViewItem).Cast<TreeViewItem>()).Concat(items);
    }
}