using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework.Primitives.TypeDefinition;
using System;

namespace Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals
{
    public interface INodeField : IVisualNodeComponent
    {
        public const string InputKey = "Input";
        public const string OutputKey = "Output";

        event EventHandler<object> AnyValueChanged;

        object this[object key] { get; set; }

        ILaminarValue DisplayedValue { get; }

        void AddValue(object key, object value, bool isUserEditable);

        void AddValue<T>(object key, bool isUserEditable);

        ILaminarValue GetValue(object key);
    }
}