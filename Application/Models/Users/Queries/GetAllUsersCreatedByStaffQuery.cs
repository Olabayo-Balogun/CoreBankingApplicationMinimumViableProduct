namespace Application.Model.Users.Queries
{
	public class GetAllUsersCreatedByStaffQuery
	{
		public string UserId { get; set; }
		public bool IsDeleted { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
