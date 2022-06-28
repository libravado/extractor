using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ExtractorFunc.Persistence;
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
            var config = builder.GetContext().Configuration;
            var dbConnection = config.GetConnectionString("SourceDb");
            builder.Services.AddDbContext<SourceDbContext>(options => options.UseSqlServer(dbConnection));

            builder.Services.AddScoped<IClaimDocsRepo, ClaimDocsEfRepo>();
            builder.Services.AddScoped<IDataExtractRepo, DataExtractRepo>();
            builder.Services.AddScoped<IBlobClientService, BlobClientService>();
            builder.Services.AddScoped<IRunConfigParser, RunConfigParser>();
        }
    }
}