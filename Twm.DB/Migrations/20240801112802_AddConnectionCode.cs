using Microsoft.EntityFrameworkCore.Migrations;

namespace Twm.DB.Migrations
{
    public partial class AddConnectionCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "public",
                table: "Connections",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                schema: "public",
                table: "Connections");
        }
    }
}
