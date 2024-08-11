using System.Diagnostics;
using System.IO;
using System.Windows;
using Brushes = System.Windows.Media.Brushes;
namespace Movedor;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();
    private int contador = 0;
    private string UltimaPastaSelecionada = "";
    public string Nome { get; set; }
    public string Tamanho { get; set; }
    private async void SelectFolderButton_Click(object sender, RoutedEventArgs e)
    {
        Zerado();
        using FolderBrowserDialog janela = new();
        janela.Description = "Selecione a pasta raiz";
        janela.UseDescriptionForTitle = true;
        janela.ShowNewFolderButton = true;
        janela.SelectedPath = UltimaPastaSelecionada;

        if (janela.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string caminhoSelecionado = janela.SelectedPath;

            try
            {
                UltimaPastaSelecionada = caminhoSelecionado;
                Stopwatch cronometro = new();
                cronometro.Start();
                await MoverParaRaiz(caminhoSelecionado);
                cronometro.Stop();

                ExibirMensagemStatus();
                ExibirTempo(cronometro);

            }
            catch (Exception ex)
            {
                MensagemErro.Text = $"\nErro: {ex.Message}";
                MensagemErro.Foreground = Brushes.Red;
            }
        }
    }

    private void ExibirTempo(Stopwatch tempo)
    {
        TimeSpan tempoDecorrido = tempo.Elapsed;
        string tempoFormatado = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                              tempoDecorrido.Hours,
                                              tempoDecorrido.Minutes,
                                              tempoDecorrido.Seconds);

        TempoDecorrido.Text = tempoFormatado.Equals("00:00:00")
                ? $"Executou em {tempo.Elapsed.Milliseconds}ms\n" : $"Executou em: {tempoFormatado}\n";
    }

    private void ExibirMensagemStatus()
    {
        MensagemStatus.Foreground = Brushes.Green;
        if (contador == 1) MensagemStatus.Text = $"Arquivo movido com sucesso!";
        else if (contador == 0)
        {
            MensagemStatus.Foreground = Brushes.Red;
            MensagemStatus.Text = $"Nenhum Arquivo foi movido!";
        }
        else if (contador > 1) MensagemStatus.Text = $"{contador} arquivos movidos com sucesso!";

    }
    private async Task MoverParaRaiz(string pastaRaiz)
    {
        string[] listaSubPastas = Directory.GetDirectories(pastaRaiz, "*", SearchOption.AllDirectories);

        int totalArquivos = listaSubPastas.Sum(pasta => Directory.GetFiles(pasta).Length);
        int arquivosProcessados = 0;
        foreach (string pasta in listaSubPastas)
        {
            string[] arquivos = Directory.GetFiles(pasta);
            foreach (string file in arquivos)
            {
                string pastaDestino = Path.Combine(pastaRaiz, Path.GetFileName(file));

                if (File.Exists(pastaDestino))
                    MensagemErro.Text += $"\nO arquivo {Path.GetFileName(file)} já existe na pasta.";
                else
                {
                    File.Move(file, pastaDestino);
                    AdicionarArquivoNaLista(pastaDestino);
                    contador++;
                    arquivosProcessados++;
                    Progresso.Value = (double)arquivosProcessados / totalArquivos * 100;
                    await Task.Delay(10);
                }
            }
        }

        DeletarPastas(listaSubPastas);
    }

    private void DeletarPastas(string[] subpastas)
    {
        string[] subpastasOrdenadas = subpastas.OrderByDescending(pasta => pasta.Count(c => c == '\\')).ToArray();
        foreach (string subPasta in subpastasOrdenadas)
        {
            try
            {
                if (Directory.GetFiles(subPasta).Length == 0 && Directory.GetDirectories(subPasta).Length == 0)
                    Directory.Delete(subPasta);
            }
            catch (Exception ex)
            {
                MensagemErro.Text += $"\nErro ao deletar a pasta {subPasta}: {ex.Message}";
            }
        }
    }
    private void AdicionarArquivoNaLista(string caminhoArquivo)
    {
        FileInfo info = new(caminhoArquivo);

        string tamanhoConvertido = $"{(info.Length / 1024.0):F3} kb";
        if (info.Length / 1024.0 > 1024)
            tamanhoConvertido = $"{(info.Length / 1024.0 / 1024.0):F3} mb";

        MovidosListBox.Items.Add(new ArquivoInfo
        {
            Rota = caminhoArquivo,
            Nome = Path.GetFileNameWithoutExtension(caminhoArquivo),
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
        MensagemErro.Text = "";
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
            ProcessStartInfo AbrirComNavegadorPadrao = new()
            {
                FileName = link,
                UseShellExecute = true
            };

            Process.Start(AbrirComNavegadorPadrao);
        }
        catch (Exception)
        {
            ProcessStartInfo AbrirNavegador = new()
            {
                FileName = "cmd",
                Arguments = $"/c start {link}",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(AbrirNavegador);
        }
    }

    private void Click_AbrirArquivo(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            if (MovidosListBox.SelectedItem is ArquivoInfo arquivoSelecionado)
            {
                string caminhoCompleto = arquivoSelecionado.Rota;
                Process.Start(new ProcessStartInfo
                {
                    FileName = caminhoCompleto,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception)
        {
            return;

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
