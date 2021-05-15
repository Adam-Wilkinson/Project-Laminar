﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Laminar_Core;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.NodeSystem;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using Laminar_Core.Scripting.Advanced;
using Laminar_Core.Scripting.Advanced.Editing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Laminar_Avalonia.NodeDisplaySystem
{
    public class AdvancedScriptInputsDisplay : TemplatedControl
    {
        public static readonly StyledProperty<ObservableCollection<NodeDisplay>> InputNodesProperty = AvaloniaProperty.Register<AdvancedScriptInputsDisplay, ObservableCollection<NodeDisplay>>(nameof(InputNodes));
        public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<AdvancedScriptInputsDisplay, Orientation>(nameof(Orientation), Orientation.Vertical);
        public static readonly StyledProperty<IEnumerable<TypeInfoRecord>> AllTypeInfoProperty = AvaloniaProperty.Register<AdvancedScriptInputsDisplay, IEnumerable<TypeInfoRecord>>(nameof(AllTypeInfo));

        private readonly INodeFactory _nodeFactory;

        private ToggleButton _toggleAddMenuButton;
        private NodeDisplay _lastClickedDisplay;
        private Vector _dragOffset;

        public AdvancedScriptInputsDisplay()
        {
            _nodeFactory = App.LaminarInstance.Factory.GetImplementation<INodeFactory>();
            InputNodes = new();
            DataContextChanged += NodeTreeInputDisplay_DataContextChanged;

            AllTypeInfo = App.LaminarInstance.AllRegisteredTypes;
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

        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public void AddInputOfType(Type type)
        {
            (DataContext as IAdvancedScript).Inputs.Add(type);
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

        private void RemoveNode(object item)
        {
            InputNodes.Remove(item as NodeDisplay);
        }

        private void AddNode(object item)
        {
            if (item is InputNode inputNode)
            {
                INodeContainer nodeContainer = _nodeFactory.Get(inputNode);
                NodeDisplay newNode = new() { CoreNode = nodeContainer };
                InputNodes.Add(newNode);
                newNode.PointerPressed += (o, e) =>
                {
                    _lastClickedDisplay = new NodeDisplay() { CoreNode = nodeContainer };
                    _dragOffset = e.GetPosition(newNode);
                };

                newNode.PointerReleased += (o, e) =>
                {
                    _lastClickedDisplay = null;
                };

                newNode.CoreNode.IsLive = true;
                newNode.CoreNode.CoreNode.GetNameLabel().NeedsEditing = true;
            }
        }

        private void NodeTreeInputDisplay_DataContextChanged(object sender, EventArgs e)
        {
            if (DataContext is IAdvancedScript nodeTree)
            {
                ((INotifyCollectionChanged)nodeTree.Inputs.InputNodes).CollectionChanged += NodeTreeInputDisplay_CollectionChanged;
                foreach (InputNode inputNode in nodeTree.Inputs.InputNodes)
                {
                    AddNode(inputNode);
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
    }
}
