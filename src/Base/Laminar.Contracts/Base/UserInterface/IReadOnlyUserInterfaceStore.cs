namespace Laminar.Contracts.Base.UserInterface;

public interface IReadOnlyUserInterfaceStore
{
    public bool HasImplementation(Type definitionType);

    public bool TryGetUserInterface(Type definitionType, out Type userInterfaceType);
}
