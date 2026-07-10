using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.Enums.ActionResolutions;
using Laminar.Domain.Exceptions;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.Storage.FileExplorer.UserActions;

internal readonly struct MoveStorageItemAction(
     IFileSystemItem item, 
     IFileSystemFolder destinationFolder,
     int? targetIndex,
     FileBrowserActionDependencies dependencies)
     : IUserAction
{
    public bool CanExecute => true;
     
    public IFileSystemItem Target => item;
    
    public Task<IUserActionResult> Execute()
    {
        if (item is IFileSystemRootFolder)
            return Task.FromResult(IUserActionResult.Error(new CannotMoveRootFolderException(item.UserFriendlyName)));
        
        if (item.ParentFolder is not { } oldFolder) 
            return Task.FromResult(IUserActionResult.Error(new InvalidOperationException("Cannot move storage item without parent")));
        
        var indexInOldFolder = oldFolder.LoadOrGetContents().IndexOf(item);
        var indexInDestinationFolder = targetIndex ?? destinationFolder.LoadOrGetContents().Count;
     
        if (destinationFolder.LoadOrGetContents().FirstOrDefault(NameEqualsItemName) is { } clash && !Equals(clash, item))
        {
            FileBrowserActionDependencies actionDependencies = dependencies;
            MoveStorageItemAction moveAction = this;
            IFileSystemItem targetItem = item;
            IFileSystemFolder destination = destinationFolder;
            
            return Task.FromResult<IUserActionResult>(new ResolvableError<NamingConflictResolution> 
            {
                Exception = new DestinationContainsItemOfThatNameException(destinationFolder.UserFriendlyName, item.UserFriendlyName),
                Resolve = resolution => resolution switch
                {
                    NamingConflictResolution.ReplaceItem => new AlternativeActionFound(new CompoundAction(new DeleteStorageItemAction(clash, actionDependencies), moveAction)),
                    NamingConflictResolution.IncrementName => new AlternativeActionFound(new CompoundAction(
                        new RenameStorageItemAction(actionDependencies.FileSystem.GetNameWithoutExtension(targetItem.Path) + " (1)", targetItem, actionDependencies), moveAction)),
                    _ => throw new InvalidOperationException(),
                },
                OnCancelled = () =>
                {
                    oldFolder.Refresh();
                    destination.Refresh();
                }
            });
        }

        try
        {
            dependencies.CommandService.Move(item, destinationFolder, indexInDestinationFolder);
        }
        catch (IOException exception)
        {
            return Task.FromResult(IUserActionResult.Error(exception));
        }

        return Task.FromResult(IUserActionResult.Success(new MoveStorageItemAction(item, oldFolder, indexInOldFolder, dependencies)));
    }
    
    private bool NameEqualsItemName(IFileSystemItem comparisonItem)
        => item.UserFriendlyName.Equals(comparisonItem.UserFriendlyName, FileSystemPath.RuntimeStringComparison);
}