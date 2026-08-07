using Laminar.Domain.ValueObjects;

namespace Laminar.Domain.Exceptions;

public class CannotFindPluginException(string id, SemanticVersion version) : Exception($"Unable to find plugin {id} of version {version}")
{
    public string Id => id;
    
    public SemanticVersion Version => version;
}