using FolderFlex.ViewModel;
using FolderFlexCommon.Messages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FolderFlex.View;

public partial class FolderFlexMain : Window
{
    private readonly FolderFlexViewModel _viewModel;
    private readonly FolderFlexMessageProviderViewModel _languageController;

    public FolderFlexMain()
    {
        InitializeComponent();

        _languageController = (FolderFlexMessageProviderViewModel)this.FindResource("FolderFlexMessageProviderViewModel");

        _viewModel = new FolderFlexViewModel(this, _languageController);

        DataContext = _viewModel;

        string selectedLanguage = MessageMap.ListLanguages().FirstOrDefault(x => x.Key == _languageController.Language).Value;

        LanguageCombo.SelectedItem = selectedLanguage;
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void Click_SelectSource(object sender, RoutedEventArgs e)
    {
        ClearScreen();
        _viewModel.SelectSource();
    }

    private void Click_SelectDestination(object sender, RoutedEventArgs e)
    {
        ClearScreen();
        _viewModel.SelectDestination();
    }

    private void ClearScreen()
    {
        if (_viewModel.Contador == 0) return;

        _viewModel.Contador = 0;
        _viewModel.Progresso = 0;
        _viewModel.Cronometro.Reset();
        _languageController.StatusMessage = MessageMap.GetMessage("select_to_start");
    }

    private async void StartMove_Click(object sender, RoutedEventArgs e)
    {
        StackContainer.Children.Clear();
        await _viewModel.StartMovement();
    }

    private void Cancelation_Click(object sender, RoutedEventArgs e) => _viewModel.Cancel();

    private void ButtonGithub_Click(object sender, RoutedEventArgs e) => _viewModel.LinkIcon();

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        string selectedLanguageKey = MessageMap.ListLanguages().FirstOrDefault(x => x.Value == LanguageCombo.SelectedItem.ToString()).Key;

        _languageController.Language = selectedLanguageKey;

        MessageMap.SetDefaultLanguage(selectedLanguageKey);

        _languageController.StatusMessage = MessageMap.GetMessage("select_to_start");

        if (!string.IsNullOrEmpty(_languageController.AllDoneLabel))
        {
            _languageController.AllDoneLabel = string.Format(
                MessageMap.GetMessage("all_done"),
                _viewModel.ArquivosProcessados,
                _viewModel.TempoFormatado
            );
        }
    }

    private void ThemeToggleButton_Click(object sender, RoutedEventArgs e) => _viewModel.ToggleTheme();
}
