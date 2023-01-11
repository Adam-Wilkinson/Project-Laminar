namespace Laminar.PluginFramework.UserInterfaces;

public class Slider : IUserInterfaceDefinition
{
    public Slider(double min, double max)
    {
        Max = max;
        Min = min;
    }

    public double Max { get; }

    public double Min { get; }

    public double Increment { get; init; }
}