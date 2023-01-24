using System.Collections.ObjectModel;
using Laminar.Contracts.Scripting.NodeWrapping;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class NodeCollection : ObservableCollection<IWrappedNode>, INodeCollection
{
}
