using OpenFlow_PluginFramework.Primitives;
using OpenFlow_PluginFramework.Primitives.TypeDefinition;
using System;

namespace OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals
{
    public interface INodeField : IVisualNodeComponent
    {
        public const string InputKey = "Input";
        public const string OutputKey = "Output";

        event EventHandler<object> AnyValueChanged;
        event EventHandler<object> ValueStoreChanged;

        object this[object key] { get; set; }

        ILaminarValue DisplayedValue { get; }

        object Input { get; set; }

        object Output { get; set; }

        void AddValue(object key, ILaminarValue value);

        ILaminarValue GetDisplayValue(object key);
    }
}