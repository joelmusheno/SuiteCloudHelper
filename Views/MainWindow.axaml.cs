using Avalonia.Controls;
using Avalonia.Interactivity;
using SuiteCloudFileUploadHelper.Models;
using SuiteCloudFileUploadHelper.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SuiteCloudFileUploadHelper.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}