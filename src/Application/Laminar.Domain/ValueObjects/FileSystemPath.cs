namespace Laminar.Domain.ValueObjects;

public readonly struct FileSystemPath(string path) : IEquatable<FileSystemPath>
{
    public FileSystemPath ChildPath(string childName) => new(Path.Join(path, childName));
    
    public FileSystemPath? Parent => Path.GetDirectoryName(path) is { } parent ? new(parent) : null;
    
    public string Extension => Path.GetExtension(path);
    
    public string Name => Path.GetFileNameWithoutExtension(path);
    
    public string NameAndExtension => Path.GetFileName(path);
    
    public override string ToString()
    {   
        return path;
    }

    public override bool Equals(object? obj)
    {
        return obj is FileSystemPath fsp && EqualityComparer<string>.Default.Equals(fsp.ToString(), path);
    }

    public override int GetHashCode() => path.GetHashCode();

    public static bool operator ==(FileSystemPath left, FileSystemPath right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FileSystemPath left, FileSystemPath right)
    {
        return !(left == right);
    }

    public bool Equals(FileSystemPath other)
    {
        return path == other.ToString();
    }
}