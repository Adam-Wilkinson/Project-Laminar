using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base;
using Laminar.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Base.ActionSystem;

internal class UserActionManager(IServiceProvider serviceProvider) : IUserActionManager
{
    private readonly Dictionary<string, IUserActionScope> _scopes = [];
    
    public IUserActionScope GetScope(string key, IUserActionSimplifier[] simplifiers)
    {
        if (_scopes.TryGetValue(key, out var existingScope))
        {
            foreach (var simplifier in simplifiers)
            {
                existingScope.AddSimplifier(simplifier);
            }

            return existingScope;
        }

        var newScope = ActivatorUtilities.CreateInstance<UserActionScope>(serviceProvider);
        foreach (var simplifier in simplifiers)
        {
            newScope.AddSimplifier(simplifier);
        }
        _scopes[key] = newScope;
        return newScope;
    }
}
