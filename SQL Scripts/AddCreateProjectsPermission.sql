-- Add CreateProjects Permission
-- This allows Manager, Principal Estimator, and Admin to create new projects

-- Add the permission
IF NOT EXISTS (SELECT 1 FROM permissions WHERE PermissionName = 'CreateProjects')
BEGIN
    INSERT INTO permissions (PermissionName, Category, DisplayName, Description)
    VALUES ('CreateProjects', 'Projects', 'Create Projects', 'Ability to create new projects');
    PRINT 'Added CreateProjects permission';
END
ELSE
BEGIN
    PRINT 'CreateProjects permission already exists';
END

-- Get the permission ID
DECLARE @CreateProjectsPermissionId INT;
SELECT @CreateProjectsPermissionId = PermissionId FROM permissions WHERE PermissionName = 'CreateProjects';

-- Grant to Manager (RoleId = 3)
IF NOT EXISTS (SELECT 1 FROM role_permissions WHERE RoleId = 3 AND PermissionId = @CreateProjectsPermissionId)
BEGIN
    INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
    VALUES (3, @CreateProjectsPermissionId, 1);
    PRINT 'Granted CreateProjects to Manager';
END
ELSE
BEGIN
    UPDATE role_permissions SET IsGranted = 1 WHERE RoleId = 3 AND PermissionId = @CreateProjectsPermissionId;
    PRINT 'Updated CreateProjects for Manager';
END

-- Grant to Principal Estimator (RoleId = 4)
IF NOT EXISTS (SELECT 1 FROM role_permissions WHERE RoleId = 4 AND PermissionId = @CreateProjectsPermissionId)
BEGIN
    INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
    VALUES (4, @CreateProjectsPermissionId, 1);
    PRINT 'Granted CreateProjects to Principal Estimator';
END
ELSE
BEGIN
    UPDATE role_permissions SET IsGranted = 1 WHERE RoleId = 4 AND PermissionId = @CreateProjectsPermissionId;
    PRINT 'Updated CreateProjects for Principal Estimator';
END

-- Grant to Admin (RoleId = 5) - for consistency, though Admin has all permissions via code
IF NOT EXISTS (SELECT 1 FROM role_permissions WHERE RoleId = 5 AND PermissionId = @CreateProjectsPermissionId)
BEGIN
    INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
    VALUES (5, @CreateProjectsPermissionId, 1);
    PRINT 'Granted CreateProjects to Admin';
END
ELSE
BEGIN
    UPDATE role_permissions SET IsGranted = 1 WHERE RoleId = 5 AND PermissionId = @CreateProjectsPermissionId;
    PRINT 'Updated CreateProjects for Admin';
END

-- Verify the changes
SELECT 
    CASE rp.RoleId
        WHEN 3 THEN 'Manager'
        WHEN 4 THEN 'Principal Estimator'
        WHEN 5 THEN 'Admin'
    END AS RoleName,
    p.PermissionName,
    p.DisplayName,
    CASE WHEN rp.IsGranted = 1 THEN 'YES' ELSE 'NO' END AS HasPermission
FROM permissions p
LEFT JOIN role_permissions rp ON p.PermissionId = rp.PermissionId
WHERE p.PermissionName = 'CreateProjects' AND rp.RoleId IN (3, 4, 5)
ORDER BY rp.RoleId;
