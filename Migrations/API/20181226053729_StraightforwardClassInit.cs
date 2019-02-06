using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AudiocraticAPI.Migrations.API
{
    public partial class StraightforwardClassInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.CreateTable(
            //     name: "AspNetUsers",
            //     columns: table => new
            //     {
            //         Id = table.Column<string>(nullable: false),
            //         UserName = table.Column<string>(nullable: true),
            //         NormalizedUserName = table.Column<string>(nullable: true),
            //         Email = table.Column<string>(nullable: true),
            //         NormalizedEmail = table.Column<string>(nullable: true),
            //         EmailConfirmed = table.Column<bool>(nullable: false),
            //         PasswordHash = table.Column<string>(nullable: true),
            //         SecurityStamp = table.Column<string>(nullable: true),
            //         ConcurrencyStamp = table.Column<string>(nullable: true),
            //         PhoneNumber = table.Column<string>(nullable: true),
            //         PhoneNumberConfirmed = table.Column<bool>(nullable: false),
            //         TwoFactorEnabled = table.Column<bool>(nullable: false),
            //         LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
            //         LockoutEnabled = table.Column<bool>(nullable: false),
            //         AccessFailedCount = table.Column<int>(nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            //     });

            migrationBuilder.CreateTable(
                name: "Deals",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HubSpotID = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deals", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DealID = table.Column<int>(nullable: false),
                    HubSpotID = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Contacts_Deals_DealID",
                        column: x => x.DealID,
                        principalTable: "Deals",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DealStatusChanges",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChangeDateTime = table.Column<DateTime>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    DealID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealStatusChanges", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DealStatusChanges_Deals_DealID",
                        column: x => x.DealID,
                        principalTable: "Deals",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_DealID",
                table: "Contacts",
                column: "DealID");

            migrationBuilder.CreateIndex(
                name: "IX_DealStatusChanges_DealID",
                table: "DealStatusChanges",
                column: "DealID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "DealStatusChanges");

            migrationBuilder.DropTable(
                name: "Deals");
        }
    }
}
