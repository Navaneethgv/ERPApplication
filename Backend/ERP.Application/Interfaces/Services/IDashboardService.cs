using ERP.Application.DTOs;

namespace ERP.Application.Interfaces.Services;

public interface IDashboardService
{
    AdminDashboardDto GetAdminDashboard();
    EmployeeDashboardDto GetEmployeeDashboard(int? employeeId);
    CustomerDashboardDto GetCustomerDashboard(int customerId);
}
