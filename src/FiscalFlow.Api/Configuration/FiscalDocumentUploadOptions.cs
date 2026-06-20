namespace FiscalFlow.Api.Configuration;

public sealed class FiscalDocumentUploadOptions
{
    public const string SectionName =
        "FiscalDocumentUpload";

    public const long DefaultMaxFileSizeBytes =
        2 * 1024 * 1024;

    public long MaxFileSizeBytes { get; init; } =
        DefaultMaxFileSizeBytes;
}
