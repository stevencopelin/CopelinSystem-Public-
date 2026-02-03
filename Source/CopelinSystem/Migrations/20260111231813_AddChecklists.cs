using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CopelinSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddChecklists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChecklistTemplates",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistTemplates", x => x.TemplateId);
                });

            migrationBuilder.CreateTable(
                name: "FileSystemItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsFolder = table.Column<bool>(type: "bit", nullable: false),
                    PhysicalPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileSystemItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileSystemItems_FileSystemItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "FileSystemItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FileSystemItems_project_list_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "project_list",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionNotificationDismissals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DismissedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionNotificationDismissals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionNotificationDismissals_SubmissionTokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "SubmissionTokens",
                        principalColumn: "Token",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubmissionNotificationDismissals_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistSections",
                columns: table => new
                {
                    SectionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    SectionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistSections", x => x.SectionId);
                    table.ForeignKey(
                        name: "FK_ChecklistSections_ChecklistTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "ChecklistTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectChecklists",
                columns: table => new
                {
                    ChecklistInstanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectChecklists", x => x.ChecklistInstanceId);
                    table.ForeignKey(
                        name: "FK_ProjectChecklists_ChecklistTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "ChecklistTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistQuestions",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    HelpText = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistQuestions", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_ChecklistQuestions_ChecklistSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "ChecklistSections",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistQuestionOptions",
                columns: table => new
                {
                    OptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistQuestionOptions", x => x.OptionId);
                    table.ForeignKey(
                        name: "FK_ChecklistQuestionOptions_ChecklistQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "ChecklistQuestions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistResponses",
                columns: table => new
                {
                    ResponseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChecklistInstanceId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    ResponseValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistResponses", x => x.ResponseId);
                    table.ForeignKey(
                        name: "FK_ChecklistResponses_ChecklistQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "ChecklistQuestions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChecklistResponses_ProjectChecklists_ChecklistInstanceId",
                        column: x => x.ChecklistInstanceId,
                        principalTable: "ProjectChecklists",
                        principalColumn: "ChecklistInstanceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistQuestionOptions_QuestionId",
                table: "ChecklistQuestionOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistQuestions_SectionId",
                table: "ChecklistQuestions",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistResponses_ChecklistInstanceId",
                table: "ChecklistResponses",
                column: "ChecklistInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistResponses_QuestionId",
                table: "ChecklistResponses",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistSections_TemplateId",
                table: "ChecklistSections",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_FileSystemItems_ParentId",
                table: "FileSystemItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FileSystemItems_ProjectId",
                table: "FileSystemItems",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectChecklists_TemplateId",
                table: "ProjectChecklists",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionNotificationDismissals_TokenId",
                table: "SubmissionNotificationDismissals",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionNotificationDismissals_UserId",
                table: "SubmissionNotificationDismissals",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChecklistQuestionOptions");

            migrationBuilder.DropTable(
                name: "ChecklistResponses");

            migrationBuilder.DropTable(
                name: "FileSystemItems");

            migrationBuilder.DropTable(
                name: "SubmissionNotificationDismissals");

            migrationBuilder.DropTable(
                name: "ChecklistQuestions");

            migrationBuilder.DropTable(
                name: "ProjectChecklists");

            migrationBuilder.DropTable(
                name: "ChecklistSections");

            migrationBuilder.DropTable(
                name: "ChecklistTemplates");
        }
    }
}
