using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Laminar_Core.NodeSystem.Nodes;

namespace Laminar_Avalonia.NodeDisplaySystem
{
    public class NodeDisplay : TemplatedControl
    {
        public static readonly StyledProperty<INodeContainer> CoreNodeProperty = AvaloniaProperty.Register<NodeDisplay, INodeContainer>(nameof(CoreNode));
        public static readonly StyledProperty<bool> ShowConnectorsProperty = AvaloniaProperty.Register<NodeDisplay, bool>(nameof(ShowConnectors), true);

        public NodeDisplay()
        {
            CoreNode = App.LaminarInstance.LoadedNodeManager.LoadedNodes.FirstGroup()[0];
            this.GetObservable(CoreNodeProperty).Subscribe(newNode =>
            {
                if (newNode is not null)
                {
                    newNode.ErrorState.OnChange += (error) =>
                    {
                        IsError = error;
                    };
                }
            });
        }

        public bool ShowConnectors
        {
            get => GetValue(ShowConnectorsProperty);
            set => SetValue(ShowConnectorsProperty, value);
        }

        public INodeContainer CoreNode
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
