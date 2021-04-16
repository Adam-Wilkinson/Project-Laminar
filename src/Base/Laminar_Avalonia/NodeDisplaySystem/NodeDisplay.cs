namespace Laminar_Avalonia.NodeDisplaySystem
{
    using System;
    using System.Diagnostics;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.Primitives;
    using Laminar_Core.NodeSystem;
    using Laminar_Core.NodeSystem.Nodes;
    using Laminar_PluginFramework.Primitives;

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
