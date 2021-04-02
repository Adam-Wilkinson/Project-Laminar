using System;
using System.Collections.Generic;

namespace OpenFlow_Core.Management.UserInterface
{
    public class UserInterfaceStore
    {
        private readonly Dictionary<string, object> _userInterfacesByName = new();

        public UserInterfaceStore(string firstUserInterfaceName, object firstUserInterface)
        {
            RegisterUserInterface(firstUserInterfaceName, firstUserInterface);
        }

        public bool TryGetUserInterface(string userInterfaceName, out object userInterface)
        {
            if (_userInterfacesByName.TryGetValue(userInterfaceName, out object internalUserInterface))
            {
                userInterface = PrepareForReturning(internalUserInterface);
                return true;
            }

            userInterface = default;
            return false;
        }

        public bool RegisterUserInterface(string name, object userInterface) => _userInterfacesByName.TryAdd(name, userInterface);

        private static object PrepareForReturning(object internalUserInterface)
        {
            if (typeof(Type).IsAssignableFrom(internalUserInterface.GetType()))
            {
                return Activator.CreateInstance((Type)internalUserInterface);
            }

            return internalUserInterface;
        }
    }
}