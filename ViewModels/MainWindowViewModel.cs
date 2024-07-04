using SuiteCloudFileUploadHelper.Models;
using SuiteCloudFileUploadHelper.Suitecloud.Package;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace SuiteCloudFileUploadHelper.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly DirectoryInfo? _packageRoot;
    private readonly FileInfo _fileToUpload;

    public string FileToUploadName => _fileToUpload.Name;
    public string SdfFolderName => _packageRoot?.Name ?? string.Empty;
    public ImmutableArray<SdfPackage> SdfAccountsAvailable { get; }

    public bool Complete { get; set; }
    public bool Success { get; set; }

    public MainWindowViewModel(FileInfo fileInfo)
    {
        _fileToUpload = fileInfo;
        _packageRoot = SuitecloudConfiguration.FindPackageRoot(_fileToUpload);

        if (_packageRoot == null)
        {
            throw new ArgumentException("Unable to find suitecloud.config.js in folder hierarchy");
        }

        var selectedAccount = SuitecloudConfiguration.GetSelectedAccount(_packageRoot);

        SdfAccountsAvailable = SuitecloudConfiguration.GetAccountsAvailable(selectedAccount);
    }

    public void SendToAccounts()
    {
        Complete = false;

        foreach (var package in SdfAccountsAvailable.Where(p => p.IsChecked))
        {
            Console.WriteLine($"{package.Name}, {package.IsChecked}");
            UpdateProjectJsonFileSdfFolder(_packageRoot, package.Name);
            package.Success = ExecuteShellCommand(_packageRoot!);
        }

        Success = SdfAccountsAvailable.Any(p => p is { IsChecked: true, Success: false });
        Complete = true;
    }

    private void UpdateProjectJsonFileSdfFolder(DirectoryInfo? packageRoot, string accountName)
    {
        var packageDefinition = new PackageDefinition { DefaultAuthId = accountName };
        File.WriteAllText(packageRoot!.FullName + "/project.json",
            JsonSerializer.Serialize(packageDefinition));
    }

    private bool ExecuteShellCommand(DirectoryInfo sdfFolderDirectoryInfo)
    {
        var suiteCloudFileUploadCommand = "suitecloud file:upload --paths " +
                                          _fileToUpload.FullName.Replace(
                                              _packageRoot!.FullName + "/src/FileCabinet",
                                              string.Empty);

        Console.WriteLine(suiteCloudFileUploadCommand);


        var isWindows = RuntimeInformation
            .IsOSPlatform(OSPlatform.Windows);

        var fileName = isWindows ? "cmd.exe" : "/bin/bash";
        var arguments = isWindows ? $"/c \"{suiteCloudFileUploadCommand}\"" : $"-c \"{suiteCloudFileUploadCommand}\"";

        var processInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = sdfFolderDirectoryInfo.FullName
        };

        var process = new Process { StartInfo = processInfo };

        process.Start();

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Console.Write(stdout);
        Console.Write(stderr);

        return process.ExitCode != 0;
    }
}