using Movedor.Util;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Movedor.ViewModel;
public class MainWindowsViewModel : INotifyPropertyChanged
{
    #region PROPRIEDADES

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    private CancellationTokenSource _cancelador;
    public CancellationTokenSource Cancelador
    {
        get => _cancelador;
        set
        {
            _cancelador = value;
            OnPropertyChanged(nameof(Cancelador));
        }
    }
    private int _contador;
    public int Contador
    {
        get => _contador;
        set
        {
            _contador = value;
            OnPropertyChanged(nameof(Contador));
        }
    }
    private bool _renomear = false;
    public bool Renomear
    {
        get => _renomear;
        set
        {
            _renomear = value;
            OnPropertyChanged(nameof(Renomear));
        }
    }
    private double _progresso;
    public double Progresso
    {
        get => _progresso;
        set
        {
            _progresso = value;
            OnPropertyChanged(nameof(Progresso));
        }
    }
    private string _mensagemStatus = "Selecione uma pasta para começar...";
    public string MensagemStatus
    {
        get => _mensagemStatus;
        set
        {
            _mensagemStatus = value;
            OnPropertyChanged(nameof(MensagemStatus));
        }
    }
    private string _mensagemErro;
    public string MensagemErro
    {
        get => _mensagemErro;
        set
        {
            _mensagemErro = value;
            OnPropertyChanged(nameof(MensagemErro));
        }
    }
    private string _ultimaPastaSelecionada;
    public string UltimaPastaSelecionada
    {
        get => _ultimaPastaSelecionada;
        set
        {
            _ultimaPastaSelecionada = value;
            OnPropertyChanged(nameof(UltimaPastaSelecionada));
        }
    }
    private string _pastaDestino;
    public string PastaDestino
    {
        get => _pastaDestino;
        set
        {
            _pastaDestino = value;
            OnPropertyChanged(nameof(_pastaDestino));
        }
    }
    public string Nome { get; set; }
    public string Tamanho { get; set; }
    public ObservableCollection<ArquivoInfo> ArquivosMovidos { get; private set; }
    public Stopwatch Cronometro { get; private set; }
    public int ArquivosProcessados = 0;
    #endregion PROPRIEDADES
    public MainWindowsViewModel()
    {
        Cancelador = new CancellationTokenSource();
        ArquivosMovidos = new ObservableCollection<ArquivoInfo>();
        Cronometro = new Stopwatch();
    }
    public async Task SelecionarPasta()
    {
        using FolderBrowserDialog janela = new();
        janela.Description = "SELECIONE A PASTA RAIZ";
        janela.UseDescriptionForTitle = true;
        janela.ShowNewFolderButton = true;
        janela.SelectedPath = UltimaPastaSelecionada;

        if (janela.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string caminhoDestino = "";
            string caminhoSelecionado = janela.SelectedPath;

            caminhoDestino = string.IsNullOrEmpty(PastaDestino) ? caminhoSelecionado : PastaDestino;
            Cancelador = new CancellationTokenSource();

            try
            {
                UltimaPastaSelecionada = caminhoSelecionado;
                Cronometro.Start();
                await MoverParaRaiz(caminhoSelecionado, caminhoDestino, Cancelador.Token);
                Cronometro.Stop();
            }
            catch (OperationCanceledException)
            {
                MensagemErro += $"\nOperação cancelada pelo usuário após mover {Contador}.";
            }
            catch (Exception ex)
            {
                MensagemErro += $"\nErro: {ex.Message}";
            }
        }
    }
    public async Task<string> SelecionarDestino()
    {
        using FolderBrowserDialog janela = new();
        janela.Description = "SELECIONE O DESTINO DOS ARQUIVOS";
        janela.UseDescriptionForTitle = true;
        janela.ShowNewFolderButton = true;
        janela.SelectedPath = UltimaPastaSelecionada;

        if (janela.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            _pastaDestino = janela.SelectedPath;
            DirectoryInfo infoPasta = new(_pastaDestino);
            MensagemStatus = $"Tudo será movido para: {infoPasta.Name}";
            return janela.SelectedPath;
        }
        else
        {
            _pastaDestino = "";
            MensagemStatus = $"Tudo será movido para a Raiz da pasta";
            return janela.SelectedPath;
        }
    }
    private async Task MoverParaRaiz(string pastaRaiz, string destino, CancellationToken cancelador)
    {
        DirectoryInfo infoPasta = new(pastaRaiz);
        string[] listaSubPastas = Directory.GetDirectories(pastaRaiz, "*", SearchOption.AllDirectories);

        if (listaSubPastas.Length <= 0)
        {
            MensagemErro += $"Nenhuma subpasta encontrada em: {infoPasta.Name} \n";
            return;
        }

        int totalArquivos = listaSubPastas.Sum(pasta => Directory.GetFiles(pasta).Length);
        ArquivosProcessados = 0;
        foreach (string pasta in listaSubPastas)
        {
            DirectoryInfo subPastaInfo = new(pastaRaiz);
            cancelador.ThrowIfCancellationRequested();
            string[] arquivos = Directory.GetFiles(pasta);
            foreach (string file in arquivos)
            {
                string pastaDestino = Path.Combine(destino, Path.GetFileName(file));

                if (!File.Exists(pastaDestino))
                {
                    File.Move(file, pastaDestino);
                    AdicionarArquivoNaLista(pastaDestino);
                    Contador++;
                    AtualizarProgresso(totalArquivos);
                    await Task.Delay(10, cancelador);
                }
                else if (File.Exists(pastaDestino) && Renomear)
                {
                    string diretorio = Path.GetDirectoryName(pastaDestino);
                    string nomeArquivo = Path.GetFileNameWithoutExtension(pastaDestino);
                    string extensao = Path.GetExtension(pastaDestino);

                    int contador = 1;
                    string novoCaminho;

                    do
                    {
                        string novoNomeArquivo = $"{nomeArquivo} ({contador}){extensao}";
                        novoCaminho = Path.Combine(diretorio, novoNomeArquivo);
                        contador++;
                    }
                    while (File.Exists(novoCaminho));

                    File.Move(file, novoCaminho);
                    AdicionarArquivoNaLista(novoCaminho);
                    Contador++;
                    AtualizarProgresso(totalArquivos);
                }
                else
                {
                    if (string.IsNullOrEmpty(PastaDestino))
                        MensagemErro += $"O arquivo {Path.GetFileName(file)} já existe na pasta {destino}.\n";
                    else
                        MensagemErro += $"O arquivo {Path.GetFileName(file)} já existe na pasta {_pastaDestino}.\n";

                    AtualizarProgresso(totalArquivos);
                    await Task.Delay(10, cancelador);
                }
            }
        }

        DeletarPastas(listaSubPastas);
    }
    private void AdicionarArquivoNaLista(string pastaDestino)
    {
        FileInfo info = new(pastaDestino);

        string tamanhoConvertido = $"{(info.Length / 1024.0):F2} Kb";
        if (info.Length / 1024.0 > 1024)
            tamanhoConvertido = $"{(info.Length / 1024.0 / 1024.0):F2} Mb";

        ArquivosMovidos.Add(new ArquivoInfo
        {
            Rota = pastaDestino,
            Nome = Path.GetFileNameWithoutExtension(pastaDestino),
            Tamanho = tamanhoConvertido,
            Extensao = Path.GetExtension(pastaDestino)?.TrimStart('.')
        });
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
                MensagemErro += $"\nErro ao deletar a pasta {subPasta}:\n{ex.Message}";
            }
        }
    }
    public void LinkIcone()
        => AbrirSite("https://github.com/CassioJhones/Movedor");
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
    public void AbrirArquivo(System.Windows.Controls.ListBox listBox)
    {
        try
        {
            if (listBox.SelectedItem is ArquivoInfo arquivoSelecionado)
            {
                string caminhoCompleto = arquivoSelecionado.Rota;
                Process.Start(new ProcessStartInfo
                {
                    FileName = caminhoCompleto,
                    UseShellExecute = true
                });
            }
        }
        catch (Win32Exception)
        {
            MensagemErro = $"\nArquivo não encontrado";
        }
        catch (Exception erro)
        {
            MensagemErro = $"\n{erro.Message}";
        }
    }
    public void Cancelar()
    => Cancelador.Cancel();
    public void AtualizarProgresso(int totalArquivos)
    {
        ArquivosProcessados += 1;
        Progresso = (double)ArquivosProcessados / totalArquivos * 100;
    }
}
