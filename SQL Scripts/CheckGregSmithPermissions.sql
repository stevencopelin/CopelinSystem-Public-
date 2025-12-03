-- Check Greg Smith's user details and role
SELECT 
    UserId,
    Firstname,
    Lastname,
    Email,
    Region,
    UserType,
    CASE UserType
        WHEN 1 THEN 'Read Only'
        WHEN 2 THEN 'Estimator'
        WHEN 3 THEN 'Manager'
        WHEN 4 THEN 'Principal Estimator'
        WHEN 5 THEN 'Admin'
    END AS RoleName,
    AdUsername
FROM users
WHERE Firstname LIKE '%Greg%' AND Lastname LIKE '%Smith%';

-- Check what permissions Principal Estimator role (UserType = 4) has
SELECT 
    p.PermissionName,
    p.DisplayName,
    p.Category,
    rp.IsGranted
FROM permissions p
LEFT JOIN role_permissions rp ON p.PermissionId = rp.PermissionId AND rp.RoleId = 4
ORDER BY p.Category, p.PermissionName;

-- Specifically check for ManageUsers and ManagePermissions for Principal Estimator
SELECT 
    p.PermissionName,
    p.DisplayName,
    CASE WHEN rp.IsGranted = 1 THEN 'YES' ELSE 'NO' END AS HasPermission
FROM permissions p
LEFT JOIN role_permissions rp ON p.PermissionId = rp.PermissionId AND rp.RoleId = 4
WHERE p.PermissionName IN ('ManageUsers', 'ManagePermissions')
ORDER BY p.PermissionName;
