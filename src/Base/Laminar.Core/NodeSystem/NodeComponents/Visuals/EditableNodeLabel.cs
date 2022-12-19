using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.NodeSystem.NodeComponents.Visuals
{
    public class EditableNodeLabel : NodeLabel, IEditableNodeLabel
    {
        private bool _needsEditing = false;

        public EditableNodeLabel(IObservableValue<string> name, IFlow flowInput, IFlow flowOutput, IOpacity opacity, IObservableValue<string> labelText) 
            : base(name, flowInput, flowOutput, opacity, labelText)
        {
        }

        public bool NeedsEditing
        {
            get
            {
                if (_needsEditing)
                {
                    _needsEditing = false;
                    return true;
                }

                return false;
            }
            set => _needsEditing = value;
        }
    }
}
