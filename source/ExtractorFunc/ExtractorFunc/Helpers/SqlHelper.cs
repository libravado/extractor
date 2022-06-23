using System.Data;
using System.Data.SqlClient;
using ExtractorFunc.Models;

namespace ExtractorFunc.Helpers;

internal static class SqlHelper
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

    public static List<ClaimDocument> GatherDocumentData(
        string connectionString,
        RunConfig triggerData)
    {
        using var sqlConnection = new SqlConnection(connectionString);
        sqlConnection.Open();

        using var command = new SqlCommand(SourceDocSql, sqlConnection);
        command.Parameters.Add(SqlParamDateFrom, SqlDbType.DateTime2);
        command.Parameters.Add(SqlParamDateTo, SqlDbType.DateTime2);
        command.Parameters.Add(SqlParamPracticeIdsCsv, SqlDbType.VarChar);
        command.Parameters[SqlParamDateFrom].Value = triggerData.ClaimsCreatedFrom;
        command.Parameters[SqlParamDateTo].Value = triggerData.ClaimsCreatedTo;
        command.Parameters[SqlParamPracticeIdsCsv].Value = triggerData.PracticeIds == null
            ? DBNull.Value
            : string.Join(',', triggerData.PracticeIds);

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
