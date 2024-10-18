using Microsoft.EntityFrameworkCore.Migrations;

namespace Twm.DB.Migrations
{
    public partial class AddInstrumentListType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                schema: "public",
                table: "InstrumentLists",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                schema: "public",
                table: "InstrumentLists");
        }
    }
}
