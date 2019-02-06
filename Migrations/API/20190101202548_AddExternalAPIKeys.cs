using Microsoft.EntityFrameworkCore.Migrations;

namespace AudiocraticAPI.Migrations.API
{
    public partial class AddExternalAPIKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConstantContactPrivateKey",
                table: "APIKey",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConstantContactPublicKey",
                table: "APIKey",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HubSpotKey",
                table: "APIKey",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConstantContactPrivateKey",
                table: "APIKey");

            migrationBuilder.DropColumn(
                name: "ConstantContactPublicKey",
                table: "APIKey");

            migrationBuilder.DropColumn(
                name: "HubSpotKey",
                table: "APIKey");
        }
    }
}
