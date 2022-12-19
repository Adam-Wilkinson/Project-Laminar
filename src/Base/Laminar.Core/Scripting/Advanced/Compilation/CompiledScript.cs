using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;

namespace Laminar_Core.Scripting.Advanced.Compilation
{
    public class CompiledScript : ICompiledScript
    {
        private bool _isLive;

        public bool IsLive
        {
            get => _isLive;
            set
            {
                if (_isLive != value)
                {
                    _isLive = value;

                    if (_isLive)
                    {
                        foreach (CompiledNodeWrapper wrapper in AllTriggerNodes)
                        {
                            (wrapper.CoreNode as ITriggerNode).HookupTriggers();
                        }
                    }
                    else if (!_isLive)
                    {
                        foreach (CompiledNodeWrapper wrapper in AllTriggerNodes)
                        {
                            (wrapper.CoreNode as ITriggerNode).RemoveTriggers();
                        }
                    }
                }
            }
        }

        public List<CompiledNodeWrapper> AllTriggerNodes { get; } = new();

        public List<ILaminarValue> Inputs { get; } = new();

        public IAdvancedScript OriginalScript { get; set; }

        public void Dispose()
        {
            foreach (CompiledNodeWrapper wrapper in AllTriggerNodes)
            {
                wrapper.Dispose();
            }
        }
    }
}
