using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InsightsHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSource",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    LastSynced = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSource", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Opportunity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Sub = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opportunity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedView",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Meta = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedView", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Team",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackItem",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Meta = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Sentiment = table.Column<string>(type: "text", nullable: false),
                    OpportunityId = table.Column<string>(type: "text", nullable: true),
                    UserType = table.Column<string>(type: "text", nullable: false),
                    Platform = table.Column<string>(type: "text", nullable: false),
                    AiNote = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackItem_Opportunity_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityDataSource",
                columns: table => new
                {
                    DataSourcesId = table.Column<string>(type: "text", nullable: false),
                    OpportunitiesId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityDataSource", x => new { x.DataSourcesId, x.OpportunitiesId });
                    table.ForeignKey(
                        name: "FK_OpportunityDataSource_DataSource_DataSourcesId",
                        column: x => x.DataSourcesId,
                        principalTable: "DataSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityDataSource_Opportunity_OpportunitiesId",
                        column: x => x.OpportunitiesId,
                        principalTable: "Opportunity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityTag",
                columns: table => new
                {
                    OpportunitiesId = table.Column<string>(type: "text", nullable: false),
                    TagsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityTag", x => new { x.OpportunitiesId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_OpportunityTag_Opportunity_OpportunitiesId",
                        column: x => x.OpportunitiesId,
                        principalTable: "Opportunity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityTag_Tag_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityTeam",
                columns: table => new
                {
                    OpportunitiesId = table.Column<string>(type: "text", nullable: false),
                    TeamsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityTeam", x => new { x.OpportunitiesId, x.TeamsId });
                    table.ForeignKey(
                        name: "FK_OpportunityTeam_Opportunity_OpportunitiesId",
                        column: x => x.OpportunitiesId,
                        principalTable: "Opportunity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityTeam_Team_TeamsId",
                        column: x => x.TeamsId,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackItemDataSource",
                columns: table => new
                {
                    DataSourcesId = table.Column<string>(type: "text", nullable: false),
                    FeedbackItemsId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackItemDataSource", x => new { x.DataSourcesId, x.FeedbackItemsId });
                    table.ForeignKey(
                        name: "FK_FeedbackItemDataSource_DataSource_DataSourcesId",
                        column: x => x.DataSourcesId,
                        principalTable: "DataSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedbackItemDataSource_FeedbackItem_FeedbackItemsId",
                        column: x => x.FeedbackItemsId,
                        principalTable: "FeedbackItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackItemTag",
                columns: table => new
                {
                    FeedbackItemsId = table.Column<string>(type: "text", nullable: false),
                    TagsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackItemTag", x => new { x.FeedbackItemsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_FeedbackItemTag_FeedbackItem_FeedbackItemsId",
                        column: x => x.FeedbackItemsId,
                        principalTable: "FeedbackItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedbackItemTag_Tag_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackItemTeam",
                columns: table => new
                {
                    FeedbackItemsId = table.Column<string>(type: "text", nullable: false),
                    TeamsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackItemTeam", x => new { x.FeedbackItemsId, x.TeamsId });
                    table.ForeignKey(
                        name: "FK_FeedbackItemTeam_FeedbackItem_FeedbackItemsId",
                        column: x => x.FeedbackItemsId,
                        principalTable: "FeedbackItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedbackItemTeam_Team_TeamsId",
                        column: x => x.TeamsId,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Team",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "property-b2b" },
                    { 2, "property-b2c" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackItem_OpportunityId",
                table: "FeedbackItem",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackItemDataSource_FeedbackItemsId",
                table: "FeedbackItemDataSource",
                column: "FeedbackItemsId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackItemTag_TagsId",
                table: "FeedbackItemTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackItemTeam_TeamsId",
                table: "FeedbackItemTeam",
                column: "TeamsId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityDataSource_OpportunitiesId",
                table: "OpportunityDataSource",
                column: "OpportunitiesId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityTag_TagsId",
                table: "OpportunityTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityTeam_TeamsId",
                table: "OpportunityTeam",
                column: "TeamsId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_Name",
                table: "Tag",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Team_Name",
                table: "Team",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedbackItemDataSource");

            migrationBuilder.DropTable(
                name: "FeedbackItemTag");

            migrationBuilder.DropTable(
                name: "FeedbackItemTeam");

            migrationBuilder.DropTable(
                name: "OpportunityDataSource");

            migrationBuilder.DropTable(
                name: "OpportunityTag");

            migrationBuilder.DropTable(
                name: "OpportunityTeam");

            migrationBuilder.DropTable(
                name: "SavedView");

            migrationBuilder.DropTable(
                name: "FeedbackItem");

            migrationBuilder.DropTable(
                name: "DataSource");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "Team");

            migrationBuilder.DropTable(
                name: "Opportunity");
        }
    }
}
