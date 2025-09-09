using FolderFlex.View;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Image = System.Windows.Controls.Image;
using Orientation = System.Windows.Controls.Orientation;
using ProgressBar = System.Windows.Controls.ProgressBar;

namespace FolderFlex.Factory.MainWindow.ComponentFactory;

public static class FileComponentFactory
{
    public static Border CreateContainerBorder() => new()
    {
        Background = Application.Current.FindResource("PrimaryBackgroundBrush") as Brush,
        BorderBrush = Application.Current.FindResource("PrimaryBorderBrush") as Brush,
        BorderThickness = new Thickness(1),
        CornerRadius = new CornerRadius(10),
        Height = 56,
        Margin = new Thickness(0, 5, 14, 0)
    };

    public static StackPanel CreateStackPanel() => new()
    {
        Orientation = Orientation.Horizontal,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Left,
    };

    public static Image CreateFileIcon()
    {
        Image image = new()
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/Recursos/file-up.png")),
            Width = 18,
            Height = 18,
            VerticalAlignment = VerticalAlignment.Center
        };
        return image;
    }

    public static TextBlock CreateFileNameTextBlock(string fileName) => new()
    {
        Text = Path.GetFileName(fileName).Length > 35 ? $"{Path.GetFileName(fileName).Substring(0, 35)}..." : Path.GetFileName(fileName),
        VerticalAlignment = VerticalAlignment.Center,
        FontSize = 14,
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(0, 0, 0, 4),
        Foreground = Application.Current.FindResource("PrimaryTextBrush") as Brush
    };

    public static Button CreateFileButton(FolderFlexMain mainWindow) => new()
    {
        Style = (Style)mainWindow.FindResource("TransparentButton"),
        Margin = new Thickness(10, 0, 0, 2),
        Cursor = System.Windows.Input.Cursors.Hand,
        HorizontalAlignment = HorizontalAlignment.Left,
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
        HorizontalAlignment = HorizontalAlignment.Left
    };

    public static TextBlock CreateFileSizeTextBlock(string fileName) => new()
    {
        Text = (new FileInfo(fileName).Length / 1024) + " kb",
        FontWeight = FontWeights.SemiBold,
        Foreground = Application.Current.FindResource("SecondaryTextBrush") as Brush,
        HorizontalAlignment = HorizontalAlignment.Right,
        Margin = new Thickness(0, 0, 50, 0),
        VerticalAlignment = VerticalAlignment.Center,
        FontSize = 12
    };

    public static Button CreateActionButton(FolderFlexMain mainWindow) => new()
    {
        Style = (Style)mainWindow.FindResource("TransparentButton"),
        Margin = new Thickness(10, 0, 10, 0),
        HorizontalAlignment = HorizontalAlignment.Right
    };

    public static Image CreateGameIcon()
    {
        Image image = new()
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/Recursos/cancel.png")),
            Width = 18,
            Height = 18,
            VerticalAlignment = VerticalAlignment.Center
        };
        return image;
    }

    public static Image CreateFileSearchIcon()
    {
        Image image = new()
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/Recursos/file-search.png")),
            Width = 18,
            Height = 18,
            VerticalAlignment = VerticalAlignment.Center
        };
        return image;
    }
}