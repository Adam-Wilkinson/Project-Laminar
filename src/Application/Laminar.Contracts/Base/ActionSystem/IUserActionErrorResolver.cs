namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionErrorResolver
{
    public Task<IUserActionErrorResolution?> TryResolve(IUserActionResult result);
}
