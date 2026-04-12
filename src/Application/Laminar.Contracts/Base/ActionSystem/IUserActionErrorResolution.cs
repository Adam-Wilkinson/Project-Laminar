namespace Laminar.Contracts.Base.ActionSystem;

public interface IUserActionErrorResolution;

public class UserActionCancelledResolution : IUserActionErrorResolution;

public record AlternativeActionFound(IUserAction AlternativeAction) : IUserActionErrorResolution;