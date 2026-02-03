-- Script to assign projects to Tim Kowitz and John Smith for testing user-specific widgets
-- Run this script in SQL Server Management Studio or Azure Data Studio

-- First, let's find Tim and John's User IDs
SELECT UserId, Firstname, Lastname, Email 
FROM users 
WHERE (Firstname = 'Tim' AND Lastname = 'Kowitz') 
   OR (Firstname = 'John' AND Lastname = 'Smith');

-- Find some projects to assign (get the first 2 in-progress projects)
SELECT TOP 2 ProjectId, ProjectName, ProjectStatus, ProjectUserIds
FROM project_list
WHERE ProjectStatus = 1 -- In Progress
ORDER BY ProjectDateCreated DESC;

-- INSTRUCTIONS:
-- 1. Run the above queries to get the UserIds for Tim and John
-- 2. Run the above query to get 2 project IDs
-- 3. Update the variables below with the actual IDs from your database
-- 4. Then run the UPDATE statements

-- Example (replace with actual values):
-- DECLARE @TimUserId INT = 5;  -- Replace with Tim's actual UserId
-- DECLARE @JohnUserId INT = 6; -- Replace with John's actual UserId
-- DECLARE @Project1Id INT = 1; -- Replace with first project ID
-- DECLARE @Project2Id INT = 2; -- Replace with second project ID

-- Assign Project 1 to Tim Kowitz
-- UPDATE project_list 
-- SET ProjectUserIds = CAST(@TimUserId AS VARCHAR(10))
-- WHERE ProjectId = @Project1Id;

-- Assign Project 2 to John Smith
-- UPDATE project_list 
-- SET ProjectUserIds = CAST(@JohnUserId AS VARCHAR(10))
-- WHERE ProjectId = @Project2Id;

-- Verify the assignments
-- SELECT ProjectId, ProjectName, ProjectUserIds
-- FROM project_list
-- WHERE ProjectId IN (@Project1Id, @Project2Id);
