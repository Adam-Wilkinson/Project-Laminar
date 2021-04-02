namespace OpenFlow_Avalonia.NodeDisplaySystem
{
    using System.Collections.Generic;
    using System.Reactive;
    using Avalonia;
    using Avalonia.Controls.Primitives;
    using Avalonia.Controls.Shapes;
    using Avalonia.Input;
    using Avalonia.Layout;
    using OpenFlow_Core.Nodes;
    using ReactiveUI;

    public class NodePickerTabBar : TemplatedControl
    {
        public static readonly StyledProperty<List<NodeBase>> CurrentDisplayNodesProperty = AvaloniaProperty.Register<NodePickerTabBar, List<NodeBase>>(nameof(CurrentDisplayNodes));
        public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<NodePickerTabBar, Orientation>("Orientation", Orientation.Horizontal);

        public NodePickerTabBar()
        {
            CurrentDisplayNodes = App.Instance.LoadedNodeManager.LoadedNodes.FirstGroup();
            SelectItem = ReactiveCommand.Create<List<NodeBase>>(nodes =>
            {
                CurrentDisplayNodes = nodes;
            });
        }

        public Dictionary<string, LoadedNodeManager.NodeCatagories> Catagories => App.Instance.LoadedNodeManager.LoadedNodes.SubCatagories;

        public List<NodeBase> CurrentDisplayNodes
        {
            get => GetValue(CurrentDisplayNodesProperty);
            set => SetValue(CurrentDisplayNodesProperty, value);
        }

        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public ReactiveCommand<List<NodeBase>, Unit> SelectItem { get; }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e != null && e.Source is Rectangle sourceRect && sourceRect.Tag is NodeBase selectedNode)
            {
                DragDropHandler.StartDrop(e, "NodeDisplay", new NodeDisplay() { CoreNode = selectedNode }, null, e.GetPosition(sourceRect));
            }
        }
    }
}
