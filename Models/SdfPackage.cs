using System.Diagnostics;

namespace SuiteCloudFileUploadHelper.Models;

[DebuggerDisplay("Name = {Name}, IsChecked = {IsChecked}")]
public class SdfPackage
{
    public string Name { get; set; }
    public bool IsChecked { get; set; }
}