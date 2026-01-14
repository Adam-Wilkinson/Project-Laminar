using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Laminar.Avalonia.DragDrop;

public class TreeViewDropAcceptor : DropAcceptor<TreeView>
{
    /// <summary>
    /// Gets all the receptacles for a TreeView where one may wish to drop a TreeViewItem
    /// </summary>
    /// <param name="treeView">The tree view for which receptacles are being generated. </param>
    /// <returns></returns>
    protected override IEnumerable<Receptacle> GetReceptacles(TreeView treeView)
    {
        // We need this information for the receptacle tags
        int[] indicesInEachLevel = ArrayPool<int>.Shared.Rent(4);
        int levelOfCurrentCollapse = int.MaxValue;
        
        try
        {
            indicesInEachLevel[0] = 0;
            // The loop renders the receptacles one item behind its current item, because if the level is about to
            // decrease, we create several receptacles; one for each level.  
            TreeViewItem? previousItem = null;
            foreach (var currentItem in treeView.GetVisualDescendants().OfType<TreeViewItem>())
            {
                if (previousItem is null)
                {
                    previousItem = currentItem;
                    continue;
                }
                
                // Skip items that are not currently visible. IsVisible may be insufficient since the drag event
                // may collapse the item without its visibility getting a chance to be updated.
                if (!currentItem.IsVisible || currentItem.Level > levelOfCurrentCollapse)
                {
                    continue;
                }
                levelOfCurrentCollapse = currentItem.IsExpanded ? int.MaxValue : currentItem.Level;
                
                
                foreach (var receptacle in GetItemReceptacles(previousItem, currentItem, indicesInEachLevel, treeView))
                {
                    yield return receptacle;
                }

                previousItem = currentItem;
            }

            // We now have the trailing last item that hasn't been handled
            if (previousItem is null) yield break;
            foreach (var receptacle in GetItemReceptacles(previousItem, null, indicesInEachLevel, treeView)) 
            {
                yield return receptacle;
            }
        }
        finally
        {
            ArrayPool<int>.Shared.Return(indicesInEachLevel);
        }
    }

    private static IEnumerable<Receptacle> GetItemReceptacles(
        TreeViewItem currentItem, 
        TreeViewItem? nextItem, 
        int[] indicesInEachLevel,
        TreeView rootTreeView)
    {
        if (currentItem.HeaderPresenter?.TransformToVisual(rootTreeView) is not { } matrixToTreeView) yield break;
        if (currentItem.RenderTransform is { Value.HasInverse: true })
        {
            matrixToTreeView *= currentItem.RenderTransform.Value.Invert();
        }
        
        var headerBounds = currentItem.HeaderPresenter.Bounds.TransformToAABB(matrixToTreeView);
        var nextItemLevel = nextItem?.Level ?? 0;
        int numberOfReceptacles = Math.Max(currentItem.Level - nextItemLevel + 1, 1);
        double heightOfReceptacles = headerBounds.Height / numberOfReceptacles;
        var currentParent = currentItem.GetLogicalParent() as ItemsControl;
        for (int i = 0; i < numberOfReceptacles; i++)
        {
            int index = indicesInEachLevel[currentItem.Level - i];
            yield return new Receptacle(new RectangleGeometry(new Rect(
                0, headerBounds.Top + heightOfReceptacles * i, headerBounds.Right, heightOfReceptacles)), 
                new TreeViewItemReceptacleInfo(currentParent!.DataContext, index));
            currentParent = currentParent.GetLogicalParent() as ItemsControl;
        }
        
        indicesInEachLevel[currentItem.Level]++;
        // If the level will increase, we need to zero it out
        if (nextItem is not null && nextItem.Level > currentItem.Level)
        {
            indicesInEachLevel[nextItem.Level] = 0;
        }
    }

    public record struct TreeViewItemReceptacleInfo(object? ReceptacleParentDataContext, int ReceptacleIndex);

    protected override IPen DebugReceptaclePen => new Pen(Brushes.Beige, 1.5);
}