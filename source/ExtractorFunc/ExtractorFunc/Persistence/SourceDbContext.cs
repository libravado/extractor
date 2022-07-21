#pragma warning disable 1591

using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ExtractorFunc.Persistence.Models;

namespace ExtractorFunc.Persistence
{
    public partial class SourceDbContext : DbContext
    {
        public SourceDbContext()
        {
        }

        public SourceDbContext(DbContextOptions<SourceDbContext> options)
            : base(options)
        {
            if (Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            {
                var connection = (SqlConnection)Database.GetDbConnection();

                if (connection.ConnectionString.Contains("database.windows.net", StringComparison.CurrentCultureIgnoreCase))
                {
                    connection.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result;
                }
            }
        }

        public virtual DbSet<Claim> Claims { get; set; } = null!;
        public virtual DbSet<ClaimUpload> ClaimUploads { get; set; } = null!;
        public virtual DbSet<PracticePolicy> PracticePolicies { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Claim>(entity =>
            {
                entity.ToTable("claims");

                entity.HasIndex(e => new { e.Id, e.PracticePolicyId, e.StatusId, e.PolicyTypeId, e.PaymentRecipientId }, "IX_Claims-20201123-112544-#16192");

                entity.HasIndex(e => new { e.StatusId, e.DeletedAt }, "IX_Claims_StatusId_DeletedAt");

                entity.HasIndex(e => new { e.DeletedAt, e.StatusId }, "nci_wi_claims_73FEFD1F0F7AD8765B347C25DB454006");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AdditionalInfo)
                    .HasColumnType("text")
                    .HasColumnName("additional_info");

                entity.Property(e => e.AssignedByUser).HasMaxLength(128);

                entity.Property(e => e.AssignedDate).HasColumnType("datetime");

                entity.Property(e => e.AssignedToUser).HasMaxLength(128);

                entity.Property(e => e.AuthSmsId).HasColumnName("auth_sms_id");

                entity.Property(e => e.ClaimAcceptedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("claim_accepted_at");

                entity.Property(e => e.ClaimAuthorisationMethodId).HasColumnName("claim_authorisation_method_id");

                entity.Property(e => e.ClaimAuthorisationUserId)
                    .HasMaxLength(128)
                    .HasColumnName("claim_authorisation_user_id");

                entity.Property(e => e.ClaimRejectedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("claim_rejected_at");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CremationCosts).HasColumnType("decimal(19, 4)");

                entity.Property(e => e.DateFrom)
                    .HasColumnType("datetime")
                    .HasColumnName("date_from");

                entity.Property(e => e.DateTo)
                    .HasColumnType("datetime")
                    .HasColumnName("date_to");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Details)
                    .HasColumnType("text")
                    .HasColumnName("details");

                entity.Property(e => e.Diagnosis)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("diagnosis");

                entity.Property(e => e.DiscountDetail)
                    .HasMaxLength(300)
                    .HasColumnName("discount_detail");

                entity.Property(e => e.FormVersion)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("form_version");

                entity.Property(e => e.HistoryDescription)
                    .HasColumnType("text")
                    .HasColumnName("history_description");

                entity.Property(e => e.IsDiscountIncluded).HasColumnName("is_discount_included");

                entity.Property(e => e.IsDiscountScheme).HasColumnName("is_discount_scheme");

                entity.Property(e => e.NoHistory).HasColumnName("no_history");

                entity.Property(e => e.NoHistoryReason)
                    .HasMaxLength(300)
                    .HasColumnName("no_history_reason");

                entity.Property(e => e.ParentId).HasColumnName("parent_id");

                entity.Property(e => e.PaymentRecipientId).HasColumnName("payment_recipient_id");

                entity.Property(e => e.PolicyTypeId).HasColumnName("Policy_Type_Id");

                entity.Property(e => e.PracticePolicyId).HasColumnName("practice_policy_id");

                entity.Property(e => e.PublishedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("published_at");

                entity.Property(e => e.Reconciled).HasColumnName("reconciled");

                entity.Property(e => e.RelatedCondition).HasColumnName("related_condition");

                entity.Property(e => e.RelatedDescription)
                    .HasColumnType("text")
                    .HasColumnName("related_description");

                entity.Property(e => e.SignsInfo)
                    .HasColumnType("text")
                    .HasColumnName("signs_info");

                entity.Property(e => e.StatusId).HasColumnName("status_id");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.UserId)
                    .HasMaxLength(128)
                    .HasColumnName("user_id");

                entity.HasOne(d => d.PracticePolicy)
                    .WithMany(p => p.Claims)
                    .HasForeignKey(d => d.PracticePolicyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_claims_policies");
            });

            modelBuilder.Entity<ClaimUpload>(entity =>
            {
                entity.ToTable("claim_uploads");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.BlobUri).HasColumnType("text");

                entity.Property(e => e.ClaimId).HasColumnName("claim_id");

                entity.Property(e => e.ContentType).HasMaxLength(128);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.Filename)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("filename");

                entity.Property(e => e.Md5)
                    .HasMaxLength(32)
                    .IsUnicode(false)
                    .HasColumnName("md5")
                    .IsFixedLength();

                entity.Property(e => e.StorageId)
                    .HasMaxLength(38)
                    .IsUnicode(false)
                    .HasColumnName("storage_id")
                    .IsFixedLength();

                entity.Property(e => e.TemporaryStorageId)
                    .HasMaxLength(128)
                    .HasColumnName("temporary_storage_id");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.UserId)
                    .HasMaxLength(128)
                    .HasColumnName("user_id");

                entity.HasOne(d => d.Claim)
                    .WithMany(p => p.ClaimUploads)
                    .HasForeignKey(d => d.ClaimId)
                    .HasConstraintName("FK_claim_uploads_claims");
            });

            modelBuilder.Entity<PracticePolicy>(entity =>
            {
                entity.ToTable("practice_policies");

                entity.HasIndex(e => new { e.PracticeId, e.PolicyId }, "idx_Practice_policies_policy_id");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DateRegistered)
                    .HasColumnType("datetime")
                    .HasColumnName("date_registered");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.IsRescue).HasColumnName("is_rescue");

                entity.Property(e => e.PolicyId).HasColumnName("policy_id");

                entity.Property(e => e.PracticeId).HasColumnName("practice_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.UserId)
                    .HasMaxLength(128)
                    .HasColumnName("user_id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
#pragma warning restore 1591
