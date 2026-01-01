using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedIndustryAndIndustryFieldEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up (MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long> (
                name: "IndustryId",
                table: "Users",
                type: "bigint",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable (
                name: "Industries",
                columns: table => new
                {
                    Id = table.Column<long> (type: "bigint", nullable: false)
                        .Annotation ("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string> (type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string> (type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool> (type: "bit", nullable: false),
                    DateCreated = table.Column<DateTime> (type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime> (type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string> (type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateDeleted = table.Column<DateTime> (type: "datetime2", maxLength: 100, nullable: true),
                    DeletedBy = table.Column<string> (type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<string> (type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey ("PK_Industries", x => x.Id);
                });

            migrationBuilder.CreateTable (
                name: "IndustryFields",
                columns: table => new
                {
                    Id = table.Column<long> (type: "bigint", nullable: false)
                        .Annotation ("SqlServer:Identity", "1, 1"),
                    IndustryId = table.Column<long> (type: "bigint", nullable: false),
                    Name = table.Column<string> (type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataType = table.Column<string> (type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsRequired = table.Column<bool> (type: "bit", nullable: false),
                    Order = table.Column<int> (type: "int", nullable: false),
                    ToolTip = table.Column<string> (type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool> (type: "bit", nullable: false),
                    DateCreated = table.Column<DateTime> (type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime> (type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string> (type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateDeleted = table.Column<DateTime> (type: "datetime2", maxLength: 100, nullable: true),
                    DeletedBy = table.Column<string> (type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<string> (type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey ("PK_IndustryFields", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down (MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable (
                name: "Industries");

            migrationBuilder.DropTable (
                name: "IndustryFields");

            migrationBuilder.DropColumn (
                name: "IndustryId",
                table: "Users");
        }
    }
}
