#pragma warning disable 1591


namespace ExtractorFunc.Persistence.Models
{
    public partial class ClaimUpload
    {
        public int Id { get; set; }
        public string Filename { get; set; } = null!;
        public string StorageId { get; set; } = null!;
        public string BlobUri { get; set; } = null!;
        public string Md5 { get; set; } = null!;
        public int TypeId { get; set; }
        public string UserId { get; set; } = null!;
        public int? ClaimId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? TemporaryStorageId { get; set; }
        public string? ContentType { get; set; }

        public virtual Claim? Claim { get; set; }
    }
}
#pragma warning restore 1591
