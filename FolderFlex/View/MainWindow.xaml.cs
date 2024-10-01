using FolderFlex.ViewModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Brushes = System.Windows.Media.Brushes;
namespace FolderFlex.View;

public partial class MainWindow : Window
{
    private MainWindowsViewModel _viewModel;
    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainWindowsViewModel();
        DataContext = _viewModel;
    }
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
    private void ExibirTempo(Stopwatch tempo)
    {
        TimeSpan tempoDecorrido = tempo.Elapsed;
        string tempoFormatado = string.Format($"{tempoDecorrido.Hours:D2}:{tempoDecorrido.Minutes:D2}:{tempoDecorrido.Seconds:D2}");

        TempoDecorrido.Text = tempoFormatado.Equals("00:00:00")
            ? $"Executou em {tempo.Elapsed.Milliseconds}ms\n"
            : $"Executou em: {tempoFormatado}\n";
    }
    private void ExibirMensagemStatus()
    {
        MensagemStatus.Foreground = _viewModel.Contador > 0 ? Brushes.Green : Brushes.Red;
        string acao = _viewModel.SomenteCopiar ? "copiado" : "movido";
        if (_viewModel.Contador == 1) _viewModel.MensagemStatus = $"Arquivo {acao} com sucesso!";
        else if (_viewModel.Contador == 0) _viewModel.MensagemStatus = $"Nenhum Arquivo foi {acao}!";
        else if (_viewModel.Contador > 1) _viewModel.MensagemStatus = $"{_viewModel.Contador} arquivos {acao}s com sucesso!";
    }
    private void LimparTela()
    {
        if (_viewModel.Contador == 0) return;
        _viewModel.Contador = 0;
        _viewModel.Progresso = 0;
        _viewModel.ArquivosMovidos.Clear();
        _viewModel.Cronometro.Reset();
        _viewModel.MensagemStatus = "Selecione uma pasta para começar...";
        _viewModel.MensagemErro = "";
        MensagemStatus.Foreground = Brushes.Black;
        TempoDecorrido.Text = "";
    }
    private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => _viewModel.LinkIcone();
    private void Click_AbrirArquivo(object sender, MouseButtonEventArgs e)
        => _viewModel.AbrirArquivo(ArquivosListBox);
    private void Click_Cancelar(object sender, RoutedEventArgs e)
        => _viewModel.Cancelar();
    private void AbrirPastaDoArquivo_Click(object sender, RoutedEventArgs e)
        => _viewModel.OpcaoAbertura(ArquivosListBox);
    private async void Click_Iniciar(object sender, RoutedEventArgs e)
    {
        await _viewModel.IniciarMovimento();

        ExibirMensagemStatus();
        ExibirTempo(_viewModel.Cronometro);
    }

    private async void VerificarVersao(object sender, RoutedEventArgs e)
    {
        await AtualizacaoChecker.VerificarAtualizacaoAsync();
    }
}
