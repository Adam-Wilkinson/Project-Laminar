namespace BasicFunctionality.Nodes.Maths.Arithmetic;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Laminar.PluginFramework.NodeSystem;
using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;

public class NodeDivide : IFunctionNode
{
    public string NodeName => "Divide";

    public ValueInput<double> FirstNumber = new("Numerator", 0.0);
    public ValueInput<double> SecondNumber = new("Denominator", 1.0);
    public ValueOutput<double> ResultNumber = new("Result", 0.0);

    public IEnumerable<INodeComponent> Fields
    {
        get
        {
            yield return null;
        }
    }

    public void Evaluate()
    {
        ResultNumber.Value = FirstNumber / SecondNumber;
    }
}
