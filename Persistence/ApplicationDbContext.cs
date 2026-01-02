using Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System.Text.Json;
using System.Text.Json.Serialization;

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

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            // converter: Dictionary <-> JSON string
            var dictionaryConverter = new ValueConverter<Dictionary<string, string>?, string?> (
                v => v == null ? null : JsonSerializer.Serialize (v, jsonOptions),
                v => string.IsNullOrEmpty (v) ? null : JsonSerializer.Deserialize<Dictionary<string, string>> (v, jsonOptions)
            );

            // comparer: used by EF Core change tracking and snapshotting
            var dictionaryComparer = new ValueComparer<Dictionary<string, string>?> (
                (d1, d2) =>
                    // treat nulls equal, otherwise compare serialized form for deterministic equality
                    d1 == null && d2 == null ? true :
                    d1 == null || d2 == null ? false :
                    JsonSerializer.Serialize (d1, jsonOptions) == JsonSerializer.Serialize (d2, jsonOptions),
                d => d == null ? 0 : JsonSerializer.Serialize (d, jsonOptions).GetHashCode (),
                d => d == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>> (JsonSerializer.Serialize (d, jsonOptions), jsonOptions)
            );

            builder.Entity<Transaction> ()
                .Property (t => t.MetaData)
                .HasConversion (dictionaryConverter)
                .Metadata.SetValueComparer (dictionaryComparer); // attach comparer

            // provider column type
            builder.Entity<Transaction> ().Property (t => t.MetaData).HasColumnType ("nvarchar(max)").IsRequired (false);
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<EmailRequest> EmailRequests { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<Industry> Industries { get; set; }
        public DbSet<IndustryField> IndustryFields { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Upload> Uploads { get; set; }
        public DbSet<User> Users { get; set; }

    }
}