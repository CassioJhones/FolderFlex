using FolderFlex.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderFlex.Services
{
    static class FileService
    {
        public static void OpenFile(string filePath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
           
        }
        
        public static void OpenLink(string url)
        {
            try
            {
                ProcessStartInfo Open = new()
                {
                    FileName = url,
                    UseShellExecute = true
                };

                Process.Start(Open);
            }
            catch (Exception)
            {
                ProcessStartInfo Open = new()
                {
                    FileName = "cmd",
                    Arguments = $"/c start {url}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(Open);
            }
        }

        public static string RenameFile(string originalPath)
        {
            string? folder = Path.GetDirectoryName(originalPath) ?? string.Empty;
            string? fileName = Path.GetFileNameWithoutExtension(originalPath) ?? string.Empty;
            string? ext = Path.GetExtension(originalPath) ?? string.Empty;

            int count = 1;
            string newPath;

            do
            {
                string novoNomeArquivo = $"{fileName} ({count}){ext}";
                newPath = Path.Combine(folder, novoNomeArquivo);
                count++;
            }
            while (File.Exists(newPath));
            return newPath;
        }

    }
}
