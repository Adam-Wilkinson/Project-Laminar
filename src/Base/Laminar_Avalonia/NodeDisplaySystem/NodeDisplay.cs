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
        public static readonly StyledProperty<INodeBase> CoreNodeProperty = AvaloniaProperty.Register<NodeDisplay, INodeBase>(nameof(CoreNode));
        public static readonly StyledProperty<bool> IsEditingNameProperty = AvaloniaProperty.Register<NodeDisplay, bool>(nameof(IsEditingName), false);
        public static readonly StyledProperty<bool> ShowConnectorsProperty = AvaloniaProperty.Register<NodeDisplay, bool>(nameof(ShowConnectors), true);

        private TextBox _nameEnterBox;

        public NodeDisplay()
        {
            ShowConnectors = true;
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

        public bool IsEditingName
        {
            get => GetValue(IsEditingNameProperty);
            set => SetValue(IsEditingNameProperty, value);
        }

        public INodeBase CoreNode
        {
            get => GetValue(CoreNodeProperty);
            set => SetValue(CoreNodeProperty, value);
        }

        public bool Selected
        {
            get => Classes.Contains(":selected");
            set
            {
                if (value is false)
                {
                    StopEditingName();
                }
                PseudoClasses.Set(":selected", value);
            }
        }

        public bool IsError
        {
            get => Classes.Contains(":error");
            set => PseudoClasses.Set(":error", value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            e.NameScope.Find<TextBlock>("PART_NameDisplay").DoubleTapped += delegate { StartEditingName(); };
            _nameEnterBox = e.NameScope.Find<TextBox>("PART_NameEditor");
            _nameEnterBox.LostFocus += delegate { StopEditingName(); };
            if (IsEditingName)
            {
                StartEditingName();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            NodeDisplay_KeyDown(this, e);
        }

        private void NodeDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter)
            {
                StopEditingName();
            };
        }

        private void StopEditingName()
        {
            IsEditingName = false;
        }

        public void StartEditingName()
        {
            IsEditingName = true;
            _nameEnterBox?.SelectAll();
            _nameEnterBox?.Focus();
        }
    }
}
