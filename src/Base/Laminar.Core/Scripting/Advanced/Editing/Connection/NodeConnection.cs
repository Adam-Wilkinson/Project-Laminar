using System;

namespace Laminar_Core.Scripting.Advanced.Editing.Connection
{
    public class NodeConnection : INodeConnection
    {
        public NodeConnection(IConnector outputConnector, IConnector inputConnector)
        {
            OutputConnector = outputConnector;
            InputConnector = inputConnector;
        }

        public IConnector InputConnector { get; }

        public IConnector OutputConnector { get; }

        public event EventHandler OnBreak;

        public void Activate()
        {
        }

        public void Break()
        {
            InputConnector.RemoveConnection(this);
            OutputConnector.RemoveConnection(this);
            OnBreak?.Invoke(this, new EventArgs());
        }

        public IConnector Opposite(IConnector connector)
        {
            if (InputConnector == connector)
            {
                return OutputConnector;
            }

            if (OutputConnector == connector)
            {
                return InputConnector;
            }

            return null;
        }
    }
}
