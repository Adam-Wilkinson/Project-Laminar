using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Base.ActionSystem;

public class ActionFromParentScope : IUserAction
{
    public Task<IUserActionResult> Execute()
    {
        throw new NotImplementedException();
    }
}