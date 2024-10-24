using Microsoft.CodeAnalysis;

namespace Laminar.PluginFramework.SourceGeneration;

internal static class LaminarDiagnostics
{
    private readonly static DiagnosticDescriptor ObjectNotPartialErrorDescriptor = new("PL001", "Partial Error", "Use of the attribute {0} on line {1} requires this to be marked as partial for source generation", "Usage", DiagnosticSeverity.Error, true);
    public static Diagnostic ObjectNotPartialError(string attributeName, int lineNumber, Location location) => Diagnostic.Create(ObjectNotPartialErrorDescriptor, location, attributeName, lineNumber);


    private readonly static DiagnosticDescriptor ClassIsSubclassErrorDescriptor = new("PL002", "Subclass Error", "Use of the attribute {0} on line {1} requires this class to not be defined as a subclass", "Usage", DiagnosticSeverity.Error, true);
    public static Diagnostic ClassIsSubclassError(string attributeName, int lineNumber, Location location) => Diagnostic.Create(ClassIsSubclassErrorDescriptor, location, attributeName, lineNumber);


    private readonly static DiagnosticDescriptor TypeDoesNotInheritErrorDescriptor = new("PL003", "Does not Inherit Error", "Use of the attribute {0} requires this field to be of a type implementing {1}", "Usage", DiagnosticSeverity.Error, true);
    public static Diagnostic TypeDoesNotInheritError(string attributeName, string typeName, Location location) => Diagnostic.Create(TypeDoesNotInheritErrorDescriptor, location, attributeName, typeName);
}
