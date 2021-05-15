namespace Laminar_Core.NodeSystem.NodeComponents
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Laminar_PluginFramework.NodeSystem.NodeComponents;
    using Laminar_PluginFramework.NodeSystem.Nodes;
    using Laminar_PluginFramework.Primitives;

    public abstract class NodeComponent : INotifyPropertyChanged, INodeComponent
    {
        private bool _isVisible = true;

        public NodeComponent(IOpacity opacity)
        {
            Opacity = opacity;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<bool> VisibilityChanged;

        public virtual INode ParentNode { get; set; }

        public abstract IList VisualComponentList { get; }

        public Action<INodeComponent> RemoveAction { get; set; }

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

        public abstract INodeComponent Clone();

        public virtual void CloneTo(INodeComponent component)
        {
            component.ParentNode = ParentNode;
            component.RemoveAction = RemoveAction;
            component.Opacity.Value = Opacity.Value;
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
