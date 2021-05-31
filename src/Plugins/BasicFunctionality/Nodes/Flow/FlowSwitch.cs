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
        private readonly INodeField valueInput = Constructor.NodeField("Switch Value").WithInput(Constructor.TypeDefinitionManager());

        private readonly INodeLabel OutputsLabel = Constructor.NodeLabel("Possible Values");

        private readonly INodeLabel defaultOutput = Constructor.NodeLabel("Default").WithFlowOutput();

        private readonly INodeComponentDictionary flowOutputs = Constructor.NodeComponentDictionary().WithElement(
                typeof(bool),
                Constructor.NodeComponentList(
                    Constructor.NodeField("True").WithValue("Displayed", Constructor.TypeDefinition(true, null, "DefaultDisplay"), false).WithFlowOutput(),
                    Constructor.NodeField("False").WithValue("Displayed", Constructor.TypeDefinition(false, null, "DefaultDisplay"), false).WithFlowOutput()));

        public FlowSwitch()
        {
            valueInput.GetValue(INodeField.InputKey).TypeDefinitionChanged += FlowSwitch_TypeDefinitionChanged;
        }

        public string NodeName => "Switch";

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return valueInput;
                yield return Constructor.Separator();
                yield return OutputsLabel;
                yield return Constructor.Separator();
                yield return flowOutputs;
            }
        }

        public void Evaluate()
        {
            Debug.WriteLine("Flow switch hit");
            foreach (IVisualNodeComponent field in flowOutputs.VisualComponentList)
            {
                if (field is INodeField valueField && valueField.DisplayedValue != null && valueField.DisplayedValue.Value.Equals(valueInput.GetInput()))
                {
                    field.FlowOutput.Activate();
                    Debug.WriteLine($"Flow output {field.Name} activated");
                    return;
                }
            }

            defaultOutput.FlowOutput.Activate();
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
            INodeField output = Constructor.NodeField(x.ToString()).WithValue("Displayed", Constructor.RigidTypeDefinitionManager(typeDef.DefaultValue, null, "DefaultDisplay"), false).WithFlowOutput();
            output.DisplayedValue.Value = x;
            return output;
        }
    }
}
