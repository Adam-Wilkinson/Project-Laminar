namespace Laminar_Avalonia.NodeDisplaySystem
{
    using System.Collections.Generic;
    using System.Reactive;
    using Avalonia;
    using Avalonia.Controls.Primitives;
    using Avalonia.Controls.Shapes;
    using Avalonia.Input;
    using Avalonia.Layout;
    using Laminar_Core.NodeSystem;
    using Laminar_Core.NodeSystem.Nodes;
    using ReactiveUI;

    public class NodePickerTabBar : TemplatedControl
    {
        public static readonly StyledProperty<List<INodeBase>> CurrentDisplayNodesProperty = AvaloniaProperty.Register<NodePickerTabBar, List<INodeBase>>(nameof(CurrentDisplayNodes));
        public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<NodePickerTabBar, Orientation>("Orientation", Orientation.Horizontal);
        public static readonly StyledProperty<Dictionary<string, LoadedNodeManager.NodeCatagories>> CatagoriesProperty = AvaloniaProperty.Register<NodePickerTabBar, Dictionary<string, LoadedNodeManager.NodeCatagories>>(nameof(Catagories));

        public NodePickerTabBar()
        {
            CurrentDisplayNodes = App.LaminarInstance.LoadedNodeManager.LoadedNodes.FirstGroup();
            Catagories = App.LaminarInstance.LoadedNodeManager.LoadedNodes.SubCatagories;
            SelectItem = ReactiveCommand.Create<List<INodeBase>>(nodes =>
            {
                CurrentDisplayNodes = nodes;
            });
        }

        public Dictionary<string, LoadedNodeManager.NodeCatagories> Catagories
        {
            get => GetValue(CatagoriesProperty);
            set => SetValue(CatagoriesProperty, value);
        }

        public List<INodeBase> CurrentDisplayNodes
        {
            get => GetValue(CurrentDisplayNodesProperty);
            set => SetValue(CurrentDisplayNodesProperty, value);
        }

        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public ReactiveCommand<List<INodeBase>, Unit> SelectItem { get; }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e != null && e.Source is Rectangle sourceRect && sourceRect.Tag is INodeBase selectedNode)
            {
                DragDropHandler.StartDrop(e, "NodeDisplay", new NodeDisplay() { CoreNode = selectedNode }, null, e.GetPosition(sourceRect));
            }
        }
    }
}
