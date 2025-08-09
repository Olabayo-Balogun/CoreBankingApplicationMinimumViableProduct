namespace Application.Model.Users.Queries
{
	public class GetCountOfAllUserByCountryQuery
	{
		public string Country { get; set; }
		public bool IsResidence { get; set; }
		public bool IsDeleted { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
