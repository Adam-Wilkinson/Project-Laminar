namespace Laminar.Domain;

public record TypeInfo(string UserFriendlyName, object EditorDefinition, object ViewerDefinition, string HexColor, object DefaultValue);