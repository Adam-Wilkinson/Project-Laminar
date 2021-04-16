namespace Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals
{
    using Laminar_PluginFramework.NodeSystem.Nodes;
    using Laminar_PluginFramework.Primitives;
    using System;
    using System.Collections.Generic;

    public static class ComponentFlowHandler
    {
        private static readonly Dictionary<IVisualNodeComponent, FlowState> FlowStates = new();

        public static void SetFlowInput(this IVisualNodeComponent field, bool inputState = true)
        {
            if (FlowStates.TryGetValue(field, out FlowState _))
            {
                FlowStates[field].Input.Value = inputState;
            }
            else
            {
                FlowStates[field] = new FlowState(NewObservable(inputState), NewObservable(false));
            }
        }

        public static void SetFlowOutput(this IVisualNodeComponent field, bool outputState = true)
        {
            if (FlowStates.TryGetValue(field, out FlowState _))
            {
                FlowStates[field].Output.Value = outputState;
            }
            else
            {
                FlowStates[field] = new FlowState(NewObservable(false), NewObservable(outputState));
            }
        }

        public static IObservableValue<bool> GetFlowInput(this IVisualNodeComponent field)
        {
            if (!FlowStates.ContainsKey(field))
            {
                FlowStates[field] = new FlowState(NewObservable(false), NewObservable(false));
            }

            return FlowStates[field].Input;
        }

        public static IObservableValue<bool> GetFlowOutput(this IVisualNodeComponent field)
        {
            if (!FlowStates.ContainsKey(field))
            {
                FlowStates[field] = new FlowState(NewObservable(false), NewObservable(false));
            }

            return FlowStates[field].Output;
        }

        private record FlowState(IObservableValue<bool> Input, IObservableValue<bool> Output);

        private static IObservableValue<bool> NewObservable(bool value)
        {
            IObservableValue<bool> output = Laminar.New<IObservableValue<bool>>();

            output.Value = value;

            return output;
        }
    }
}
