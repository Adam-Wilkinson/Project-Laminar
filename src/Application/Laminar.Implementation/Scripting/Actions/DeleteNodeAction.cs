using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Laminar.Implementation.Scripting.Actions;

public class DeleteNodeAction(IWrappedNode node, ICollection<IWrappedNode> nodeCollection)
    : IUserAction
{
    public event EventHandler? CanExecuteChanged;
    
    public bool CanExecute { get; } = nodeCollection.Contains(node);

    public Task<IUserActionResult> Execute()
    {
        nodeCollection.Remove(node);
        return Task.FromResult(IUserActionResult.Success(new AddNodeAction(node, nodeCollection)));
    }
}
