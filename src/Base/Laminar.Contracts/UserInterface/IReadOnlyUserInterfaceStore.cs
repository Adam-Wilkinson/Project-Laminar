namespace Laminar.Contracts.UserInterface;

public interface IReadOnlyUserInterfaceStore
{
    public bool HasFrontendOfType(string frontendKey);

    public bool HasImplementation(string frontendKey, Type definitionType);

    public bool TryGetUserInterface(string frontendKey, Type definitionType, out Type userInterfaceType);
}
