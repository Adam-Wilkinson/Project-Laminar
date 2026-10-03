using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

internal sealed class ActionHistoryEvent
{
    private IUserAction _nextAction;
    private ActionHistoryTraversalType _nextExpectedTraversalType;
    private readonly IReadOnlyList<ActionHistoryEventParticipant> _participants;

    public ActionHistoryEvent(IUserAction originalAction,
        ActionHistoryTraversalType firstTraversalType,
        IEnumerable<ActionHistory> participants)
    {
        _participants = [.. participants.Select(x => new ActionHistoryEventParticipant(x, this, firstTraversalType))];
        _nextAction = originalAction;
        _nextExpectedTraversalType = firstTraversalType;
    }

    public bool IsExecutable { get; private set; } = true;
    
    public async Task<UserActionResult> Traverse(ActionHistoryTraversalType type, IUserActionResolver resolver)
    {
        if (!IsExecutable)
        {
            return UserActionResult.Success();
        }
        
        if (type != _nextExpectedTraversalType)
        {
            throw new InvalidOperationException($"This event is not expected to be executed in the direction {type}");
        }
        
        var progressThroughParticipants = 0;
        var rollingResult = UserActionResult.Success();
        foreach (var participant in _participants)
        {
            rollingResult.Merge(await participant.Traverse(type));
            if (!rollingResult.Succeeded)
            {
                break;
            }
            
            progressThroughParticipants++;
        }

        if (rollingResult.Succeeded)
        {
            var actionOutcome = await resolver.Resolve(_nextAction);
            switch (actionOutcome)
            {
                case UserActionSuccess success:
                    _nextAction = success.InverseAction;
                    _nextExpectedTraversalType = type.Reverse;
                    return success.ToActionResult().Merge(rollingResult);
                case UserActionError error:
                    rollingResult.Merge(UserActionResult.Error(error.Exception));
                    break;
                default:
                    rollingResult.Merge(UserActionResult.Error(new InvalidOperationException($"Invalid resoled action outcome: {actionOutcome}")));
                    break;
            }
        }
        
        // ERROR: Restore
        IsExecutable = false;
        for (var i = 0; i < progressThroughParticipants; i++)
        {
            rollingResult.Merge(await _participants[i].Traverse(type));
        }

        return rollingResult;
    }
}