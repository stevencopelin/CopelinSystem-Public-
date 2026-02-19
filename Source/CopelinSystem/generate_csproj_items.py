
import os

source_files = """
./Migrations/20260216013038_InitialBaseLine.cs
./Migrations/20260216013038_InitialBaseLine.Designer.cs
./Migrations/ApplicationDbContextModelSnapshot.cs
./Routes.razor
./Layout/ExternalLayout.razor
./Layout/LoginLayout.razor
./Layout/ReconnectModal.razor
./Layout/MainLayout.razor
./Layout/SubmissionNotificationDropdown.razor
./appsettings.json
./App.razor
./Shared/FileViewerModal.razor
./Models/Region.cs
./Models/ProjectEmailAttachment.cs
./Models/HelpArticle.cs
./Models/Contractor.cs
./Models/Client.cs
./Models/ProjectTasks.cs
./Models/FileSystemItemConfiguration.cs
./Models/UserProductivity.cs
./Models/FileSystemItem.cs
./Models/Consultant.cs
./Models/StatusCode.cs
./Models/EmployeeRole.cs
./Models/RolePermission.cs
./Models/Users.cs
./Models/Permission.cs
./Models/ExternalRegionEmail.cs
./Models/ProjectList.cs
./Models/SubmissionToken.cs
./Models/UserProjectPreference.cs
./Models/SubmissionNotificationDismissal.cs
./Models/Projects.cs
./Models/HelpSection.cs
./Models/SubmissionNotificationDto.cs
./Models/ProjectEmail.cs
./Models/Checklists.cs
./Models/ClientContact.cs
./Models/TaskConfiguration.cs
./Models/Employee.cs
./Properties/launchSettings.json
./_Imports.razor
./Controllers/AccountController.cs
./Controllers/TestEmailController.cs
./Controllers/FilesController.cs
./Pages/ProjectView.razor
./Pages/NotFound.razor
./Pages/DebugPermissions.razor
./Pages/TestEmail.razor
./Pages/ContractorManagement.razor
./Pages/UserManagement.razor
./Pages/DebugPermissions2.razor
./Pages/Estimator/Contracts.razor
./Pages/PermissionManagement.razor
./Pages/EstimatorTools.razor
./Pages/Admin/RegionExtManagement.razor
./Pages/ProjectPrediction.razor
./Pages/IsoForms/Admin/TemplateManager.razor
./Pages/IsoForms/Admin/TemplateEditor.razor
./Pages/IsoForms/ProjectChecklists.razor
./Pages/IsoForms/ChecklistRunner.razor
./Pages/Error.razor
./Pages/Logout.razor
./Pages/Login.razor
./Pages/FileExplorer/FolderTree.razor
./Pages/FileExplorer/ContextMenu.razor
./Pages/FileExplorer/TreeView.razor
./Pages/FileExplorer/FileExplorer.razor
./Pages/FileExplorer/FileExplorerModels.cs
./Pages/FileExplorer/FileListView.razor
./Pages/EmployeeManagement.razor
./Pages/ConsultantManagement.razor
./Pages/ClientManagement.razor
./Pages/AddProject.razor
./Pages/External/RequestInfo.razor
./Pages/External/ProjectSubmission.razor
./Pages/Home.razor
./Pages/ProjectExplorer.razor
./Pages/ProjectEdit.razor
./Pages/TaskDurationManagement.razor
./Pages/Help/HelpAdmin.razor
./Pages/Help/HelpCenter.razor
./Pages/Reports/ReportsDashboard.razor
./Pages/Reports/FinancialReport.razor
./Pages/Reports/ProjectPerformanceReport.razor
./Pages/Reports/OperationalReport.razor
./Pages/Reports/ProductivityReport.razor
./Services/ReportingService.cs
./Services/ChecklistService.cs
./Services/ClientService.cs
./Services/TaskConfigurationService.cs
./Services/UserService.cs
./Services/EmailService.cs
./Services/FileSystemService.cs
./Services/RegionEmailService.cs
./Services/ContractorService.cs
./Services/CopelinAuthStateProvider.cs
./Services/ConsultantService.cs
./Services/ApplicationDbContext.cs
./Services/PermissionService.cs
./Services/AuthenticationService.cs
./Services/SubmissionTokenService.cs
./Services/HtmlExportService.cs
./Services/EmailReceiverService.cs
./Services/ProjectService.cs
./Services/HelpSeeder.cs
./Services/EmployeeService.cs
./Services/PasswordHasher.cs
./Services/HelpService.cs
./Program.cs
"""

wwwroot_files = """
wwwroot/js/dragdrop.js
wwwroot/js/download.js
wwwroot/js/document-preview.js
wwwroot/js/mammoth.browser.min.js
wwwroot/js/xlsx.full.min.js
wwwroot/js/notifications.js
wwwroot/js/fullscreen.js
wwwroot/app.css
wwwroot/uploads/logo/Queensland-Government-Logo.png
wwwroot/uploads/help/9eb6b046-028c-4b8d-8e08-ded3d045caee.mp4
wwwroot/uploads/help/405f59a2-3c49-4816-9621-dce04a6f544a.mp4
wwwroot/uploads/help/e01908fd-b341-496d-9afa-f0187defc512.png
wwwroot/uploads/help/4ee77300-fe10-4b46-8369-72336be5640f.png
wwwroot/uploads/help/5847e1ee-bdd8-417e-b44b-7bb9fc5c1820.png
wwwroot/uploads/help/2118f6e4-92f2-433f-8639-4c96a37621b8.png
wwwroot/uploads/help/8a2cc91d-9c8e-45ee-8df4-6943a0ba45bc.mp4
wwwroot/uploads/help/4dee5dd0-a12f-45c2-adfd-4db02f49b060.png
wwwroot/uploads/help/a1dd420d-4f67-4ab8-af3d-7362a1ac6abe.png
wwwroot/uploads/help/939e7234-677d-4838-8590-8eb330046353.png
wwwroot/uploads/help/02cc0b19-3b18-46aa-adb5-cc82264d6e3a.png
wwwroot/uploads/help/32143b8d-7c31-4a3d-9ffc-e28f1a5dc992.mp4
wwwroot/uploads/help/60bf567e-12ed-4aa4-9b5b-b9e28755c92a.png
wwwroot/uploads/help/56ef028a-933e-4d15-b9bf-f0d5f6ae56ad.png
wwwroot/uploads/help/9a885371-0aff-4548-b9ee-742edb5f4890.png
wwwroot/uploads/help/37610185-62cd-48ec-a96f-a2d345be78c1.png
wwwroot/uploads/help/275bf817-bf58-4b17-8da3-b62239550735.mp4
wwwroot/uploads/help/ddc4e928-db2d-4802-8d5b-0bb2eaf5f84f.png
wwwroot/uploads/help/961222df-8e01-489e-b894-41b19b7e6142.png
wwwroot/uploads/help/fcad9b38-6e08-47e1-9dc7-0ad856f766cd.png
wwwroot/uploads/help/4a36d301-842f-4054-887f-715630fdf6e0.png
wwwroot/uploads/help/ba1ed8c4-7ebc-4d29-9c78-5fd9404db680.png
wwwroot/uploads/help/028313d4-26f6-4aef-b335-ed6055add05c.png
wwwroot/uploads/help/700b79aa-9d6c-4ee2-ad1a-d713a976fefe.png
wwwroot/uploads/help/111ce84d-ab52-44bd-8447-08df108228fe.png
wwwroot/uploads/help/39ae9308-c30d-4e65-8893-29fba6cfb070.png
wwwroot/uploads/help/c1642d20-ab15-4408-90f6-ba0a8071aeb1.png
wwwroot/uploads/help/ae5509f8-b49e-4bb6-8fd8-b59e9bef9ab2.png
wwwroot/uploads/help/296453f0-ed36-44bb-95c6-ae981bf3d15a.png
wwwroot/uploads/help/c6c0cf13-0249-4f25-b80f-64918757b50d.mp4
wwwroot/uploads/help/9f921376-2cb3-499f-b391-29e45ee9be2f.png
wwwroot/uploads/help/69cb9a03-7f2e-497a-9f99-2d8da0f59d1e.mp4
wwwroot/favicon.png
wwwroot/lib/bootstrap/dist/css/bootstrap.min.css
wwwroot/lib/bootstrap/dist/css/bootstrap-grid.rtl.min.css
wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.rtl.css
wwwroot/lib/bootstrap/dist/css/bootstrap-grid.rtl.min.css.map
wwwroot/lib/bootstrap/dist/css/bootstrap.rtl.min.css.map
wwwroot/lib/bootstrap/dist/css/bootstrap.rtl.css.map
wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.rtl.css
wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.min.css.map
wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.css
wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.rtl.min.css.map
wwwroot/lib/bootstrap/dist/css/bootstrap.css
wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.min.css.map
wwwroot/lib/bootstrap/dist/css/bootstrap-grid.css.map
wwwroot/lib/bootstrap/dist/css/bootstrap-grid.min.css
wwwroot/lib/bootstrap/dist/css/bootstrap.rtl.min.css
wwwroot/lib/bootstrap/dist/css/bootstrap.css.map
wwwroot/lib/bootstrap/dist/css/bootstrap-grid.rtl.css.map
wwwroot/lib/bootstrap/dist/css/bootstrap.min.css.map
wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.min.css
wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.rtl.min.css.map
wwwroot/lib/bootstrap/dist/css/bootstrap.rtl.css
wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.rtl.css.map
wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.rtl.css.map
wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.css
wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.min.css
wwwroot/lib/bootstrap/dist/css/bootstrap-grid.css
wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.css.map
wwwroot/lib/bootstrap/dist/css/bootstrap-utilities.rtl.min.css
wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.rtl.min.css
wwwroot/lib/bootstrap/dist/css/bootstrap-grid.min.css.map
wwwroot/lib/bootstrap/dist/css/bootstrap-grid.rtl.css
wwwroot/lib/bootstrap/dist/css/bootstrap-reboot.css.map
wwwroot/lib/bootstrap/dist/js/bootstrap.esm.min.js
wwwroot/lib/bootstrap/dist/js/bootstrap.esm.js
wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.js
wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.min.js.map
wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.js.map
wwwroot/lib/bootstrap/dist/js/bootstrap.esm.js.map
wwwroot/lib/bootstrap/dist/js/bootstrap.js
wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.min.js
wwwroot/lib/bootstrap/dist/js/bootstrap.min.js
wwwroot/lib/bootstrap/dist/js/bootstrap.esm.min.js.map
wwwroot/lib/bootstrap/dist/js/bootstrap.js.map
wwwroot/lib/bootstrap/dist/js/bootstrap.min.js.map
"""

print("  <ItemGroup>")

for line in source_files.strip().split('\n'):
    line = line.strip()
    if not line or line == "./.vs/CopelinSystem.slnx/v18/DocumentLayout.backup.json" or line == "./.vs/CopelinSystem.slnx/v18/DocumentLayout.json":
        continue
    
    clean_path = line.replace("./", "", 1).replace("\\", "/")
    
    if clean_path.endswith(".cs"):
        print(f'    <Compile Include="{clean_path}" />')
    else:
        print(f'    <Content Include="{clean_path}" />')

for line in wwwroot_files.strip().split('\n'):
    line = line.strip()
    if not line: continue
    print(f'    <Content Include="{line}" />')

print("  </ItemGroup>")
