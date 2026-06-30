using System.ComponentModel;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.FileExplorer;

internal sealed class LaminarFileResource<TValue, TData> : ILaminarFileResource<TValue> 
    where TValue : class, IEncodableDataOwner<IEncodableData>, IEncodableDataOwner<TData>
    where TData : class, IEncodableData
{
    private readonly IResourceOnDisk<TValue> _resourceOnDisk;

    public LaminarFileResource(IPersistentDataManager dataManager, IPersistentDataTranscoder transcoder, IDecodingFactory<TValue, TData> factory, LaminarStorageFile file)
    {
        _resourceOnDisk = dataManager.GetResourceOnDisk(file.Path, transcoder, factory);
        File = file;
        File.PropertyChanged += OnFilePropertyChanged;
        File.OnDeleted += OnFileDeleted;
    }

    private void OnFileDeleted(object? sender, EventArgs e)
    {
        Dispose();
        OnDeleted?.Invoke(this, EventArgs.Empty);
    }

    private void OnFilePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ILaminarStorageItem.Path))
        {
            _resourceOnDisk.Location = File.Path;
        }
    }

    public ILaminarStorageFile File { get; }

    public TValue Value => _resourceOnDisk.Resource;
    
    public event EventHandler? OnDeleted;

    public void Dispose()
    {
        _resourceOnDisk.Dispose();
        File.PropertyChanged -= OnFilePropertyChanged;
        File.OnDeleted -= OnFileDeleted;
    }
}