using Laminar_Core.Scripting;
using Laminar_Core.Scripting.Advanced.Instancing;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laminar_Core.Scripting.Advanced.Editing.Connection.ConnectorManagers
{
    public interface IConnectorManager
    {
        public static IEnumerable<Type> AllImplementingTypes { get; } = typeof(Connector).Assembly.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(IConnectorManager)));

        event EventHandler ExistsChanged;

        IObservableValue<string> HexColour { get; }

        void Initialize(IVisualNodeComponent component, ConnectorType connectionType);

        bool ConnectorExists();

        bool ConnectorExclusiveCheck();

        void ConnectionAddedAction(IConnectorManager manager);

        void ConnectionRemovedAction(IConnectorManager manager);

        bool CompatibilityCheck(IConnectorManager toCheck);

        void Activate(IAdvancedScriptInstance instance, PropagationDirection direction);
    }
}
