using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanningPoker.Migrations
{
    /// <inheritdoc />
    public partial class AddRoundNameToGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoundName",
                table: "Games",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoundName",
                table: "Games");
        }
    }
}
