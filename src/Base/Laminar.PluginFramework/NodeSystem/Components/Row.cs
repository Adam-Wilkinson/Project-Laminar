using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.Components;

public class Row<TInput, TOutput> : SingleItemNodeComponent
    where TInput : IInput
    where TOutput : IOutput
{
    internal Row(INodeComponentFactory factory, TInput? input, IDisplayValue centralDisplay, TOutput? output)
    {
        Input = input;
        Output = output;
        Display = centralDisplay;
        ChildComponent = factory.CreateSingleRow(input, centralDisplay, output);
    }

    public TInput? Input { get; }

    public TOutput? Output { get; }

    public IDisplayValue Display { get; }
}

public static class RowFactoryExtension
{
    public static Row<TInput, TOutput> Row<TInput, TOutput>(this INodeComponentFactory componetFactory, TInput? input, IDisplayValue centralDisplay, TOutput? output)
        where TInput : IInput
        where TOutput : IOutput
        => new(componetFactory, input, centralDisplay, output);
}
