using Application.Interface.Infrastructure;

using Hangfire;

namespace API.Jobs
{
	public static class BackgroundJobs
	{
		public static IConfiguration? config;
		public static void Initialize (IConfiguration Configuration)
		{
			config = Configuration;
		}

		public static async Task RegisterJobs ()
		{
			CancellationToken cancellationToken = CancellationToken.None;
			RecurringJob.AddOrUpdate<IEmailerService> ("SendUnsentEmails", x => x.SendUnsentEmailsAsync (), Cron.Minutely);
		}
	}
}
