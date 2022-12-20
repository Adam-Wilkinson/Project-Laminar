using System;
using BasicFunctionality.Nodes.Flow;
using BasicFunctionality.Nodes.Maths.Arithmetic;
using BasicFunctionality.Nodes.Maths.Functions;
using BasicFunctionality.Nodes.StringOperations;
using Laminar_PluginFramework.Registration;
using BasicFunctionality.Nodes.Maths.Comparisons;
using Laminar_PluginFramework.Primitives;
using BasicFunctionality.Nodes.Triggers;
using Laminar_PluginFramework.UserInterfaces;

namespace BasicFunctionality;

public class PluginFront : IPlugin
{
    public Platforms Platforms { get; } = Platforms.Windows | Platforms.Mac | Platforms.Linux;

    public string PluginName { get; } = "Base plugin functionality";

    public string PluginDescription { get; } = "Implements all the base functionality for Project: Laminar";

    public void Register(IPluginHost host)
    {
        host.RegisterType<double>("#FF0000", "Number", 0.0, new NumberEntry(), new StringViewer { MaxStringLength = 6}, null);
        host.RegisterType<string>("#0000FF", "Text", "", new StringEditor(), new StringViewer(), null);
        host.RegisterType<bool>("#00FFFF", "Condition", false, new ToggleSwitch(), new StringViewer(), null);
        host.RegisterType<Action>("00FF00", "Button", null, new DefaultViewer(), new DefaultViewer(), null);

        host.AddNodeToMenu<ManualTriggerNode>("Triggers");
        host.AddNodeToMenu<NodeAdd, NodeDifference, NodeMultiply, NodeDivide, Round>("Number", "Arithmetic");
        host.AddNodeToMenu<NodeSine>("Number", "Functions");
        host.AddNodeToMenu<Equal>("Number", "Comparisons");
        host.AddNodeToMenu<Node_Join_Strings, CharacterCounter>("Text");
        host.AddNodeToMenu<FlowSwitch, WaitForTrigger>("Flow Control");
    }
}
