namespace OpenFlow_Core.Nodes.VisualNodeComponentDisplays
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using OpenFlow_Core.Management.UserInterface;
    using OpenFlow_Core.Nodes.Connectors;
    using OpenFlow_Core.Nodes.NodeComponents.Visuals;
    using OpenFlow_Core.Primitives;
    using OpenFlow_PluginFramework.NodeSystem;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using OpenFlow_PluginFramework.Primitives;

    public class NodeFieldDisplay : VisualNodeComponentDisplay<NodeField>, INotifyPropertyChanged
    {
        public NodeFieldDisplay(NodeField childField, NodeBase parentNode) : base(parentNode, childField)
        {
            ChildComponent.ValueStoreChanged += BaseField_ValueStoreChanged;

            ChildComponent.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(NodeField.DisplayedValue))
                {
                    UpdateDisplayedValue();
                }
            };

            UIs = new UIManager();
            UpdateDisplayedValue();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Action RemoveSelf => ChildComponent.RemoveSelf;

        public ILaminarValue DisplayedValue => ChildComponent.DisplayedValue;

        public UIManager UIs { get; }

        protected override bool TryUpdateConnector(IConnector connector, ConnectionType connectionType, out IConnector newConnector)
        {
            if (base.TryUpdateConnector(connector, connectionType, out newConnector))
            {
                return true;
            }

            ILaminarValue relevantValue = connectionType == ConnectionType.Input ? ChildComponent.GetDisplayValue(INodeField.InputKey) : ChildComponent.GetDisplayValue(INodeField.OutputKey);
            if (relevantValue != null && connector is not ValueConnector)
            {
                newConnector = new ValueConnector(relevantValue, ParentNode, connectionType);
                return true;
            }

            newConnector = default;
            return false;
        }

        protected override HorizontalAlignment CalculateAlignment()
        {
            return
                InputConnector.Value is null && OutputConnector.Value is not null ? HorizontalAlignment.Right : (
                InputConnector.Value is not null && OutputConnector.Value is null ? HorizontalAlignment.Left : 
                HorizontalAlignment.Middle);
        }

        private void BaseField_ValueStoreChanged(object sender, object e)
        {
            if (e as string is INodeField.InputKey)
            {
                UpdateInput();
            }
            else if (e as string is INodeField.OutputKey)
            {
                UpdateOutput();
            }
        }

        private void UpdateDisplayedValue()
        {
            UIs.SetChildValue(DisplayedValue);
            DisplayedValue.Name = ChildComponent.Name;
            NotifyPropertyChanged(nameof(DisplayedValue));
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
