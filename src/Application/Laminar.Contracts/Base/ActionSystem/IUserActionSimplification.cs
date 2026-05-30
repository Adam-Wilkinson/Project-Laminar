namespace Laminar.Contracts.Base.ActionSystem;

/// <summary>
/// Allows a composite user action session to be simplified into less total actions
/// </summary>
public interface IUserActionSimplification
{
    private static readonly IUserActionSimplification NoSimplification = new NoSimplification();
    private static readonly IUserActionSimplification ReversesAction = new ReversesAction();
    private static readonly IUserActionSimplification OverridesAction = new OverridesAction();

    /// <summary>
    /// There is no simplification after the previous action
    /// </summary>
    public static IUserActionSimplification None() => NoSimplification;
    
    /// <summary>
    /// The action reverses the previous action, for example an action followed by its inverse
    /// </summary>
    public static IUserActionSimplification Undoes() => ReversesAction;
    
    /// <summary>
    /// The action overrides the previous action, for example renaming the same item twice
    /// </summary>
    public static  IUserActionSimplification Overrides() => OverridesAction;
    
    /// <summary>
    /// The previous action followed by this action is better understood as one overall action
    /// </summary>
    /// <param name="action">The new overall action</param>
    public static IUserActionSimplification NewEffectiveAction(IUserAction action) => new NewEffectiveAction(action);
}

public class NoSimplification : IUserActionSimplification;

public class ReversesAction : IUserActionSimplification;

public class OverridesAction : IUserActionSimplification;

public record NewEffectiveAction(IUserAction NewAction) : IUserActionSimplification;