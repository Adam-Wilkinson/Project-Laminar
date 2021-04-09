namespace OpenFlow_Core.Nodes.NodeTreeSystem.Obselete
{

    /*
    public record NodeConnection(IConnector Output, IConnector Input)
    {
        public static bool Construct(IConnector connector1, IConnector connector2, out NodeConnection connection)
        {
            static bool CheckCompat(IConnector output, IConnector input) => input.ConnectionType == ConnectionType.Input && output.ConnectionType == ConnectionType.Output;

            if (CheckCompat(connector1, connector2))
            {
                connection = new NodeConnection(connector1, connector2);
                return true;
            }
            else if (CheckCompat(connector2, connector1))
            {
                connection = new NodeConnection(connector2, connector1);
                return true;
            }
            else
            {
                connection = default;
                return false;
            }
        }
    }
    */
}
