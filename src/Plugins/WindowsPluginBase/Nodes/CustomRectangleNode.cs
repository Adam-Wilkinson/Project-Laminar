using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsPluginBase.Window;

namespace WindowsPluginBase.Nodes
{
    class CustomRectangleNode : IFunctionNode
    {
        private readonly INodeField _outputField = Constructor.NodeField("Output").WithOutput<Rectangle>();
        private readonly INodeField _leftField = Constructor.NodeField("Left").WithInput<double>();
        private readonly INodeField _topField = Constructor.NodeField("Top").WithInput<double>();
        private readonly INodeField _widthField = Constructor.NodeField("Width").WithInput<double>();
        private readonly INodeField _heightField = Constructor.NodeField("Height").WithInput<double>();

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return _outputField;
                yield return _leftField;
                yield return _topField;
                yield return _widthField;
                yield return _heightField;
            }
        }

        public string NodeName { get; } = "Custom Rectangle";

        public void Evaluate()
        {
            _outputField.SetOutput(new Rectangle
            {
                Rect = new RECT
                {
                    Left = (int)_leftField.GetInput<double>(),
                    Top = (int)_topField.GetInput<double>(),
                    Right = (int)_leftField.GetInput<double>() + (int)_widthField.GetInput<double>(),
                    Bottom = (int)_topField.GetInput<double>() + (int)_heightField.GetInput<double>(),
                }
            });
        }
    }
}
