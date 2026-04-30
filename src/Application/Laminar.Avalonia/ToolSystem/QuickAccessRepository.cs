using System.Collections.Generic;

namespace Laminar.Avalonia.ToolSystem;

public class QuickAccessRepository
{
    private readonly Dictionary<string, Toolbox> _quickAccess = [];
    
    public Toolbox FromKey(string key)
    {
        if (_quickAccess.TryGetValue(key, out var toolbox)) return toolbox;
        
        toolbox = new Toolbox { NameKey = $"Quick access: '{key}'"};
        _quickAccess.Add(key, toolbox);
        return toolbox;
    }
}