using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightsHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddColorAndAiNotesToOpportunity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiNotes",
                table: "Opportunity",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Opportunity",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiNotes",
                table: "Opportunity");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Opportunity");
        }
    }
}
