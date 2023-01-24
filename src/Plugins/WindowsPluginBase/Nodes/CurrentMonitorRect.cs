using Laminar.PluginFramework.NodeSystem;
using WindowsPluginBase.Window;

namespace WindowsPluginBase.Nodes;

public class CurrentMonitorRect : INode
{
    //private readonly INodeField _topField = Constructor.NodeField("Top").WithOutput<double>();
    //private readonly INodeField _leftField = Constructor.NodeField("Left").WithOutput<double>();
    //private readonly INodeField _widthField = Constructor.NodeField("Width").WithOutput(1920.0);
    //private readonly INodeField _heightField = Constructor.NodeField("Height").WithOutput(1080.0);

    public string NodeName { get; } = "Current Monitor Info";

    public void Evaluate()
    {
        // Rectangle monitorSize = WindowHooks.CurrentMonitorSize();
        //_topField.SetOutput((double)monitorSize.Rect.Top);
        //_leftField.SetOutput((double)monitorSize.Rect.Left);
        //_widthField.SetOutput((double)monitorSize.Rect.Right - monitorSize.Rect.Left);
        //_heightField.SetOutput((double)monitorSize.Rect.Bottom - monitorSize.Rect.Top);
    }
}
