namespace OpenFlow_Avalonia.NodeDisplaySystem
{
    using System;
    using System.Diagnostics;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.Primitives;
    using OpenFlow_Core.NodeSystem;
    using OpenFlow_Core.NodeSystem.Nodes;
    using OpenFlow_PluginFramework.Primitives;

    public class NodeDisplay : TemplatedControl
    {
        public static readonly StyledProperty<INodeBase> CoreNodeProperty = AvaloniaProperty.Register<NodeDisplay, INodeBase>(nameof(CoreNode));

        public NodeDisplay()
        {
            _ = this.GetObservable(CoreNodeProperty).Subscribe(newNode =>
              {
                  if (newNode != null)
                  {
                      newNode.ErrorState.PropertyChanged += (o, e) =>
                      {
                          IsError = (o as IObservableValue<bool>).Value;
                      };
                      newNode.Tag = this;
                  }
              });
        }

        public INodeBase CoreNode
        {
            get => GetValue(CoreNodeProperty);
            set => SetValue(CoreNodeProperty, value);
        }

        public bool Selected
        {
            get => Classes.Contains(":selected");
            set => PseudoClasses.Set(":selected", value);
        }

        public bool IsError
        {
            get => Classes.Contains(":error");
            set => PseudoClasses.Set(":error", value);
        }
    }
}
