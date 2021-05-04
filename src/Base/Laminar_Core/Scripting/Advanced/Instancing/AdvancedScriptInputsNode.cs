using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Collections;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Laminar_Core.Scripting.Advanced.Instancing
{
    public class AdvancedScriptInputsNode : INode
    {
        private readonly INodeComponentList AllInputs = Constructor.NodeComponentList();
        private IAdvancedScriptInstance _scriptInstance;

        public void SetInstance(IAdvancedScriptInstance instance)
        {
            _scriptInstance = instance;
            NodeName = instance.Script.Name.Value;
        }

        public void BindToInputs(IAdvancedScriptInputs inputs)
        {
            int index = 0;
            foreach (INodeContainer container in inputs.InputNodes)
            {
                RegisterContainer(container, index);
                index++;
            }
            ((INotifyCollectionChanged)inputs.InputNodes).CollectionChanged += AdvancedScriptInputsNode_CollectionChanged;
        }

        private void AdvancedScriptInputsNode_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    int i = 0;
                    foreach (object newContainer in e.NewItems)
                    {
                        RegisterContainer(newContainer as INodeContainer, e.NewStartingIndex + i);
                        i++;
                    }
                    break;
            }
        }

        private void RegisterContainer(INodeContainer container, int index)
        {
            INodeField newField = Constructor.NodeField(container.NameLabel.LabelText.Value).WithValue("display", ((InputNodeContainer<InputNode>)container).GetValue(null), true);
            container.NameLabel.LabelText.OnChange += s =>
            {
                newField.Name.Value = s;
            };
            newField.GetValue("display").OnChange += (o) =>
            {
                ((InputNodeContainer<InputNode>)container).SetValue(_scriptInstance, o);
            };
            AllInputs.Insert(index, newField);
        }

        public void ManualTriggerAll()
        {
            foreach (INodeField field in AllInputs)
            {
                field.GetValue("display").OnChange(field["display"]);
            }
        }

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return AllInputs;
            }
        }

        public string NodeName { get; private set; }
    }
}
