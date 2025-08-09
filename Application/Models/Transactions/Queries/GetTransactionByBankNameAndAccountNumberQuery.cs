namespace Application.Model.Transactions.Queries
{
	public class GetTransactionByBankNameAndAccountNumberQuery
	{
		public string AccountNumber { get; set; }
		public bool IsDepositor { get; set; }
		public string BankName { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public bool IsDeleted { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
