using System.Collections.Generic;
using Laminar.PluginFramework.NodeSystem;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;
public class NodeAdd : INode
{
    public string NodeName { get; } = "Add";


    public void Evaluate()
    {
        //double total = 0;

        //foreach (INodeField field in addFields)
        //{
        //    total += field.GetInput<double>();
        //}

        //totalField.SetOutput(total);
    }
}
