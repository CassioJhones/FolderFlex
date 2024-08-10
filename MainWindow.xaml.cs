using System.Windows;
using Brushes = System.Windows.Media.Brushes;
namespace Movedor;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();
    private int contador = 0;
    private string ultimaPastaSelecionada = "";
    public string Nome { get; set; }
    public string Tamanho { get; set; }
    private async void SelectFolderButton_Click(object sender, RoutedEventArgs e)
    {
        Zerado();
        using FolderBrowserDialog janela = new();
        janela.Description = "Selecione a pasta raiz";
        janela.UseDescriptionForTitle = true;
        janela.ShowNewFolderButton = true;
        janela.SelectedPath = ultimaPastaSelecionada;

        if (janela.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string caminhoSelecionado = janela.SelectedPath;

            try
            {
                ultimaPastaSelecionada = caminhoSelecionado;
                System.Diagnostics.Stopwatch tempo = new();
                tempo.Start();
                await MoverParaRaiz(caminhoSelecionado);
                tempo.Stop();

                TimeSpan tempoDecorrido = tempo.Elapsed;
                string tempoFormatado = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                                      tempoDecorrido.Hours,
                                                      tempoDecorrido.Minutes,
                                                      tempoDecorrido.Seconds);
                MensagemStatus.Text = $"{contador} arquivos movidos com sucesso!";

                TempoDecorrido.Text = tempoFormatado.Equals("00:00:00")
                ? $"Executado em {tempo.Elapsed.Milliseconds}ms\n" : $"Executou em: {tempoFormatado}\n";

                MensagemStatus.Foreground = Brushes.Green;
            }
            catch (Exception ex)
            {
                MensagemStatus.Text = $"Erro: {ex.Message}";
                MensagemStatus.Foreground = Brushes.Red;
            }
        }
    }

    private async Task MoverParaRaiz(string pastaRaiz)
    {
        string[] listaSubPastas = System.IO.Directory.GetDirectories(pastaRaiz, "*", System.IO.SearchOption.AllDirectories);

        int totalArquivos = listaSubPastas.Sum(subPasta => System.IO.Directory.GetFiles(subPasta).Length);
        int arquivosProcessados = 0;
        foreach (string subPasta in listaSubPastas)
        {
            string[] arquivos = System.IO.Directory.GetFiles(subPasta);
            foreach (string file in arquivos)
            {
                string pastaDestino = System.IO.Path.Combine(pastaRaiz, System.IO.Path.GetFileName(file));

                if (System.IO.File.Exists(pastaDestino))
                    MensagemStatus.Text += $"O arquivo {System.IO.Path.GetFileName(file)} já existe na pasta raiz. Pulando arquivo.";
                else
                {
                    System.IO.File.Move(file, pastaDestino);
                    AdicionarArquivoNaLista(pastaDestino);
                    contador++;
                    arquivosProcessados++;
                    Progresso.Value = (double)arquivosProcessados / totalArquivos * 100;
                    await Task.Delay(10);
                }
            }
        }

        foreach (string subPasta in listaSubPastas)
        {
            if (System.IO.Directory.GetFiles(subPasta).Length == 0 && System.IO.Directory.GetDirectories(subPasta).Length == 0) ;
            System.IO.Directory.Delete(subPasta);
        }
    }
    private void AdicionarArquivoNaLista(string caminhoArquivo)
    {
        System.IO.FileInfo info = new(caminhoArquivo);

        string tamanhoConvertido = $"{(info.Length / 1024.0):F3} kb";
        if (info.Length / 1024.0 > 1024)
            tamanhoConvertido = $"{(info.Length / 1024.0 / 1024.0):F3} mb";

        MovidosListBox.Items.Add(new ArquivoInfo
        {
            Rota = caminhoArquivo,
            Nome = System.IO.Path.GetFileNameWithoutExtension(caminhoArquivo),
            Tamanho = tamanhoConvertido,
            Extensao = info.Extension.Substring(1, info.Extension.Length - 1)
        });
    }
    private void Zerado()
    {
        contador = 0;
        Progresso.Value = 0;
        MovidosListBox.Items.Clear();
        MensagemStatus.Text = "Selecione uma pasta para começar...";
        MensagemStatus.Foreground = Brushes.Black;
        TempoDecorrido.Text = "";
    }

    private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        AbrirSite("https://github.com/CassioJhones");
        AbrirSite("https://github.com/CassioJhones/Movedor");
    }

    private void AbrirSite(string link)
    {
        try
        {
            System.Diagnostics.ProcessStartInfo AbrirComNavegadorPadrao = new()
            {
                FileName = link,
                UseShellExecute = true
            };

            System.Diagnostics.Process.Start(AbrirComNavegadorPadrao);
        }
        catch (Exception)
        {
            System.Diagnostics.ProcessStartInfo AbrirNavegador = new()
            {
                FileName = "cmd",
                Arguments = $"/c start {link}",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            System.Diagnostics.Process.Start(AbrirNavegador);
        }
    }

    private void Click_AbrirArquivo(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (MovidosListBox.SelectedItem is ArquivoInfo arquivoSelecionado)
        {
            string caminhoCompleto = arquivoSelecionado.Rota;
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = caminhoCompleto,
                UseShellExecute = true
            });
        }
    }
}
public class ArquivoInfo
{
    public string? Rota { get; set; }
    public string? Nome { get; set; }
    public string? Tamanho { get; set; }
    public string? Extensao { get; set; }
}
