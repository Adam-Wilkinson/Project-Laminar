using System;
using System.Collections.Generic;

namespace Laminar_PluginFramework.Primitives
{
    public interface IValueConstraint<T>
    {
        Func<T, T> MyFunc { get; set; }

        Func<T, T> TotalFunc { get; }

        void AddToEndOfChain(IValueConstraint<T> constraints);
    }
}