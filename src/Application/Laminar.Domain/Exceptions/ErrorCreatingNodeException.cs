namespace Laminar.Domain.Exceptions;

public class ErrorCreatingNodeException(string nodeName, Exception inner)
    : Exception($"Error creating node {nodeName}", inner)
{
    public string NodeName { get; } = nodeName;
}