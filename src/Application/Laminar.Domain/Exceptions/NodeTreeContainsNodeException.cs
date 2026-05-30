namespace Laminar.Domain.Exceptions;

public class NodeTreeContainsNodeException(object node) : Exception($"The node tree already contains node {node}");