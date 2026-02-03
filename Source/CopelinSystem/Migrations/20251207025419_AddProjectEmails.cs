using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CopelinSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "project_emails",
                columns: table => new
                {
                    EmailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    ProjectWr = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmailSubject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EmailFrom = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    EmailTo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmailBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailBodyHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsMatched = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_emails", x => x.EmailId);
                    table.ForeignKey(
                        name: "FK_project_emails_project_list_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "project_list",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "project_email_attachments",
                columns: table => new
                {
                    AttachmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_email_attachments", x => x.AttachmentId);
                    table.ForeignKey(
                        name: "FK_project_email_attachments_project_emails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "project_emails",
                        principalColumn: "EmailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_email_attachments_EmailId",
                table: "project_email_attachments",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_project_emails_ProjectId",
                table: "project_emails",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_email_attachments");

            migrationBuilder.DropTable(
                name: "project_emails");
        }
    }
}
