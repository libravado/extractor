using System.Text.Json;
using System.Text.RegularExpressions;
using ExtractorFunc.Models;

namespace ExtractorFunc.Services;

/// <inheritdoc cref="IRunConfigParser"/>
public class RunConfigParser : IRunConfigParser
{
    private const string CsvMatchesRegex = "\"[^\"]*\"|[^,\\s*]+";

    /// <inheritdoc/>
    public RunConfig ParseCsv(Stream input)
    {
        using var reader = new StreamReader(input);
        var headers = Regex.Matches(reader.ReadLine()!, CsvMatchesRegex);
        var dicto = Regex.Matches(reader.ReadLine()!, CsvMatchesRegex)
            .Select((m, i) => new { label = headers[i].Value.Trim('"'), value = m.Value.Trim('"') })
            .ToDictionary(kvp => kvp.label, kvp => kvp.value);

        var from = DateTime.Parse(dicto[nameof(RunConfig.ClaimsCreatedFrom)]);
        var to = DateTime.Parse(dicto[nameof(RunConfig.ClaimsCreatedTo)]);
        var ids = dicto.TryGetValue(nameof(RunConfig.PracticeIds), out var idsString)
            ? idsString.Split(',').Select(id => int.Parse(id)).ToList()
            : null;

        return new(from, to, ids);
    }

    /// <inheritdoc/>
    public RunConfig ParseJson(Stream input)
        => JsonSerializer.Deserialize<RunConfig>(input)!;
}
