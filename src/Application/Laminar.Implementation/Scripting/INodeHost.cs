using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base.PluginLoading;

namespace Laminar.Implementation.Scripting;

public interface INodeHost
{
    public IRuntimeHost Runtime { get; }
    
    public IUserActionScope ActionScope { get; }
    
    public INodeCollection Nodes { get; }
}