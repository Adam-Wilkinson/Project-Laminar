namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionErrorResolution;

public class CancelledByUser : IUserActionErrorResolution;

public record AlternativeActionFound(IUserAction AlternativeAction) : IUserActionErrorResolution;