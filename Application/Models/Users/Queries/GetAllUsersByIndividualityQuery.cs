namespace Application.Model.Users.Queries
{
	public class GetAllUsersByIndividualityQuery
	{
		public bool IsIndividual { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
