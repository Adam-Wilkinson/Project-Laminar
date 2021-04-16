using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.Primitives.UserInterface
{
    public interface IUserInterfaceManager
    {
        object this[string key] { get; }

        void SetChildValue(ILaminarValue childValue);

        IUserInterfaceManager Clone();
    }
}