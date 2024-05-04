using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SuiteCloudFileUploadHelper.ViewModels;
using System;
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
        var viewModel = DataContext as MainWindowViewModel;
        foreach (var package in viewModel!.SdfAccountsAvailable.Where(p => p.IsChecked))
        {
            Console.WriteLine($"{package.Name}, {package.IsChecked}");
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
}