using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Scripting;

public interface IScript : IEncodableDataOwner<IPersistentDictionary>
{
    public INodeTree WritableNodeTree { get; }

    public IScriptExecutionInstance ExecutionInstance { get; }

    public ScriptState State { get; }
}
