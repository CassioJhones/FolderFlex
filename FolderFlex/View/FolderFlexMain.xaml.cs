using FolderFlex.ViewModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace FolderFlex.View
{
    public partial class FolderFlexMain : Window
    {
        private readonly FolderFlexViewModel _viewModel;
        public FolderFlexMain()
        {
            InitializeComponent();
            _viewModel = new FolderFlexViewModel(this);
            DataContext = _viewModel;

        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
        private void LimparTela()
        {
            if (_viewModel.Contador == 0) return;
            _viewModel.Contador = 0;
            _viewModel.Progresso = 0;
            _viewModel.ArquivosMovidos.Clear();
            _viewModel.Cronometro.Reset();
            _viewModel.MensagemStatus = "Selecione as pastas para começar";
            _viewModel.MensagemErro = "";
            //MensagemStatus.Foreground = Brushes.Black;
            //TempoDecorrido.Text = "";
        }

        private void Click_Cancelar(object sender, RoutedEventArgs e) => _viewModel.Cancelar();

        private async void StartMove_Click(object sender, RoutedEventArgs e)
        {
            StackContainer.Children.Clear();
            await _viewModel.IniciarMovimento();
            //ExibirMensagemStatus();
            //ExibirTempo(_viewModel.Cronometro);

        }

        private void Cancelation_Click(object sender, RoutedEventArgs e) => _viewModel.Cancelar();

        private void ButtonGithub_Click(object sender, RoutedEventArgs e) => _viewModel.LinkIcone();

    }
}
