using System.Collections.Generic;
using Laminar.PluginFramework.NodeSystem;
using WindowsPluginBase.Window;

namespace WindowsPluginBase.Nodes;

public class SetWindowLayout : INode
{
    // private readonly INodeField _layoutInput = Constructor.NodeField("Layout").WithInput<AllWindowsLayout>();

    public string NodeName { get; } = "Set Window Layout";

    public void Evaluate()
    {
        // AllWindowsLayout.SetCurrentLayout(_layoutInput.GetInput<AllWindowsLayout>());
    }
}
