using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.PluginManagement
{
    public interface IRegisteredPlugin
    {
        string PluginName { get; }

        string PluginDescription { get; }

        public IReadOnlyDictionary<string, Type> RegisteredNodes { get; }

        public bool ContainsNode(INode node);

        void Load();

        void Unload();
    }
}
