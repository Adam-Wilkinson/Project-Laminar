namespace Laminar_Core.NodeSystem.NodeTreeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Laminar_PluginFramework.NodeSystem;
    using Laminar_PluginFramework.NodeSystem.Nodes;

    public class TypeConverter
    {
        private static readonly Dictionary<TwoTypes, Type> Converters = new();

        public static bool TryAddConverter<T1, T2, TNode>()
            where TNode : IFunctionNode
        {
            if (Converters.ContainsKey(new TwoTypes(typeof(T1), typeof(T2))))
            {
                return false;
            }

            Converters.Add(new TwoTypes(typeof(T1), typeof(T2)), typeof(TNode));
            return true;
        }

        public static bool TryGetConverter(Type inputType, Type outputType, out Type converterType)
        {
            bool result = Converters.TryGetValue(new TwoTypes(inputType, outputType), out Type converter);
            converterType = converter;
            return result;
        }

        private record TwoTypes(Type T1, Type T2);
    }
}
