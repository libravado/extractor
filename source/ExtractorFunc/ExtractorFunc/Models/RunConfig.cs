namespace ExtractorFunc.Models;

/// <summary>
/// Run configuration.
/// </summary>
/// <param name="ClaimsCreatedFrom">Minimum date for claim created.</param>
/// <param name="ClaimsCreatedTo">Maximum date for claim created.</param>
/// <param name="PracticeIds">Practice ids to include.</param>
public record RunConfig(
    DateTime ClaimsCreatedFrom,
    DateTime ClaimsCreatedTo,
    IEnumerable<int>? PracticeIds);
