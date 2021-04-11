using System;
using OpenFlow_Inbuilt.Nodes.Flow;
using OpenFlow_Inbuilt.Nodes.Maths.Arithmetic;
using OpenFlow_Inbuilt.Nodes.Maths.Functions;
using OpenFlow_Inbuilt.Nodes.StringOperations;
using OpenFlow_PluginFramework.Registration;
using OpenFlow_Inbuilt.Nodes.Maths.Comparisons;
using OpenFlow_Inbuilt.Nodes.Input.MouseInput;
using Avalonia.Controls;
using OpenFlow_Inbuilt.UserControls;
using OpenFlow_PluginFramework.Primitives;

namespace OpenFlow_Inbuilt
{
    public class Plugin_Front : IPlugin
    {
        public Platforms Platforms { get; } = Platforms.Windows | Platforms.Mac | Platforms.Linux;

        public string PluginName { get; } = "Base plugin functionality";

        public string PluginDescription { get; } = "Implements all the base functionality for Laminar";

        public void Register(IPluginHost host)
        {
            host.RegisterType<double>("#FF0000", "Number", 0.0, "NumberEditor", "StringDisplay");
            host.RegisterType<string>("#0000FF", "Text", "", "StringEditor", "StringDisplay");
            host.RegisterType<bool>("#00FFFF", "Condition", false, "StringDisplay", "StringDisplay");
            host.RegisterType<Action>("00FF00", "Button", null, "DefaultDisplay", "ActionDisplay");
            host.RegisterType<MouseButtonEnum>("#FFFF00", "Mouse Button", MouseButtonEnum.LeftButton, "EnumEditor", "StringDisplay");

            host.RegisterDisplay<IControl>("DefaultDisplay", typeof(DefaultDisplay));
            host.RegisterDisplay<IControl>("StringDisplay", typeof(StringDisplay));
            host.RegisterDisplay<IControl>("ActionDisplay", typeof(ActionDisplay));
            host.RegisterEditor<IControl>("NumberEditor", typeof(NumberEditor));
            host.RegisterEditor<IControl>("StringEditor", typeof(StringEditor));
            host.RegisterEditor<IControl>("EnumEditor", typeof(EnumEditor));
            host.RegisterEditor<IControl>("SliderEditor", typeof(SliderEditor));


            host.AddNodeToMenu<NodeAdd, NodeDifference, NodeMultiply, NodeDivide, SliderTest>("Number", "Arithmetic");
            host.AddNodeToMenu<NodeSine>("Number", "Functions");
            host.AddNodeToMenu<Equal>("Number", "Comparisons");
            host.AddNodeToMenu<Node_Join_Strings>("Text");
            host.AddNodeToMenu<FlowSwitch>("Flow Control");
            host.AddNodeToMenu<MouseButton>("Input", "Mouse");
        }
    }
}
