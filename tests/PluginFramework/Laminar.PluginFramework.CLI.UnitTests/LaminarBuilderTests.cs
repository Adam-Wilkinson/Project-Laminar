using Laminar.PluginFramework.CLI.Packing;

namespace Laminar.PluginFramework.CLI.UnitTests;

public class LaminarBuilderTests
{
    [Fact]
    public async Task ShouldParseVersion()
    {
        const string pluginUnderTest =
        """
        {
            "id": "example-plugin",
            "major-version": 1,
            "minor-version": 0
        }                    
        """;
        
        var pluginData = PluginData.Parse(pluginUnderTest);
        var pluginVersion = PackageBuilder.GetPluginVersion(pluginData);

        pluginVersion.Should().Be("example-plugin.1.0");
    }
    
    [Fact]
    public async Task ShouldParseVersionWithPatch()
    {
        const string pluginUnderTest =
            """
            {
                "id": "example-plugin",
                "major-version": 1,
                "minor-version": 0,
                "patch-version": 10
            }                    
            """;
        
        var pluginData = PluginData.Parse(pluginUnderTest);
        var pluginVersion = PackageBuilder.GetPluginVersion(pluginData);

        pluginVersion.Should().Be("example-plugin.1.0.10");
    }
    
    [Fact]
    public async Task ShouldParseVersionWithPrerelease()
    {
        const string pluginUnderTest =
            """
            {
                "id": "example-plugin",
                "major-version": 1,
                "minor-version": 0,
                "prerelease-version": "beta"
            }                    
            """;
        
        var pluginData = PluginData.Parse(pluginUnderTest);
        var pluginVersion = PackageBuilder.GetPluginVersion(pluginData);

        pluginVersion.Should().Be("example-plugin.1.0-beta");
    }
}