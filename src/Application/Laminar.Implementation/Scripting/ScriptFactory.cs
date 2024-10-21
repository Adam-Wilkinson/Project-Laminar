using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Domain.Notification;
using Laminar.Implementation.Scripting.Connections;
using Laminar.Implementation.Scripting.NodeWrapping;

namespace Laminar.Implementation.Scripting;

internal class ScriptFactory : IScriptFactory
{
    private readonly IScriptExecutionManager _scriptExecutionManager;
    private readonly IUserActionManager _userActionManager;
    private readonly INotifyCollectionChangedHelper _notifyCollectionChangedHelper;

    public ScriptFactory(IScriptExecutionManager scriptExecutionManager, INotifyCollectionChangedHelper notifyCollectionChangedHelper, IUserActionManager userActionManager)
    {
        _scriptExecutionManager = scriptExecutionManager;
        _userActionManager = userActionManager;
        _notifyCollectionChangedHelper = notifyCollectionChangedHelper;
    }

    public IScript CreateScript() => new EditableScript(_scriptExecutionManager, _notifyCollectionChangedHelper, new ConnectionCollection(_userActionManager), new NodeCollection());
}
