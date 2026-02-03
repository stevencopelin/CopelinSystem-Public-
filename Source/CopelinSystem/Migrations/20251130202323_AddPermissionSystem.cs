using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CopelinSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    RolePermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<byte>(type: "tinyint", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    IsGranted = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.RolePermissionId);
                    table.ForeignKey(
                        name: "FK_role_permissions_permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_permissions_PermissionName",
                table: "permissions",
                column: "PermissionName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_PermissionId",
                table: "role_permissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_RoleId_PermissionId",
                table: "role_permissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            // Seed Permissions
            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "PermissionName", "Category", "DisplayName", "Description" },
                values: new object[,]
                {
                    { "ViewProjects", "Projects", "View Projects", "Can view project list and details" },
                    { "EditProjects", "Projects", "Edit Projects", "Can edit project information" },
                    { "DeleteProjects", "Projects", "Delete Projects", "Can delete projects" },
                    { "AssignProjects", "Projects", "Assign Projects", "Can assign users to projects" },
                    
                    { "ViewTasks", "Tasks", "View Tasks", "Can view tasks" },
                    { "AddTasks", "Tasks", "Add Tasks", "Can add new tasks" },
                    { "EditTasks", "Tasks", "Edit Tasks", "Can edit existing tasks" },
                    { "DeleteTasks", "Tasks", "Delete Tasks", "Can delete tasks" },
                    
                    { "ViewProductivity", "Productivity", "View Productivity", "Can view productivity entries" },
                    { "AddProductivity", "Productivity", "Add Productivity", "Can add productivity entries" },
                    { "EditProductivity", "Productivity", "Edit Productivity", "Can edit productivity entries" },
                    { "DeleteProductivity", "Productivity", "Delete Productivity", "Can delete productivity entries" },
                    
                    { "ManageClients", "Clients", "Manage Clients", "Can manage client information" },
                    { "ManageConsultants", "Consultants", "Manage Consultants", "Can manage consultant information" },
                    { "ManageContractors", "Contractors", "Manage Contractors", "Can manage contractor information" },
                    { "ManageEmployees", "Employees", "Manage Employees", "Can manage employee information" },
                    { "ManageUsers", "Users", "Manage Users", "Can manage all user accounts and permissions" }
                });

            // Seed Role Permissions (matching current system logic)
            // RoleId: 1=ReadOnly, 2=Estimator, 3=Manager, 4=PrincipalEstimator, 5=Admin
            
            migrationBuilder.Sql(@"
                -- ReadOnly (1): Can only view
                INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
                SELECT 1, PermissionId, 1 FROM permissions WHERE PermissionName IN ('ViewProjects', 'ViewTasks', 'ViewProductivity');

                -- Estimator (2): Can view and edit projects, add tasks/productivity
                INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
                SELECT 2, PermissionId, 1 FROM permissions WHERE PermissionName IN 
                ('ViewProjects', 'EditProjects', 'ViewTasks', 'AddTasks', 'EditTasks', 'ViewProductivity', 'AddProductivity', 'EditProductivity');

                -- Manager (3): Estimator + delete + manage clients/consultants/contractors
                INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
                SELECT 3, PermissionId, 1 FROM permissions WHERE PermissionName IN 
                ('ViewProjects', 'EditProjects', 'DeleteProjects', 'AssignProjects',
                 'ViewTasks', 'AddTasks', 'EditTasks', 'DeleteTasks',
                 'ViewProductivity', 'AddProductivity', 'EditProductivity', 'DeleteProductivity',
                 'ManageClients', 'ManageConsultants', 'ManageContractors');

                -- PrincipalEstimator (4): Manager + manage employees
                INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
                SELECT 4, PermissionId, 1 FROM permissions WHERE PermissionName IN 
                ('ViewProjects', 'EditProjects', 'DeleteProjects', 'AssignProjects',
                 'ViewTasks', 'AddTasks', 'EditTasks', 'DeleteTasks',
                 'ViewProductivity', 'AddProductivity', 'EditProductivity', 'DeleteProductivity',
                 'ManageClients', 'ManageConsultants', 'ManageContractors', 'ManageEmployees');

                -- Admin (5): All permissions
                INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
                SELECT 5, PermissionId, 1 FROM permissions;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "permissions");
        }
    }
}
