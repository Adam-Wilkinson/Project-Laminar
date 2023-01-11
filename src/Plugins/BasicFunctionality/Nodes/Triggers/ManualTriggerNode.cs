using System;
using Laminar.PluginFramework.NodeSystem;

namespace BasicFunctionality.Nodes.Triggers;

public class ManualTriggerNode : INode
{
    public string NodeName => "Manual Trigger";

    public event EventHandler Trigger;

    //public void RemoveTriggers()
    //{
    //    _sourceField["Displayed"] = (Action)(() => { });
    //}

    //public void HookupTriggers()
    //{
    //    _sourceField["Displayed"] = (Action)(() =>
    //    {
    //        Trigger?.Invoke(this, new EventArgs());
    //    });
    //}

    public void Evaluate()
    {
    }
}
