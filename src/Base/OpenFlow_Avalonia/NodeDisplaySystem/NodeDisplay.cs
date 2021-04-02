namespace OpenFlow_Avalonia.NodeDisplaySystem
{
    using System;
    using System.Diagnostics;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.Primitives;
    using OpenFlow_Core.Nodes;

    public class NodeDisplay : TemplatedControl
    {
        public static readonly StyledProperty<NodeBase> CoreNodeProperty = AvaloniaProperty.Register<NodeDisplay, NodeBase>(nameof(CoreNode));

        public NodeDisplay()
        {
            _ = this.GetObservable(CoreNodeProperty).Subscribe(newNode =>
              {
                  if (newNode != null)
                  {
                      newNode.ErrorStateChanged += (o, e) =>
                      {
                          IsError = e;
                      };
                      newNode.Tag = this;
                  }
              });
        }

        public NodeBase CoreNode
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
