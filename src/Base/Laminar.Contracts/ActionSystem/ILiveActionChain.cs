namespace Laminar.Contracts.ActionSystem;

public interface ILiveActionChain
{
    public void Commit();

    public void ExecuteAction(IUserAction action);

    public void Reset();
}
