using FolderFlex.Factory.MainWindow.ComponentFactory;
using FolderFlex.Services;
using FolderFlex.Services.ErrorManager;
using FolderFlex.View;
using FolderFlexCommon.Messages;
using FolderFlexCommon.Settings;
using FolderFlexCommon.Settings.ApplicationSettings;
using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using ProgressBar = System.Windows.Controls.ProgressBar;

namespace FolderFlex.ViewModel;

public class FolderFlexViewModel : INotifyPropertyChanged
{
    #region PROPERTIES

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public CancellationTokenSource? Cancelador { get; set; }

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

    private bool _separarPorExtensao = false;
    public bool SepararPorExtensao
    {
        get => _separarPorExtensao;
        set
        {
            _separarPorExtensao = value;
            OnPropertyChanged(nameof(SepararPorExtensao));
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

    private string? _pastaDestino;
    public string? PastaDestino
    {
        get => _pastaDestino;
        set
        {
            _pastaDestino = value;
            OnPropertyChanged(nameof(PastaDestino));
        }
    }
    private string? _versionLabel;
    public string? VersionLabel
    {
        get => _versionLabel;
        set
        {
            _versionLabel = value;
            OnPropertyChanged(nameof(VersionLabel));
        }
    }

    private string? _pastaOrigem;
    public string? PastaOrigem
    {
        get => _pastaOrigem;
        set
        {
            _pastaOrigem = value;
            OnPropertyChanged(nameof(PastaOrigem));
        }
    }

    public Stopwatch Cronometro { get; private set; }

    public int ArquivosProcessados = 0;

    private readonly List<string> fileComponents = [];

    private readonly FolderFlexMain _mainWindow;

    private readonly List<string> namesRegistered = [];

    private readonly ErrorHandler errorHandler;

    private readonly FolderFlexMessageProviderViewModel _languageController;
    private Visibility _allDoneVisibility = Visibility.Hidden;

    public Visibility AllDoneVisibility
    {
        get => _allDoneVisibility;
        set
        {
            _allDoneVisibility = value;
            OnPropertyChanged(nameof(AllDoneVisibility));
        }
    }

    private string? _tempoFormatado;

    public string? TempoFormatado
    {
        get => _tempoFormatado;
        set
        {
            _tempoFormatado = value;
            OnPropertyChanged(nameof(TempoFormatado));
        }
    }

    #endregion PROPERTIES
    private readonly ApplicationSettings _settings;
    public FolderFlexViewModel(FolderFlexMain mainWindow, FolderFlexMessageProviderViewModel languageController)
    {
        VersionLabel = Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();
        Cancelador = new CancellationTokenSource();
        Cronometro = new Stopwatch();

        errorHandler = new ErrorHandler();
        errorHandler.Attach(new ErrorLogger());

        _languageController = languageController;
        _settings = ApplicationSettings.New(new IniFileParameterStore("config.flx"));
        _mainWindow = mainWindow;
    }

    public void SelectSource()
    {
        PastaDestino = string.Empty;

        FolderBrowserDialog dialog = DialogService.OpenFolderDialog(MessageMap.GetMessage("select_root_folder"), selectedPath: UltimaPastaSelecionada);
        try
        {
            PastaOrigem = dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : string.Empty;
        }
        finally
        {
            dialog.Dispose();
        }
    }

    public string SelectDestination()
    {
        FolderBrowserDialog dialog = DialogService.OpenFolderDialog(MessageMap.GetMessage("select_destination_folder"), selectedPath: UltimaPastaSelecionada);

        try
        {
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                PastaDestino = dialog.SelectedPath;

                DirectoryInfo infoPasta = new(PastaDestino);

                _languageController.StatusMessage = string.Format(MessageMap.GetMessage("all_files_moved_to"), (SomenteCopiar ? "copiados" : "movidos"), infoPasta.Name);
                return dialog.SelectedPath;
            }

            PastaDestino = string.Empty;

            _languageController.StatusMessage = string.Format(MessageMap.GetMessage("without_destiny_moved_to"), (SomenteCopiar ? "copiados" : "movidos"));

            return dialog.SelectedPath;
        }
        finally
        {
            dialog.Dispose();
        }
    }

    public async Task MoveToRoot(string pastaRaiz, string destino, CancellationToken cancelador)
    {
        DirectoryInfo infoPasta = new(pastaRaiz);

        string[] listaSubPastas = Directory.GetDirectories(pastaRaiz, "*", SearchOption.AllDirectories);
        string[] listaArquivosSoltos = Directory.GetFiles(pastaRaiz, "*", SearchOption.TopDirectoryOnly);

        if (listaSubPastas.Length <= 0 && listaArquivosSoltos.Length <= 0)
        {
            errorHandler.AddError(string.Format(MessageMap.GetMessage("folder_file_not_found"), infoPasta.Name));

            return;
        }

        int totalArquivos = listaSubPastas.Sum(pasta => Directory.GetFiles(pasta).Length) + listaArquivosSoltos.Length;

        string[] listaCompleta = listaArquivosSoltos.Concat(listaSubPastas).ToArray();

        await ProcessFilesOrFolders(listaCompleta, destino, cancelador, totalArquivos);

        if (!_languageController.SomenteCopiar)
            DeleteFolders(listaSubPastas);
    }

    private async Task ProcessFilesOrFolders(string[] lista, string destino, CancellationToken cancelador, int totalArquivos)
    {
        SemaphoreSlim semaphore = new(10);
        List<Task> tasks = [];

        foreach (string item in lista)
        {
            cancelador.ThrowIfCancellationRequested();

            if (Directory.Exists(item))
            {
                string[] arquivos = Directory.GetFiles(item);
                string[] subPastas = Directory.GetDirectories(item);

                await ProcessFilesOrFolders(arquivos, destino, cancelador, totalArquivos);

                await ProcessFilesOrFolders(subPastas, destino, cancelador, totalArquivos);

                continue;
            }

            string destinoFinal = destino;
            if (SepararPorExtensao)
            {
                string extensao = Path.GetExtension(item).TrimStart('.');
                if (string.IsNullOrEmpty(extensao))
                {
                    extensao = "SemExtensao";
                }
                destinoFinal = Path.Combine(destino, extensao);
                Directory.CreateDirectory(destinoFinal);
            }

            string destinoArquivo = Path.Combine(destinoFinal, Path.GetFileName(item));

            if (File.Exists(destinoArquivo)) continue;

            if (_mainWindow.Height < 580) _mainWindow.Height = 580;

            (CancellationToken canceladorItem, ProgressBar progressBar) = AddFileComponent(item);

            await semaphore.WaitAsync(cancelador);

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await MoveCopy(item, destinoArquivo, totalArquivos, progressBar, cancelador, canceladorItem);
                }
                catch (Exception)
                {
                    if (canceladorItem.IsCancellationRequested) return;

                    if (cancelador.IsCancellationRequested) throw new OperationCanceledException();
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancelador));

        }

        await Task.WhenAll(tasks);
    }

    private (CancellationToken itemCancelator, ProgressBar?)
        AddFileComponent(string file)
    {
        CancellationTokenSource cancelatorItem = new();

        if (fileComponents.Contains(file))
        {
            return (cancelatorItem.Token, null);
        }

        fileComponents.Add(file);

        int index = fileComponents.IndexOf(file);

        Border border = FileComponentFactory.CreateContainerBorder();

        Grid grid = new();

        StackPanel stackPanel = FileComponentFactory.CreateStackPanel();

        System.Windows.Controls.Image fileIcon = FileComponentFactory.CreateFileIcon();

        stackPanel.Children.Add(fileIcon);

        TextBlock fileNameTextBlock = FileComponentFactory.CreateFileNameTextBlock(file);

        stackPanel.Children.Add(fileNameTextBlock);

        Button fileButton = FileComponentFactory.CreateFileButton(_mainWindow);

        fileButton.Content = stackPanel;

        grid.Children.Add(fileButton);

        ProgressBar progressBar = FileComponentFactory.CreateItemProgressBar(_mainWindow);

        fileButton.Click += (s, e) =>
        {
            if (File.Exists(file) && progressBar?.Value == 100)
            {
                FileService.OpenFile(file);
            }
        };

        grid.Children.Add(progressBar);

        TextBlock fileSizeTextBlock = FileComponentFactory.CreateFileSizeTextBlock(file);

        grid.Children.Add(fileSizeTextBlock);

        Button actionButton = FileComponentFactory.CreateActionButton(_mainWindow);

        System.Windows.Controls.Image cancelIcon = FileComponentFactory.CreateGameIcon();

        cancelIcon.Name = $"CancelIcon{index}";

        _mainWindow.RegisterName(cancelIcon.Name, cancelIcon);

        namesRegistered.Add(cancelIcon.Name);

        System.Windows.Controls.Image fileSearchIcon = FileComponentFactory.CreateFileSearchIcon();

        fileSearchIcon.Name = $"SearchIcon{index}";

        _mainWindow.RegisterName(fileSearchIcon.Name, fileSearchIcon);

        namesRegistered.Add(fileSearchIcon.Name);

        actionButton.Content = new StackPanel
        {
            Children =
            {
                cancelIcon,
                fileSearchIcon
            }
        };

        actionButton.Click += (s, e) =>
        {
            if (cancelIcon.Visibility == Visibility.Visible)
            {
                cancelatorItem.Cancel();

                progressBar.Visibility = Visibility.Hidden;

                cancelIcon.Visibility = Visibility.Collapsed;

                actionButton.Visibility = Visibility.Collapsed;

                return;
            }
            string? caminhoDestino = string.IsNullOrEmpty(PastaDestino) ? PastaOrigem : PastaDestino;

            if (Directory.Exists(caminhoDestino))
                Process.Start("explorer", caminhoDestino);
        };

        grid.Children.Add(actionButton);

        border.Child = grid;

        _mainWindow.StackContainer.Children.Add(border);

        _mainWindow.ScrollViewerContainer.ScrollToEnd();

        return (cancelatorItem.Token, progressBar);
    }

    private async Task MoveCopy
        (string arquivo, string destinoArquivo, int totalArquivos, ProgressBar? progressBar, CancellationToken cancelador, CancellationToken canceladorItem)
    {
        FileInfo fileInfo = new(arquivo);
        long fileSize = fileInfo.Length;
        long totalBytesCopied = 0;

        if (!File.Exists(destinoArquivo))
        {
            using (FileStream sourceStream = new(arquivo, FileMode.Open, FileAccess.Read))
            using (FileStream destinationStream = new(destinoArquivo, FileMode.CreateNew, FileAccess.Write))
            {
                byte[] buffer = new byte[81920];
                int bytesRead;
                while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancelador)) > 0)
                {
                    await destinationStream.WriteAsync(buffer, 0, bytesRead, canceladorItem);
                    totalBytesCopied += bytesRead;

                    double progressPercentage = (double)totalBytesCopied / fileSize * 100;

                    progressBar?.Dispatcher.Invoke(() => progressBar.Value = progressPercentage, DispatcherPriority.Render);

                    await Task.Delay(10, canceladorItem);

                    if (canceladorItem.IsCancellationRequested)
                        progressBar?.Dispatcher.Invoke(() => progressBar.Visibility = Visibility.Hidden, DispatcherPriority.Render);
                }
            }

            progressBar?.Dispatcher.Invoke(() => progressBar.Value = 100, DispatcherPriority.Render);

            _mainWindow.Dispatcher.Invoke(() =>
            {
                int index = fileComponents.IndexOf(arquivo);

                FrameworkElement? searchIcon = (FrameworkElement)_mainWindow.FindName($"SearchIcon{index}");
                FrameworkElement? cancelIcon = (FrameworkElement)_mainWindow.FindName($"CancelIcon{index}");

                searchIcon!.Visibility = Visibility.Visible;
                cancelIcon!.Visibility = Visibility.Collapsed;
            });

            Contador++;
            UpdateProgress(totalArquivos);

            return;
        }

        if (Renomear)
        {
            string novoCaminho = FileService.RenameFile(destinoArquivo);

            File.Move(arquivo, novoCaminho);

            Contador++;

            UpdateProgress(totalArquivos);

            return;
        }

        errorHandler.AddError(string.Format(MessageMap.GetMessage("file_already_exist_on_path"), Path.GetFileName(arquivo), Path.GetDirectoryName(destinoArquivo)));

        UpdateProgress(totalArquivos);

        await Task.Delay(10, cancelador);
    }

    private void BeforeStart()
    {
        Cancelador?.Cancel();

        Cancelador = new CancellationTokenSource();

        ArquivosProcessados = 0;
        TempoFormatado = string.Empty;
        AllDoneVisibility = Visibility.Hidden;
        Cronometro.Reset();
        ClearRegisteredNames();
    }

    private void ClearRegisteredNames()
    {
        fileComponents.Clear();
        namesRegistered.ForEach(name => _mainWindow.UnregisterName(name));
        namesRegistered.Clear();
    }

    public async Task StartMovement()
    {
        string? caminhoDestino = string.IsNullOrEmpty(PastaDestino) ? PastaOrigem : PastaDestino;
        BeforeStart();

        try
        {
            UltimaPastaSelecionada = PastaOrigem;

            Cronometro.Start();

            await MoveToRoot(PastaOrigem, caminhoDestino, Cancelador.Token);

            Cronometro.Stop();
            TimeSpan tempoDecorrido = Cronometro.Elapsed;
            FinishProcessing(tempoDecorrido);
        }
        catch (OperationCanceledException)
        {
            errorHandler.AddError(string.Format(MessageMap.GetMessage("operation_cancelled_after_move"), SomenteCopiar ? "copiar" : "mover", Contador));

            ClearRegisteredNames();

            _mainWindow.StackContainer.Children.Clear();
        }
        catch (DirectoryNotFoundException)
        {
            errorHandler.AddError(MessageMap.GetMessage("root_folder_not_exists"));
        }
        catch (IOException)
        {
            errorHandler.AddError(MessageMap.GetMessage("file_used_by_process"));
        }
        catch (Exception ex)
        {
            errorHandler.AddError(string.Format(MessageMap.GetMessage("throw_error"), ex.Message));
        }
        finally
        {
            if (errorHandler.GetErrors().Count > 0)
            {
                MessageBox.Show(string.Join('\n', errorHandler.GetErrors()));
                errorHandler.ClearErrors();
            }
            ;
        }
    }

    public void FinishProcessing(TimeSpan? tempoDecorrido = null)
    {
        if (tempoDecorrido is null)
            return;

        TempoFormatado = tempoDecorrido?.TotalSeconds < 1
            ? $"{tempoDecorrido?.Milliseconds}ms"
            : tempoDecorrido?.TotalMinutes < 1
            ? $"{tempoDecorrido?.Seconds}s {tempoDecorrido?.Milliseconds}ms"
            : $"{tempoDecorrido?.Minutes}m {tempoDecorrido?.Seconds}s {tempoDecorrido?.Milliseconds}ms";

        AllDoneVisibility = Visibility.Visible;
        _languageController.AllDoneLabel = string.Format(MessageMap.GetMessage("all_done"), ArquivosProcessados, TempoFormatado);
    }

    public void DeleteFolders(string[] subpastas)
    {
        foreach (string subPasta in subpastas.OrderByDescending(pasta => pasta.Count(c => c == '\\')))
        {
            try
            {
                DeleteRecursively(subPasta);
            }
            catch (Exception ex)
            {
                errorHandler.AddError(string.Format(MessageMap.GetMessage("error_when_delete_folder"), subPasta, ex.Message));
            }
        }
    }

    private void DeleteRecursively(string folderPath)
    {
        foreach (string file in Directory.GetFiles(folderPath))
            File.Delete(file);

        foreach (string subDir in Directory.GetDirectories(folderPath))
            DeleteRecursively(subDir);

        Directory.Delete(folderPath, true);
    }

    public void LinkIcon()
        => FileService.OpenLink("https://github.com/CassioJhones/FolderFlex");

    public void UpdateProgress(int totalArquivos)
    {
        ArquivosProcessados += 1;
        Progresso = (double)ArquivosProcessados / totalArquivos * 100;
    }

    public void Cancel()
    {
        Cancelador?.Cancel();
        if (_mainWindow.Height >= 580) _mainWindow.Height = 340;
        PastaDestino = string.Empty;
        PastaOrigem = string.Empty;
    }

    public void ToggleTheme()
    {
        string currentTheme = _settings.Theme ?? "LightTheme";
        string newTheme = currentTheme == "LightTheme" ? "DarkTheme" : "LightTheme";

        ThemeService.ApplyTheme(newTheme);
        _settings.Theme = newTheme;
    }
}