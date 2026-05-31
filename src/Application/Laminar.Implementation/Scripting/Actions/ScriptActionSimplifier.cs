using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Scripting.Actions;

internal class ScriptActionSimplifier : IUserActionSimplifier
{
    public IUserActionSimplification Simplify(IUserAction first, IUserAction second) => (first, second) switch
    {
        (MoveNodeAction firstMove, MoveNodeAction secondMove) 
            when firstMove.Node == secondMove.Node => MergeMoves(firstMove, secondMove),
        
        (AddNodeAction addFirstAction, DeleteNodeAction deleteSecondAction)
            when addFirstAction.Node == deleteSecondAction.Node => IUserActionSimplification.Undoes(),
        
        (DeleteNodeAction deleteFirstAction, AddNodeAction addSecondAction) 
            when deleteFirstAction.Node == addSecondAction.Node => IUserActionSimplification.Undoes(),
        
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

        return IUserActionSimplification.NewEffectiveAction(new MoveNodeAction(firstMove.Node, totalMove));   
    }
    
    private static bool ConnectionMatches(EstablishConnectionAction establishAction, SeverConnectionAction severAction) 
        => Equals(establishAction.InputConnector, severAction.InputConnector) && 
           Equals(establishAction.OutputConnector, severAction.OutputConnector); 
}