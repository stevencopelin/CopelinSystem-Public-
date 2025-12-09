# Admin Interface - Quick Reference Guide

## EmployeeService Methods Quick Reference

### Employee CRUD
```csharp
// Get all active employees
var employees = await EmployeeService.GetAllEmployees();

// Get all employees (including inactive) - for admin
var allEmployees = await EmployeeService.GetAllEmployeesIncludingInactive();

// Get employees with formatted roles - for admin display
var employeesWithRoles = await EmployeeService.GetAllEmployeesWithRoles();
// Returns: List<(Employee Employee, List<string> Roles)>

// Get single employee
var employee = await EmployeeService.GetEmployeeById(employeeId);

// Add new employee
var newEmployee = new Employee 
{ 
    FullName = "John Smith", 
    RegionId = 1,
    Active = true 
};
await EmployeeService.AddEmployee(newEmployee);

// Update employee
employee.FullName = "John A. Smith";
await EmployeeService.UpdateEmployee(employee);

// Deactivate employee (soft delete)
await EmployeeService.DeleteEmployee(employeeId);

// Reactivate employee
await EmployeeService.ActivateEmployee(employeeId);
```

### Role Management
```csharp
// Get employees by role
var supervisors = await EmployeeService.GetSupervisors();
var estimators = await EmployeeService.GetSeniorEstimators();
var byRole = await EmployeeService.GetEmployeesByRole(RoleType.Supervisor);

// Get roles for specific employee
var roles = await EmployeeService.GetEmployeeRoles(employeeId);
// Returns: List<RoleType>

// Add role to employee
await EmployeeService.AddEmployeeRole(employeeId, RoleType.Supervisor);

// Remove role from employee
await EmployeeService.RemoveEmployeeRole(employeeId, RoleType.Supervisor);
```

### Region CRUD
```csharp
// Get all regions
var regions = await EmployeeService.GetAllRegions();

// Get single region
var region = await EmployeeService.GetRegionById(regionId);

// Add new region
var newRegion = new Region { RegionName = "SEQ - South East Qld" };
await EmployeeService.AddRegion(newRegion);

// Update region
region.RegionName = "SEQ - South East Queensland";
await EmployeeService.UpdateRegion(region);

// Delete region (only if no employees)
bool success = await EmployeeService.DeleteRegion(regionId);
if (!success) 
{
    // Region has employees or error occurred
}
```

### Validation
```csharp
// Check if employee name exists (add scenario)
if (await EmployeeService.EmployeeNameExists("John Smith"))
{
    // Show error: Name already exists
}

// Check if employee name exists (edit scenario)
if (await EmployeeService.EmployeeNameExists("John Smith", excludeEmployeeId: 5))
{
    // Show error: Another employee has this name
}

// Check if region name exists (add scenario)
if (await EmployeeService.RegionNameExists("SEQ - South East Qld"))
{
    // Show error: Region already exists
}

// Check if region name exists (edit scenario)
if (await EmployeeService.RegionNameExists("SEQ - South East Qld", excludeRegionId: 1))
{
    // Show error: Another region has this name
}
```

### Reporting
```csharp
// Get employee count by region
var byRegion = await EmployeeService.GetEmployeeCountByRegion();
// Returns: Dictionary<string, int>
// Example: { "SEQ - South East Qld": 5, "SWQ - South West Qld": 3 }

// Get employee count by role
var byRole = await EmployeeService.GetEmployeeCountByRole();
// Returns: Dictionary<string, int>
// Example: { "Supervisor": 8, "Senior Estimator": 6 }
```

---

## RoleType Enum Values

```csharp
public enum RoleType
{
    Supervisor = 1,
    SeniorEstimator = 2,
    PrincipalEstimator = 3
}
```

---

## Common Admin Page Patterns

### Employee List Page
```razor
@inject EmployeeService EmployeeService

<table>
    <thead>
        <tr>
            <th>Name</th>
            <th>Region</th>
            <th>Roles</th>
            <th>Status</th>
            <th>Created</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var (employee, roles) in employeesWithRoles)
        {
            <tr>
                <td>@employee.FullName</td>
                <td>@(employee.Region?.RegionName ?? "-")</td>
                <td>@string.Join(", ", roles)</td>
                <td>@(employee.Active ? "Active" : "Inactive")</td>
                <td>@employee.DateCreated.ToString("dd/MM/yyyy")</td>
                <td>
                    <button @onclick="() => EditEmployee(employee.EmployeeId)">Edit</button>
                    @if (employee.Active)
                    {
                        <button @onclick="() => DeactivateEmployee(employee.EmployeeId)">Deactivate</button>
                    }
                    else
                    {
                        <button @onclick="() => ActivateEmployee(employee.EmployeeId)">Activate</button>
                    }
                </td>
            </tr>
        }
    </tbody>
</table>

@code {
    private List<(Employee, List<string>)> employeesWithRoles = new();

    protected override async Task OnInitializedAsync()
    {
        employeesWithRoles = await EmployeeService.GetAllEmployeesWithRoles();
    }

    private async Task DeactivateEmployee(int employeeId)
    {
        bool confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Deactivate this employee?");
        if (confirmed)
        {
            await EmployeeService.DeleteEmployee(employeeId);
            employeesWithRoles = await EmployeeService.GetAllEmployeesWithRoles();
        }
    }

    private async Task ActivateEmployee(int employeeId)
    {
        await EmployeeService.ActivateEmployee(employeeId);
        employeesWithRoles = await EmployeeService.GetAllEmployeesWithRoles();
    }
}
```

### Employee Add/Edit Form
```razor
@inject EmployeeService EmployeeService

<div class="form-group">
    <label>Full Name</label>
    <input type="text" class="form-control" @bind="employee.FullName" />
</div>

<div class="form-group">
    <label>Region</label>
    <select class="form-control" @bind="employee.RegionId">
        <option value="">-- Select Region --</option>
        @foreach (var region in regions)
        {
            <option value="@region.RegionId">@region.RegionName</option>
        }
    </select>
</div>

<div class="form-group">
    <label>Roles</label>
    <div>
        <label><input type="checkbox" @bind="isSupervisor" /> Supervisor</label><br/>
        <label><input type="checkbox" @bind="isSeniorEstimator" /> Senior Estimator</label><br/>
        <label><input type="checkbox" @bind="isPrincipalEstimator" /> Principal Estimator</label>
    </div>
</div>

<button class="btn btn-primary" @onclick="SaveEmployee">Save</button>

@code {
    [Parameter] public int? EmployeeId { get; set; }
    
    private Employee employee = new();
    private List<Region> regions = new();
    private bool isSupervisor = false;
    private bool isSeniorEstimator = false;
    private bool isPrincipalEstimator = false;

    protected override async Task OnInitializedAsync()
    {
        regions = await EmployeeService.GetAllRegions();
        
        if (EmployeeId.HasValue)
        {
            // Edit mode
            employee = await EmployeeService.GetEmployeeById(EmployeeId.Value);
            var roles = await EmployeeService.GetEmployeeRoles(EmployeeId.Value);
            
            isSupervisor = roles.Contains(RoleType.Supervisor);
            isSeniorEstimator = roles.Contains(RoleType.SeniorEstimator);
            isPrincipalEstimator = roles.Contains(RoleType.PrincipalEstimator);
        }
    }

    private async Task SaveEmployee()
    {
        // Validate name
        bool nameExists = await EmployeeService.EmployeeNameExists(
            employee.FullName, 
            EmployeeId
        );
        
        if (nameExists)
        {
            await JSRuntime.InvokeVoidAsync("alert", "Employee name already exists");
            return;
        }

        // Save employee
        if (EmployeeId.HasValue)
        {
            await EmployeeService.UpdateEmployee(employee);
        }
        else
        {
            employee = await EmployeeService.AddEmployee(employee);
            EmployeeId = employee.EmployeeId;
        }

        // Update roles
        var currentRoles = await EmployeeService.GetEmployeeRoles(EmployeeId.Value);
        
        // Supervisor
        if (isSupervisor && !currentRoles.Contains(RoleType.Supervisor))
            await EmployeeService.AddEmployeeRole(EmployeeId.Value, RoleType.Supervisor);
        else if (!isSupervisor && currentRoles.Contains(RoleType.Supervisor))
            await EmployeeService.RemoveEmployeeRole(EmployeeId.Value, RoleType.Supervisor);
        
        // Senior Estimator
        if (isSeniorEstimator && !currentRoles.Contains(RoleType.SeniorEstimator))
            await EmployeeService.AddEmployeeRole(EmployeeId.Value, RoleType.SeniorEstimator);
        else if (!isSeniorEstimator && currentRoles.Contains(RoleType.SeniorEstimator))
            await EmployeeService.RemoveEmployeeRole(EmployeeId.Value, RoleType.SeniorEstimator);
        
        // Principal Estimator
        if (isPrincipalEstimator && !currentRoles.Contains(RoleType.PrincipalEstimator))
            await EmployeeService.AddEmployeeRole(EmployeeId.Value, RoleType.PrincipalEstimator);
        else if (!isPrincipalEstimator && currentRoles.Contains(RoleType.PrincipalEstimator))
            await EmployeeService.RemoveEmployeeRole(EmployeeId.Value, RoleType.PrincipalEstimator);

        // Navigate back to list
        Navigation.NavigateTo("/admin/employees");
    }
}
```

### Region List Page
```razor
@inject EmployeeService EmployeeService

<table>
    <thead>
        <tr>
            <th>Region Name</th>
            <th>Employee Count</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var region in regions)
        {
            <tr>
                <td>@region.RegionName</td>
                <td>@(employeeCountByRegion.ContainsKey(region.RegionName) ? employeeCountByRegion[region.RegionName] : 0)</td>
                <td>
                    <button @onclick="() => EditRegion(region.RegionId)">Edit</button>
                    <button @onclick="() => DeleteRegion(region.RegionId)">Delete</button>
                </td>
            </tr>
        }
    </tbody>
</table>

@code {
    private List<Region> regions = new();
    private Dictionary<string, int> employeeCountByRegion = new();

    protected override async Task OnInitializedAsync()
    {
        regions = await EmployeeService.GetAllRegions();
        employeeCountByRegion = await EmployeeService.GetEmployeeCountByRegion();
    }

    private async Task DeleteRegion(int regionId)
    {
        bool confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Delete this region?");
        if (confirmed)
        {
            bool success = await EmployeeService.DeleteRegion(regionId);
            if (success)
            {
                regions = await EmployeeService.GetAllRegions();
                employeeCountByRegion = await EmployeeService.GetEmployeeCountByRegion();
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", "Cannot delete region with assigned employees");
            }
        }
    }
}
```

---

## Important Notes

1. **Always validate before saving**
   - Use `EmployeeNameExists()` and `RegionNameExists()`
   - Pass `excludeId` parameter when editing

2. **Handle soft delete properly**
   - Use `DeleteEmployee()` to deactivate
   - Use `ActivateEmployee()` to reactivate
   - Use `GetAllEmployeesIncludingInactive()` in admin interface

3. **Region deletion safety**
   - `DeleteRegion()` returns `false` if employees exist
   - Show appropriate error message to user

4. **Role management**
   - Get current roles before updating
   - Add missing roles
   - Remove unchecked roles
   - Unique constraint prevents duplicates

5. **Display formatting**
   - Use `GetAllEmployeesWithRoles()` for easy display
   - Roles are already formatted as strings
   - Region names include full description
