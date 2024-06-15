﻿using SuiteCloudFileUploadHelper.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SuiteCloudFileUploadHelper.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly DirectoryInfo? _baseSdfPackageFolderDirectoryInfo;
    private readonly string _selectedAccount;

    public FileInfo FileToUpload { get; }
    public string FileToUploadName => FileToUpload.Name;
    public DirectoryInfo? SdfFolderDirectoryInfo => _baseSdfPackageFolderDirectoryInfo;
    public string SdfFolderName => _baseSdfPackageFolderDirectoryInfo.Name;
    public SdfPackage[] SdfAccountsAvailable { get; }

    public MainWindowViewModel(FileInfo fileInfo)
    {
        FileToUpload = fileInfo;
        _baseSdfPackageFolderDirectoryInfo = FindSuiteCloudConfigDirectory(FileToUpload);

        if (_baseSdfPackageFolderDirectoryInfo == null)
        {
            throw new ArgumentException("Unable to find suitecloud.config.js in folder hierarchy");
        }
 
        var projectJsonPath = Path.Combine(_baseSdfPackageFolderDirectoryInfo.FullName, "project.json");
        var packageDefinition = JsonSerializer.Deserialize<PackageDefinition>(File.ReadAllText(projectJsonPath));
        _selectedAccount = packageDefinition?.DefaultAuthId ?? string.Empty;

        SdfAccountsAvailable = GetSdfAccountsAvailable();
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

