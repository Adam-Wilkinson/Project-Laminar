namespace Laminar.Contracts.Storage.FileExplorer;

public interface ILaminarStorageFile : ILaminarStorageItem
{
    public long SizeOnDisk { get; }
}
