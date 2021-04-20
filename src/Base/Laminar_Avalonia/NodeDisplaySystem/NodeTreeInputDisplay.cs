using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Laminar_Core;
using Laminar_Core.NodeSystem.Nodes;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive;

namespace Laminar_Avalonia.NodeDisplaySystem
{
    public class NodeTreeInputDisplay : TemplatedControl
    {
        public static readonly StyledProperty<ObservableCollection<NodeDisplay>> InputNodesProperty = AvaloniaProperty.Register<NodeTreeInputDisplay, ObservableCollection<NodeDisplay>>(nameof(InputNodes));
        public static readonly StyledProperty<IEnumerable<TypeInfoRecord>> AllTypeInfoProperty = AvaloniaProperty.Register<NodeTreeInputDisplay, IEnumerable<TypeInfoRecord>>(nameof(AllTypeInfo));

        private ToggleButton _toggleAddMenuButton;

        public NodeTreeInputDisplay()
        {
            InputNodes = new();
            ((INotifyCollectionChanged)App.LaminarInstance.ActiveNodeTree.Value.Inputs.InputNodes).CollectionChanged += NodeTreeInputDisplay_CollectionChanged;
            AllTypeInfo = App.LaminarInstance.AllRegisteredTypes;
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
            NodeDisplay newNode = new() { CoreNode = item as INodeBase };
            newNode.ShowConnectors = false;
            newNode.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            InputNodes.Add(newNode);
            newNode.PointerPressed += (o, e) =>
            {
                DragDropHandler.StartDrop(e, "NodeDisplay", new NodeDisplay() { CoreNode = item as INodeBase }, null, e.GetPosition(newNode));
            };
            newNode.StartEditingName();
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
            App.LaminarInstance.ActiveNodeTree.Value.Inputs.Add(type);
            _toggleAddMenuButton.IsChecked = false;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _toggleAddMenuButton = e.NameScope.Find<ToggleButton>("PART_AddNodeButton");
        }

        public record DisplayType(string HexColour, string TypeName);
    }
}
