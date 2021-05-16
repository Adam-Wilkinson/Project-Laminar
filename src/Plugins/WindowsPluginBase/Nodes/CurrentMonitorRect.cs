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
    public class CurrentMonitorRect : IFunctionNode
    {
        private readonly INodeField _topField = Constructor.NodeField("Top").WithOutput<double>();
        private readonly INodeField _leftField = Constructor.NodeField("Left").WithOutput<double>();
        private readonly INodeField _widthField = Constructor.NodeField("Width").WithOutput<double>();
        private readonly INodeField _heightField = Constructor.NodeField("Height").WithOutput<double>();

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return _widthField;
                yield return _heightField;
                yield return _topField;
                yield return _leftField;
            }
        }

        public string NodeName { get; } = "Current Monitor Info";

        public void Evaluate()
        {
            Rectangle monitorSize = WindowHooks.CurrentMonitorSize();
            _topField.SetOutput((double)monitorSize.Rect.Top);
            _leftField.SetOutput((double)monitorSize.Rect.Left);
            _widthField.SetOutput((double)monitorSize.Rect.Right - monitorSize.Rect.Left);
            _heightField.SetOutput((double)monitorSize.Rect.Bottom - monitorSize.Rect.Top);
        }
    }
}
