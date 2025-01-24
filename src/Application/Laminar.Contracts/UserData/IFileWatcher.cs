namespace Laminar.Contracts.UserData;

public interface IFileWatcher : IDisposable
{
    public NotifyFilters NotifyFilter { get; set; }
    
    public bool EnableRaisingEvents { get; set; }
    
    public event FileSystemEventHandler? Changed;
}