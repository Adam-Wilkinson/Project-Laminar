using System;

namespace Laminar_Core.Primitives.UserInterface;

public interface IUserInterfaceRegister
{
    bool RegisterUserInterface<T>(string name, T userInterface);
    bool RegisterUserInterface<T>(string name, Type userInterface);
    bool TryGetUserInterface(string typeName, string interfaceName, out object userInterface);
}
