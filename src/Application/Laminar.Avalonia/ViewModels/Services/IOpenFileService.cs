using Laminar.Contracts.Storage.FileExplorer;

namespace Laminar.Avalonia.ViewModels.Services;

public interface IOpenFileService
{
    public Task RequestOpenFile(IFileSystemFile file);

    public event EventHandler<IFileSystemFile>? FileOpened;
    
    public event EventHandler<IFileSystemFile>? FileClosed;
    
    public bool FileIsOpen(IFileSystemFile file);
}