using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CopelinSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAppBranding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_branding",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    footer_html = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_locked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_branding", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "app_branding",
                columns: new[] { "id", "footer_html", "is_locked" },
                values: new object[] { 1, "<footer class=\"main-footer\">\n    <strong> {{Year}} <a href=\"#\">Estimating Module | Copelin System</a> - </strong>\n    Qld Governement - QBuild.\n    <div class=\"float-right d-none d-sm-inline-block\">\n        <b>Version</b> {{Version}}\n    </div>\n</footer>", true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_branding");
        }
    }
}
