using System;
using Laminar_Inbuilt.Nodes.Flow;
using Laminar_Inbuilt.Nodes.Maths.Arithmetic;
using Laminar_Inbuilt.Nodes.Maths.Functions;
using Laminar_Inbuilt.Nodes.StringOperations;
using Laminar_PluginFramework.Registration;
using Laminar_Inbuilt.Nodes.Maths.Comparisons;
using Avalonia.Controls;
using Laminar_Inbuilt.UserControls;
using Laminar_PluginFramework.Primitives;
using BasicFunctionality.Nodes.Flow;
using Laminar_Inbuilt.Nodes.Triggers;

namespace Laminar_Inbuilt
{
    public class Plugin_Front : IPlugin
    {
        public Platforms Platforms { get; } = Platforms.Windows | Platforms.Mac | Platforms.Linux;

        public string PluginName { get; } = "Base plugin functionality";

        public string PluginDescription { get; } = "Implements all the base functionality for Project: Laminar";

        public void Register(IPluginHost host)
        {
            host.RegisterType<double>("#FF0000", "Number", 0.0, "NumberEditor", "StringDisplay", true);
            host.RegisterType<string>("#0000FF", "Text", "", "StringEditor", "StringDisplay", true);
            host.RegisterType<bool>("#00FFFF", "Condition", false, "BoolEditor", "StringDisplay", true);
            host.RegisterType<Action>("00FF00", "Button", null, "DefaultDisplay", "ActionDisplay", false);

            host.RegisterDisplay<IControl, DefaultDisplay>("DefaultDisplay");
            host.RegisterDisplay<IControl, StringDisplay>("StringDisplay");
            host.RegisterDisplay<IControl, ActionDisplay>("ActionDisplay");
            host.RegisterEditor<IControl, NumberEditor>("NumberEditor");
            host.RegisterEditor<IControl, StringEditor>("StringEditor");
            host.RegisterEditor<IControl, EnumEditor>("EnumEditor");
            host.RegisterEditor<IControl, SliderEditor>("SliderEditor");
            host.RegisterEditor<IControl, BoolEditor>("BoolEditor");
            host.RegisterEditor<IControl, UserControls.ToggleSwitch>("ToggleSwitch");
            host.RegisterEditor<IControl, UserControls.CheckBox>("CheckBox");

            host.AddNodeToMenu<ManualTriggerNode>("Triggers");
            host.AddNodeToMenu<NodeAdd, NodeDifference, NodeMultiply, NodeDivide, Round>("Number", "Arithmetic");
            host.AddNodeToMenu<NodeSine>("Number", "Functions");
            host.AddNodeToMenu<Equal>("Number", "Comparisons");
            host.AddNodeToMenu<Node_Join_Strings, CharacterCounter>("Text");
            host.AddNodeToMenu<FlowSwitch, WaitForTrigger>("Flow Control");
        }
    }
}
