using System.ComponentModel;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.FileExplorer;

internal sealed class FileResource<TValue, TData> : IFileResource<TValue> 
    where TValue : class, IEncodableDataOwner<IEncodableData>, IEncodableDataOwner<TData>
    where TData : class, IEncodableData
{
    private readonly IDataOnDisk<TData> _dataOnDisk;

    public FileResource(IPersistentDataManager dataManager, IPersistentDataTranscoder transcoder, IDecodingFactory<TValue, TData> factory, FileSystemFile fileSystemFile)
    {
        _dataOnDisk = dataManager.GetDataOnDisk<TData>(fileSystemFile.Path, transcoder);

        Value = factory.FromPersistentData(_dataOnDisk.Data);
        
        FileSystemFile = fileSystemFile;
        FileSystemFile.PropertyChanged += OnFileSystemFilePropertyChanged;
        FileSystemFile.OnDeleted += OnFileSystemFileDeleted;
    }

    private void OnFileSystemFileDeleted(object? sender, EventArgs e)
    {
        Dispose();
        OnDeleted?.Invoke(this, EventArgs.Empty);
    }

    private void OnFileSystemFilePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IFileSystemItem.Path))
        {
            _dataOnDisk.Location = FileSystemFile.Path;
        }
    }

    public IFileSystemFile FileSystemFile { get; }

    public TValue Value { get; }

    public event EventHandler? OnDeleted;

    public void Dispose()
    {
        _dataOnDisk.Dispose();
        FileSystemFile.PropertyChanged -= OnFileSystemFilePropertyChanged;
        FileSystemFile.OnDeleted -= OnFileSystemFileDeleted;
    }
}