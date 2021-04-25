using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Laminar_Core;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Laminar_Avalonia.NodeDisplaySystem
{
    public class NodeTreeInputDisplay : TemplatedControl
    {
        public static readonly StyledProperty<ObservableCollection<NodeDisplay>> InputNodesProperty = AvaloniaProperty.Register<NodeTreeInputDisplay, ObservableCollection<NodeDisplay>>(nameof(InputNodes));
        public static readonly StyledProperty<IEnumerable<TypeInfoRecord>> AllTypeInfoProperty = AvaloniaProperty.Register<NodeTreeInputDisplay, IEnumerable<TypeInfoRecord>>(nameof(AllTypeInfo));

        private ToggleButton _toggleAddMenuButton;
        private NodeDisplay _lastClickedDisplay;
        private Vector _dragOffset;

        public NodeTreeInputDisplay()
        {
            InputNodes = new();
            DataContextChanged += NodeTreeInputDisplay_DataContextChanged;

            AllTypeInfo = App.LaminarInstance.AllRegisteredTypes;
        }

        private void NodeTreeInputDisplay_DataContextChanged(object sender, EventArgs e)
        {
            if (DataContext is INodeTree nodeTree)
            {
                ((INotifyCollectionChanged)nodeTree.Inputs.InputNodes).CollectionChanged += NodeTreeInputDisplay_CollectionChanged;
                foreach (INodeContainer container in nodeTree.Inputs.InputNodes)
                {
                    AddNode(container);
                }
            }
        }

        private void NodeTreeInputDisplay_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object item in e.NewItems)
                    {
                        AddNode(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (object item in e.OldItems)
                    {
                        RemoveNode(item);
                    }
                    break;
            }
        }

        private void RemoveNode(object item)
        {
            InputNodes.Remove(item as NodeDisplay);
        }

        private void AddNode(object item)
        {
            NodeDisplay newNode = new() { CoreNode = item as INodeContainer };
            newNode.ShowConnectors = false;
            newNode.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            InputNodes.Add(newNode);
            newNode.PointerPressed += (o, e) =>
            {
                _lastClickedDisplay = new NodeDisplay() { CoreNode = item as INodeContainer };
                _dragOffset = e.GetPosition(newNode);
            };

            newNode.PointerReleased += (o, e) =>
            {
                _lastClickedDisplay = null;
            };

            newNode.CoreNode.IsLive = true;
            newNode.CoreNode.NameLabel.NeedsEditing = true;
        }

        public IEnumerable<TypeInfoRecord> AllTypeInfo
        {
            get => GetValue(AllTypeInfoProperty);
            set => SetValue(AllTypeInfoProperty, value);
        }

        public ObservableCollection<NodeDisplay> InputNodes
        {
            get => GetValue(InputNodesProperty);
            set => SetValue(InputNodesProperty, value);
        }

        public void AddInputOfType(Type type)
        {
            (DataContext as INodeTree).Inputs.Add(type);
            _toggleAddMenuButton.IsChecked = false;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _toggleAddMenuButton = e.NameScope.Find<ToggleButton>("PART_AddNodeButton");
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (_lastClickedDisplay is not null)
            {
                DragDropHandler.StartDrop(e, "NodeDisplay", _lastClickedDisplay, null, _dragOffset);
                _lastClickedDisplay = null;
            }
        }

        public record DisplayType(string HexColour, string TypeName);
    }
}
