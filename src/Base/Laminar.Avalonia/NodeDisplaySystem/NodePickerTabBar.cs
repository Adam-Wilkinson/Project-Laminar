using System.Collections.Generic;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Laminar.Contracts.NodeSystem;
using Laminar.Domain;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace Laminar.Avalonia.NodeDisplaySystem
{
    public class NodePickerTabBar : TemplatedControl
    {
        private NodeControlManager _nodeControlManager = new();
        private ILoadedNodeManager _nodeManager;

        public static readonly StyledProperty<IReadOnlyList<INodeWrapper>> CurrentDisplayNodesProperty = AvaloniaProperty.Register<NodePickerTabBar, IReadOnlyList<INodeWrapper>>(nameof(CurrentDisplayNodes));
        public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<NodePickerTabBar, Orientation>("Orientation", Orientation.Horizontal);
        public static readonly StyledProperty<IReadOnlyList<ItemCatagory<INodeWrapper>>> CatagoriesProperty = AvaloniaProperty.Register<NodePickerTabBar, IReadOnlyList<ItemCatagory<INodeWrapper>>>(nameof(Catagories));

        public NodePickerTabBar()
        {
            _nodeManager = App.LaminarInstance.ServiceProvider.GetService<ILoadedNodeManager>();
            Catagories = _nodeManager.LoadedNodes.SubCatagories;
            CurrentDisplayNodes = Catagories[0].Items;
            SelectItem = ReactiveCommand.Create<List<INodeWrapper>>(nodes =>
            {
                CurrentDisplayNodes = nodes;
            });
        }

        public IReadOnlyList<ItemCatagory<INodeWrapper>> Catagories
        {
            get => GetValue(CatagoriesProperty);
            set => SetValue(CatagoriesProperty, value);
        }

        public IReadOnlyList<INodeWrapper> CurrentDisplayNodes
        {
            get => GetValue(CurrentDisplayNodesProperty);
            set => SetValue(CurrentDisplayNodesProperty, value);
        }

        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public ReactiveCommand<List<INodeWrapper>, Unit> SelectItem { get; }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e != null && e.Source is Panel sourceRect && sourceRect.Tag is INodeWrapper selectedNode)
            {
                INodeWrapper newNode = selectedNode.Clone(null);
                DragDropHandler.StartDrop(e, "NodeDisplay", _nodeControlManager.GetControl(newNode), newNode, e.GetPosition(sourceRect));
            }
        }
    }
}
