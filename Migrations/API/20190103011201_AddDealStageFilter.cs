using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AudiocraticAPI.Migrations.API
{
    public partial class AddDealStageFilter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DealStatusChanges");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Deals",
                newName: "StageID");

            migrationBuilder.CreateTable(
                name: "DealStageChanges",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(nullable: false),
                    ChangeDateTime = table.Column<DateTime>(nullable: false),
                    DealID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealStageChanges", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DealStageChanges_Deals_DealID",
                        column: x => x.DealID,
                        principalTable: "Deals",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DealStageChanges_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DealStageFilters",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(nullable: false),
                    StageID = table.Column<string>(nullable: false),
                    StageName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealStageFilters", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DealStageFilters_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DealStageChanges_DealID",
                table: "DealStageChanges",
                column: "DealID");

            migrationBuilder.CreateIndex(
                name: "IX_DealStageChanges_UserId",
                table: "DealStageChanges",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DealStageFilters_UserId",
                table: "DealStageFilters",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DealStageChanges");

            migrationBuilder.DropTable(
                name: "DealStageFilters");

            migrationBuilder.RenameColumn(
                name: "StageID",
                table: "Deals",
                newName: "Status");

            migrationBuilder.CreateTable(
                name: "DealStatusChanges",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChangeDateTime = table.Column<DateTime>(nullable: false),
                    DealID = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
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
                    table.ForeignKey(
                        name: "FK_DealStatusChanges_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DealStatusChanges_DealID",
                table: "DealStatusChanges",
                column: "DealID");

            migrationBuilder.CreateIndex(
                name: "IX_DealStatusChanges_UserId",
                table: "DealStatusChanges",
                column: "UserId");
        }
    }
}
