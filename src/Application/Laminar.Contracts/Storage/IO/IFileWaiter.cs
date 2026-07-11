using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Storage.IO;

public interface IFileWaiter : IDisposable
{
    public event EventHandler? FileCreated;
    
    public event EventHandler? WaitWarning;
    
    public bool IsFileCreated { get; }

    public FileSystemPath FilePath { get; }
}