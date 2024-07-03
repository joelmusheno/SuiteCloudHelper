using SuiteCloudFileUploadHelper.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SuiteCloudFileUploadHelper.Suitecloud.Package;

public class SuitecloudConfiguration
{
    public static ImmutableArray<SdfPackage> GetAccountsAvailable(string selectedAccount)
    {
        var command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "cmd"
            : "/bin/bash";

        var commandArguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "/c suitecloud account:manageauth --list"
            : "-c \"suitecloud account:manageauth --list\"";

        var startInfo = new ProcessStartInfo
        {
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
                environments.Add(new SdfPackage
                {
                    Name = parts[0].Trim(), IsChecked = parts[0].Trim().Equals(selectedAccount)
                });
            }
        }

        return [..environments];
    }

    public static DirectoryInfo? FindPackageRoot(FileInfo fileInfo)
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

    public static string GetSelectedAccount(DirectoryInfo packageRoot)
    {
        var projectJsonPath = Path.Combine(packageRoot.FullName, "project.json");
        var packageDefinition = JsonSerializer.Deserialize<PackageDefinition>(File.ReadAllText(projectJsonPath));
        var selectedAccount = packageDefinition?.DefaultAuthId ?? string.Empty;
        return selectedAccount;
    }
}