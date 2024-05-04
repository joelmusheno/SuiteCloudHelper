using SuiteCloudFileUploadHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SuiteCloudFileUploadHelper.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly FileInfo _fileInfo;
    private readonly DirectoryInfo _baseSdfPackageFolder;
    private readonly string _selectedAccount;

    public MainWindowViewModel(FileInfo fileInfo)
    {
        _fileInfo = fileInfo;
        _baseSdfPackageFolder = FindSuiteCloudConfigDirectory(_fileInfo);

        if (_baseSdfPackageFolder == null)
        {
            throw new ArgumentException("Unable to find suitecloud.config.js in folder hierarchy");
        }
 
        var projectJsonPath = Path.Combine(_baseSdfPackageFolder.FullName, "project.json");
        var projectJsonContent = File.ReadAllText(projectJsonPath);
        var projectObj = JsonDocument.Parse(projectJsonContent).RootElement;
        _selectedAccount = projectObj.GetProperty("defaultAuthId").GetString() ?? string.Empty;

        SdfAccountsAvailable = GetSdfAccountsAvailable();
    }

    public string FileToUpload => _fileInfo.Name;
    public string SdfFolder => _baseSdfPackageFolder.Name;
    public SdfPackage[] SdfAccountsAvailable { get; }

    private DirectoryInfo FindSuiteCloudConfigDirectory(FileInfo fileInfo)
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

    public SdfPackage[] GetSdfAccountsAvailable()
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
        result = ansiEscapeRegex.Replace(result, string.Empty);
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

