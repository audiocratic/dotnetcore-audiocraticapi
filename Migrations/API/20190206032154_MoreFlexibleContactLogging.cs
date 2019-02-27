using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AudiocraticAPI.Migrations.API
{
    public partial class MoreFlexibleContactLogging : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Deals_DealID",
                table: "Contacts");

            migrationBuilder.AlterColumn<int>(
                name: "HubSpotID",
                table: "Contacts",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "DealID",
                table: "Contacts",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Contacts",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Deals_DealID",
                table: "Contacts",
                column: "DealID",
                principalTable: "Deals",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Deals_DealID",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Contacts");

            migrationBuilder.AlterColumn<int>(
                name: "HubSpotID",
                table: "Contacts",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DealID",
                table: "Contacts",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Deals_DealID",
                table: "Contacts",
                column: "DealID",
                principalTable: "Deals",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
