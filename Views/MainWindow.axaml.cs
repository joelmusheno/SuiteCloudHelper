using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SuiteCloudFileUploadHelper.ViewModels;
using System;
using System.IO;
using System.Linq;

namespace SuiteCloudFileUploadHelper.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void CopyToAccountButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            Console.WriteLine("Failed to obtain ViewModel");
            return;
        }

        foreach (var package in viewModel.SdfAccountsAvailable.Where(p => p.IsChecked))
        {
            var suiteCloudFileUploadCommand = "suitecloud file:upload --paths " +
                viewModel.FileToUpload.FullName.Replace(viewModel.SdfFolderDirectoryInfo.FullName + "/src/FileCabinet", 
                    string.Empty);

            Console.WriteLine($"{package.Name}, {package.IsChecked}");
            Console.WriteLine(suiteCloudFileUploadCommand);

            ExecuteShellCommand(suiteCloudFileUploadCommand, viewModel.SdfFolderDirectoryInfo);
        }
    }

    protected void OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                Close();
                break;
        }
    }

    private void ExecuteShellCommand(string suiteCloudFileUploadCommand, DirectoryInfo sdfFolderDirectoryInfo)
    {
        var isWindows = System.Runtime.InteropServices.RuntimeInformation
            .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

        var fileName = isWindows ? "cmd.exe" : "/bin/bash";
        var arguments = isWindows ? $"/c \"{suiteCloudFileUploadCommand}\"" : $"-c \"{suiteCloudFileUploadCommand}\"";
        
        var processInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = sdfFolderDirectoryInfo.FullName
        };

        var process = new System.Diagnostics.Process { StartInfo = processInfo };

        process.Start();

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Console.Write(stdout);
        Console.Write(stderr);

        if (process.ExitCode != 0)
        {
            throw new Exception("Unable to upload files");
        }
    }
}