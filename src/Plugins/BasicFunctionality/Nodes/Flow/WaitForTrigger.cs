using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BasicFunctionality.Nodes.Flow
{
    public class WaitForTrigger : IFlowNode
    {
        private readonly INodeLabel triggerFlowInput = Constructor.NodeLabel("Trigger to wait for").WithFlowInput().WithFlowOutput();
        private readonly INodeField hasTimeout = Constructor.NodeField("Timeout").WithValue("display", Constructor.RigidTypeDefinitionManager(false, "ToggleSwitch", null), true);
        private readonly INodeField TimeoutTime = Constructor.NodeField("Timeout Time").WithInput(Constructor.TypeDefinition(1.0).WithUnits("sec"));

        private readonly Timer resetTimer = new() { AutoReset = false };
        private bool isWaiting = false;

        public WaitForTrigger()
        {
            resetTimer.Elapsed += ResetTimer_Elapsed;
            TimeoutTime.IsVisible = (bool)hasTimeout["display"];
            hasTimeout.GetValue("display").OnChange += (o, b) =>
            {
                TimeoutTime.IsVisible = (bool)b;
            };
        }

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return triggerFlowInput;
                yield return hasTimeout;
                yield return TimeoutTime;
            }
        }

        public string NodeName { get; } = "Wait for Trigger";

        public void Evaluate()
        {
            if (isWaiting)
            {
                return;
            }

            triggerFlowInput.FlowInput.Activated += TriggerFlowOut;
            isWaiting = true;
            if ((bool)hasTimeout["display"])
            {
                resetTimer.Interval = TimeoutTime.GetInput<double>() * 1000;
                resetTimer.Start();
            }
        }

        private void ResetTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RemoveTrigger();
        }

        private void TriggerFlowOut(object sender, EventArgs e)
        {
            if (!isWaiting)
            {
                return;
            }

            triggerFlowInput.FlowOutput.Activate();
            RemoveTrigger();
        }

        private void RemoveTrigger()
        {
            if (!isWaiting)
            {
                return;
            }

            isWaiting = false;
            resetTimer.Stop();
            triggerFlowInput.FlowInput.Activated -= TriggerFlowOut;
        }
    }
}
