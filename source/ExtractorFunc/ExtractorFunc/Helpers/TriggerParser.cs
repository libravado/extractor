using System.Text.Json;
using System.Text.RegularExpressions;
using ExtractorFunc.Models;

namespace ExtractorFunc.Helpers;

internal static class TriggerParser
{
    private const string CsvMatchesRegex = "\"[^\"]*\"|[^,\\s*]+";

    public static RunConfig ReadTriggerConfig(Stream input, string fileName)
    {
        var fileExtension = Path.GetExtension(fileName);
        switch (fileExtension?.ToLower())
        {
            case ".json":
                return JsonSerializer.Deserialize<RunConfig>(input)
                    ?? throw new InvalidOperationException("Failed to parse json file.");
            case ".csv":
                using (var reader = new StreamReader(input))
                {
                    var headers = Regex.Matches(reader.ReadLine()!, CsvMatchesRegex);
                    var dicto = Regex.Matches(reader.ReadLine()!, CsvMatchesRegex)
                        .Select((m, i) => new { label = headers[i].Value.Trim('"'), value = m.Value.Trim('"') })
                        .ToDictionary(kvp => kvp.label, kvp => kvp.value);

                    var from = DateTime.Parse(dicto[nameof(RunConfig.ClaimsCreatedFrom)]);
                    var to = DateTime.Parse(dicto[nameof(RunConfig.ClaimsCreatedTo)]);
                    var ids = dicto[nameof(RunConfig.PracticeIds)].Split(',').Select(id => int.Parse(id)).ToList();
                    return new(from, to, ids);
                }
            default:
                throw new ArgumentException($"Unrecognised trigger file format: {fileExtension}");
        }
    }
}
