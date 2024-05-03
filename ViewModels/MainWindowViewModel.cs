using System.IO;

namespace SuiteCloudFileUploadHelper.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly FileInfo _fileInfo;

    public MainWindowViewModel(FileInfo fileInfo)
    {
        _fileInfo = fileInfo;
    }

    public string File => _fileInfo.Name;
}
