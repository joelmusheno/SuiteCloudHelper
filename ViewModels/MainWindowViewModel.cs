using SuiteCloudFileUploadHelper.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SuiteCloudFileUploadHelper.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly DirectoryInfo? _baseSdfPackageFolderDirectoryInfo;
    private readonly string _selectedAccount;
    private readonly FileInfo _fileToUpload;
    private DirectoryInfo? SdfFolderDirectoryInfo => _baseSdfPackageFolderDirectoryInfo;

    public string FileToUploadName => _fileToUpload.Name;
    public string SdfFolderName => _baseSdfPackageFolderDirectoryInfo?.Name ?? string.Empty;
    public SdfPackage[] SdfAccountsAvailable { get; }

    public MainWindowViewModel(FileInfo fileInfo)
    {
        _fileToUpload = fileInfo;
        _baseSdfPackageFolderDirectoryInfo = FindSuiteCloudConfigDirectory(_fileToUpload);

        if (_baseSdfPackageFolderDirectoryInfo == null)
        {
            throw new ArgumentException("Unable to find suitecloud.config.js in folder hierarchy");
        }
 
        var projectJsonPath = Path.Combine(_baseSdfPackageFolderDirectoryInfo.FullName, "project.json");
        var packageDefinition = JsonSerializer.Deserialize<PackageDefinition>(File.ReadAllText(projectJsonPath));
        _selectedAccount = packageDefinition?.DefaultAuthId ?? string.Empty;

        SdfAccountsAvailable = GetSdfAccountsAvailable();
    }

    public void SendToAccounts()
    {
        foreach (var package in SdfAccountsAvailable.Where(p => p.IsChecked))
        {
            var suiteCloudFileUploadCommand = "suitecloud file:upload --paths " +
                                              _fileToUpload.FullName.Replace(SdfFolderDirectoryInfo.FullName + "/src/FileCabinet", 
                                                  string.Empty);

            Console.WriteLine($"{package.Name}, {package.IsChecked}");
            Console.WriteLine(suiteCloudFileUploadCommand);

            UpdateProjectJsonFileSdfFolder(SdfFolderDirectoryInfo, package.Name);
            package.Success = ExecuteShellCommand(suiteCloudFileUploadCommand, SdfFolderDirectoryInfo);
        }
    }
    
    private void UpdateProjectJsonFileSdfFolder(DirectoryInfo? baseFolderDirectoryInfo, string accountName)
    {
        var packageDefinition = new PackageDefinition { DefaultAuthId = accountName };
        File.WriteAllText(baseFolderDirectoryInfo.FullName + "/project.json",
            JsonSerializer.Serialize(packageDefinition));
    }
    
    private bool ExecuteShellCommand(string suiteCloudFileUploadCommand, DirectoryInfo? sdfFolderDirectoryInfo)
    {
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

    private DirectoryInfo? FindSuiteCloudConfigDirectory(FileInfo fileInfo)
    {
        var currentDirectory = fileInfo.Directory;

        while (currentDirectory != null)
        {
            if (File.Exists(Path.Combine(currentDirectory.FullName, "suitecloud.config.js")))
            {
                return currentDirectory;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }

    private SdfPackage[] GetSdfAccountsAvailable()
    {

        var command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "cmd"
            : "/bin/bash";

        var commandArguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "/c suitecloud account:manageauth --list"
            : "-c \"suitecloud account:manageauth --list\"";

        var startInfo = new ProcessStartInfo {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = command,
            Arguments = commandArguments,
            RedirectStandardOutput = true
        };

        var environments = new List<SdfPackage>();
        using var process = Process.Start(startInfo);
        using var reader = process?.StandardOutput;
        var ansiEscapeRegex = new Regex(@"\x1B\[([0-9]{1,2}(;[0-9]{1,2})?)?[m|K]?");
        var result = reader?.ReadToEnd();
        result = ansiEscapeRegex.Replace(result!, string.Empty);
        var lines = result.Split('\n');
        foreach (var line in lines)
        {
            var parts = line.Split('|');
            if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
            {
                environments.Add(new SdfPackage {
                    Name = parts[0].Trim(), IsChecked = parts[0].Trim().Equals(_selectedAccount)
                });
            }
        }

        return environments.ToArray();
    }
}

