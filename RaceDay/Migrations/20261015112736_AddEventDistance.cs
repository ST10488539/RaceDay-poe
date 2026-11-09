using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.Migrations
{
    /// <inheritdoc />
    public partial class AddEventDistance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Distance",
                table: "Events",
                type: "decimal(6,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Distance",
                table: "Events");
        }
    }
}
