using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Extensions.ServiceInitializers;

public static class ServiceProviderFactoryExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddFactory<T1, TResult>()
        {
            var factory = ActivatorUtilities.CreateFactory<TResult>([typeof(T1)]);
            return serviceCollection.AddSingleton<Func<T1, TResult>>(sp => (o1) => factory.Invoke(sp, [o1]));
        }

        public IServiceCollection AddFactory<T1, TInterface, TResult>()
            where TResult : TInterface
        {
            var factory = ActivatorUtilities.CreateFactory<TResult>([typeof(T1)]);
            return serviceCollection.AddSingleton<Func<T1, TInterface>>(sp => o1 => factory.Invoke(sp, [o1]));
        }
    }
}