using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SuiteCloudFileUploadHelper.ViewModels;
using SuiteCloudFileUploadHelper.Views;
using System;
using System.IO;
using System.Linq;

namespace SuiteCloudFileUploadHelper;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var absoluteFilePath = desktop.Args?.FirstOrDefault();
            if (!File.Exists(absoluteFilePath))
            {
                ArgumentNullException.ThrowIfNull(absoluteFilePath);
            }

            var fileInfo = new FileInfo(absoluteFilePath); 
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(fileInfo),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}