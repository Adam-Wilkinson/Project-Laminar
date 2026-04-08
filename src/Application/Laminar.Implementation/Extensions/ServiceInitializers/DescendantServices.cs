using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Extensions.ServiceInitializers;

public static class DescendantServices
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection AddDescendantsTransient<T>()
        { 
            foreach (var descendantType in DescendantTypes<T>())
            {
                collection.AddTransient(descendantType);
                collection.AddTransient(typeof(T), descendantType);
            }

            return collection;
        }

        public IServiceCollection AddDescendantsScoped<T>()
        {
            foreach (var descendantType in DescendantTypes<T>())
            {
                collection.AddScoped(descendantType);
                collection.AddScoped(typeof(T), descendantType);
            }

            return collection;
        }

        public IServiceCollection AddDescendantsSingleton<T>()
        {
            foreach (var descendantType in DescendantTypes<T>())
            {
                collection.AddSingleton(descendantType);
                collection.AddSingleton(typeof(T), descendantType);
            }
        
            return collection;
        }
    }
    
    private static IEnumerable<Type> DescendantTypes<T>()
        => typeof(T).Assembly.GetTypes()
            .Where(x => x is { IsClass: true, IsAbstract: false } && x.IsAssignableTo(typeof(T)));
}