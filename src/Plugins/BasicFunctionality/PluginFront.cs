using System;
using BasicFunctionality.Nodes.Flow;
using BasicFunctionality.Nodes.Maths.Arithmetic;
using BasicFunctionality.Nodes.Maths.Comparisons;
using BasicFunctionality.Nodes.Maths.Functions;
using BasicFunctionality.Nodes.StringOperations;
using BasicFunctionality.Nodes.Triggers;
using Laminar.PluginFramework.Registration;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace BasicFunctionality;

public class PluginFront : IPlugin
{
    public Platforms Platforms { get; } = Platforms.Windows | Platforms.Mac | Platforms.Linux;

    public string PluginName { get; } = "Base plugin functionality";

    public string PluginDescription { get; } = "Implements all the base functionality for Project: Laminar";

    public void Register(IPluginHost host)
    {
        host.RegisterType<double>("#FF0000", "Number", 0.0, new NumberEntry(), new StringViewer { MaxStringLength = 6 }, null);
        host.RegisterType<string>("#0000FF", "Text", "", new StringEditor(), new StringViewer(), null);
        host.RegisterType<bool>("#00FFFF", "Condition", false, new ToggleSwitch(), new StringViewer(), null);
        host.RegisterType<Action>("00FF00", "Button", null, new DefaultViewer(), new DefaultViewer(), null);

        host.AddNodeToMenu<ManualTriggerNode>("Triggers");
        host.AddNodeToMenu<AddNode, DifferenceNode, MultiplyNode, DivideNode, RoundNode, SliderTestNode>("Number", "Arithmetic");
        host.AddNodeToMenu<SineNode>("Number", "Functions");
        host.AddNodeToMenu<EqualsNode>("Number", "Comparisons");
        host.AddNodeToMenu<JoinStringsNode, CharacterCounterNode>("Text");
        host.AddNodeToMenu<FlowSwitchNode, WaitForTriggerNode>("Flow Control");
    }
}
