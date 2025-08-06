using Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Persistence
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options) : base (options)
		{
		}
		protected override void OnModelCreating (ModelBuilder builder)
		{
			base.OnModelCreating (builder);
		}

		public DbSet<AccountDetail> AccountDetails { get; set; }
		public DbSet<AuditLog> AuditLogs { get; set; }
		public DbSet<Bank> Banks { get; set; }
		public DbSet<Branch> Branches { get; set; }
		public DbSet<EmailLog> EmailLogs { get; set; }
		public DbSet<EmailRequest> EmailRequests { get; set; }
		public DbSet<EmailTemplate> EmailTemplates { get; set; }
		public DbSet<Payment> Payments { get; set; }
		public DbSet<Transaction> Transactions { get; set; }
		public DbSet<Upload> Uploads { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<Wallet> Wallets { get; set; }

	}
}