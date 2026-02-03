-- Grant ManageUsers and ManagePermissions to Principal Estimator Role
-- This allows Principal Estimators to access User Management and Permission Management pages

-- First, get the permission IDs we need
DECLARE @ManageUsersPermissionId INT;
DECLARE @ManagePermissionsPermissionId INT;

SELECT @ManageUsersPermissionId = PermissionId 
FROM permissions 
WHERE PermissionName = 'ManageUsers';

SELECT @ManagePermissionsPermissionId = PermissionId 
FROM permissions 
WHERE PermissionName = 'ManagePermissions';

-- Grant ManageUsers permission to Principal Estimator (RoleId = 4)
IF NOT EXISTS (SELECT 1 FROM role_permissions WHERE RoleId = 4 AND PermissionId = @ManageUsersPermissionId)
BEGIN
    INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
    VALUES (4, @ManageUsersPermissionId, 1);
    PRINT 'Granted ManageUsers permission to Principal Estimator';
END
ELSE
BEGIN
    UPDATE role_permissions 
    SET IsGranted = 1 
    WHERE RoleId = 4 AND PermissionId = @ManageUsersPermissionId;
    PRINT 'Updated ManageUsers permission for Principal Estimator to granted';
END

-- Grant ManagePermissions permission to Principal Estimator (RoleId = 4)
IF NOT EXISTS (SELECT 1 FROM role_permissions WHERE RoleId = 4 AND PermissionId = @ManagePermissionsPermissionId)
BEGIN
    INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
    VALUES (4, @ManagePermissionsPermissionId, 1);
    PRINT 'Granted ManagePermissions permission to Principal Estimator';
END
ELSE
BEGIN
    UPDATE role_permissions 
    SET IsGranted = 1 
    WHERE RoleId = 4 AND PermissionId = @ManagePermissionsPermissionId;
    PRINT 'Updated ManagePermissions permission for Principal Estimator to granted';
END

-- Verify the changes
SELECT 
    'Principal Estimator' AS RoleName,
    p.PermissionName,
    p.DisplayName,
    CASE WHEN rp.IsGranted = 1 THEN 'YES' ELSE 'NO' END AS HasPermission
FROM permissions p
LEFT JOIN role_permissions rp ON p.PermissionId = rp.PermissionId AND rp.RoleId = 4
WHERE p.PermissionName IN ('ManageUsers', 'ManagePermissions')
ORDER BY p.PermissionName;
