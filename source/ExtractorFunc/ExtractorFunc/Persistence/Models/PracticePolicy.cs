#pragma warning disable 1591


namespace ExtractorFunc.Persistence.Models
{
    public partial class PracticePolicy
    {
        public PracticePolicy()
        {
            Claims = new HashSet<Claim>();
        }

        public int Id { get; set; }
        public int PracticeId { get; set; }
        public int PolicyId { get; set; }
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? DateRegistered { get; set; }
        public bool? IsRescue { get; set; }

        public virtual ICollection<Claim> Claims { get; set; }
    }
}
#pragma warning restore 1591
