using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Laminar.Avalonia.Settings;
using Microsoft.Extensions.Logging;

namespace Laminar.Avalonia.InitializationTargets;

public class ColorInitialization(Application topLevel, ILogger<ColorInitialization> logger) : IAfterApplicationBuiltTarget
{
    public const string LaminarAccentKey = "LaminarAccent";
    public const string LaminarForegroundKey = "LaminarForeground";
    public const string LaminarAccentForegroundKey = "LaminarAccentForeground";
    public const string LaminarAccentBackgroundKey = "LaminarAccentBackground";
    
    private const double BackgroundAlpha = 0.25;
    private const double ForegroundBrightness = 0.6;
    
    private Color _themeAccent = Color.Parse("#2555");
    private Color _themeForeground;
    private Color _themeBackground;
    private bool _manualColors;
    private Color _manualAccent;
    private Color _manualForeground;
    private Color _manualBackground;

    public void OnApplicationBuilt()
    {
        _themeForeground = topLevel.ActualThemeVariant == ThemeVariant.Light ? Colors.Black : Colors.White;
        
        topLevel.Resources.GetResourceObservable("LaminarThemeAccent").Subscribe(new AnonymousObserver<object?>(primary =>
        {
            if (primary is not Color primaryColor || primaryColor == _themeAccent) return;
            _themeAccent = primaryColor;
            ColorsChanged();
        }));
        
        topLevel.Resources.GetResourceObservable("LaminarThemeForeground").Subscribe(new AnonymousObserver<object?>(foreground =>
        {
            if (foreground is not Color foregroundColor || foregroundColor == _themeForeground) return;
            _themeForeground = foregroundColor;
            ColorsChanged();
        }));
        
        topLevel.Resources.GetResourceObservable("LaminarThemeBackground").Subscribe(new AnonymousObserver<object?>(background =>
        {
            if (background is not Color backgroundColor || backgroundColor == _themeBackground) return;
            _themeBackground = backgroundColor;
            ColorsChanged();
        }));

        Setting<bool>.OnChange(topLevel, "SettingsRoot.InterfaceSettings.ColorsFromTheme", colorsFromTheme =>
        {
            if (_manualColors == colorsFromTheme) return;
            _manualColors = colorsFromTheme;
            ColorsChanged();
        });

        Setting<Color>.OnChange(topLevel, "SettingsRoot.InterfaceSettings.AccentColor", accentColor =>
        {
            if (_manualAccent ==  accentColor) return;
            _manualAccent = accentColor;
            ColorsChanged();
        });
        
        Setting<Color>.OnChange(topLevel, "SettingsRoot.InterfaceSettings.ForegroundColor", foregroundColor =>
        {
            if (_manualForeground ==  foregroundColor) return;
            _manualForeground = foregroundColor;
            ColorsChanged();
        });
        
        Setting<Color>.OnChange(topLevel, "SettingsRoot.InterfaceSettings.BackgroundColor", backgroundColor =>
        {
            if (_manualBackground ==  backgroundColor) return;
            _manualBackground = backgroundColor;
            ColorsChanged();
        });

        topLevel.ActualThemeVariantChanged += (_, _) => ColorsChanged();
    }

    private void ColorsChanged()
    {
        Color accent = _manualColors ? _manualAccent : _themeAccent;
        Color foreground = _manualColors ?  _manualForeground : _themeForeground;
        Color background = _manualColors ? _manualBackground : _themeBackground;
        topLevel.Resources[LaminarAccentKey] = accent;
        topLevel.Resources[LaminarForegroundKey] = foreground;
        topLevel.Resources["LaminarBackground"] = background;

        if (topLevel.Styles.FirstOrDefault(x => x is FluentTheme) is FluentTheme fluentTheme )
        {
            fluentTheme.Palettes[ThemeVariant.Light] = new ColorPaletteResources { Accent = accent }; 
            fluentTheme.Palettes[ThemeVariant.Dark] = new ColorPaletteResources { Accent = accent };
        }
        
        topLevel.Resources[LaminarAccentBackgroundKey] =
            new Color((byte)(accent.A * BackgroundAlpha), accent.R, accent.G, accent.B);

        var accentHsl = accent.ToHsl();
        var foregroundHsl = foreground.ToHsl();
        
        var accentForeground = new HslColor(
            1, 
            accentHsl.H, 
            Mix(foregroundHsl.S, accentHsl.S), 
            Mix(foregroundHsl.L, accentHsl.L));

        topLevel.Resources[LaminarAccentForegroundKey] = accentForeground.ToRgb();
    }
    
    private static double Mix(double foreground, double accent) 
        => ForegroundBrightness * foreground + (1 - ForegroundBrightness) * accent;
}