using System;
using System.Collections.Generic;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;

namespace Laminar.Implementation.Scripting.Actions;

public class AddNodeAction(IWrappedNode node, ICollection<IWrappedNode> nodeCollection)
    : IUserAction
{
    public event EventHandler? CanExecuteChanged;
    
    public bool CanExecute => true;

    public IUserActionResult Execute()
    {
        nodeCollection.Add(node);
        return IUserActionResult.Success(new DeleteNodeAction(node, nodeCollection));
    }
}
