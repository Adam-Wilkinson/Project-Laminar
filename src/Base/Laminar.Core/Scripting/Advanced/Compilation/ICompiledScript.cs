using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Scripting.Advanced.Compilation
{
    public interface ICompiledScript : IDisposable
    {
        bool IsLive { get; set; }

        List<CompiledNodeWrapper> AllTriggerNodes { get; }

        List<ILaminarValue> Inputs { get; }

        IAdvancedScript OriginalScript { get; set; }
    }
}
