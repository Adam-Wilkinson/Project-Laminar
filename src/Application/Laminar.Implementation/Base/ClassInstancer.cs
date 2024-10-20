using System;
using Laminar.Contracts.Base;

namespace Laminar.Implementation.Base;

internal class ClassInstancer : IClassInstancer
{
    private readonly IServiceProvider _serviceProvider;

    public ClassInstancer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T CreateInstance<T>() => (T)CreateInstance(typeof(T));

    public object CreateInstance(Type type)
    {
        foreach (var constructor in type.GetConstructors())
        {
            if (constructor.IsPublic)
            {
                int i = 0;
                var constructorParams = constructor.GetParameters();
                object[] paramValues = new object[constructorParams.Length];
                while (i < constructorParams.Length && _serviceProvider.GetService(constructorParams[i].ParameterType) is object paramValue)
                {
                    paramValues[i] = paramValue;
                    i++;
                }

                if (i == constructorParams.Length)
                {
                    return Activator.CreateInstance(type, paramValues);
                }
            }
        }

        return null;
    }
}
