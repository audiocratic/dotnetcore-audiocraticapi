using Microsoft.EntityFrameworkCore.Migrations;

namespace AudiocraticAPI.Migrations.API
{
    public partial class CorrectHubspotIDToInt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "HubSpotID",
                table: "Deals",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "HubSpotID",
                table: "Contacts",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "HubSpotID",
                table: "Deals",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "HubSpotID",
                table: "Contacts",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
