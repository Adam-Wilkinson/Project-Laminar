using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;
using System;

namespace Laminar_Core.Primitives
{
    public class Flow : IFlow
    {
        private bool _exists = false;

        public bool Exists
        {
            get => _exists;
            set
            {
                if (value != _exists)
                {
                    _exists = value;
                    ExistsChanged?.Invoke(this, _exists);
                }
            }
        }

        public IVisualNodeComponent ParentComponent { get; set; }

        public event EventHandler<bool> ExistsChanged;
        public event EventHandler Activated;

        public void Activate()
        {
            if (Exists)
            {
                Activated?.Invoke(this, new EventArgs());
            }
        }
    }
}
