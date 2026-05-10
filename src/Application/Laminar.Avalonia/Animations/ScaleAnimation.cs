using System;
using Avalonia;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;

namespace Laminar.Avalonia.Animations;

public class ScaleAnimation
{
    public static readonly AttachedProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.RegisterAttached<PositionAnimation, Visual, TimeSpan>("Duration", TimeSpan.Zero);
    public static TimeSpan GetDuration(Visual visual) => visual.GetValue(DurationProperty);
    public static void SetDuration(Visual visual, TimeSpan value) => visual.SetValue(DurationProperty, value);
    
    static ScaleAnimation()
    {
        DurationProperty.Changed.AddClassHandler<Visual>(OnDurationChanged);
    }

    private static void OnDurationChanged(Visual visual,
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
        compositionVisual.SetImplicitScaleAnimation(null);
    }

    private static void AddTransitionTo(Visual visual, TimeSpan duration)
    {
        if (!visual.IsAttachedToVisualTree())
        {
            visual.AttachedToVisualTree +=  (_, __) => AddTransitionTo(visual, duration);
            return;
        }
        
        if (ElementComposition.GetElementVisual(visual) is not { } compositionVisual) return;
        Compositor compositor = compositionVisual.Compositor;
        Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = nameof(CompositionVisual.Scale);
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = duration;
        compositionVisual.SetImplicitScaleAnimation(offsetAnimation);
    }
}