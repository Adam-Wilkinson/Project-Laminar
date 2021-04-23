using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.NodeSystem
{
    public interface INodeCloner
    {
        public T Clone<T>(T toClone) where T : INode;
    }
}
