using System.Text.Json;
using System.Text.Json.Serialization;
using ExtractorFunc.Models;

namespace ExtractorFunc.Helpers;

internal static class RunDataHelper
{
    public static MemoryStream PrepareRunDataJson(
        DateTime localTimestamp,
        RunConfig runConfig,
        object data)
    {
        var jsonStream = new MemoryStream();
        var jsonOpts = new JsonSerializerOptions { WriteIndented = true };
        jsonOpts.Converters.Add(new JsonStringEnumConverter());
        JsonSerializer.Serialize(jsonStream, new { localTimestamp, runConfig, data }, jsonOpts);
        jsonStream.Seek(0, SeekOrigin.Begin);
        return jsonStream;
    }
}
