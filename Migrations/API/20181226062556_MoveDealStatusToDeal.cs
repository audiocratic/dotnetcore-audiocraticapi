using Microsoft.EntityFrameworkCore.Migrations;

namespace AudiocraticAPI.Migrations.API
{
    public partial class MoveDealStatusToDeal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "DealStatusChanges");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Deals",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Deals");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "DealStatusChanges",
                nullable: true);
        }
    }
}
