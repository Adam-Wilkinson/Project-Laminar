namespace Laminar.PluginFramework.Json.UnitTests;

public class PluginDataTests
{
    [Fact]
    public void ShouldParseVersion()
    {
        const string pluginUnderTest =
            """
            {
                "id": "example-plugin",
                "major-version": 1,
                "minor-version": 0
            }                    
            """;
        
        var pluginVersion = PluginData.Parse(pluginUnderTest).GetPluginVersion();

        pluginVersion.Should().Be("1.0");
    }
    
    [Fact]
    public void ShouldParseVersionWithPatch()
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
        
        var pluginVersion = PluginData.Parse(pluginUnderTest).GetPluginVersion();

        pluginVersion.Should().Be("1.0.10");
    }
    
    [Fact]
    public void ShouldParseVersionWithPrerelease()
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
        
        var pluginVersion = PluginData.Parse(pluginUnderTest).GetPluginVersion();

        pluginVersion.Should().Be("1.0-beta");
    }
}