using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightsHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAiProcessedToFeedbackItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAiProcessed",
                table: "FeedbackItem",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAiProcessed",
                table: "FeedbackItem");
        }
    }
}
