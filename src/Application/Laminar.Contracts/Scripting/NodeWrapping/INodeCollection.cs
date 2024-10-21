using Laminar.Domain.Notification;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface INodeCollection : IReadOnlyObservableCollection<IWrappedNode>, IList<IWrappedNode>
{
}
