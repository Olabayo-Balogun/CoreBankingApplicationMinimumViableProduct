using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
	/// <inheritdoc />
	public partial class ModifiedDailyTransferLimitToDailyDepositLimit : Migration
	{
		/// <inheritdoc />
		protected override void Up (MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn (
				name: "MaximumDailyTransferLimitAmount",
				table: "Accounts",
				newName: "MaximumDailyDepositLimitAmount");
		}

		/// <inheritdoc />
		protected override void Down (MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn (
				name: "MaximumDailyDepositLimitAmount",
				table: "Accounts",
				newName: "MaximumDailyTransferLimitAmount");
		}
	}
}
