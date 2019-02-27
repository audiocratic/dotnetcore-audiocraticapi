using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AudiocraticAPI.Migrations.API
{
    public partial class ContactList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactListAddLogs");

            migrationBuilder.CreateTable(
                name: "ContactList",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ContactID = table.Column<int>(nullable: false),
                    ListID = table.Column<string>(nullable: false),
                    ListName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactList", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ContactList_Contacts_ContactID",
                        column: x => x.ContactID,
                        principalTable: "Contacts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactList_ContactID",
                table: "ContactList",
                column: "ContactID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactList");

            migrationBuilder.CreateTable(
                name: "ContactListAddLogs",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AddedDateTime = table.Column<DateTime>(nullable: false),
                    ContactID = table.Column<int>(nullable: false),
                    ListID = table.Column<string>(nullable: false),
                    ListName = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactListAddLogs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ContactListAddLogs_Contacts_ContactID",
                        column: x => x.ContactID,
                        principalTable: "Contacts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactListAddLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactListAddLogs_ContactID",
                table: "ContactListAddLogs",
                column: "ContactID");

            migrationBuilder.CreateIndex(
                name: "IX_ContactListAddLogs_UserId",
                table: "ContactListAddLogs",
                column: "UserId");
        }
    }
}
