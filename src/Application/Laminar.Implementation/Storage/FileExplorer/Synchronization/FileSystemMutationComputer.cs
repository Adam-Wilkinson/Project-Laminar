using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.FileExplorer.Synchronization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer.Synchronization;

internal sealed class FileSystemMutationComputer(
    Func<IFileSystemEventHashBucket> hashBucketFactory,
    ILogger<FileSystemMutationComputer> logger) 
    : IFileSystemMutationComputer
{
    private readonly List<FileSystemEvent> _allEvents = [];
    
    public IEnumerable<FileSystemGraphMutation> ComputeMutationsAndClear()
    {
        IFileSystemEventHashBucket creationEvents = hashBucketFactory();
        IFileSystemEventHashBucket deletionEvents = hashBucketFactory();

        foreach (var fileSystemEventArgs in _allEvents)
        {
            if (fileSystemEventArgs.ChangeType == WatcherChangeTypes.Created)
            {
                creationEvents.AddEvent(fileSystemEventArgs);
            }
            else if (fileSystemEventArgs.ChangeType == WatcherChangeTypes.Deleted)
            {
                deletionEvents.AddEvent(fileSystemEventArgs);
            }
        }
        
        HashSet<FileSystemEvent> handledArgs = [];
        
        foreach (var evt in _allEvents)
        {
            if (handledArgs.Contains(evt))
            {
                continue;
            }
            
            if (evt.ChangeType == WatcherChangeTypes.Deleted 
                && creationEvents.TryGetInfoForPath(evt.OldPath, out var creationHashInfo))
            {
                if (creationHashInfo.State is ItemHashCodeState.Clash 
                    || creationHashInfo.Event is not { } correspondingCreation)
                {
                    logger.LogError("Storage item hashing clash. Unable to resolve move file system event");
                    continue;
                }
                
                handledArgs.Add(evt);
                handledArgs.Add(correspondingCreation);
                yield return new FileSystemGraphMutation(FileSystemGraphMutationType.Move, evt.OldPath, correspondingCreation.NewPath);
                continue;
            }

            if (evt.ChangeType == WatcherChangeTypes.Created
                && deletionEvents.TryGetInfoForPath(evt.NewPath, out var deletionHashInfo))
            {
                if (deletionHashInfo.State is ItemHashCodeState.Clash
                    || deletionHashInfo.Event is not { } correspondingDeletion)
                {
                    logger.LogError("Storage item hashing clash. Unable to resolve move file system event");
                    continue;
                }
                
                handledArgs.Add(correspondingDeletion);
                handledArgs.Add(evt);
                yield return new FileSystemGraphMutation(FileSystemGraphMutationType.Move, correspondingDeletion.OldPath, evt.NewPath);
                continue;
            }

            yield return evt.ChangeType switch
            {
                WatcherChangeTypes.Created => new(FileSystemGraphMutationType.Creation, evt.OldPath, evt.NewPath),
                WatcherChangeTypes.Deleted => new(FileSystemGraphMutationType.Deletion, evt.OldPath, evt.NewPath),
                WatcherChangeTypes.Renamed => new(FileSystemGraphMutationType.Rename, evt.OldPath, evt.NewPath),
                var unknown => throw new InvalidOperationException($"Unable to handle file system event {unknown}")
            };
        }
        
        _allEvents.Clear();
    }

    public void AddEvent(FileSystemEvent fileSystemEvent)
    {
        // We only care about changes to the file system layout
        if (fileSystemEvent.ChangeType == WatcherChangeTypes.Changed)
        {
            return;
        }
        
        _allEvents.Add(fileSystemEvent);
    }
}