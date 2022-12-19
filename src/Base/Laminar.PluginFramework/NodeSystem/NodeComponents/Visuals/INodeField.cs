using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;

namespace Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals
{
    public interface INodeField : IVisualNodeComponent
    {
        public const string InputKey = "Input";
        public const string OutputKey = "Output";

        event EventHandler<object> AnyValueChanged;

        object this[object key] { get; set; }

        object DisplayedValueKey { get; set; }

        ILaminarValue DisplayedValue { get; }

        IEnumerable<KeyValuePair<object, ILaminarValue>> AllValues { get; }

        void AddValue(object key, object value, bool isUserEditable);

        void AddValue<T>(object key, bool isUserEditable);

        void AddValue(object key, ILaminarValue value);

        ILaminarValue GetValue(object key);
    }
}