using Microsoft.EntityFrameworkCore.Migrations;

namespace Identificator_Serv.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(nullable: true),
                    Prefix = table.Column<string>(nullable: true),
                    Next_Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Idents",
                columns: table => new
                {
                    IdentId = table.Column<int>(nullable: false),
                    GroupId = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Idents", x => new { x.IdentId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_Idents_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Prefix",
                table: "Groups",
                column: "Prefix",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Idents_GroupId",
                table: "Idents",
                column: "GroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Idents");

            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}
