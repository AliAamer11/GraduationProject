using Microsoft.EntityFrameworkCore.Migrations;

namespace GraduationProject.Migrations
{
    public partial class ModifacationitemsandInputDoumentDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Items");

            migrationBuilder.AddColumn<int>(
                name: "ExceededMinimumRange",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "InputDocumentDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Supplier",
                table: "InputDocumentDetails",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExceededMinimumRange",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "InputDocumentDetails");

            migrationBuilder.DropColumn(
                name: "Supplier",
                table: "InputDocumentDetails");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Items",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
