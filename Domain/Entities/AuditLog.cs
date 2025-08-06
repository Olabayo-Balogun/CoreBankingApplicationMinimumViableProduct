namespace Domain.Entities
{
	public class AuditLog : AuditableEntity
	{
		public string PublicId { get; set; }
		public string Name { get; set; }
		public string Payload { get; set; }
	}
}
