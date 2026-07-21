using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Avalonia.ViewModels.Services;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia.ViewModels;

public sealed partial class FileViewModel(
    FileViewModelFactory fileViewModelFactory,
    IExceptionHandler exceptionHandler) : ViewModelBase, IDisposable
{
    private OpenFile? _openFile;
    private CancellationTokenSource? _openFileCts;
    
    [ObservableProperty] 
    public partial IFileSystemFile? CurrentFile { get; set; }

    [Persistent, ObservableProperty]
    public partial FileSystemPath? OpenFilePath { get; set; }

    [ObservableProperty]
    public partial ViewModelBase? CurrentFileViewModel { get; private set; }

    public void Dispose()
    {
        OpenFilePath = null;
    }
    
    private void OnOpenFileDeleted(object? sender, EventArgs e)
    {
        OpenFilePath = null;
    }
    
    partial void OnOpenFilePathChanged(FileSystemPath? value)
    {
        _openFileCts?.Cancel();
        _openFileCts = new CancellationTokenSource();
        
        _ = ChangeOpenFileAsync(value, _openFileCts.Token);
    }

    private async Task ChangeOpenFileAsync(FileSystemPath? value, CancellationToken cancellationToken)
    {
        try
        {
            if (Equals(_openFile?.FileResource.File.Path, value)) return;
            
            _openFile?.Deleted -= OnOpenFileDeleted;
            _openFile?.FileResource.File.PropertyChanged -= FileOnPropertyChanged;
            _openFile?.Dispose();
            _openFile = null;
            CurrentFile = null;
            CurrentFileViewModel = null;
            
            if (value is null) return;

            _openFile = await fileViewModelFactory.Open(value.Value, cancellationToken);
            _openFile?.Deleted += OnOpenFileDeleted;
            _openFile?.FileResource.File.PropertyChanged += FileOnPropertyChanged;

            CurrentFile = _openFile?.FileResource.File;
            CurrentFileViewModel = _openFile?.FileViewModel;
        }
        catch (Exception ex)
        {
            await exceptionHandler.OnExceptionAsync(ex, cancellationToken);
        }
    }
    
    private void FileOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IFileSystemItem.Path))
        {
            OpenFilePath = _openFile?.FileResource.File.Path;
        }
    }
}