using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.FileExplorer.Synchronization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer.Synchronization;

public class FileSystemSynchronizer(
    IFileSystemGraph targetGraph,
    IFileSystemMutationComputer mutationComputer,
    IGraphMutationApplier graphMutationApplier,
    IFileSystemItemRepository itemRepository,
    IFileSystemDiscrepancyComputer discrepancyComputer,
    ILogger<FileSystemSynchronizer> logger) : IFileSystemSynchronizer
{
    public void OnFileSystemEvent(FileSystemEvent e)
    {
        mutationComputer.AddEvent(e);
    }

    public void ReconcileAndReset(IReadOnlyCollection<IFileSystemFolder> targetFolders)
    {
        ReconcileAndResetInternal(targetFolders);
        
        // Clear the item repository outdated cache before next pass
        _ = itemRepository.DetachOutdatedItems();
    }

    private void ReconcileAndResetInternal(IReadOnlyCollection<IFileSystemFolder> targetFolders)
    {
        foreach (var mutation in mutationComputer.ComputeMutationsAndClear(itemRepository.DetachOutdatedItems()))
        {
            graphMutationApplier.Apply(mutation, targetGraph);
        }

        List<FileSystemEvent> differences = [];

        foreach (var changedFolder in targetFolders)
        {
            differences.AddRange(discrepancyComputer.ComputeFolderDiscrepancies(changedFolder));
        }

        if (differences.Count == 0)
        {
            return;
        }
        
        logger.LogWarning("Incremental file system changes failed, computing differences manually. This will not identify rename events");

        foreach (var difference in differences)
        {
            mutationComputer.AddEvent(difference);
        }

        foreach (var mutation in mutationComputer.ComputeMutationsAndClear())
        {
            graphMutationApplier.Apply(mutation, targetGraph);
        }

        foreach (var incorrectFolder in targetFolders.Where(x => discrepancyComputer.ComputeFolderDiscrepancies(x).Any()))
        {
            logger.LogError("Neither incremental nor difference-based reconciliation worked for folder {incorrectFolder}. Triggering hard refresh", incorrectFolder);
            incorrectFolder.Refresh();
        }
    }
}