namespace Laminar.Domain.Contexts;

public enum ExecutionReason
{
    Trigger,
    UserChangedValue,
    InterfaceRefresh,
}