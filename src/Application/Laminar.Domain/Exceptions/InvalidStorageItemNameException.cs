namespace Laminar.Domain.Exceptions;

public class InvalidStorageItemNameException(string name) : IOException($"The file or folder name '{name}' contains invalid characters")
{
    public string Name => name;
}
