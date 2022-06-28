#pragma warning disable 1591

using System;
using System.Collections.Generic;

namespace ExtractorFunc.Persistence.Models
{
    public partial class Claim
    {
        public Claim()
        {
            ClaimUploads = new HashSet<ClaimUpload>();
        }

        public int Id { get; set; }
        public int PracticePolicyId { get; set; }
        public int StatusId { get; set; }
        public int TypeId { get; set; }
        public int? ParentId { get; set; }
        public string Diagnosis { get; set; } = null!;
        public string? Details { get; set; }
        public string? SignsInfo { get; set; }
        public string? AdditionalInfo { get; set; }
        public bool NoHistory { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool RelatedCondition { get; set; }
        public bool Reconciled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string UserId { get; set; } = null!;
        public string? HistoryDescription { get; set; }
        public string? RelatedDescription { get; set; }
        public DateTime? ClaimAcceptedAt { get; set; }
        public DateTime? ClaimRejectedAt { get; set; }
        public int? AuthSmsId { get; set; }
        public int? ClaimAuthorisationMethodId { get; set; }
        public string? ClaimAuthorisationUserId { get; set; }
        public string? AssignedToUser { get; set; }
        public DateTime? AssignedDate { get; set; }
        public string? AssignedByUser { get; set; }
        public int? PolicyTypeId { get; set; }
        public int PaymentRecipientId { get; set; }
        public bool? IsDiscountIncluded { get; set; }
        public string? NoHistoryReason { get; set; }
        public string? DiscountDetail { get; set; }
        public bool? IsDiscountScheme { get; set; }
        public decimal FormVersion { get; set; }
        public int? SpitfireClaimId { get; set; }
        public int? SpitfireClaimAccountId { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool? IncludeCremationCosts { get; set; }
        public decimal? CremationCosts { get; set; }
        public bool? Emergency { get; set; }

        public virtual PracticePolicy PracticePolicy { get; set; } = null!;
        public virtual ICollection<ClaimUpload> ClaimUploads { get; set; }
    }
}
#pragma warning restore 1591
