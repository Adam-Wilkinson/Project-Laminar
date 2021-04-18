using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.Primitives.UserInterface
{
    public interface IUserInterfaceManager
    {
        object this[string key] { get; }

        void SetChildValue(ILaminarValue childValue);

        public IUserInterfaceRegister Displays { get; set; }

        public IUserInterfaceRegister Editors { get; set; }
    }
}