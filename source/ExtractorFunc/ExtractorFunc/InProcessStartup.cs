using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ExtractorFunc.Repos;
using ExtractorFunc.Services;

[assembly: FunctionsStartup(typeof(ExtractorFunc.InProcessStartup))]

namespace ExtractorFunc
{
    /// <summary>
    /// Start-up for the in-process function.
    /// </summary>
    public class InProcessStartup : FunctionsStartup
    {
        /// <inheritdoc/>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<IClaimDocsRepo, ClaimDocsSqlRepo>();
            builder.Services.AddScoped<IDataExtractRepo, DataExtractRepo>();
            builder.Services.AddScoped<IBlobClientService, BlobClientService>();
            builder.Services.AddScoped<IRunConfigParser, RunConfigParser>();
        }
    }
}