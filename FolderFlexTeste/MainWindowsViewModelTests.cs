using FolderFlex.ViewModel;
namespace FolderFlexTeste;

public class MainWindowsViewModelTests
{
    [Fact]
    public void Contador_DeveIniciarEmZero()
    {
        // Arrange
        MainWindowsViewModel viewModel = new();

        // Act
        int contadorInicial = viewModel.Contador;

        // Assert
        Assert.Equal(0, contadorInicial);
    }
}