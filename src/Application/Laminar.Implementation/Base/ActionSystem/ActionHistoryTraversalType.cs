namespace Laminar.Implementation.Base.ActionSystem;

internal sealed class ActionHistoryTraversalType
{
    static ActionHistoryTraversalType()
    {
        Undo = new ActionHistoryTraversalType(-1, null!, HistoryTraversalDirection.Backwards);
        Redo = new ActionHistoryTraversalType(+1, Undo, HistoryTraversalDirection.Forwards);
        Undo.Reverse = Redo;
    }
    
    public static ActionHistoryTraversalType Undo { get; }

    public static ActionHistoryTraversalType Redo { get; }

    private ActionHistoryTraversalType(int increment, ActionHistoryTraversalType reverse, HistoryTraversalDirection direction)
    {
        Reverse = reverse;
        Increment = increment;
        Direction = direction;
    }
    
    public int Increment { get; }

    public ActionHistoryTraversalType Reverse { get; private set; }

    public HistoryTraversalDirection Direction { get; }
}

internal enum HistoryTraversalDirection
{
    Forwards,
    Backwards
}