using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Avalonia.ViewModels;

public sealed partial class OpenFileViewModel(IServiceProvider serviceProvider) : ViewModelBase, IDisposable
{
    private IFileResource<IEncodableDataOwner<IEncodableData>>? _fileResource;
    private IDisposable? _currentViewModelScope;

    public void Close()
    {
        _fileResource?.Dispose();
        _fileResource?.Deleted -= OnFileResourceDeleted;
        _fileResource = null;
        
        _currentViewModelScope?.Dispose();
        _currentViewModelScope = null;
        
        (FileContents as IDisposable)?.Dispose();
        FileContents = null;
    }
    
    public void OpenFile<TValue, TData, TViewModel>(
        FileNavigatorItemViewModel fileViewModel,
        IPersistentDataTranscoder dataTranscoder,
        IDecodingFactory<TValue, TData> decodingFactory,
        Func<IServiceProvider, TValue, TViewModel> viewModelFactory)
        where TData : class, IEncodableData
        where TValue : class, IEncodableDataOwner<TData>
        where TViewModel : ViewModelBase
    {
        if (fileViewModel.CoreItem is not IFileSystemFile coreFile)
        {
            throw new ArgumentException("Opening a view model that's not a file");
        }

        if (coreFile.Info.ContentsType != typeof(TValue))
        {
            throw new ArgumentException("Opening a view model of the incorrect type");
        }

        Close();
        var resource = coreFile.GetContentsAsResource(dataTranscoder, decodingFactory);
        var scopedViewModel =
            new ScopedViewModel<TViewModel>(serviceProvider, sp => viewModelFactory(sp, resource.Value));
        _currentViewModelScope = scopedViewModel;
        _fileResource = resource;
        _fileResource.Deleted += OnFileResourceDeleted;
        FileContents = scopedViewModel.ViewModel;
    }

    private void OnFileResourceDeleted(object? sender, EventArgs e)
    {
        Close();
    }

    public IFileSystemFile? CurrentlyOpenFile => _fileResource?.File;

    [ObservableProperty]
    public partial ViewModelBase? FileContents { get; private set; }

    public void Dispose()
    {
        _fileResource?.Dispose();
    }
}