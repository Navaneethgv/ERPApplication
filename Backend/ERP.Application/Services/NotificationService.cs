



namespace ERP.Application.Services;
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public List<NotificationDto> GetAll(string? role = null, int limit = 20)
    {
        var list = _notificationRepository.GetAll(role);
        return list.Take(limit).Select(MapToDto).ToList();
    }

    public int GetUnreadCount(string? role = null)
    {
        return _notificationRepository.GetUnreadCount(role);
    }

    public NotificationDto CreateOrderNotification(Sale sale, string customerName)
    {
        var notif = new Notification
        {
            Title = "New Order Placed",
            Message = $"{customerName} placed order #{sale.SaleOrderNumber} for {sale.TotalAmount:C}",
            Type = "Order",
            OrderId = sale.SaleId,
            OrderNumber = sale.SaleOrderNumber,
            CustomerName = customerName,
            TotalAmount = sale.TotalAmount,
            OrderStatus = sale.Status.ToString(),
            CreatedAt = TimeHelper.Now,
            IsRead = false,
            TargetRole = "Employee"
        };

        var created = _notificationRepository.Add(notif);
        return MapToDto(created);
    }

    public bool MarkAsRead(int notificationId)
    {
        return _notificationRepository.MarkAsRead(notificationId);
    }

    public int MarkAllAsRead(string? role = null)
    {
        return _notificationRepository.MarkAllAsRead(role);
    }

    private static NotificationDto MapToDto(Notification n) => new()
    {
        NotificationId = n.NotificationId,
        Title = n.Title,
        Message = n.Message,
        Type = n.Type,
        OrderId = n.OrderId,
        OrderNumber = n.OrderNumber,
        CustomerName = n.CustomerName,
        TotalAmount = n.TotalAmount,
        OrderStatus = n.OrderStatus,
        CreatedAt = n.CreatedAt,
        IsRead = n.IsRead,
        TargetRole = n.TargetRole
    };
}


