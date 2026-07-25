using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.IO;

public sealed class FileWaiter : IFileWaiter
{
    private readonly Timer? _warningTimer;
    private IFileWatcher? _watcher;

    public FileWaiter(FileSystemPath path, TimeSpan? waitWarningDuration, IFileSystem fileSystem)
    {
        IsFileCreated = fileSystem.Exists(path);
        FilePath = path;

        if (IsFileCreated) return;

        var parentPath = path.Parent ?? throw new InvalidOperationException("Cannot wait for file at path without parent");

        _watcher = fileSystem.GetFileWatcher(parentPath, path.NameAndExtension);
        _watcher.EnableRaisingEvents = true;
        _watcher.Created += OnFileCreated;

        if (waitWarningDuration.HasValue)
        {
            _warningTimer = new Timer(OnWarningTimerElapsed, null, waitWarningDuration.Value, Timeout.InfiniteTimeSpan);
        }
    }

    public event EventHandler? FileCreated;

    public event EventHandler? WaitWarning;

    public bool IsFileCreated { get; private set; }

    public FileSystemPath FilePath { get; }

    private void OnWarningTimerElapsed(object? state)
    {
        if (IsFileCreated)
            return;

        WaitWarning?.Invoke(this, EventArgs.Empty);
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath != FilePath || e.ChangeType != WatcherChangeTypes.Created)
            return;

        _warningTimer?.Dispose();

        _watcher?.Dispose();
        _watcher = null;

        IsFileCreated = true;
        FileCreated?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _warningTimer?.Dispose();
        _watcher?.Dispose();
        _watcher = null;
    }
}