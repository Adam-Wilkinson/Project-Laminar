using System.Collections.ObjectModel;

namespace Laminar_Core.Scripts
{
    public interface IScriptCollection
    {
        public ObservableCollection<IScriptInstance> Scripts { get; }
    }
}
