namespace Domain.DTO
{
    public class AuditableEntityDto
    {
        public long? Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? DateDeleted { get; set; }
        public string? DeletedBy { get; set; }
        public string? CreatedBy { get; set; }
    }
}
