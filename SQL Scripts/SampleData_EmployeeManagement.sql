-- Employee Management System - Sample Data Script
-- Run this script to populate initial test data for the employee management system

-- =====================================================
-- 1. Add Regions
-- =====================================================
INSERT INTO regions (RegionName) VALUES 
('SEQ - South East Qld'),
('SWQ - South West Qld'),
('WBB - Wide Bay Burnett'),
('FNQ - Far North Qld');

-- =====================================================
-- 2. Add Sample Employees
-- =====================================================
-- SEQ Region Employees
INSERT INTO employees (FullName, RegionId, Active) VALUES 
('John Smith', 1, 1),
('Sarah Williams', 1, 1),
('Michael Brown', 1, 1);

-- SWQ Region Employees
INSERT INTO employees (FullName, RegionId, Active) VALUES 
('Jane Doe', 2, 1),
('Robert Taylor', 2, 1);

-- WBB Region Employees
INSERT INTO employees (FullName, RegionId, Active) VALUES 
('Emily Davis', 3, 1),
('David Wilson', 3, 1);

-- FNQ Region Employees
INSERT INTO employees (FullName, RegionId, Active) VALUES 
('Lisa Anderson', 4, 1),
('James Martinez', 4, 1);

-- =====================================================
-- 3. Assign Roles to Employees
-- =====================================================
-- RoleType: 1 = Supervisor, 2 = SeniorEstimator, 3 = PrincipalEstimator

-- SEQ Region Roles
INSERT INTO employee_roles (EmployeeId, RoleType) VALUES 
(1, 1),  -- John Smith - Supervisor
(2, 2),  -- Sarah Williams - Senior Estimator
(3, 1),  -- Michael Brown - Supervisor
(3, 2);  -- Michael Brown - Also Senior Estimator (multi-role example)

-- SWQ Region Roles
INSERT INTO employee_roles (EmployeeId, RoleType) VALUES 
(4, 1),  -- Jane Doe - Supervisor
(5, 2);  -- Robert Taylor - Senior Estimator

-- WBB Region Roles
INSERT INTO employee_roles (EmployeeId, RoleType) VALUES 
(6, 1),  -- Emily Davis - Supervisor
(7, 3);  -- David Wilson - Principal Estimator

-- FNQ Region Roles
INSERT INTO employee_roles (EmployeeId, RoleType) VALUES 
(8, 1),  -- Lisa Anderson - Supervisor
(9, 2);  -- James Martinez - Senior Estimator

-- =====================================================
-- 4. Verification Queries
-- =====================================================

-- View all employees with their regions and roles
SELECT 
    e.EmployeeId,
    e.FullName,
    r.RegionName,
    e.Active,
    CASE er.RoleType
        WHEN 1 THEN 'Supervisor'
        WHEN 2 THEN 'Senior Estimator'
        WHEN 3 THEN 'Principal Estimator'
    END AS RoleName
FROM employees e
LEFT JOIN regions r ON e.RegionId = r.RegionId
LEFT JOIN employee_roles er ON e.EmployeeId = er.EmployeeId
WHERE e.Active = 1
ORDER BY r.RegionName, e.FullName;

-- View supervisors only
SELECT 
    e.EmployeeId,
    e.FullName,
    r.RegionName
FROM employees e
LEFT JOIN regions r ON e.RegionId = r.RegionId
INNER JOIN employee_roles er ON e.EmployeeId = er.EmployeeId
WHERE e.Active = 1 AND er.RoleType = 1
ORDER BY e.FullName;

-- View senior/principal estimators
SELECT 
    e.EmployeeId,
    e.FullName,
    r.RegionName,
    CASE er.RoleType
        WHEN 2 THEN 'Senior Estimator'
        WHEN 3 THEN 'Principal Estimator'
    END AS RoleName
FROM employees e
LEFT JOIN regions r ON e.RegionId = r.RegionId
INNER JOIN employee_roles er ON e.EmployeeId = er.EmployeeId
WHERE e.Active = 1 AND er.RoleType IN (2, 3)
ORDER BY e.FullName;

-- =====================================================
-- Notes:
-- =====================================================
-- - All employees are set to Active = 1 (true)
-- - Employee IDs will auto-increment starting from 1
-- - Michael Brown (ID 3) has both Supervisor and Senior Estimator roles
--   to demonstrate multi-role functionality
-- - Each region has at least one supervisor and one estimator
-- - Adjust the data as needed for your specific requirements
