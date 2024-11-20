using FolderFlex.View;
using MahApps.Metro.IconPacks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using Orientation = System.Windows.Controls.Orientation;
using ProgressBar = System.Windows.Controls.ProgressBar;

namespace FolderFlex.Factory.MainWindow.ComponentFactory;

public static class FileComponentFactory
{
    public static Border CreateContainerBorder() => new()
    {
        Background = System.Windows.Media.Brushes.White,
        BorderBrush = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#EBECF4")!,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(10),
        Height = 56,
        Margin = new Thickness(0, 5, 14, 0)
    };

    public static StackPanel CreateStackPanel() => new()
    {
        Orientation = Orientation.Horizontal,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
    };

    public static PackIconPhosphorIcons CreateFileIcon() => new()
    {
        Kind = PackIconPhosphorIconsKind.FileArrowUpLight,
        Margin = new Thickness(0, 2, 7, 0),
        Foreground = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#1f1446")!
    };

    public static TextBlock CreateFileNameTextBlock(string fileName) => new()
    {
        Text = Path.GetFileName(fileName).Length > 35 ? $"{Path.GetFileName(fileName).Substring(0, 35)}..." : Path.GetFileName(fileName),
        VerticalAlignment = VerticalAlignment.Center,
        FontSize = 14,
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(0, 0, 0, 4),
        Foreground = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#1f1446")!
    };

    public static Button CreateFileButton(FolderFlexMain mainWindow) => new()
    {
        Style = (Style)mainWindow.FindResource("TransparentButton"),
        Margin = new Thickness(10, 0, 0, 2),
        Cursor = System.Windows.Input.Cursors.Hand,
        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
        ToolTip = "Abrir Arquivo"
    };

    public static ProgressBar CreateItemProgressBar(FolderFlexMain mainWindow) => new()
    {
        Style = (Style)mainWindow.FindResource("RoundedProgressBar"),
        Value = 0,
        Height = 10,
        Width = 340,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(10, 40, 0, 0),
        HorizontalAlignment = System.Windows.HorizontalAlignment.Left
    };

    public static TextBlock CreateFileSizeTextBlock(string fileName) => new()
    {
        Text = (new FileInfo(fileName).Length / 1024) + " kb",
        FontWeight = FontWeights.SemiBold,
        Foreground = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#A0A3BD")!,
        HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
        Margin = new Thickness(0, 0, 50, 0),
        VerticalAlignment = VerticalAlignment.Center,
        FontSize = 12
    };

    public static Button CreateActionButton(FolderFlexMain mainWindow) => new()
    {
        Style = (Style)mainWindow.FindResource("TransparentButton"),
        Margin = new Thickness(10, 0, 10, 0),
        HorizontalAlignment = System.Windows.HorizontalAlignment.Right
    };

    public static PackIconGameIcons CreateGameIcon() => new()
    {
        Kind = PackIconGameIconsKind.Cancel,
        Foreground = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#AAB8C2")!,
        Height = 18
    };

    public static PackIconLucide CreateFileSearchIcon() => new()
    {
        Kind = PackIconLucideKind.FileSearch,
        Foreground = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#AAB8C2")!,
        Height = 18,
        Visibility = Visibility.Collapsed
    };
}
