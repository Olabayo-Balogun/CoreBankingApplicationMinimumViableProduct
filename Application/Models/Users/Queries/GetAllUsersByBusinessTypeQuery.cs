namespace Application.Model.Users.Queries
{
	public class GetAllUsersByBusinessTypeQuery
	{
		public string BusinessType { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
