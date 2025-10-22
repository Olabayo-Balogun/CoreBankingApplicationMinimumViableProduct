namespace Application.Interface.Infrastructure
{
	public interface IEmailerService
	{
		Task SendUnsentEmailsAsync ();
	}
}
