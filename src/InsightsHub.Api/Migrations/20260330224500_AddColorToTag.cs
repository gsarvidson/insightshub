using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightsHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddColorToTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Tag",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Tag");
        }
    }
}
