using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Scripting.Actions;

internal class ScriptActionSimplifier : IUserActionSimplifier
{
    public IUserActionSimplification Simplify(IUserAction first, IUserAction second) => (first, second) switch
    {
        (MoveNodeAction firstMove, MoveNodeAction secondMove) 
            when firstMove.NodeContainer == secondMove.NodeContainer => MergeMoves(firstMove, secondMove),
        
        (AddNodeAction addFirstAction, DeleteNodeAction deleteSecondAction)
            when addFirstAction.NodeContainer == deleteSecondAction.NodeContainer => IUserActionSimplification.Undoes(),
        
        (DeleteNodeAction deleteFirstAction, AddNodeAction addSecondAction) 
            when deleteFirstAction.NodeContainer == addSecondAction.NodeContainer => IUserActionSimplification.Undoes(),
        
        (SeverConnectionAction severFirstAction, EstablishConnectionAction establishSecondAction) 
            when ConnectionMatches(establishSecondAction, severFirstAction) => IUserActionSimplification.Undoes(),
        
        (EstablishConnectionAction establishFirstAction, SeverConnectionAction severSecondAction)
            when ConnectionMatches(establishFirstAction, severSecondAction) => IUserActionSimplification.Undoes(),
        
        _ => IUserActionSimplification.None(),
    };

    private static IUserActionSimplification MergeMoves(MoveNodeAction firstMove, MoveNodeAction secondMove)
    {
        var totalMove = firstMove.LocationDelta + secondMove.LocationDelta;

        if (totalMove.SquaredDistance() < 1e-10) return IUserActionSimplification.Undoes();

        return IUserActionSimplification.NewEffectiveAction(new MoveNodeAction(firstMove.NodeContainer, totalMove));   
    }
    
    private static bool ConnectionMatches(EstablishConnectionAction establishAction, SeverConnectionAction severAction) 
        => Equals(establishAction.InputConnector, severAction.InputConnector) && 
           Equals(establishAction.OutputConnector, severAction.OutputConnector); 
}