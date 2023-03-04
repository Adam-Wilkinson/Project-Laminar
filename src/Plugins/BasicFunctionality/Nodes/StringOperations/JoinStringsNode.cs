using Laminar.PluginFramework.NodeSystem;

namespace BasicFunctionality.Nodes.StringOperations;
public partial class JoinStringsNode : INode
{
    public string NodeName => "Join Strings";

    public void Evaluate()
    {
        //string output = string.Empty;
        ////foreach (INodeField field in combineStrings)
        ////{
        ////    output += field.GetInput<string>();
        ////}

        //combinedString.SetOutput(output);
    }
}
