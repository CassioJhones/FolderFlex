using FolderFlex.ViewModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;

namespace FolderFlex.View;

public sealed partial class FolderFlexMain : Window, IDisposable
{
    private readonly FolderFlexViewModel _viewModel;
    private bool _disposed;

    public FolderFlexMain()
    {
        InitializeComponent();

        _viewModel = new FolderFlexViewModel();
        DataContext = _viewModel;

        _viewModel.FileItems.CollectionChanged += OnFileItemsChanged;
    }

    protected override void OnClosed(EventArgs e)
    {
        Dispose();
        base.OnClosed(e);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _viewModel.FileItems.CollectionChanged -= OnFileItemsChanged;
        _viewModel.Dispose();
        GC.SuppressFinalize(this);
    }

    private void OnFileItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
            ScrollViewerContainer.ScrollToEnd();
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
