using Laminar.Contracts.Scripting.NodeWrapping;

namespace Laminar.Implementation.Scripting;

public interface INodeCollection
{
    public void AddNode(INodeContainer nodeContainer);
    
    public bool DeleteNode(INodeContainer nodeContainer);
    
    public bool ContainsNode(INodeContainer nodeContainer);
}