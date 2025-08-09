namespace Application.Model.Users.Queries
{
	public class GetAllUsersByStateRequestViewModel
	{
		public string State { get; set; }
		public bool IsResidence { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
		public CancellationToken CancellationToken { get; set; }

	}
}