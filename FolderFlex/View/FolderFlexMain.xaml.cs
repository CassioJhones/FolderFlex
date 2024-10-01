using FolderFlex.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("opaa");
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
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
         => _viewModel.LinkIcone();
        //private void Click_AbrirArquivo(object sender, MouseButtonEventArgs e)
        //    => _viewModel.AbrirArquivo(ArquivosListBox);
        private void Click_Cancelar(object sender, RoutedEventArgs e)
            => _viewModel.Cancelar();
        //private void AbrirPastaDoArquivo_Click(object sender, RoutedEventArgs e)
        //    => _viewModel.OpcaoAbertura(ArquivosListBox);

        private async void StartMove_Click(object sender, RoutedEventArgs e)
        {
            StackContainer.Children.Clear();
            await _viewModel.IniciarMovimento();
            //ExibirMensagemStatus();
            //ExibirTempo(_viewModel.Cronometro);

        }

        private void Cancelation_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Cancelar();
        }
    }
}
