using System.Timers;
using Laminar.PluginFramework.NodeSystem;

namespace BasicFunctionality.Nodes.Flow;

public class WaitForTrigger : INode
{
    //private readonly Timer resetTimer = new() { AutoReset = false };
    //private readonly bool isWaiting = false;

    //public WaitForTrigger()
    //{
    //    resetTimer.Elapsed += ResetTimer_Elapsed;
    //    TimeoutTime.IsVisible = (bool)hasTimeout["display"];
    //    hasTimeout.GetValue("display").OnChange += (o, b) =>
    //    {
    //        TimeoutTime.IsVisible = (bool)b;
    //    };
    //}

    public string NodeName { get; } = "Wait for Trigger";

    public void Evaluate()
    {
        //if (isWaiting)
        //{
        //    return;
        //}

        //triggerFlowInput.FlowInput.Activated += TriggerFlowOut;
        //isWaiting = true;
        //if ((bool)hasTimeout["display"])
        //{
        //    resetTimer.Interval = TimeoutTime.GetInput<double>() * 1000;
        //    resetTimer.Start();
        //}
    }

    //private void ResetTimer_Elapsed(object sender, ElapsedEventArgs e)
    //{
    //    RemoveTrigger();
    //}

    //private void TriggerFlowOut(object sender, EventArgs e)
    //{
    //    if (!isWaiting)
    //    {
    //        return;
    //    }

    //    triggerFlowInput.FlowOutput.Activate();
    //    RemoveTrigger();
    //}

    //private void RemoveTrigger()
    //{
    //    if (!isWaiting)
    //    {
    //        return;
    //    }

    //    isWaiting = false;
    //    resetTimer.Stop();
    //    triggerFlowInput.FlowInput.Activated -= TriggerFlowOut;
    //}
}
