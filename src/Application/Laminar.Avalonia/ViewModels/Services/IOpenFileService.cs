namespace Laminar.Avalonia.ViewModels.Services;

public interface IOpenFileService
{
    public Task RequestOpenFile(FileNavigatorItemViewModel file);
}