using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

internal static class ActionHistoryTraversalExtensions
{
    extension(ActionHistory history)
    {
        /// <summary>
        /// Moves the action cursor in the given history until <see cref="targetEvent"/> has been moved past
        /// <remarks>Result includes an <see cref="ArgumentOutOfRangeException"/> if there is no history in the given direction, and an <see cref="InvalidOperationException"/> if the event is not found</remarks>
        /// </summary>
        /// <param name="traversalType">The direction in which to search</param>
        /// <param name="targetEvent">The event to move past</param>
        /// <returns>The result of executing all the actions</returns>
        public async Task<UserActionResult> MovePastEvent(ActionHistoryTraversalType traversalType, ActionHistoryEvent targetEvent)
        {
            if (history.GetNext(traversalType) is not { } firstEvent)
            {
                return UserActionResult.Error(new ArgumentOutOfRangeException(nameof(traversalType)));
            }
        
            var rollingResult = UserActionResult.Success();
            while (history.GetNext(traversalType) is { } currentEvent)
            {
                rollingResult.Merge(await history.ShiftCursor(traversalType));

                if (!rollingResult.Succeeded)
                {
                    rollingResult.Merge(await history.MovePastEvent(traversalType.Reverse, firstEvent));
                    return rollingResult;
                }
            
                if (currentEvent == targetEvent)
                {
                    return rollingResult;
                }
            }
        
            rollingResult.Merge(UserActionResult.Error(new InvalidOperationException("Target event not found")));
            return rollingResult;
        }

        /// <summary>
        /// Traverses the action history in the given direction until a valid action is executed
        /// </summary>
        /// <param name="traversalType">The direction to traverse in</param>
        /// <returns>The result of the execution. If no valid action is found, this is just success</returns>
        public async Task<UserActionResult> ExecuteNextValid(ActionHistoryTraversalType traversalType)
        {
            var result = UserActionResult.Success();
            while (history.GetNext(traversalType) is { } currentEvent)
            {
                result.Merge(await history.ShiftCursor(traversalType));
                if (currentEvent.IsExecutable)
                {
                    return result;
                }
            }

            return result;
        }
    }
}