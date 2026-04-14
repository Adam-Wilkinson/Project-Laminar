using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;

namespace Laminar.Implementation.Scripting.Actions;

public class AddNodeAction(IWrappedNode node, ICollection<IWrappedNode> nodeCollection)
    : IUserAction
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }

    public bool CanExecute => true;

    public Task<IUserActionResult> Execute()
    {
        nodeCollection.Add(node);
        return Task.FromResult(IUserActionResult.Success(new DeleteNodeAction(node, nodeCollection)));
    }
}
