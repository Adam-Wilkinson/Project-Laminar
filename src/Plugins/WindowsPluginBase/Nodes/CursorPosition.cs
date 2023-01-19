using System;
using System.Collections.Generic;
using Laminar.PluginFramework.NodeSystem;
using WindowsPluginBase.Window;

namespace WindowsPluginBase.Nodes;

public class CursorPosition : INode
{
    //private readonly INodeField _XField = Constructor.NodeField("X").WithOutput<double>();
    //private readonly INodeField _YField = Constructor.NodeField("Y").WithOutput<double>();

    public string NodeName { get; } = "Cursor Position";

    public void Evaluate()
    {
        Point currentPos = WindowHooks.CurrentCursorPosition();
        //_XField.SetOutput(Convert.ToDouble(currentPos.X));
        //_YField.SetOutput(Convert.ToDouble(currentPos.Y));
    }
}
