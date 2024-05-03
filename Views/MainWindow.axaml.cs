using Avalonia.Controls;
using Avalonia.Interactivity;
using SuiteCloudFileUploadHelper.ViewModels;
using System;

namespace SuiteCloudFileUploadHelper.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void CopyToAccountButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as MainWindowViewModel;
        foreach (var package in viewModel!.SdfAccountsAvailable)
        {
            Console.WriteLine($"{package.Name}, {package.IsChecked}");
        }
    }
}