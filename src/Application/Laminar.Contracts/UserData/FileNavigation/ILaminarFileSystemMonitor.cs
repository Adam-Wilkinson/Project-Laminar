namespace Laminar.Contracts.UserData.FileNavigation;

public interface ILaminarFileSystemMonitor
{
    IDisposable StartMonitoring(ILaminarStorageRootFolder folder);
}
