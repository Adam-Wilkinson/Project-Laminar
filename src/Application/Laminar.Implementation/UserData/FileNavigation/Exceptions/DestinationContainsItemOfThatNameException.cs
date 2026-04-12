using System.IO;

namespace Laminar.Implementation.UserData.FileNavigation.Exceptions;

public class DestinationContainsItemOfThatNameException(string destinationFolder, string itemName) : IOException($"The folder '{destinationFolder}' already contains an item of name '{itemName}'")
{
    public string DestinationFolder { get; } = destinationFolder;
    public string ItemName { get; } = itemName;
}