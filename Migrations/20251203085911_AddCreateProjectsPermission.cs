using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CopelinSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCreateProjectsPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add CreateProjects permission
            migrationBuilder.Sql(@"
                INSERT INTO permissions (PermissionName, Category, DisplayName, Description)
                VALUES ('CreateProjects', 'Projects', 'Create Projects', 'Can create new projects');
            ");

            // Grant CreateProjects permission to appropriate roles
            // Manager (3), PrincipalEstimator (4), and Admin (5) can create projects
            migrationBuilder.Sql(@"
                -- Manager (3): Add CreateProjects permission
                INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
                SELECT 3, PermissionId, 1 FROM permissions WHERE PermissionName = 'CreateProjects';

                -- PrincipalEstimator (4): Add CreateProjects permission
                INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
                SELECT 4, PermissionId, 1 FROM permissions WHERE PermissionName = 'CreateProjects';

                -- Admin (5): Add CreateProjects permission
                INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
                SELECT 5, PermissionId, 1 FROM permissions WHERE PermissionName = 'CreateProjects';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove role permissions for CreateProjects
            migrationBuilder.Sql(@"
                DELETE FROM role_permissions 
                WHERE PermissionId IN (SELECT PermissionId FROM permissions WHERE PermissionName = 'CreateProjects');
            ");

            // Remove CreateProjects permission
            migrationBuilder.Sql(@"
                DELETE FROM permissions WHERE PermissionName = 'CreateProjects';
            ");
        }
    }
}
