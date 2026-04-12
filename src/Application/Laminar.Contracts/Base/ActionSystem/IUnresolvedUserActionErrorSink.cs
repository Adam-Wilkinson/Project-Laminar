namespace Laminar.Contracts.Base.ActionSystem;

public interface IUnresolvedUserActionErrorSink
{
    public Task OnError(IUserAction action, UserActionError error);
}
