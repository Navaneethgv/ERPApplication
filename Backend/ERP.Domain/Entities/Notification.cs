using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

public class Notification
{
    public int NotificationId { get; set; }
    public string Title { get; set; } = "New Order Placed";
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Order"; // Order, System, Info
    public int? OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string OrderStatus { get; set; } = "Confirmed";
    public DateTime CreatedAt { get; set; } = TimeHelper.Now;
    public bool IsRead { get; set; } = false;
    public string TargetRole { get; set; } = "Employee"; // Admin, Employee
}

