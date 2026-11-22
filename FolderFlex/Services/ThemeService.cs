using System.Windows;

namespace FolderFlex.Services;

public static class ThemeService
{
    public static void ApplyTheme(string themeName)
    {
        if (Application.Current is null)
            return;

        string normalizedTheme = string.Equals(themeName, "DarkTheme", StringComparison.OrdinalIgnoreCase)
            ? "DarkTheme"
            : "LightTheme";

        var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
        ResourceDictionary? existingTheme = mergedDictionaries
            .FirstOrDefault(d => d.Source?.OriginalString.Contains("Theme.xaml", StringComparison.OrdinalIgnoreCase) == true);

        if (existingTheme is not null)
            mergedDictionaries.Remove(existingTheme);

        Uri themeUri = new($"pack://application:,,,/FolderFlex;component/Themes/{normalizedTheme}.xaml", UriKind.Absolute);
        mergedDictionaries.Add(new ResourceDictionary { Source = themeUri });
    }
}
