using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Nodes.Connectors
{
    class ConnectionManager : IConnectionManager
    {
        private readonly List<Func<ConnectionType, IConnector, IConnector>> connectionChecks = new();
        private INodeBase _parentNode;

        public ConnectionManager(IObservableValue<IConnector> input, IObservableValue<IConnector> output)
        {
            InputConnector = input;
            OutputConnector = output;
        }

        public IObservableValue<IConnector> InputConnector { get; }

        public IObservableValue<IConnector> OutputConnector { get; }

        public INodeBase ParentNode
        {
            get => _parentNode;
            set
            {
                _parentNode = value;
                UpdateInput();
                UpdateOutput();
            }
        }

        public void AddConnectionCheck<T>(Func<ConnectionType, T> connectionCheck) where T : IConnector
        {
            connectionChecks.Add((connectionType, oldConnector) =>
            {
                T newConnector = connectionCheck(connectionType);
                if (newConnector != null && (oldConnector == null || !oldConnector.GetType().IsAssignableFrom(newConnector.GetType())))
                {
                    return newConnector;
                }

                return null;
            });
        }

        public void UpdateInput()
        {
            IConnector newVal = null;
            int i = 0;
            while (newVal is null && i < connectionChecks.Count)
            {
                newVal = connectionChecks[i](ConnectionType.Input, InputConnector.Value);
                i++;
            }

            InputConnector.Value = newVal;
        }

        public void UpdateOutput()
        {
            IConnector newVal = null;
            int i = 0;
            while (newVal is null && i < connectionChecks.Count)
            {
                newVal = connectionChecks[i](ConnectionType.Output, OutputConnector.Value);
                i++;
            }

            OutputConnector.Value = newVal;
        }
    }
}
