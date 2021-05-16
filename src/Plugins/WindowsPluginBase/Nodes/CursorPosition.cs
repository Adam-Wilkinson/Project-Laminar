using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using WindowsPluginBase.Window;

namespace WindowsPluginBase.Nodes
{
    public class CursorPosition : IFunctionNode
    {
        private readonly INodeField _XField = Constructor.NodeField("X").WithOutput<double>();
        private readonly INodeField _YField = Constructor.NodeField("Y").WithOutput<double>();

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return _XField;
                yield return _YField;
            }
        }

        public string NodeName { get; } = "Cursor Position";

        public void Evaluate()
        {
            Point currentPos = WindowHooks.CurrentCursorPosition();
            _XField.SetOutput(currentPos.X);
            _YField.SetOutput(currentPos.Y);
        }
    }
}
