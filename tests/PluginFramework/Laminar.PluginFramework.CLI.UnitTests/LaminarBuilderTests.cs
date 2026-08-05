using DotnetHelper;
using Laminar.PluginFramework.CLI.Packing;

namespace Laminar.PluginFramework.CLI.UnitTests;

public class LaminarBuilderTests
{
    private const string PluginUnderTest =
"""
{
    "id": "example-plugin",
    "major-version": 1,
    "minor-version": 0
}                    
""";

    [Fact]
    public async Task ShouldParseVersion()
    {
        var pluginData = PluginData.Parse(PluginUnderTest);
        var pluginVersion = PackageBuilder.GetPluginVersion(pluginData);

        pluginVersion.Should().Be("example-plugin.1.0");
    }
}