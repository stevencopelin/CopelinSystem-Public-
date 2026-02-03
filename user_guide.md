# Copelin Work Flow Management System - User Guide

## 1. Introduction
Welcome to the Copelin Work Flow Management System. This guide provides step-by-step instructions for using the core features of the application, including managing projects, tracking tasks, organizing files, and generating reports.

## 2. Getting Started

### Logging In
Access the system via your web browser. You will be prompted to log in with your credentials.
-   **Dashboard**: Upon login, you will see the main dashboard or home page, providing quick access to your projects and alerts.
-   **Sidebar Navigation**: Use the menu on the left to navigate between **Projects**, **Files**, **Reports**, and **Settings**.

## 3. Project Management

The **Project View** is the central hub for all project-related activities.

### Viewing a Project
Navigate to a project to view its details. The project card displays:
-   **Status Bar**: Current status (e.g., Created, In Progress, On Hold, Done).
-   **Key Metrics**: Work Request (WR) Number, Dates (Estimated Completion, Tender Close), and Financials (Actual Price, Indicative Price).
-   **Project Information**: Client contact details, description, and assigned estimator.

### Editing Project Details
You can update most project details directly on the page using **Inline Editing**:
1.  Hover over a field like **Description**, **Work Order**, or **Actual Price**.
2.  Click the field to enter edit mode.
3.  Type your changes and click away (or press Enter) to save.
    *   *Note: Detailed logs are kept for all changes.*

### Managing Tasks
The **Task List** section helps you track specific activities within a project.
-   **Add Task**: Click **"New Task"** to add a to-do item.
-   **Update Status**: Tasks move from **Pending** -> **In-Progress** -> **Done**.
-   **Edit/Delete**: Use the **Action** dropdown on a task row to view details, edit, or delete it (permissions permitting).

### Tracking Productivity
Estimators can log time against tasks in the **Productivity / Comments** section.
-   **Add Entry**: Click **"New Productivity"** to log hours and add comments.
-   **History**: View a historical log of who worked on what task and for how long.

### Project Emails
The system automatically tracks emails related to the project (matched via WR Number).
-   Click **"Project Emails"** at the top or scroll to the Email section to view correspondence.
-   Attachments are indicated by a paperclip icon.

## 4. File Management

The **File Explorer** provides a robust interface for managing project documents.

### Navigation
-   **Tree View (Left Panel)**: Drill down through the hierarchy:
    `Region` > `Financial Year` > `Location` > `Project` > `Folders`.
-   **Breadcrumbs**: Use the top bar to jump back to parent folders.

### Managing Files
-   **Upload**: Click the **"Upload"** button to select multiple files from your computer.
-   **New Folder**: Organize files by creating subfolders.
-   **Search**: Use the search bar to find files across the entire project (or system, depending on context).

### Quick Actions (Right-Click)
Right-click on any file or folder to access the Context Menu:
-   **View / Download**: Preview or open the file.
-   **Rename**: Change the file name.
-   **Delete**: Remove the file (requires confirmation).

## 5. External Requests & Collaboration

Collaborate with other departments (Procurement, WOC, etc.) without giving them full system access.

### Requesting Information
1.  On the Project View, click **"Request Internal Info"**.
2.  Select the **Department** you need information from (e.g., Procurement, REQ).
3.  **Generate Link**: The system creates a unique, secure link.
4.  **Send**: You can copy the link manually or use the **"Send via Email"** button to email it directly to the department (email addresses are auto-populated based on the region).

### Submitting Information (External User Journey)
External users receiving the link will see a simplified **Submission Page**:
1.  They can view basic project details (Name, Location, WR).
2.  They can update specific locked fields: **WO Number**, **REQ Number**, or **PO Number**.
3.  Clicking **"Submit Updates"** saves the data directly to the project and invalidates the one-time link.

### Notifications
When an external user submits information:
-   A **Notification Bell** icon in the top navigation bar will alert you.
-   Click the icon to see a list of recent submissions.
-   Clicking a notification takes you directly to the updated project.
-   Click the **X** to dismiss the notification.

## 6. Reports & Analytics

Access the **Reporting Dashboard** via the sidebar to view high-level metrics.
-   **Key Indicators**: Active Projects, FY Revenue, Active Clients.
-   **Top Performers**: Highlights the most productive users.
-   **Charts**: Visual breakdowns of Project Status and Revenue by Region.

Detailed reports are available for:
-   **Financials**: In-depth revenue analysis.
-   **Projects**: Status and progress reports.
-   **Productivity**: User efficiency and time tracking.
-   **Operational**: Client and location statistics.

## 7. Help & Support
If you need assistance:
-   Visit the **Help Center** in the application to browse articles and guides.
-   Administrators can manage help content via the **Help Admin** page.


User Secrets Setup
To securely store your email password, run the following commands in your terminal:

# Navigate to the project directory
cd /Users/portfox/Projects/CopelinSystem-Dec25/Source/CopelinSystem
# Set the email password (replace 'YOUR_PASSWORD' with the actual password)
dotnet user-secrets set "EmailSettings:Password" "YOUR_PASSWORD"
# Verify the secret is set
dotnet user-secrets list
Note for Production: On your production server, create an Environment Variable named EmailSettings__Password with the value of the password.