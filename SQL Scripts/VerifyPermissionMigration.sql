-- Verify Permission System Migration
-- Run this to confirm the migration applied successfully

-- Check if permissions table exists and has data
SELECT COUNT(*) AS PermissionCount FROM permissions;

-- View all permissions
SELECT 
    PermissionId,
    PermissionName,
    Category,
    DisplayName,
    Description
FROM permissions
ORDER BY Category, PermissionId;

-- Check if role_permissions table exists and has data
SELECT COUNT(*) AS RolePermissionCount FROM role_permissions;

-- View role permissions summary (count per role)
SELECT 
    RoleId,
    CASE RoleId
        WHEN 1 THEN 'Read Only'
        WHEN 2 THEN 'Estimator'
        WHEN 3 THEN 'Manager'
        WHEN 4 THEN 'Principal Estimator'
        WHEN 5 THEN 'Admin'
    END AS RoleName,
    COUNT(*) AS PermissionCount
FROM role_permissions
WHERE IsGranted = 1
GROUP BY RoleId
ORDER BY RoleId;

-- View detailed permissions for each role
SELECT 
    rp.RoleId,
    CASE rp.RoleId
        WHEN 1 THEN 'Read Only'
        WHEN 2 THEN 'Estimator'
        WHEN 3 THEN 'Manager'
        WHEN 4 THEN 'Principal Estimator'
        WHEN 5 THEN 'Admin'
    END AS RoleName,
    p.Category,
    p.PermissionName,
    p.DisplayName,
    rp.IsGranted
FROM role_permissions rp
INNER JOIN permissions p ON rp.PermissionId = p.PermissionId
WHERE rp.IsGranted = 1
ORDER BY rp.RoleId, p.Category, p.PermissionName;
