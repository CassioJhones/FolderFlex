using Microsoft.Win32;

namespace FolderFlex.Services;

internal static class DialogService
{
    public static string? SelectFolder(string title, string? initialDirectory = null)
    {
        OpenFolderDialog dialog = new()
        {
            Title = title,
            InitialDirectory = string.IsNullOrWhiteSpace(initialDirectory) ? string.Empty : initialDirectory,
            Multiselect = false
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }
}
