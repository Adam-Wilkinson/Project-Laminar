using System.Collections.ObjectModel;

namespace Laminar_Core.Scripting
{
    public interface IScriptCollection
    {
        public ObservableCollection<IScriptInstance> Scripts { get; }
    }
}
