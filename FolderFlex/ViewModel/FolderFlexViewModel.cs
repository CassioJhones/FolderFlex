using FolderFlex.Models;
using FolderFlex.Services;
using FolderFlex.Util;
using FolderFlexCommon.Extensions;
using FolderFlexCommon.Messages;
using FolderFlexCommon.Settings;
using FolderFlexCommon.Settings.ApplicationSettings;
using System.Buffers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace FolderFlex.ViewModel;

public sealed class FolderFlexViewModel : ObservableObject, IDisposable
{
    private const double CompactWindowHeight = 340;
    private const double ExpandedWindowHeight = 580;
    private const int SmallCopyBufferSize = 81_920;
    private const int LargeCopyBufferSize = 1_048_576;
    private const long LargeBufferThresholdBytes = 10L * 1024 * 1024;
    private const long LargeFileConcurrencyThresholdBytes = 100L * 1024 * 1024;
    private const int MaxParallelSmallTransfers = 10;
    private const int MaxParallelLargeTransfers = 2;
    private const int ErrorNotSameDeviceHResult = unchecked((int)0x80070011);

    private readonly ApplicationSettings _settings;
    private readonly ErrorLogService _errorService = ErrorLogService.Shared;
    private readonly Lock _reservationLock = new();

    private CancellationTokenSource? _cancellationTokenSource;
    private int _contador;
    private int _arquivosProcessados;
    private string? _ultimaPastaSelecionada;

    public FolderFlexViewModel()
    {
        _settings = ApplicationSettings.New(new IniFileParameterStore("config.flx"));
        Messages = new FolderFlexMessageProviderViewModel(_settings);
        Messages.PropertyChanged += OnMessagesPropertyChanged;

        VersionLabel = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;

        SelectSourceCommand = new RelayCommand(SelectSource);
        SelectDestinationCommand = new RelayCommand(SelectDestination);
        StartCommand = new AsyncRelayCommand(StartMovementAsync, () => !IsProcessing);
        CancelCommand = new RelayCommand(Cancel);
        OpenGitHubCommand = new RelayCommand(() => FileService.OpenLink("https://github.com/CassioJhones/FolderFlex"));
        ToggleThemeCommand = new RelayCommand(ToggleTheme);
    }

    public FolderFlexMessageProviderViewModel Messages { get; }
    public ObservableCollection<FileTransferItem> FileItems { get; } = [];

    public string VersionLabel { get; }

    public int Contador => Volatile.Read(ref _contador);
    public int ArquivosProcessados => Volatile.Read(ref _arquivosProcessados);

    public bool IsProcessing
    {
        get => field;
        private set => SetProperty(ref field, value);
    }

    public double WindowHeight
    {
        get => field;
        set => SetProperty(ref field, value);
    } = CompactWindowHeight;

    public bool Renomear
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public bool SepararPorExtensao
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public bool SomenteCopiar
    {
        get => field;
        set
        {
            if (SetProperty(ref field, value))
                Messages.SomenteCopiar = value;
        }
    }

    public double Progresso
    {
        get => field;
        private set => SetProperty(ref field, value);
    }

    public string PastaOrigem
    {
        get => field;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string PastaDestino
    {
        get => field;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string TempoFormatado
    {
        get => field;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    public ICommand SelectSourceCommand { get; }
    public ICommand SelectDestinationCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand OpenGitHubCommand { get; }
    public ICommand ToggleThemeCommand { get; }

    public void SelectSource()
    {
        ResetStatusIfNeeded();
        PastaDestino = string.Empty;

        string? selectedPath = DialogService.SelectFolder(
            MessageMap.GetMessage("select_root_folder"),
            _ultimaPastaSelecionada);

        PastaOrigem = selectedPath ?? string.Empty;
    }

    public void SelectDestination()
    {
        ResetStatusIfNeeded();

        string? selectedPath = DialogService.SelectFolder(
            MessageMap.GetMessage("select_destination_folder"),
            _ultimaPastaSelecionada);

        string actionVerb = SomenteCopiar ? "copiados" : "movidos";

        if (!string.IsNullOrEmpty(selectedPath))
        {
            PastaDestino = selectedPath;
            DirectoryInfo infoPasta = new(PastaDestino);
            Messages.StatusMessage = MessageMap.FormatMessage("all_files_moved_to", actionVerb, infoPasta.Name);
            return;
        }

        PastaDestino = string.Empty;
        Messages.StatusMessage = MessageMap.FormatMessage("without_destiny_moved_to", actionVerb);
    }

    public async Task StartMovementAsync()
    {
        string caminhoDestino = string.IsNullOrWhiteSpace(PastaDestino) ? PastaOrigem : PastaDestino;
        BeforeStart();
        IsProcessing = true;

        try
        {
            if (string.IsNullOrWhiteSpace(PastaOrigem) || !Directory.Exists(PastaOrigem))
            {
                _errorService.AddError(MessageMap.GetMessage("root_folder_not_exists"));
                return;
            }

            _ultimaPastaSelecionada = PastaOrigem;

            long startTimestamp = Stopwatch.GetTimestamp();
            bool completed = await MoveToRootAsync(PastaOrigem, caminhoDestino, _cancellationTokenSource!.Token);
            TimeSpan elapsed = Stopwatch.GetElapsedTime(startTimestamp);

            if (completed)
                FinishProcessing(elapsed);
        }
        catch (OperationCanceledException)
        {
            _errorService.AddError(MessageMap.FormatMessage(
                "operation_cancelled_after_move",
                SomenteCopiar ? "copiar" : "mover",
                Contador));

            ClearFileItems();
        }
        catch (DirectoryNotFoundException)
        {
            _errorService.AddError(MessageMap.GetMessage("root_folder_not_exists"));
        }
        catch (IOException)
        {
            _errorService.AddError(MessageMap.GetMessage("file_used_by_process"));
        }
        catch (Exception ex)
        {
            _errorService.AddError(MessageMap.FormatMessage("throw_error", ex.Message));
        }
        finally
        {
            IsProcessing = false;

            string[] errors = _errorService.DrainErrors();
            if (errors.Length > 0)
                MessageBox.Show(string.Join(Environment.NewLine, errors), "FolderFlex", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    public void Cancel()
    {
        _cancellationTokenSource?.Cancel();

        if (WindowHeight >= ExpandedWindowHeight)
            WindowHeight = CompactWindowHeight;

        PastaDestino = string.Empty;
        PastaOrigem = string.Empty;
    }

    public void ToggleTheme()
    {
        string newTheme = string.Equals(_settings.Theme, "LightTheme", StringComparison.OrdinalIgnoreCase)
            ? "DarkTheme"
            : "LightTheme";

        ThemeService.ApplyTheme(newTheme);
        _settings.Theme = newTheme;
    }

    private async Task<bool> MoveToRootAsync(string pastaRaiz, string destino, CancellationToken cancellationToken)
    {
        DirectoryInfo infoPasta = new(pastaRaiz);

        (string[] subPastas, string[] filesToProcess) = await Task.Run(() =>
        (
            Directory.GetDirectories(pastaRaiz, "*", SearchOption.AllDirectories),
            Directory.GetFiles(pastaRaiz, "*", SearchOption.AllDirectories)
        ), cancellationToken);

        if (subPastas.Length == 0 && filesToProcess.Length == 0)
        {
            _errorService.AddError(MessageMap.FormatMessage("folder_file_not_found", infoPasta.Name));
            return false;
        }

        bool processed = await ProcessFilesAsync(filesToProcess, destino, cancellationToken);
        if (!processed)
            return false;

        if (!SomenteCopiar)
            DeleteEmptyFolders(subPastas);

        return true;
    }

    private async Task<bool> ProcessFilesAsync(string[] filesToProcess, string destino, CancellationToken cancellationToken)
    {
        int totalArquivos = filesToProcess.Length;
        HashSet<string> reservedTargets = [with(StringComparer.OrdinalIgnoreCase)];
        List<FileTransferItem> plannedItems = [with(totalArquivos)];
        long requiredBytes = 0;

        foreach (string sourceFile in filesToProcess)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string destinoFinal = destino;
            if (SepararPorExtensao)
            {
                string extensao = Path.GetExtension(sourceFile).TrimStart('.');
                if (string.IsNullOrEmpty(extensao))
                    extensao = "SemExtensao";

                destinoFinal = Path.Combine(destino, extensao);
            }

            string destinoArquivo = Path.Combine(destinoFinal, Path.GetFileName(sourceFile));

            if (PathsAreEqual(sourceFile, destinoArquivo))
                continue;

            string? resolvedDestination = ResolveDestinationPath(sourceFile, destinoArquivo, reservedTargets);
            if (resolvedDestination is null)
            {
                UpdateProgress(totalArquivos);
                continue;
            }

            long fileSize = File.Exists(sourceFile) ? new FileInfo(sourceFile).Length : 0;
            bool requiresByteCopy = SomenteCopiar || !IsSameVolume(sourceFile, resolvedDestination);
            if (requiresByteCopy)
                requiredBytes += fileSize;

            plannedItems.Add(new FileTransferItem(sourceFile, resolvedDestination, destino, fileSize));
        }

        if (requiredBytes > 0 && !HasSufficientDiskSpace(destino, requiredBytes, out long availableBytes))
        {
            foreach (FileTransferItem item in plannedItems)
                item.Dispose();

            _errorService.AddError(MessageMap.FormatMessage(
                "insufficient_disk_space",
                requiredBytes.ToFileSizeLabel(),
                availableBytes.ToFileSizeLabel()));

            return false;
        }

        using SemaphoreSlim smallFileSemaphore = new(MaxParallelSmallTransfers);
        using SemaphoreSlim largeFileSemaphore = new(MaxParallelLargeTransfers);
        List<Task> tasks = [with(plannedItems.Count)];

        foreach (FileTransferItem transferItem in plannedItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string? destinationDirectory = Path.GetDirectoryName(transferItem.DestinationPath);
            if (!string.IsNullOrEmpty(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            if (WindowHeight < ExpandedWindowHeight)
                WindowHeight = ExpandedWindowHeight;

            FileItems.Add(transferItem);

            bool requiresByteCopy = SomenteCopiar || !IsSameVolume(transferItem.SourcePath, transferItem.DestinationPath);
            bool isLargeStreamTransfer = requiresByteCopy && transferItem.FileSize >= LargeFileConcurrencyThresholdBytes;
            SemaphoreSlim semaphore = isLargeStreamTransfer ? largeFileSemaphore : smallFileSemaphore;

            await semaphore.WaitAsync(cancellationToken);

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await TransferFileAsync(transferItem, totalArquivos, cancellationToken);
                }
                catch (OperationCanceledException) when (transferItem.CancellationToken.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                {
                    // Individual file transfer cancelled by the user.
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _errorService.AddError(MessageMap.FormatMessage("throw_error", ex.Message));
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
        return true;
    }

    private string? ResolveDestinationPath(string sourceFile, string destinoArquivo, HashSet<string> reservedTargets)
    {
        lock (_reservationLock)
        {
            bool exists = File.Exists(destinoArquivo) || reservedTargets.Contains(destinoArquivo);
            if (!exists)
            {
                reservedTargets.Add(destinoArquivo);
                return destinoArquivo;
            }

            if (Renomear)
                return FileService.GetUniqueFilePath(destinoArquivo, reservedTargets);
        }

        _errorService.AddError(MessageMap.FormatMessage(
            "file_already_exist_on_path",
            Path.GetFileName(sourceFile),
            Path.GetDirectoryName(destinoArquivo)));

        return null;
    }

    private async Task TransferFileAsync(FileTransferItem item, int totalArquivos, CancellationToken globalCancellationToken)
    {
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            globalCancellationToken,
            item.CancellationToken);

        CancellationToken token = linkedCts.Token;
        token.ThrowIfCancellationRequested();

        if (!SomenteCopiar && IsSameVolume(item.SourcePath, item.DestinationPath))
        {
            try
            {
                File.Move(item.SourcePath, item.DestinationPath, overwrite: false);
                CompleteTransferItem(item, totalArquivos);
                return;
            }
            catch (IOException ex) when (ex.HResult == ErrorNotSameDeviceHResult)
            {
                // Reparse point or mounted volume pointing to another drive: fall back to streaming copy.
            }
        }

        await CopyStreamWithProgressAsync(item, totalArquivos, token);
    }

    private async Task CopyStreamWithProgressAsync(FileTransferItem item, int totalArquivos, CancellationToken token)
    {
        long fileSize = item.FileSize;
        long totalBytesCopied = 0;
        double lastReportedProgress = -1;
        int bufferSize = fileSize >= LargeBufferThresholdBytes ? LargeCopyBufferSize : SmallCopyBufferSize;

        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        bool copyCompleted = false;

        try
        {
            FileStreamOptions sourceOptions = new()
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.Read,
                BufferSize = bufferSize,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
            };

            FileStreamOptions destinationOptions = new()
            {
                Mode = FileMode.CreateNew,
                Access = FileAccess.Write,
                Share = FileShare.None,
                BufferSize = bufferSize,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
                PreallocationSize = fileSize
            };

            await using (FileStream sourceStream = new(item.SourcePath, sourceOptions))
            await using (FileStream destinationStream = new(item.DestinationPath, destinationOptions))
            {
                Memory<byte> memory = buffer.AsMemory(0, bufferSize);
                int bytesRead;

                while ((bytesRead = await sourceStream.ReadAsync(memory, token)) > 0)
                {
                    await destinationStream.WriteAsync(memory[..bytesRead], token);
                    totalBytesCopied += bytesRead;

                    double progressPercentage = fileSize > 0
                        ? (double)totalBytesCopied / fileSize * 100
                        : 100;

                    if (progressPercentage - lastReportedProgress >= 1 || progressPercentage >= 100)
                    {
                        lastReportedProgress = progressPercentage;
                        item.Progress = progressPercentage;
                    }
                }
            }

            copyCompleted = true;

            if (!SomenteCopiar)
                File.Delete(item.SourcePath);

            CompleteTransferItem(item, totalArquivos);
        }
        catch (OperationCanceledException)
        {
            if (!copyCompleted && File.Exists(item.DestinationPath))
            {
                try { File.Delete(item.DestinationPath); }
                catch (IOException) { }
            }

            item.IsCancelled = true;
            throw;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private void CompleteTransferItem(FileTransferItem item, int totalArquivos)
    {
        item.Progress = 100;
        item.IsCompleted = true;

        Interlocked.Increment(ref _contador);
        OnPropertyChanged(nameof(Contador));
        UpdateProgress(totalArquivos);
    }

    private void DeleteEmptyFolders(string[] subpastas)
    {
        foreach (string subPasta in subpastas.OrderByDescending(static pasta => pasta.DirectoryDepth))
        {
            try
            {
                if (Directory.Exists(subPasta) && !Directory.EnumerateFileSystemEntries(subPasta).Any())
                    Directory.Delete(subPasta, recursive: false);
            }
            catch (Exception ex)
            {
                _errorService.AddError(MessageMap.FormatMessage("error_when_delete_folder", subPasta, ex.Message));
            }
        }
    }

    private void BeforeStart()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        Interlocked.Exchange(ref _contador, 0);
        Interlocked.Exchange(ref _arquivosProcessados, 0);
        OnPropertyChanged(nameof(Contador));
        OnPropertyChanged(nameof(ArquivosProcessados));

        Progresso = 0;
        TempoFormatado = string.Empty;
        Messages.AllDoneLabel = string.Empty;

        ClearFileItems();
    }

    private void FinishProcessing(TimeSpan elapsed)
    {
        TempoFormatado = elapsed.ToFormattedDuration();
        Messages.AllDoneLabel = MessageMap.FormatMessage("all_done", ArquivosProcessados, TempoFormatado);
    }

    private void UpdateProgress(int totalArquivos)
    {
        int processed = Interlocked.Increment(ref _arquivosProcessados);
        OnPropertyChanged(nameof(ArquivosProcessados));
        Progresso = totalArquivos > 0 ? (double)processed / totalArquivos * 100 : 100;
    }

    private void ResetStatusIfNeeded()
    {
        if (Contador == 0)
            return;

        Interlocked.Exchange(ref _contador, 0);
        OnPropertyChanged(nameof(Contador));
        Progresso = 0;
        Messages.StatusMessage = MessageMap.GetMessage("select_to_start");
    }

    private void ClearFileItems()
    {
        foreach (FileTransferItem item in FileItems)
            item.Dispose();

        FileItems.Clear();
    }

    private void OnMessagesPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FolderFlexMessageProviderViewModel.Language) &&
            !string.IsNullOrEmpty(Messages.AllDoneLabel))
        {
            Messages.AllDoneLabel = MessageMap.FormatMessage("all_done", ArquivosProcessados, TempoFormatado);
        }
    }

    private static bool PathsAreEqual(string firstPath, string secondPath)
        => string.Equals(
            Path.GetFullPath(firstPath),
            Path.GetFullPath(secondPath),
            StringComparison.OrdinalIgnoreCase);

    private static bool IsSameVolume(string sourcePath, string destinationPath)
    {
        string? sourceRoot = Path.GetPathRoot(Path.GetFullPath(sourcePath));
        string? destinationRoot = Path.GetPathRoot(Path.GetFullPath(destinationPath));

        return !string.IsNullOrEmpty(sourceRoot) &&
               string.Equals(sourceRoot, destinationRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasSufficientDiskSpace(string destinationPath, long requiredBytes, out long availableBytes)
    {
        availableBytes = long.MaxValue;

        string? root = Path.GetPathRoot(Path.GetFullPath(destinationPath));
        if (string.IsNullOrEmpty(root) || root.Length < 2 || root[1] != ':')
            return true;

        try
        {
            DriveInfo drive = new(root);
            if (!drive.IsReady)
                return true;

            availableBytes = drive.AvailableFreeSpace;
            return availableBytes >= requiredBytes;
        }
        catch (IOException)
        {
            return true;
        }
    }

    public void Dispose()
    {
        Messages.PropertyChanged -= OnMessagesPropertyChanged;
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        ClearFileItems();
    }
}
