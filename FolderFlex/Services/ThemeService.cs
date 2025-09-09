using System.Windows;
using Application = System.Windows.Application;

namespace FolderFlex.Services;

public static class ThemeService
{
    public static void ApplyTheme(string themeName)
    {
        var existingTheme = Application.Current.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme.xaml"));

        if (existingTheme != null)
        {
            Application.Current.Resources.MergedDictionaries.Remove(existingTheme);
        }

        var themeUri = new Uri($"pack://application:,,,/FolderFlex;component/Themes/{themeName}.xaml", UriKind.Absolute);
        var themeDictionary = new ResourceDictionary { Source = themeUri };
        Application.Current.Resources.MergedDictionaries.Add(themeDictionary);
    }
}