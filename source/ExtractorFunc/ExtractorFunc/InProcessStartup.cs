using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Pawtal.ExtractDocs.Func.Repos;
using Pawtal.ExtractDocs.Func.Services;

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
            builder.Services.AddScoped<IClaimDocumentRepo, ClaimDocumentSqlRepo>();
            builder.Services.AddScoped<IBlobClientService, BlobClientService>();
            builder.Services.AddScoped<IRunConfigParser, RunConfigParser>();
        }
    }
}