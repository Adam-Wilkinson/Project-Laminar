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

namespace Laminar_Inbuilt
{
    public class Plugin_Front : IPlugin
    {
        public Platforms Platforms { get; } = Platforms.Windows | Platforms.Mac | Platforms.Linux;

        public string PluginName { get; } = "Base plugin functionality";

        public string PluginDescription { get; } = "Implements all the base functionality for Laminar";

        public void Dispose()
        {
        }

        public void Register(IPluginHost host)
        {
            host.RegisterType<double>("#FF0000", "Number", 0.0, "NumberEditor", "StringDisplay", true);
            host.RegisterType<string>("#0000FF", "Text", "", "StringEditor", "StringDisplay", true);
            host.RegisterType<bool>("#00FFFF", "Condition", false, "BoolEditor", "StringDisplay", true);
            host.RegisterType<Action>("00FF00", "Button", null, "DefaultDisplay", "ActionDisplay", false);

            host.RegisterDisplay<IControl>("DefaultDisplay", typeof(DefaultDisplay));
            host.RegisterDisplay<IControl>("StringDisplay", typeof(StringDisplay));
            host.RegisterDisplay<IControl>("ActionDisplay", typeof(ActionDisplay));
            host.RegisterEditor<IControl>("NumberEditor", typeof(NumberEditor));
            host.RegisterEditor<IControl>("StringEditor", typeof(StringEditor));
            host.RegisterEditor<IControl>("EnumEditor", typeof(EnumEditor));
            host.RegisterEditor<IControl>("SliderEditor", typeof(SliderEditor));
            host.RegisterEditor<IControl>("BoolEditor", typeof(BoolEditor));
            host.RegisterEditor<IControl>("ToggleSwitch", typeof(UserControls.ToggleSwitch));


            host.AddNodeToMenu<NodeAdd, NodeDifference, NodeMultiply, NodeDivide, SliderTest>("Number", "Arithmetic");
            host.AddNodeToMenu<NodeSine>("Number", "Functions");
            host.AddNodeToMenu<Equal>("Number", "Comparisons");
            host.AddNodeToMenu<Node_Join_Strings, CharacterCounter>("Text");
            host.AddNodeToMenu<FlowSwitch>("Flow Control");
        }
    }
}
