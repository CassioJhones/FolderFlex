using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderFlex.Services
{
    static class DialogService
    {
        static internal FolderBrowserDialog OpenFolderDialog(string description, string? selectedPath, bool useDescriptionForTitle = true, bool showNewFolderButton = true)
        {
            FolderBrowserDialog dialog = new()
            {
                Description = description,
                UseDescriptionForTitle = useDescriptionForTitle,
                ShowNewFolderButton = showNewFolderButton,
                SelectedPath = selectedPath ?? string.Empty
            };

             return dialog;
          
        }
    }
}
