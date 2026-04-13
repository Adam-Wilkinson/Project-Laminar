namespace Laminar.Domain.Exceptions;

public class FileWithNameExistsException(string name) : IOException($"A file or folder with the name '{name}' already exists in that folder")
{
    public string Name => name;
}
