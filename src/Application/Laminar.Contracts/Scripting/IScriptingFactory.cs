using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Scripting;

public interface IScriptingFactory : IDecodingFactory<IScript, IPersistentDictionary>
{
    IScript CreateScript();
}
