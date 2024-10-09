using FolderFlex.Util;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;

namespace FolderFlex.ViewModel;
public class MainWindowsViewModel : INotifyPropertyChanged
{
    #region PROPRIEDADES

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    private CancellationTokenSource? _cancelador;
    public CancellationTokenSource? Cancelador
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
    private bool _somenteCopiar = false;
    public bool SomenteCopiar
    {
        get => _somenteCopiar;
        set
        {
            _somenteCopiar = value;
            OnPropertyChanged(nameof(SomenteCopiar));
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
    private string _mensagemErro = "";
    public string MensagemErro
    {
        get => _mensagemErro;
        set
        {
            _mensagemErro = value;
            OnPropertyChanged(nameof(MensagemErro));
        }
    }
    private string? _ultimaPastaSelecionada;
    public string? UltimaPastaSelecionada
    {
        get => _ultimaPastaSelecionada;
        set
        {
            _ultimaPastaSelecionada = value;
            OnPropertyChanged(nameof(UltimaPastaSelecionada));
        }
    }
    private string? _pastaDestino = "Destino...";
    public string? PastaDestino
    {
        get => _pastaDestino;
        set
        {
            _pastaDestino = value;
            OnPropertyChanged(nameof(PastaDestino));
        }
    }
    private string? _pastaOrigem = "Origem...";
    public string? PastaOrigem
    {
        get => _pastaOrigem;
        set
        {
            _pastaOrigem = value;
            OnPropertyChanged(nameof(PastaOrigem));
        }
    }
    public string? Nome { get; set; }
    public string? Tamanho { get; set; }
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
    #region Tarefas

    public async Task IniciarMovimento()
    {
        string? caminhoDestino = "";
        caminhoDestino = string.IsNullOrEmpty(PastaDestino) ? PastaOrigem : PastaDestino;
        Cancelador = new CancellationTokenSource();

        try
        {
            UltimaPastaSelecionada = PastaOrigem;
            Cronometro.Start();
            await MoverParaRaiz(PastaOrigem, caminhoDestino, Cancelador.Token);
            Cronometro.Stop();
        }
        catch (OperationCanceledException)
        {
            MensagemErro += $"\nOperação cancelada pelo usuário após {(SomenteCopiar ? "copiar" : "mover")} {Contador}.";
        }
        catch (DirectoryNotFoundException)
        {
            MensagemErro += $"\nPasta raiz inexistente ou não selecionada";
        }
        catch (IOException)
        {
            MensagemErro += $"\nSendo usado por outro processo";
        }
        catch (Exception ex)
        {
            MensagemErro += $"\nErro: {ex.Message}";
        }
    }

    public async Task MoverParaRaiz(string pastaRaiz, string destino, CancellationToken cancelador)
    {
        DirectoryInfo infoPasta = new(pastaRaiz);
        string[] listaSubPastas = Directory.GetDirectories(pastaRaiz, "*", SearchOption.AllDirectories);
        string[] listaArquivosSoltos = Directory.GetFiles(pastaRaiz, "*", SearchOption.TopDirectoryOnly);

        if (listaSubPastas.Length <= 0 && listaArquivosSoltos.Length <= 0)
        {
            MensagemErro += $"Nenhuma subpasta encontrada em: {infoPasta.Name} \n";
            return;
        }

        int totalArquivos = listaSubPastas.Sum(pasta => Directory.GetFiles(pasta).Length) + listaArquivosSoltos.Length;
        ArquivosProcessados = 0;

        Task pastas = ProcessarPastas(listaSubPastas, destino, cancelador, totalArquivos);
        Task arquivos = ProcessarArquivosSoltos(listaArquivosSoltos, destino, cancelador, totalArquivos);

        await Task.WhenAll(pastas, arquivos);

        if (!SomenteCopiar)
            DeletarPastas(listaSubPastas);
    }

    private async Task ProcessarPastas(string[] listaSubPastas, string destino, CancellationToken cancelador, int totalArquivos)
    {
        foreach (string pasta in listaSubPastas)
        {
            string[] arquivos = Directory.GetFiles(pasta);
            foreach (string file in arquivos)
            {
                cancelador.ThrowIfCancellationRequested();
                string pastaDestino = Path.Combine(destino, Path.GetFileName(file));
                await MoverCopiar(file, pastaDestino, totalArquivos, cancelador);
            }
        }
    }

    private async Task MoverCopiar(string arquivo, string destinoArquivo, int totalArquivos, CancellationToken cancelador)
    {
        if (!File.Exists(destinoArquivo))
        {
            if (SomenteCopiar) File.Copy(arquivo, destinoArquivo);
            else File.Move(arquivo, destinoArquivo);

            AdicionarArquivoNaLista(destinoArquivo);
            Contador++;
            AtualizarProgresso(totalArquivos);
            await Task.Delay(10, cancelador);
        }
        else if (Renomear)
        {
            string novoCaminho = RenomearArquivo(destinoArquivo);
            File.Move(arquivo, novoCaminho);
            AdicionarArquivoNaLista(novoCaminho);
            Contador++;
            AtualizarProgresso(totalArquivos);
        }
        else
        {
            MensagemErro += $"O arquivo {Path.GetFileName(arquivo)} já existe na pasta {Path.GetDirectoryName(destinoArquivo)}.\n";

            AtualizarProgresso(totalArquivos);
            await Task.Delay(10, cancelador);
        }
    }
    private async Task ProcessarArquivosSoltos(string[] listaArquivosSoltos, string destino, CancellationToken cancelador, int totalArquivos)
    {
        foreach (string arquivo in listaArquivosSoltos)
        {
            cancelador.ThrowIfCancellationRequested();
            string destinoArquivo = Path.Combine(destino, Path.GetFileName(arquivo));

            await MoverCopiar(arquivo, destinoArquivo, totalArquivos, cancelador);
        }
    }
    #endregion Tarefas
    [SupportedOSPlatform("windows")]
    public void SelecionarOrigem()
    {
        using FolderBrowserDialog janela = new();
        janela.Description = "SELECIONE A PASTA RAIZ";
        janela.UseDescriptionForTitle = true;
        janela.ShowNewFolderButton = true;
        janela.SelectedPath = UltimaPastaSelecionada ?? "";
        PastaDestino = "";
        PastaOrigem = janela.ShowDialog() == System.Windows.Forms.DialogResult.OK ? janela.SelectedPath : "";
    }
    [SupportedOSPlatform("windows")]
    public string SelecionarDestino()
    {
        using FolderBrowserDialog janela = new();
        janela.Description = "SELECIONE O DESTINO DOS ARQUIVOS";
        janela.UseDescriptionForTitle = true;
        janela.ShowNewFolderButton = true;
        janela.SelectedPath = UltimaPastaSelecionada ?? "";

        if (janela.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            PastaDestino = janela.SelectedPath;
            DirectoryInfo infoPasta = new(PastaDestino);
            MensagemStatus = $"Tudo será {(SomenteCopiar ? "copiado" : "movido")} para: {infoPasta.Name}";
            return janela.SelectedPath;
        }
        else
        {
            PastaDestino = "";
            MensagemStatus = $"Sem o destino, Tudo será {(SomenteCopiar ? "copiado" : "movido")} para a Raiz da pasta origem";
            return janela.SelectedPath;
        }
    }
    public string RenomearArquivo(string caminhoOriginal)
    {
        string? diretorio = Path.GetDirectoryName(caminhoOriginal) ?? "";
        string? nomeArquivo = Path.GetFileNameWithoutExtension(caminhoOriginal) ?? "";
        string? extensao = Path.GetExtension(caminhoOriginal) ?? "";

        int contador = 1;
        string novoCaminho;

        do
        {
            string novoNomeArquivo = $"{nomeArquivo} ({contador}){extensao}";
            novoCaminho = Path.Combine(diretorio, novoNomeArquivo);
            contador++;
        }
        while (File.Exists(novoCaminho));
        return novoCaminho;
    }
    public void AdicionarArquivoNaLista(string pastaDestino)
    {
        FileInfo info = new(pastaDestino);
        double tamanhoKB = info.Length / 1024.0;

        string tamanhoConvertido = tamanhoKB > 1024 * 1024
            ? $"{(tamanhoKB / 1024.0 / 1024.0):F2} GB"
            : tamanhoKB > 1024 ? $"{(tamanhoKB / 1024.0):F2} MB" : $"{tamanhoKB:F2} KB";

        ArquivosMovidos.Add(new ArquivoInfo
        {
            Rota = pastaDestino,
            Nome = Path.GetFileNameWithoutExtension(pastaDestino),
            Tamanho = tamanhoConvertido,
            Extensao = Path.GetExtension(pastaDestino)?.TrimStart('.')
        });
    }
    public void DeletarPastas(string[] subpastas)
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
        => AbrirSite("https://github.com/CassioJhones/FolderFlex");
    public void AbrirSite(string link)
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
                string? caminhoCompleto = arquivoSelecionado.Rota ?? "";
                Process.Start(new ProcessStartInfo
                {
                    FileName = caminhoCompleto,
                    UseShellExecute = true
                });
            }
        }
        catch (Win32Exception)
        {
            MensagemErro += $"\nArquivo não encontrado";
        }
        catch (Exception erro)
        {
            MensagemErro += $"\n{erro.Message}";
        }
    }
    public void Cancelar()
    => Cancelador?.Cancel();
    public void AtualizarProgresso(int totalArquivos)
    {
        if (totalArquivos is 0) throw new DivideByZeroException();
        ArquivosProcessados += 1;
        Progresso = (double)ArquivosProcessados / totalArquivos * 100;
    }
    public void OpcaoAbertura(System.Windows.Controls.ListBox ArquivosListBox)
    {
        if (ArquivosListBox.SelectedItem is ArquivoInfo arquivoSelecionado)
        {
            string? diretorio = Path.GetDirectoryName(arquivoSelecionado.Rota) ?? "";

            try
            {
                if (string.IsNullOrEmpty(diretorio))
                {
                    MensagemErro += $"O caminho do diretório está vazio ou inválido: {arquivoSelecionado.Rota} \n";
                    return;
                }

                if (Directory.Exists(diretorio))
                    Process.Start("explorer.exe", diretorio);
                else
                    MensagemErro += $"Pasta não encontrada: {diretorio} \n";
            }
            catch (UnauthorizedAccessException ex)
            {
                MensagemErro += $"Acesso negado ao abrir a pasta: {diretorio}. Detalhes: {ex.Message}\n";
            }
            catch (DirectoryNotFoundException ex)
            {
                MensagemErro += $"Diretório não encontrado: {diretorio}. Detalhes: {ex.Message}\n";
            }
            catch (ArgumentException ex)
            {
                MensagemErro += $"O caminho fornecido é inválido: {diretorio}. Detalhes: {ex.Message}\n";
            }
            catch (InvalidOperationException ex)
            {
                MensagemErro += $"Erro ao iniciar o processo para abrir a pasta: {diretorio}. Detalhes: {ex.Message}\n";
            }
            catch (Exception ex)
            {
                MensagemErro += $"Erro inesperado ao tentar abrir a pasta: {diretorio}. Detalhes: {ex.Message}\n";
            }
        }
        else MensagemErro += $"Nenhum arquivo foi selecionado.\n";

    }
}