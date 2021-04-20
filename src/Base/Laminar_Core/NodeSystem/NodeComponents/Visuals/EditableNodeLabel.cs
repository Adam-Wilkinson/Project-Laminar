using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.NodeSystem.NodeComponents.Visuals
{
    public class EditableNodeLabel : NodeLabel, IEditableNodeLabel
    {
        private bool _needsEditing = false;

        public EditableNodeLabel(IObservableValue<string> name, IOpacity opacity, IObservableValue<string> labelText) 
            : base(name, opacity, labelText)
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
