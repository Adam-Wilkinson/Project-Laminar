using Microsoft.CodeAnalysis;

namespace Laminar.PluginSourceGeneration;

internal static class LaminarDiagnostics
{
    public readonly static DiagnosticDescriptor ObjectNotPartialError = new("PL001", "Partial Error", "Use of the attribute {0} on line {1} requires this to be marked as partial for source generation", "Usage", DiagnosticSeverity.Error, true);
    public readonly static DiagnosticDescriptor ClassIsSubclassError = new("PL001", "Subclass Error", "Use of the attribute {0} on line {1} requires this class to not be defined as a subclass", "Usage", DiagnosticSeverity.Error, true);

    public readonly static DiagnosticDescriptor TypeDoesNotInheritError = new("PL002", "Does not inherit erro", "Use of the attribute {0} requires this field to be of a type implementing {1}", "Usage", DiagnosticSeverity.Error, true);
}
