using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Base.ActionSystem;

internal class UserActionManager(IServiceProvider serviceProvider) : IUserActionManager
{
    public IUserActionScope CreateScope(params IUserActionSimplifier[] simplifiers)
        => ActivatorUtilities.CreateInstance<UserActionScope>(serviceProvider, (object)simplifiers);
}
