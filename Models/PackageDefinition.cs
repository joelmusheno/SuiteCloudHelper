using System.Diagnostics;
using System.Text.Json.Serialization;

namespace SuiteCloudFileUploadHelper.Models;

[DebuggerDisplay("DefaultAuthId = {DefaultAuthId}")]
public record PackageDefinition
{
    [JsonPropertyName("defaultAuthId")]
    public string DefaultAuthId { get; init; }
}