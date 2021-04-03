using OpenFlow_PluginFramework.Primitives;

namespace OpenFlow_Core.Primitives.UserInterface
{
    public interface IUserInterfaceManager
    {
        object this[string key] { get; }

        void SetChildValue(ILaminarValue childValue);

        IUserInterfaceManager Clone();
    }
}