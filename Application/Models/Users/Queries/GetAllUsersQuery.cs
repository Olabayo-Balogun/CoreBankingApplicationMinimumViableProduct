namespace Application.Model.Users.Queries
{
	public class GetAllUsersQuery
	{
		public bool IsDeleted { get; set; }
		public bool IsIndividual { get; set; }
		public bool IsStaff { get; set; }
		public string? BusinessName { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
