namespace ExtractorFunc.Models;

/// <summary>
/// Result following a run.
/// </summary>
public record RunResult
{
    /// <summary>
    /// Gets the local timestamp of the run.
    /// </summary>
    public DateTime LocalTimestamp { get; } = DateTime.Now;

    /// <summary>
    /// Gets or sets a value indicating whether the run completed ok.
    /// </summary>
    public bool RanToCompletion { get; set; }

    /// <summary>
    /// Gets or sets the number of documents found.
    /// </summary>
    public int? DocumentsFound { get; set; }

    /// <summary>
    /// Gets the number of documents originally found, by claim type.
    /// </summary>
    public Dictionary<ClaimType, int>? DocumentsFoundByClaimType { get; set; }

    /// <summary>
    /// Gets the number of documents successfully exported, by claim type.
    /// </summary>
    public Dictionary<ClaimType, int>? DocumentsExportedByClaimType { get; set; }

    /// <summary>
    /// Gets the number of failed uris.
    /// </summary>
    public int? FailedUriCount => FailedUris?.Count;

    /// <summary>
    /// Gets or sets the failed uris.
    /// </summary>
    public List<string>? FailedUris { get; set; }

    /// <summary>
    /// Gets or sets the original run configuration.
    /// </summary>
    public RunConfig? RunConfig { get; set; }

    /// <summary>
    /// Gets or sets the error message, if any.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
