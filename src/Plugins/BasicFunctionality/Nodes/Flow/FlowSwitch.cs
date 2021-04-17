namespace Laminar_Inbuilt.Nodes.Flow
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using Laminar_PluginFramework.NodeSystem.NodeComponents;
    using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using Laminar_PluginFramework.NodeSystem.NodeComponents.Collections;
    using Laminar_PluginFramework.NodeSystem.Nodes;
    using Laminar_PluginFramework.Primitives;
    using Laminar_PluginFramework.Primitives.TypeDefinition;
    using Laminar_PluginFramework;

    public class FlowSwitch : IFlowNode
    {
        private readonly INodeLabel flowInput = Constructor.NodeLabel("Flow Input").WithFlowInput();

        private readonly INodeField valueInput = Constructor.NodeField("Switch Value").WithInput(Constructor.TypeDefinitionManager());

        private readonly INodeLabel OutputsLabel = Constructor.NodeLabel("Possible Values");

        private readonly INodeLabel defaultOutput = Constructor.NodeLabel("Default").WithFlowOutput();

        private readonly INodeComponentDictionary flowOutputs = Constructor.NodeComponentDictionary().WithElement(
                typeof(bool),
                Constructor.NodeComponentList(
                    Constructor.NodeField("True").WithValue("Displayed", Constructor.ManualTypeDefinitionManager().WithAcceptedDefinition<bool>(true, "DefaultDisplay"), false).WithFlowOutput(),
                    Constructor.NodeField("False").WithValue("Displayed", Constructor.ManualTypeDefinitionManager().WithAcceptedDefinition<bool>(false, "DefaultDisplay"), false).WithFlowOutput()));

        public FlowSwitch()
        {
            valueInput.GetValue(INodeField.InputKey).TypeDefinitionChanged += FlowSwitch_TypeDefinitionChanged;
        }

        public string NodeName => "Switch";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return flowInput;
                yield return valueInput;
                yield return Constructor.Separator();
                yield return OutputsLabel;
                yield return Constructor.Separator();
                yield return flowOutputs;
            }
        }

        public IVisualNodeComponent FlowOutComponent { get; private set; }

        public void Evaluate()
        {
            foreach (IVisualNodeComponent field in flowOutputs.VisualComponentList)
            {
                if (field is INodeField valueField && valueField.DisplayedValue != null && valueField.DisplayedValue.Value.Equals(valueInput.GetInput()))
                {
                    FlowOutComponent = valueField;
                    return;
                }
            }

            FlowOutComponent = defaultOutput;
        }

        private void FlowSwitch_TypeDefinitionChanged(object sender, ITypeDefinition typeDef)
        {
            flowOutputs.HideAllComponents();
            if (!flowOutputs.ContainsKey(typeDef.ValueType))
            {
                flowOutputs.Add(typeDef.ValueType, GenerateSwitchesFor(typeDef));
            }

            flowOutputs.ShowSectionByKey(typeDef.ValueType);
        }

        private INodeComponent GenerateSwitchesFor(ITypeDefinition typeDef)
        {
            if (typeDef.ValueType.IsEnum)
            {
                return Constructor.NodeComponentList(Enum.GetNames(typeDef.ValueType).Select(x => ValueDisplay(typeDef, Enum.Parse(typeDef.ValueType, x))));
            }

            return Constructor.NodeComponentList(
                Constructor.NodeComponentAutoCloner(Constructor.NodeField("Case").WithInput(typeDef).WithFlowOutput(), 0, (x) => $"Case {x + 1}"),
                defaultOutput
            );
        }

        private static INodeField ValueDisplay(ITypeDefinition typeDef, object x)
        {
            INodeField output = Constructor.NodeField(x.ToString()).WithValue("Displayed", Constructor.RigidTypeDefinitionManager(typeDef.DefaultValue, null, "DefaultDisplay"), false);
            output.DisplayedValue.Value = x;
            output.SetFlowOutput();
            return output;
        }
    }
}
