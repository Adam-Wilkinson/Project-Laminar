using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Styling;

namespace Laminar.Avalonia.InitializationTargets;

public class ColorInitialization(TopLevel topLevel) : IAfterApplicationBuiltTarget
{
    public static readonly string LaminarAccentKey = "LaminarAccent";
    public static readonly string LaminarForegroundKey = "LaminarForeground";
    public static readonly string LaminarAccentForegroundKey = "LaminarAccentForeground";
    public static readonly string LaminarAccentBackgroundKey = "LaminarAccentBackground";
    
    private const double BackgroundAlpha = 0.25;
    private const double ForegroundBrightness = 0.8;
    private Color? _accent;
    private Color? _foreground;
    
    public void OnApplicationBuilt()
    {
        topLevel.Resources.GetResourceObservable(LaminarAccentKey).Subscribe(new AnonymousObserver<object?>(primary =>
        {
            if (primary is not Color primaryColor || primaryColor == _accent) return;
            _accent = primaryColor;
            ColorsChanged();
        }));
        
        topLevel.Resources.GetResourceObservable(LaminarForegroundKey).Subscribe(new AnonymousObserver<object?>(foreground =>
        {
            if (foreground is not Color foregroundColor || foregroundColor == _foreground) return;
            _foreground = foregroundColor;
            ColorsChanged();
        }));
    }

    private void ColorsChanged()
    {
        _accent ??= Color.Parse("#2555");
        _foreground ??= topLevel.ActualThemeVariant == ThemeVariant.Light ? Colors.Black : Colors.White;
        
        
        topLevel.Resources[LaminarAccentBackgroundKey] =
            new Color((byte)(_accent.Value.A * BackgroundAlpha), _accent.Value.R, _accent.Value.G, _accent.Value.B);

        var accentHsl = _accent.Value.ToHsl();
        var foregroundHsl = _foreground.Value.ToHsl();
        
        var accentForeground = new HslColor(
            1, 
            Mix(foregroundHsl.H, accentHsl.H), 
            Mix(foregroundHsl.S, accentHsl.S), 
            Mix(foregroundHsl.L, accentHsl.L));

        topLevel.Resources[LaminarAccentForegroundKey] = accentForeground.ToRgb();
    }
    
    private static double Mix(double foreground, double accent) 
        => ForegroundBrightness * foreground + (1 - ForegroundBrightness) * accent;
}