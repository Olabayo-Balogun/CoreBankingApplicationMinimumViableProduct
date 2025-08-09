namespace Application.Model.Users.Queries
{
	public class GetAllUsersByDateQuery
	{
		public DateTime Date { get; set; }
		public bool IsDeleted { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
