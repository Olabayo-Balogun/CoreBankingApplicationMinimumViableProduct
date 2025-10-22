using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
	/// <inheritdoc />
	public partial class ModifiedTransactionsAgain : Migration
	{
		/// <inheritdoc />
		protected override void Up (MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string> (
				name: "SenderAccountNumber",
				table: "Transactions",
				type: "nvarchar(100)",
				maxLength: 100,
				nullable: true,
				oldClrType: typeof (string),
				oldType: "nvarchar(1000)",
				oldMaxLength: 1000);

			migrationBuilder.AlterColumn<string> (
				name: "SenderAccountName",
				table: "Transactions",
				type: "nvarchar(100)",
				maxLength: 100,
				nullable: true,
				oldClrType: typeof (string),
				oldType: "nvarchar(1000)",
				oldMaxLength: 1000);

			migrationBuilder.AlterColumn<string> (
				name: "RecipientBankName",
				table: "Transactions",
				type: "nvarchar(1000)",
				maxLength: 1000,
				nullable: true,
				oldClrType: typeof (string),
				oldType: "nvarchar(1000)",
				oldMaxLength: 1000);

			migrationBuilder.AlterColumn<string> (
				name: "RecipientAccountNumber",
				table: "Transactions",
				type: "nvarchar(100)",
				maxLength: 100,
				nullable: false,
				oldClrType: typeof (string),
				oldType: "nvarchar(1000)",
				oldMaxLength: 1000);

			migrationBuilder.AlterColumn<string> (
				name: "RecipientAccountName",
				table: "Transactions",
				type: "nvarchar(100)",
				maxLength: 100,
				nullable: true,
				oldClrType: typeof (string),
				oldType: "nvarchar(1000)",
				oldMaxLength: 1000,
				oldNullable: true);
		}

		/// <inheritdoc />
		protected override void Down (MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string> (
				name: "SenderAccountNumber",
				table: "Transactions",
				type: "nvarchar(1000)",
				maxLength: 1000,
				nullable: false,
				defaultValue: "",
				oldClrType: typeof (string),
				oldType: "nvarchar(100)",
				oldMaxLength: 100,
				oldNullable: true);

			migrationBuilder.AlterColumn<string> (
				name: "SenderAccountName",
				table: "Transactions",
				type: "nvarchar(1000)",
				maxLength: 1000,
				nullable: false,
				defaultValue: "",
				oldClrType: typeof (string),
				oldType: "nvarchar(100)",
				oldMaxLength: 100,
				oldNullable: true);

			migrationBuilder.AlterColumn<string> (
				name: "RecipientBankName",
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
				name: "RecipientAccountNumber",
				table: "Transactions",
				type: "nvarchar(1000)",
				maxLength: 1000,
				nullable: false,
				oldClrType: typeof (string),
				oldType: "nvarchar(100)",
				oldMaxLength: 100);

			migrationBuilder.AlterColumn<string> (
				name: "RecipientAccountName",
				table: "Transactions",
				type: "nvarchar(1000)",
				maxLength: 1000,
				nullable: true,
				oldClrType: typeof (string),
				oldType: "nvarchar(100)",
				oldMaxLength: 100,
				oldNullable: true);
		}
	}
}
