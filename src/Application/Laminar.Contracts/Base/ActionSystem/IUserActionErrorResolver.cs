namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionErrorResolver
{
    public IUserActionErrorResolution? TryResolve(UserActionError error);

    public Task<IUserActionErrorResolution?> TryResolveAsync(UserActionError error);
}
