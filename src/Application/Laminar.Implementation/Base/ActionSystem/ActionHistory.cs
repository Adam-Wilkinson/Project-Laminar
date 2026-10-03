using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

internal class ActionHistory(IUserActionResolver resolver)
{
    private readonly List<ActionHistoryEvent> _events = [];
    
    public int CursorPosition { get; private set; }

    public int EventCount => _events.Count;

    public void AddEventAfterCursor(ActionHistoryEvent newEvent) => _events.Insert(CursorPosition, newEvent);

    public ActionHistoryEvent? GetNext(ActionHistoryTraversalType traversalType) => traversalType.Direction switch
    {
        HistoryTraversalDirection.Forwards => CursorPosition < _events.Count ? _events[CursorPosition] : null,
        HistoryTraversalDirection.Backwards => CursorPosition > 0 ? _events[CursorPosition - 1] : null,
        _ => throw new ArgumentOutOfRangeException(nameof(traversalType), traversalType, null)
    };
    
    public async Task<UserActionResult> ShiftCursor(ActionHistoryTraversalType traversalType)
    {
        if (GetNext(traversalType) is not { } toExecute)
        {
            return UserActionResult.Error(new ArgumentOutOfRangeException(nameof(traversalType)));
        }
        
        var result = await toExecute.Traverse(traversalType, resolver);
        if (result.Succeeded)
        {
            CursorPosition += traversalType.Increment;
        }
        
        return result;
    }

    public void RegisterCompletedEvent(ActionHistoryEvent historyEvent)
    {
        _events.Insert(CursorPosition, historyEvent);
        CursorPosition++;
    }
}