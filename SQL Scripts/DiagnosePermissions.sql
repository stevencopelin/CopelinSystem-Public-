-- Check current permissions for Manager (RoleId = 3) and Principal Estimator (RoleId = 4)
-- This will show if ManageUsers and ManagePermissions are actually granted in the database

SELECT 
    CASE rp.RoleId
        WHEN 3 THEN 'Manager'
        WHEN 4 THEN 'Principal Estimator'
    END AS RoleName,
    p.PermissionName,
    p.DisplayName,
    CASE WHEN rp.IsGranted = 1 THEN 'YES' ELSE 'NO' END AS IsGranted,
    CASE WHEN rp.RolePermissionId IS NULL THEN 'MISSING' ELSE 'EXISTS' END AS RecordStatus
FROM permissions p
CROSS JOIN (SELECT 3 AS RoleId UNION SELECT 4) roles
LEFT JOIN role_permissions rp ON p.PermissionId = rp.PermissionId AND rp.RoleId = roles.RoleId
WHERE p.PermissionName IN ('ManageUsers', 'ManagePermissions')
ORDER BY roles.RoleId, p.PermissionName;

-- Check all permissions for Manager role
SELECT 
    'Manager' AS RoleName,
    p.PermissionName,
    p.DisplayName,
    p.Category,
    CASE WHEN rp.IsGranted = 1 THEN 'YES' ELSE 'NO' END AS IsGranted
FROM permissions p
LEFT JOIN role_permissions rp ON p.PermissionId = rp.PermissionId AND rp.RoleId = 3
WHERE rp.IsGranted = 1
ORDER BY p.Category, p.PermissionName;

-- Check all permissions for Principal Estimator role
SELECT 
    'Principal Estimator' AS RoleName,
    p.PermissionName,
    p.DisplayName,
    p.Category,
    CASE WHEN rp.IsGranted = 1 THEN 'YES' ELSE 'NO' END AS IsGranted
FROM permissions p
LEFT JOIN role_permissions rp ON p.PermissionId = rp.PermissionId AND rp.RoleId = 4
WHERE rp.IsGranted = 1
ORDER BY p.Category, p.PermissionName;

-- Check a specific user (e.g., Greg Smith) to see their role
SELECT 
    UserId,
    Firstname,
    Lastname,
    UserType,
    CASE UserType
        WHEN 1 THEN 'Read Only'
        WHEN 2 THEN 'Estimator'
        WHEN 3 THEN 'Manager'
        WHEN 4 THEN 'Principal Estimator'
        WHEN 5 THEN 'Admin'
    END AS RoleName
FROM users
WHERE (Firstname LIKE '%Greg%' AND Lastname LIKE '%Smith%')
   OR UserType IN (3, 4)
ORDER BY UserType, Lastname;
