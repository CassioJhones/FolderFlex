using FolderFlex.ViewModel;
using FolderFlexCommon.Messages;
using System.Windows;
using System.Windows.Input;

namespace FolderFlex.View;

public partial class FolderFlexMain : Window
{
    private readonly FolderFlexViewModel _viewModel;
    private readonly FolderFlexMessageProviderViewModel _languageController;
    public FolderFlexMain()
    {
        InitializeComponent();
        _viewModel = new FolderFlexViewModel(this);

        DataContext = _viewModel;

        _languageController = (FolderFlexMessageProviderViewModel)this.FindResource("FolderFlexMessageProviderViewModel");

        string selectedLanguage = MessageMap.ListLanguages().FirstOrDefault(x => x.Key == _languageController.Language).Value;

        LanguageCombo.SelectedItem = selectedLanguage;

    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void Click_SelecionarOrigem(object sender, RoutedEventArgs e)
    {
        LimparTela();
        _viewModel.SelecionarOrigem();
    }
    private void Click_SelecionarDestino(object sender, RoutedEventArgs e)
    {
        LimparTela();
        _viewModel.SelecionarDestino();
    }
    private void LimparTela()
    {
        if (_viewModel.Contador == 0) return;

        _viewModel.Contador = 0;
        _viewModel.Progresso = 0;
        _viewModel.Cronometro.Reset();
        _viewModel.StatusMessage = MessageMap.GetMessage("select_to_start");
    }

    private void Click_Cancelar(object sender, RoutedEventArgs e) => _viewModel.Cancelar();

    private async void StartMove_Click(object sender, RoutedEventArgs e)
    {
        StackContainer.Children.Clear();
        await _viewModel.IniciarMovimento();
    }

    private void Cancelation_Click(object sender, RoutedEventArgs e) => _viewModel.Cancelar();

    private void ButtonGithub_Click(object sender, RoutedEventArgs e) => _viewModel.LinkIcone();

    private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        string selectedLanguageKey = MessageMap.ListLanguages().FirstOrDefault(x => x.Value == LanguageCombo.SelectedItem.ToString()).Key;

        _languageController.Language = selectedLanguageKey;

        MessageMap.SetDefaultLanguage(selectedLanguageKey);
    }
}
