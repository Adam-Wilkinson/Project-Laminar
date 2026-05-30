namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionManager<TSimplifier> : IUserActionManager 
    where TSimplifier : class, IUserActionSimplifier, new();

public interface IUserActionManager
{
    public IUserActionSession BeginSession();
    
    public Task<IUserActionResult> ExecuteAction(IUserAction action);
    
    public Task<IUserActionResult> Undo();

    public Task<IUserActionResult> Redo();

    public void RegisterSimplifier(IUserActionSimplifier simplifier);
}
