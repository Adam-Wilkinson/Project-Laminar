using System;

namespace Laminar.PluginFramework.Exceptions;

public class CannotCreateServiceException<T> : Exception
{
    public CannotCreateServiceException() : base($"Cannot create service of type {typeof(T)}") { }
}
