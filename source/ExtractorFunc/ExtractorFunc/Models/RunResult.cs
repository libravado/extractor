namespace ExtractorFunc.Models;

internal record RunResult
{
    public DateTime LocalTimestamp { get; } = DateTime.Now;

    public bool RanToCompletion { get; set; }

    public int? DocumentsFound { get; set; }

    public int? FailedUriCount => FailedUris?.Count;

    public List<string>? FailedUris { get; set; }

    public RunConfig? RunConfig { get; set; }

    public string? ErrorMessage { get; set; }
}
