using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ExtractorFunc.Models;

namespace ExtractorFunc.Repos;

/// <inheritdoc cref="IClaimDocsRepo"/>
public class ClaimDocsSqlRepo : IClaimDocsRepo
{
    private const string SqlParamDateFrom = "@ClaimsCreatedFrom";
    private const string SqlParamDateTo = "@ClaimsCreatedTo";
    private const string SqlParamPracticeIdsCsv = "@PracticeIdsCsv";
    private const string SqlAliasPracticeId = "PracticeId";
    private const string SqlAliasClaimId = "ClaimId";
    private const string SqlAliasClaimType = "ClaimType";
    private const string SqlAliasDocumentType = "DocumentType";
    private const string SqlAliasBlobUri = "BlobUri";
    private const string SourceDocSql = $@"
drop table  if exists #pids
select      value into #pids    from string_split({SqlParamPracticeIdsCsv}, ',')
select      pp.practice_id      {SqlAliasPracticeId},
            c.id                {SqlAliasClaimId},
            c.type_id           {SqlAliasClaimType},
            cu.type_id          {SqlAliasDocumentType},
            cast (cu.BlobUri as varchar(512)) {SqlAliasBlobUri}
from        practice_policies pp
inner join  claims c            on pp.id = c.practice_policy_id
inner join  claim_uploads cu    on c.id = cu.claim_id
left join   #pids pid           on pp.practice_id = pid.value
where       c.created_at        between {SqlParamDateFrom} and {SqlParamDateTo}
and         {SqlParamPracticeIdsCsv} is null or pid.value is not null
and         c.type_id           in (1, 2)
";

    private readonly string connectionString;

    /// <summary>
    /// Initialises a new instance of the <see cref="ClaimDocsSqlRepo"/>
    /// class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    public ClaimDocsSqlRepo(IConfiguration config)
    {
        connectionString = config.GetConnectionString("SourceDb");
    }
    
    /// <inheritdoc/>
    public List<ClaimDocument> GetClaimDocuments(RunConfig runConfig)
    {
        using var sqlConnection = new SqlConnection(connectionString);
        sqlConnection.Open();

        using var command = new SqlCommand(SourceDocSql, sqlConnection);
        command.Parameters.Add(SqlParamDateFrom, SqlDbType.DateTime2);
        command.Parameters.Add(SqlParamDateTo, SqlDbType.DateTime2);
        command.Parameters.Add(SqlParamPracticeIdsCsv, SqlDbType.VarChar);
        command.Parameters[SqlParamDateFrom].Value = runConfig.ClaimsCreatedFrom;
        command.Parameters[SqlParamDateTo].Value = runConfig.ClaimsCreatedTo;
        command.Parameters[SqlParamPracticeIdsCsv].Value = runConfig.PracticeIds == null
            ? DBNull.Value
            : string.Join(',', runConfig.PracticeIds);

        using var reader = command.ExecuteReader();
        var retVal = new List<ClaimDocument>();
        while (reader.Read())
        {
            retVal.Add(MapSqlRow(reader));
        }

        return retVal;
    }

    private static ClaimDocument MapSqlRow(SqlDataReader reader) => new(
        (int)reader[SqlAliasPracticeId],
        (int)reader[SqlAliasClaimId],
        (ClaimType)reader[SqlAliasClaimType],
        (DocumentType)reader[SqlAliasDocumentType],
        (string)reader[SqlAliasBlobUri]);
}
