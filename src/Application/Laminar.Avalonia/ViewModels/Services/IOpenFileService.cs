using Laminar.Contracts.Storage.FileExplorer;

namespace Laminar.Avalonia.ViewModels.Services;

public interface IOpenFileService
{
    public Task RequestOpenFile(IFileSystemFile newFile);

    public event EventHandler? OpenFilesChanged; 
    
    public bool FileIsOpen(IFileSystemFile file);
}