using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

internal sealed class ActionHistoryEventParticipant
{
    private readonly ActionHistory _history;

    private ActionHistoryEvent? _redoTarget;
    private ActionHistoryEvent? _undoTarget;

    public ActionHistoryEventParticipant(
        ActionHistory history, 
        ActionHistoryEvent originalEvent, 
        ActionHistoryTraversalType firstTraversalType)
    {
        _history = history;
        _redoTarget = originalEvent;
        _undoTarget = originalEvent;

        if (firstTraversalType == ActionHistoryTraversalType.Undo)
        {
            _history.RegisterCompletedEvent(originalEvent);
            _undoTarget = originalEvent;
            _redoTarget = null;
        }
        else if (firstTraversalType == ActionHistoryTraversalType.Redo)
        {
            _history.AddEventAfterCursor(originalEvent);
            _undoTarget = null;
            _redoTarget = originalEvent;
        }
    }

    public Task<UserActionResult> Traverse(ActionHistoryTraversalType traversalType) => traversalType.Direction switch
    {
        HistoryTraversalDirection.Forwards => TraverseForwards(),
        HistoryTraversalDirection.Backwards => TraverseBackwards(),
        _ => throw new ArgumentOutOfRangeException(nameof(traversalType), traversalType, null)
    };
    
    private Task<UserActionResult> TraverseForwards()
    {
        if (_redoTarget is not { } redoTargetEvent)
        {
            return Task.FromResult(UserActionResult.Error(new InvalidOperationException("Action history participant traversed forwards twice")));
        }

        _redoTarget = null;
        _undoTarget = _history.GetNext(ActionHistoryTraversalType.Redo) ?? 
                      throw new InvalidOperationException("Participating history is not positioned before its participating event");
        return _history.MovePastEvent(ActionHistoryTraversalType.Redo, redoTargetEvent);
    }

    private Task<UserActionResult> TraverseBackwards()
    {
        if (_undoTarget is not { } undoTargetEvent)
        {
            return Task.FromResult(UserActionResult.Error(new InvalidOperationException("Action history participant traversed backwards twice")));
        }

        _redoTarget = _history.GetNext(ActionHistoryTraversalType.Undo) ?? 
                      throw new InvalidOperationException("Participating history is not positioned after its participating event");;
        _undoTarget = null;
        return _history.MovePastEvent(ActionHistoryTraversalType.Undo, undoTargetEvent);
    }
}