using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedTransactionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up (MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable (
                name: "Payments");

            migrationBuilder.AlterColumn<string> (
                name: "SenderBankName",
                table: "Transactions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof (string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string> (
                name: "RecipientAccountName",
                table: "Transactions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof (string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<string> (
                name: "Channel",
                table: "Transactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string> (
                name: "Currency",
                table: "Transactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string> (
                name: "PaymentReferenceId",
                table: "Transactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string> (
                name: "PaymentService",
                table: "Transactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal> (
                name: "MaximumDailyTransferLimitAmount",
                table: "Accounts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down (MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn (
                name: "Channel",
                table: "Transactions");

            migrationBuilder.DropColumn (
                name: "Currency",
                table: "Transactions");

            migrationBuilder.DropColumn (
                name: "PaymentReferenceId",
                table: "Transactions");

            migrationBuilder.DropColumn (
                name: "PaymentService",
                table: "Transactions");

            migrationBuilder.DropColumn (
                name: "MaximumDailyTransferLimitAmount",
                table: "Accounts");

            migrationBuilder.AlterColumn<string> (
                name: "SenderBankName",
                table: "Transactions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof (string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string> (
                name: "RecipientAccountName",
                table: "Transactions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof (string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.CreateTable (
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<long> (type: "bigint", nullable: false)
                        .Annotation ("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal> (type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Channel = table.Column<string> (type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedBy = table.Column<string> (type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Currency = table.Column<string> (type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DateCreated = table.Column<DateTime> (type: "datetime2", nullable: false),
                    DateDeleted = table.Column<DateTime> (type: "datetime2", maxLength: 100, nullable: true),
                    DeletedBy = table.Column<string> (type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsConfirmed = table.Column<bool> (type: "bit", nullable: false),
                    IsDeleted = table.Column<bool> (type: "bit", nullable: false),
                    LastModifiedBy = table.Column<string> (type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModifiedDate = table.Column<DateTime> (type: "datetime2", nullable: true),
                    PaymentReferenceId = table.Column<string> (type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PaymentService = table.Column<string> (type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PublicId = table.Column<string> (type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey ("PK_Payments", x => x.Id);
                });
        }
    }
}
