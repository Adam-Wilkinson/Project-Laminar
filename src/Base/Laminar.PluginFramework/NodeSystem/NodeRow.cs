using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;

namespace Laminar.PluginFramework.NodeSystem;

public record NodeRow(IInput? Input, IValueInfo DisplayValue, IOutput? Output);