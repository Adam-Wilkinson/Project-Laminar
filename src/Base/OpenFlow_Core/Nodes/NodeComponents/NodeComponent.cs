namespace OpenFlow_Core.Nodes.NodeComponents
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;
    using OpenFlow_PluginFramework.Primitives;

    public abstract class NodeComponent : INotifyPropertyChanged, INodeComponent
    {
        private bool _isVisible = true;
        private Action<INodeComponent> _removeAction;

        public NodeComponent(IOpacity opacity)
        {
            Opacity = opacity;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<bool> VisibilityChanged;

        public virtual INode ParentNode { get; set; }

        public abstract IList VisualComponentList { get; }

        public Action RemoveSelf { get; private set; }

        public IOpacity Opacity { get; }

        public virtual bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    VisibilityChanged?.Invoke(this, _isVisible);
                }
            }
        }

        public void SetRemoveAction(Action<INodeComponent> removeAction)
        {
            _removeAction = removeAction;
            RemoveSelf = () =>
            {
                removeAction(this);
                INodeBase.NodeBases[ParentNode].TryEvaluate();
            };
        }

        public abstract INodeComponent Clone();

        protected virtual INodeComponent CloneTo(INodeComponent component)
        {
            component.ParentNode = ParentNode;
            component.SetRemoveAction(_removeAction);
            component.Opacity.Value = Opacity.Value;

            return component;
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
