using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.NodeSystem.NodeComponents.Visuals
{
    public class EditableNodeLabel : NodeLabel, IEditableNodeLabel
    {
        public EditableNodeLabel(IObservableValue<string> name, IOpacity opacity, IObservableValue<string> labelText, IObservableValue<bool> isBeingEdited) 
            : base(name, opacity, labelText)
        {
            IsBeingEdited = isBeingEdited;
        }

        public IObservableValue<bool> IsBeingEdited { get; }
    }
}
