using FolderFlex.ViewModel;
namespace FolderFlexTeste;

public class MainWindowsViewModelTests
{
    private readonly MainWindowsViewModel viewModel;
    public MainWindowsViewModelTests()
        => viewModel = new MainWindowsViewModel();
    
    [Fact(DisplayName ="Contador de arquivos deve iniciar com zero")]
    public void Contador_DeveIniciarEmZero()
    {
        int contadorInicial = viewModel.Contador;
        Assert.Equal(0, contadorInicial);
    }

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
}
