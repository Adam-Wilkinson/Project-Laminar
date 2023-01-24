namespace BasicFunctionality.Nodes.Flow;
using Laminar.PluginFramework.NodeSystem;

public class FlowSwitch : INode
{
    public string NodeName => "Switch";

    public void Evaluate()
    {
        //Debug.WriteLine("Flow switch hit");
        //foreach (IVisualNodeComponent field in flowOutputs.VisualComponentList)
        //{
        //    if (field is INodeField valueField && valueField.DisplayedValue != null && valueField.DisplayedValue.Value.Equals(valueInput.GetInput()))
        //    {
        //        field.FlowOutput.Activate();
        //        Debug.WriteLine($"Flow output {field.Name} activated");
        //        return;
        //    }
        //}

        //defaultOutput.FlowOutput.Activate();
    }

    //private void FlowSwitch_TypeDefinitionChanged(object sender, ITypeDefinition typeDef)
    //{
    //    flowOutputs.HideAllComponents();
    //    if (!flowOutputs.ContainsKey(typeDef.ValueType))
    //    {
    //        flowOutputs.Add(typeDef.ValueType, GenerateSwitchesFor(typeDef));
    //    }

    //    flowOutputs.ShowSectionByKey(typeDef.ValueType);
    //}

    //private INodeComponent GenerateSwitchesFor(ITypeDefinition typeDef)
    //{
    //    if (typeDef.ValueType.IsEnum)
    //    {
    //        return Constructor.NodeComponentList(Enum.GetNames(typeDef.ValueType).Select(x => ValueDisplay(typeDef, Enum.Parse(typeDef.ValueType, x))));
    //    }

    //    return Constructor.NodeComponentList(
    //        Constructor.NodeComponentAutoCloner(Constructor.NodeField("Case").WithInput(typeDef).WithFlowOutput(), 0, (x) => $"Case {x + 1}"),
    //        defaultOutput
    //    );
    //}

    //private static INodeField ValueDisplay(ITypeDefinition typeDef, object x)
    //{
    //    INodeField output = Constructor.NodeField(x.ToString()).WithValue("Displayed", Constructor.RigidTypeDefinitionManager(typeDef.DefaultValue, null, "DefaultDisplay"), false).WithFlowOutput();
    //    output.DisplayedValue.Value = x;
    //    return output;
    //}
}
