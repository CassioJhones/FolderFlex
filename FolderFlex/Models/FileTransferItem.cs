using FolderFlex.Services;
using FolderFlex.Util;
using FolderFlexCommon.Extensions;
using System.IO;
using System.Windows.Input;

namespace FolderFlex.Models;

public sealed class FileTransferItem : ObservableObject, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly string _destinationRoot;

    public FileTransferItem(string sourcePath, string destinationPath, string destinationRoot, long fileSize)
    {
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
        FileSize = fileSize;
        _destinationRoot = destinationRoot;
        DisplayName = sourcePath.TruncatedFileName();
        FileSizeText = fileSize.ToFileSizeLabel();

        OpenFileCommand = new RelayCommand(OpenTransferredFile);
        ActionCommand = new RelayCommand(ExecuteAction);
    }

    public string SourcePath { get; }
    public string DestinationPath { get; }
    public long FileSize { get; }
    public string DisplayName { get; }
    public string FileSizeText { get; }
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public double Progress
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public bool IsCompleted
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public bool IsCancelled
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public ICommand OpenFileCommand { get; }
    public ICommand ActionCommand { get; }

    private void OpenTransferredFile()
    {
        if (!IsCompleted)
            return;

        if (File.Exists(DestinationPath))
            FileService.OpenFile(DestinationPath);
        else if (File.Exists(SourcePath))
            FileService.OpenFile(SourcePath);
    }

    private void ExecuteAction()
    {
        if (!IsCompleted && !IsCancelled)
        {
            IsCancelled = true;
            _cancellationTokenSource.Cancel();
            return;
        }

        string folder = Path.GetDirectoryName(DestinationPath) ?? _destinationRoot;
        FileService.OpenFolder(folder);
    }

    public void Dispose() => _cancellationTokenSource.Dispose();
}
