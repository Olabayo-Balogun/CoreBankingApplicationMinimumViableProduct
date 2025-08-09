namespace Application.Model.Users.Queries
{
	public class GetAllUsersByCountryQuery
	{
		public string Country { get; set; }
		public bool IsResidence { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public CancellationToken CancellationToken { get; set; }

	}
}