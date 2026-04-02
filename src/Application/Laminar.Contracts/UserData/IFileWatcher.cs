namespace Laminar.Contracts.UserData;

public interface IFileWatcher : IDisposable
{
    public NotifyFilters NotifyFilter { get; set; }
    
    public bool EnableRaisingEvents { get; set; }
    
    public bool IncludeSubdirectories { get; set; }

    public int InternalBufferSize { get; set; }
    
    public event FileSystemEventHandler? Changed;
    
    public event FileSystemEventHandler? Created;
    
    public event FileSystemEventHandler? Deleted;
    
    public event RenamedEventHandler? Renamed;

    public event ErrorEventHandler? Error;
}