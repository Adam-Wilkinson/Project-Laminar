using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals
{
    public interface IEditableNodeLabel : INodeLabel
    {
        public IObservableValue<bool> IsBeingEdited { get; }
    }
}
