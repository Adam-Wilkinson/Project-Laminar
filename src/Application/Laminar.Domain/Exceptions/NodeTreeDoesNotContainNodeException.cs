namespace Laminar.Domain.Exceptions;

public class NodeTreeDoesNotContainNodeException(object node) : Exception($"The node {node} already exists in the node tree");