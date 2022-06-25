namespace ExtractorFunc.Models;

/// <summary>
/// Claim type.
/// </summary>
public enum ClaimType
{
    /// <summary>
    /// Pre-authorisation claim type.
    /// </summary>
    PreAuth = 1,

    /// <summary>
    /// Standard claim type.
    /// </summary>
    Claim = 2,

    /// <summary>
    /// Continuation claim type.
    /// </summary>
    Continuation = 3,
}
