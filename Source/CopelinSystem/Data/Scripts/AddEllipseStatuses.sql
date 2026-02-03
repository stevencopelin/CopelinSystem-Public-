-- Check if table exists, if not create it
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StatusCodes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StatusCodes](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Code] [nvarchar](2) NOT NULL,
        [Description] [nvarchar](100) NOT NULL,
        CONSTRAINT [PK_StatusCodes] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END

-- Enable Identity Insert to allow specifying IDs
SET IDENTITY_INSERT [dbo].[StatusCodes] ON;

-- Clear table if needed? Or just insert new ones? 
-- Assuming empty table or appending. If IDs exist, it might error. 
-- Using separate INSERTs or a single batch block.

INSERT INTO [dbo].[StatusCodes] ([Id], [Code], [Description]) VALUES
(1, 'AC', 'Access Issues'),
(2, 'AD', 'Await decision-WO'),
(3, 'AN', 'Being analysed-WR'),
(4, 'BA', 'Banked'),
(5, 'BE', 'Being Investgated-WO'),
(6, 'CA', 'CA Notification-WR'),
(7, 'CF', 'Costs to Finalise-WO'),
(8, 'CN', 'Cancelled'),
(9, 'CP', 'Cost/Doc to Fin-WO'),
(10, 'CU', 'Contractor Unavailab'),
(11, 'D1', 'No community access'),
(12, 'D2', 'No materials access'),
(13, 'D3', 'Agency request delay'),
(14, 'D4', 'Contractor delayed'),
(15, 'D5', 'Tenant request delay'),
(16, 'DC', 'Delayed - Client-WO'),
(17, 'DF', 'Defects to Comp-WO'),
(18, 'DM', 'Delayed-Materials-WO'),
(19, 'DR', 'Deferred-Await Funds'),
(20, 'DS', 'Delayed-Sub Contr-WO'),
(21, 'DV', 'Delayed-Variation-WO'),
(22, 'DW', 'Delayed-Weather-WO'),
(23, 'DZ', 'Delayed-COVID-19'),
(24, 'ES', 'Estimating-WR'),
(25, 'FD', 'Finalised-COSOL'),
(26, 'FN', 'Final-WO'),
(27, 'IN', 'Inspection Required'),
(28, 'IT', 'In Tender-WR'),
(29, 'J0', 'HO Only - NoSend JS'),
(30, 'JA', 'Job State Accepted'),
(31, 'JI', 'SYS GENERATD JS Issd'),
(32, 'JR', 'Job State Rejected'),
(33, 'JS', 'Issue Job State-WO'),
(34, 'JT', 'JS Issuing Suspended'),
(35, 'M1', 'Pre assess comp-WO'),
(36, 'M2', 'Onsite asses comp-WO'),
(37, 'M3', 'Data entry comp-WO'),
(38, 'M4', 'MAR Issued-WO'),
(39, 'NA', 'CA Notif Accepted-WR'),
(40, 'NR', 'CA Notif Rejected-WR'),
(41, 'PF', 'Doco to finalise-WO'),
(42, 'PL', 'Pipeline Future Work'),
(43, 'QA', 'Customer Approved'),
(44, 'QC', 'Quote Received'),
(45, 'QR', 'Customer Rejected'),
(46, 'QU', 'Quote/Proposal Sent'),
(47, 'RA', 'Accepted - WR'),
(48, 'RC', 'Request Cancel'),
(49, 'RD', 'Requires Dispatch'),
(50, 'RE', 'Reschedule-WO'),
(51, 'RF', 'Declined - WR'),
(52, 'RO', 'Opportunity - WR'),
(53, 'RQ', 'Intake - WR'),
(54, 'RR', 'Intake - Analysis-WR'),
(55, 'SC', 'Scoping/DueDiligence'),
(56, 'SD', 'SM Dispatch Required'),
(57, 'SU', 'Status Update-WR'),
(58, 'VA', 'Variation Approved'),
(59, 'VH', 'Variation Held'),
(60, 'VR', 'Variation Rejected'),
(61, 'WB', 'Web Portal Created'),
(62, 'WM', 'Waiting Materials'),
(63, 'XX', 'Head Office Use Only');

SET IDENTITY_INSERT [dbo].[StatusCodes] OFF;
