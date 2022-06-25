using ExtractorFunc.Models;

namespace ExtractorFunc.Services;

/// <summary>
/// Parses data into instances of <see cref="RunConfig"/>.
/// </summary>
public interface IRunConfigParser
{
    /// <summary>
    /// Parses a stream containing csv data.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <returns>Run configuration.</returns>
    public RunConfig ParseCsv(Stream input);

    /// <summary>
    /// Parses a stream containing json data.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <returns>Run configuration.</returns>
    public RunConfig ParseJson(Stream input);

    /// <summary>
    /// Parses a stream according to its format as identified by file extension.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <param name="extension">The file extension.</param>
    /// <returns>Run configuration.</returns>
    /// <exception cref="ArgumentException">Unsupported extension.</exception>
    public RunConfig Parse(Stream input, string extension)
    {
        return extension?.ToLower() switch
        {
            ".csv" => ParseCsv(input),
            ".json" => ParseJson(input),
            _ => throw new ArgumentException($"Extension not supported: {extension}"),
        };
    }
}
