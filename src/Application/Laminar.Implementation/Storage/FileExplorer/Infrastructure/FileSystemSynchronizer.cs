using System.Diagnostics.Tracing;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer.Infrastructure;

public class FileSystemSynchronizer(
    IFileSystemGraph targetGraph,
    IFileSystemMutationComputer mutationComputer,
    IFileSystemGraphMutator graphMutator,
    IFileSystemItemRepository itemRepository,
    IFileSystemDiscrepancyComputer discrepancyComputer,
    ILogger<FileSystemSynchronizer> logger) : IFileSystemSynchronizer
{
    public void OnFileSystemEvent(FileSystemEvent e)
    {
        mutationComputer.AddEvent(e);
    }

    public void ReconcileAndReset(IReadOnlyCollection<IFileSystemRootFolder> targetFolders)
    {
        foreach (var mutation in mutationComputer.ComputeMutationsAndClear())
        {
            graphMutator.Apply(mutation, targetGraph);
        }
        
        itemRepository.ClearOutdated();

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
            graphMutator.Apply(mutation, targetGraph);
        }

        foreach (var incorrectFolder in targetFolders.Where(x => discrepancyComputer.ComputeFolderDiscrepancies(x).Any()))
        {
            logger.LogError("Neither incremental nor difference-based reconciliation worked for folder {0}. Triggering hard refresh", incorrectFolder);
            incorrectFolder.Refresh();
        }
    }
}