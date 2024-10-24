using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;


namespace Laminar.Avalonia.PageTransitions;
internal class SettingsPageTransition : IPageTransition
{
    public TimeSpan Duration { get; set; }

    public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (!forward)
        {
            (to, from) = (from, to);
        }

        var tasks = new List<Task>();
        var parent = GetVisualParent(from, to);
        var translateYProperty = TranslateTransform.YProperty;
        var scaleXProperty = ScaleTransform.ScaleXProperty;
        var scaleYProperty = ScaleTransform.ScaleYProperty;

        var fromInitialScale = forward ? 1 : 0.8;
        var fromFinalScale = forward ? 0.8 : 1;

        if (from is not null)
        {
            from.IsVisible = true;
            var animation = new Animation
            {
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = translateYProperty,
                                Value = 0d,
                            },
                            new Setter
                            {
                                Property = scaleXProperty,
                                Value = fromInitialScale
                            },
                            new Setter
                            {
                                Property = scaleYProperty,
                                Value = fromInitialScale,
                            },
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = forward ? 1 : 0.5d,
                            }
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame
                    {
                        Setters = 
                        { 
                            new Setter
                            {
                                Property = scaleXProperty, 
                                Value = fromFinalScale
                            },
                            new Setter
                            {
                                Property = scaleYProperty,
                                Value = fromFinalScale
                            },
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = forward ? 0.5d : 1,
                            }
                        },
                        Cue = new Cue(1d)
                    }
                },
                Duration = Duration
            };
            tasks.Add(animation.RunAsync(from, cancellationToken));
        }

        if (to is not null)
        {
            var animation = new Animation
            {
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = translateYProperty,
                                Value = forward ? -parent.Bounds.Height : 0,
                            }
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = translateYProperty,
                                Value = forward ? 0 : -parent.Bounds.Height,
                            }
                        },
                        Cue = new Cue(1d)
                    }
                },
                Duration = Duration
            };
            tasks.Add(animation.RunAsync(to, cancellationToken));
        }

        await Task.WhenAll(tasks);

        if (!forward)
        {
            (to, from) = (from, to);
        }

        if (from is not null && !cancellationToken.IsCancellationRequested)
        {
            from.IsVisible = false;
        }
    }

    private static Visual GetVisualParent(Visual? from, Visual? to)
    {
        var p1 = (from ?? to)!.GetVisualParent();
        var p2 = (to ?? from)!.GetVisualParent();

        if (p1 != null && p2 != null && p1 != p2)
        {
            throw new ArgumentException(
                                "Controls for PageSlide must have same parent.");
        }

        return p1 ?? throw new InvalidOperationException(
                                                "Cannot determine visual parent.");
    }
}
