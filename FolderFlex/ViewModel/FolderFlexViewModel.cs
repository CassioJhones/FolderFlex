using FolderFlex.Util;
using FolderFlex.View;
using MahApps.Metro.IconPacks;
using System.Buffers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace FolderFlex.ViewModel
{
    class FolderFlexViewModel : INotifyPropertyChanged
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
        private string _mensagemStatus = "Selecione as pastas para começar";
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

        private HashSet<string> arquivosProcessados = new HashSet<string>();

        private readonly FolderFlexMain _mainWindow;

        private Dictionary<string, string> FilesDictionary = new Dictionary<string, string>();

        private List<string> namesRegistered = new List<string>();
        #endregion PROPRIEDADES

        public FolderFlexViewModel(FolderFlexMain mainWindow) {
            Cancelador = new CancellationTokenSource();
            ArquivosMovidos = new ObservableCollection<ArquivoInfo>();
            Cronometro = new Stopwatch();
            _mainWindow = mainWindow;
        }

        public void SelecionarOrigem()
        {
            using FolderBrowserDialog janela = new (){
                Description = "SELECIONE A PASTA RAIZ",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true,
                SelectedPath = UltimaPastaSelecionada ?? string.Empty
            };

            PastaDestino = string.Empty;
            PastaOrigem = janela.ShowDialog() == DialogResult.OK ? janela.SelectedPath : string.Empty;
        }

        public string SelecionarDestino()
        {
            using FolderBrowserDialog janela = new()
            {
                Description = "SELECIONE O DESTINO DOS ARQUIVOS",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true,
                SelectedPath = UltimaPastaSelecionada ?? string.Empty,
            };

            if (janela.ShowDialog() == DialogResult.OK)
            {
                PastaDestino = janela.SelectedPath;
                DirectoryInfo infoPasta = new(PastaDestino);
                MensagemStatus = $"Todos arquivos serão {(SomenteCopiar ? "copiados" : "movidos")} para: {infoPasta.Name}";
                return janela.SelectedPath;
            }

            PastaDestino = string.Empty;
            MensagemStatus = $"Sem o destino, Todos arquivos serão {(SomenteCopiar ? "copiados" : "movidos")} para a Raiz da pasta de origem";
            return janela.SelectedPath;
            
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

                    var (canceladorItem, progressBar) = AddFileComponent(file, destino);
                    try
                    {
                        await MoverCopiar(file, pastaDestino, totalArquivos, progressBar, cancelador, canceladorItem);
                    }
                    catch (Exception)
                    {
                        if (canceladorItem.IsCancellationRequested) continue;
                        if (cancelador.IsCancellationRequested) throw new OperationCanceledException();
                    }
                    
                }
            }
        }

        private (CancellationToken itemCancelator, System.Windows.Controls.ProgressBar?) AddFileComponent(string arquivo, string destino)
        {

            var cancelatorItem = new CancellationTokenSource();

            if (arquivosProcessados.Contains(arquivo))
            {
                return (cancelatorItem.Token, null); 
            }

            arquivosProcessados.Add(arquivo);

            var index = arquivosProcessados.ToList().IndexOf(arquivo);

            Border border = new()
            {
                Background = System.Windows.Media.Brushes.White,
                BorderBrush = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#EBECF4")!,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Height = 56,
                Margin = new Thickness(0, 5, 14, 0)
            };

            Grid grid = new();

            StackPanel stackPanel = new()
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            var fileIcon = new PackIconPhosphorIcons
            {
                Kind = PackIconPhosphorIconsKind.FileArrowUpLight,
                Margin = new Thickness(0, 2, 7, 0),
                Foreground = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#1f1446")!
            };

            stackPanel.Children.Add(fileIcon);

            TextBlock fileNameTextBlock = new()
            {
                Text = Path.GetFileName(arquivo).Length > 30 ? $"{Path.GetFileName(arquivo).Substring(0, 30)}..." : Path.GetFileName(arquivo),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 2),
                Foreground = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#1f1446")!
            };

            stackPanel.Children.Add(fileNameTextBlock);

            grid.Children.Add(stackPanel);

            System.Windows.Controls.ProgressBar progressBar = new System.Windows.Controls.ProgressBar
            {
                Style = (Style)_mainWindow.FindResource("RoundedProgressBar"),
                Value = 0,
                Height = 10,
                Width = 340,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 40, 0, 0),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left
            };

            grid.Children.Add(progressBar);

            TextBlock fileSizeTextBlock = new TextBlock
            {
                Text = new FileInfo(arquivo).Length / 1024 + " kb",
                FontWeight = FontWeights.SemiBold,
                Foreground = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#A0A3BD")!,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 50, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };

            grid.Children.Add(fileSizeTextBlock);

            System.Windows.Controls.Button actionButton = new System.Windows.Controls.Button
            {
                Style = (Style)_mainWindow.FindResource("TransparentButton"),
                Margin = new Thickness(10,0, 10, 0),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right
            };

            PackIconGameIcons cancelIcon = new PackIconGameIcons
            {
                Kind = PackIconGameIconsKind.Cancel,
                Foreground = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#AAB8C2")!,
                Height = 18
            };

            cancelIcon.Name = $"CancelIcon{index}";

            _mainWindow.RegisterName(cancelIcon.Name, cancelIcon);

            namesRegistered.Add(cancelIcon.Name);

            PackIconLucide fileSearchIcon = new PackIconLucide
            {
                Kind = PackIconLucideKind.FileSearch,
                Foreground = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#AAB8C2")!,
                Height = 18,
                Visibility = Visibility.Collapsed 
            };

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
                    // Executa a ação de cancelamento
                    //cancelAction?.Invoke();
                    // Alterar ícones: esconder Cancel, mostrar FileSearch

                    cancelatorItem.Cancel();

                    progressBar.Visibility = Visibility.Hidden;

                    cancelIcon.Visibility = Visibility.Collapsed;

                    return;
                    //fileSearchIcon.Visibility = Visibility.Visible;
                }
                var caminhoDestino = string.IsNullOrEmpty(PastaDestino) ? PastaOrigem : PastaDestino;

                if (Directory.Exists(caminhoDestino))
                    Process.Start("explorer.exe", caminhoDestino);
                // Executa a ação de abrir o arquivo
                //openAction?.Invoke();
            };

            grid.Children.Add(actionButton);

            border.Child = grid;

            _mainWindow.StackContainer.Children.Add(border);

            _mainWindow.ScrollViewerContainer.ScrollToEnd();

            return (cancelatorItem.Token, progressBar);
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

        private async Task MoverCopiar(string arquivo, string destinoArquivo, int totalArquivos, System.Windows.Controls.ProgressBar? progressBar, CancellationToken cancelador, CancellationToken canceladorItem)
        {
            FileInfo fileInfo = new FileInfo(arquivo); 
            long fileSize = fileInfo.Length; 
            long totalBytesCopied = 0;

            if (!File.Exists(destinoArquivo))
            {
                try
                {
                    using (FileStream sourceStream = new FileStream(arquivo, FileMode.Open, FileAccess.Read))
                    using (FileStream destinationStream = new FileStream(destinoArquivo, FileMode.CreateNew, FileAccess.Write))
                    {
                        byte[] buffer = new byte[81920];
                        int bytesRead;
                        while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancelador)) > 0)
                        {
                            await destinationStream.WriteAsync(buffer, 0, bytesRead, canceladorItem);
                            totalBytesCopied += bytesRead;

                            double progressPercentage = (double)totalBytesCopied / fileSize * 100;  

                            progressBar?.Dispatcher.Invoke(() =>
                            {
                                progressBar.Value = progressPercentage;
                            }, DispatcherPriority.Render);

                            await Task.Delay(50, canceladorItem);

                            if (canceladorItem.IsCancellationRequested)
                            {
                                progressBar?.Dispatcher.Invoke(() =>
                                {
                                    progressBar.Visibility = Visibility.Hidden;
                                }, DispatcherPriority.Render);
                               
                            }
                        }

                      
                        progressBar?.Dispatcher.Invoke(() =>
                        {
                            progressBar.Value = 100;
                        }, DispatcherPriority.Render);
                    }
                }
                finally
                {
                    AdicionarArquivoNaLista(destinoArquivo);
                    Contador++;
                    AtualizarProgresso(totalArquivos);
                }

                return;
            }

            if (Renomear)
            {
                string novoCaminho = RenomearArquivo(destinoArquivo);
                File.Move(arquivo, novoCaminho);
                AdicionarArquivoNaLista(novoCaminho);
                Contador++;
                AtualizarProgresso(totalArquivos);

                return;
            }

            MensagemErro += $"O arquivo {Path.GetFileName(arquivo)} já existe na pasta {Path.GetDirectoryName(destinoArquivo)}.\n";

            AtualizarProgresso(totalArquivos);
            await Task.Delay(10, cancelador);
            
        }
        private async Task ProcessarArquivosSoltos(string[] listaArquivosSoltos, string destino, CancellationToken cancelador, int totalArquivos)
        {
            foreach (string arquivo in listaArquivosSoltos)
            {
                cancelador.ThrowIfCancellationRequested();
                string destinoArquivo = Path.Combine(destino, Path.GetFileName(arquivo));

                var (canceladorItem, progressBar) = AddFileComponent(arquivo, destino);

                try
                {
                    await MoverCopiar(arquivo, destinoArquivo, totalArquivos, progressBar, cancelador, canceladorItem);
                }
                catch (Exception)
                {
                    if (canceladorItem.IsCancellationRequested) continue;
                    if (cancelador.IsCancellationRequested) throw new OperationCanceledException();
                }
            }
        }
        public async Task IniciarMovimento()
        {
            var caminhoDestino = string.IsNullOrEmpty(PastaDestino) ? PastaOrigem : PastaDestino;
            
            Cancelador = new CancellationTokenSource();
            
            arquivosProcessados.Clear();

            namesRegistered.ForEach(name => _mainWindow.UnregisterName(name));
            namesRegistered.Clear();

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

                arquivosProcessados.Clear();

                namesRegistered.ForEach(name => _mainWindow.UnregisterName(name));
                namesRegistered.Clear();
                _mainWindow.StackContainer.Children.Clear();

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

        public void AtualizarProgresso(int totalArquivos)
        {
            if (totalArquivos is 0) throw new DivideByZeroException();
            ArquivosProcessados += 1;
            Progresso = (double)ArquivosProcessados / totalArquivos * 100;
        }
        public void Cancelar()
        => Cancelador?.Cancel();
    }
}
