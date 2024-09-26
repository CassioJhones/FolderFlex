using FolderFlex.ViewModel;
using System.Text;
namespace FolderFlexTeste;

public class MainWindowsViewModelTests
{
    private readonly MainWindowsViewModel viewModel;
    public MainWindowsViewModelTests()
        => viewModel = new MainWindowsViewModel();

    [Fact(DisplayName = "Contador de arquivos deve iniciar com zero")]
    public void Contador_DeveIniciarEmZero()
    {
        int contadorInicial = viewModel.Contador;
        Assert.Equal(0, contadorInicial);
    }

    #region CHECKBOX PARA RENOMEAR
    [Fact(DisplayName = "Opcão de renomear os arquivos deve iniciar como false")]
    public void Renomear_DeveSerFalsoPorPadrao()
    {
        bool renomearInicial = viewModel.Renomear;
        Assert.False(renomearInicial);
    }

    [Fact(DisplayName = "Renomear deve ser alternado corretamente")]
    public void AlternarRenomear_DeveMudarValorCorretamente()
    {
        bool valorInicial = viewModel.Renomear;

        viewModel.Renomear = !valorInicial;
        Assert.NotEqual(valorInicial, viewModel.Renomear);
    }

    #endregion CHECKBOX PARA RENOMEAR

    #region BARRA DE PROGRESSO
    [Fact(DisplayName = "AtualizarProgresso deve atualizar corretamente após processar um arquivo")]
    public void AtualizarProgresso_DeveAtualizarCorretamente_AposProcessarUmArquivo()
    {
        int totalArquivos = 10;
        viewModel.ArquivosProcessados = 0;
        viewModel.Progresso = 0;

        viewModel.AtualizarProgresso(totalArquivos);

        Assert.Equal(1, viewModel.ArquivosProcessados);
        Assert.Equal(10, viewModel.Progresso);
    }

    [Fact(DisplayName = "AtualizarProgresso deve atualizar corretamente após processar todos os arquivos")]
    public void AtualizarProgresso_DeveSer100_AposProcessarTodosArquivos()
    {
        int totalArquivos = 5;
        viewModel.ArquivosProcessados = 4;
        viewModel.Progresso = 80;

        viewModel.AtualizarProgresso(totalArquivos);

        Assert.Equal(5, viewModel.ArquivosProcessados);
        Assert.Equal(100, viewModel.Progresso);
    }

    [Fact(DisplayName = "AtualizarProgresso deve lançar uma exceção se total de arquivos for zero")]
    public void AtualizarProgresso_DeveLancarExcecao_SeTotalArquivosForZero()
    {
        int totalArquivos = 0;
        Assert.Throws<DivideByZeroException>(() => viewModel.AtualizarProgresso(totalArquivos));
    }

    [Theory(DisplayName = "AtualizarProgresso deve calcular corretamente o progresso")]
    [InlineData(0, 5, 20)]
    [InlineData(1, 5, 40)]
    [InlineData(2, 5, 60)]
    public void AtualizarProgresso_DeveCalcularCorretamente(int arquivosJaProcessados, int totalArquivos, double progressoEsperado)
    {
        viewModel.ArquivosProcessados = arquivosJaProcessados;

        viewModel.AtualizarProgresso(totalArquivos);

        Assert.Equal(progressoEsperado, viewModel.Progresso);
    }
    #endregion BARRA DE PROGRESSO

    #region TAREFA PARA MOVER

    [Fact(DisplayName = "IniciarMovimento deve mover arquivos corretamente")]
    public async Task IniciarMovimento_DeveMoverArquivosCorretamente()
    {
        // Arrange
        viewModel.PastaOrigem = "C:\\TesteOrigem";
        viewModel.PastaDestino = "C:\\TesteDestino";
        Directory.CreateDirectory(viewModel.PastaOrigem);
        Directory.CreateDirectory(viewModel.PastaDestino);

        using (FileStream fs = File.Create($"{viewModel.PastaOrigem}/teste.txt"))
        {
            byte[] info = new UTF8Encoding(true).GetBytes("Texto de teste");
            fs.Write(info, 0, info.Length);
        }

        // Act
        await viewModel.IniciarMovimento();

        // Assert

        Assert.Equal(viewModel.PastaOrigem, viewModel.UltimaPastaSelecionada);
        Assert.Equal(0, viewModel.MensagemErro.Length);

        Assert.True(File.Exists($"{viewModel.PastaDestino}/teste.txt"));
        Assert.False(File.Exists($"{viewModel.PastaOrigem}/teste.txt"));

        Directory.Delete(viewModel.PastaOrigem, true);
        Directory.Delete(viewModel.PastaDestino, true);
    }

    #endregion TAREFA PARA MOVER

}
