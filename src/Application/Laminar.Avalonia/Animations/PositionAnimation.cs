using System;
using Avalonia;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.VisualTree;

namespace Laminar.Avalonia.Animations;

public class PositionAnimation
{
    public static readonly AttachedProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.RegisterAttached<PositionAnimation, Visual, TimeSpan>("Duration", TimeSpan.Zero);
    public static TimeSpan GetDuration(Visual visual) => visual.GetValue(DurationProperty);
    public static void SetDuration(Visual visual, TimeSpan value) => visual.SetValue(DurationProperty, value);
    
    
    static PositionAnimation()
    {
        DurationProperty.Changed.AddClassHandler<Visual>(OnEnableScaleShowAnimationChanged);
    }

    private static void OnEnableScaleShowAnimationChanged(Visual visual,
        AvaloniaPropertyChangedEventArgs eventArgs)
    {
        (TimeSpan oldVal, TimeSpan newVal) = eventArgs.GetOldAndNewValue<TimeSpan>();
        if (oldVal.Ticks > 0.0)
        {
            RemoveTransitionFrom(visual);
        }

        if (newVal.Ticks > 0)
        {
            AddTransitionTo(visual, newVal);
        }
    }

    private static void RemoveTransitionFrom(Visual visual)
    {
        if (ElementComposition.GetElementVisual(visual) is not { } compositionVisual) return;
        compositionVisual.ImplicitAnimations = null;
    }

    private static void AddTransitionTo(Visual visual, TimeSpan duration)
    {
        if (!visual.IsInitialized)
        {
            visual.Initialized +=  (_, __) => AddTransitionTo(visual, duration);
            return;
        }
        
        if (ElementComposition.GetElementVisual(visual) is not { } compositionVisual) return;
        Compositor compositor = compositionVisual.Compositor;
        Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = duration;
        ImplicitAnimationCollection implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
        implicitAnimationCollection["Offset"] = offsetAnimation;
        compositionVisual.ImplicitAnimations = implicitAnimationCollection;
    }
}