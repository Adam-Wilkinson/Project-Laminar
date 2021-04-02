namespace OpenFlow_Core.Management.UserInterface
{
    using System;
    using System.Collections.Generic;

    public class UserInterfaceRegister
    {
        private readonly Dictionary<string, UserInterfaceStore> _registeredUserInterfaces = new();

        public bool RegisterUserInterface<T>(string name, T userInterface) => AddUserInterfaceInternal<T>(name, userInterface);

        public bool RegisterUserInterface<T>(string name, Type userInterface) => AddUserInterfaceInternal<T>(name, userInterface);

        public bool TryGetUserInterface(string typeName, string interfaceName, out object userInterface)
        {
            if (typeName is null || interfaceName is null)
            {
                userInterface = default;
                return false;
            }

            if (_registeredUserInterfaces.TryGetValue(typeName, out UserInterfaceStore userInterfaceStore))
            {
                return userInterfaceStore.TryGetUserInterface(interfaceName, out userInterface);
            }

            userInterface = default;
            return false;
        }

        private bool AddUserInterfaceInternal<T>(string name, object userInterface)
        {
            if (_registeredUserInterfaces.TryGetValue(typeof(T).FullName, out UserInterfaceStore userInterfaceStore))
            {
                return userInterfaceStore.RegisterUserInterface(name, userInterface);
            }
            
            _registeredUserInterfaces.Add(typeof(T).FullName, new UserInterfaceStore(name, userInterface));

            return true;
        }
    }
}
