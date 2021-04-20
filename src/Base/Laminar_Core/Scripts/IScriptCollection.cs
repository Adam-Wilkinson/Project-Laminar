using System.Collections.ObjectModel;

namespace Laminar_Core.Scripts
{
    public interface IScriptCollection
    {
        public ObservableCollection<IScript> Scripts { get; }

        void AddScript();
    }
}
