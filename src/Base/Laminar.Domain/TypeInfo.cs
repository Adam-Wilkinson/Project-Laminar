namespace Laminar.Domain;

public record TypeInfo(string UserFriendlyName, object EditorDefinition, object ViewerDefinition, string HexColour, object DefaultValue);