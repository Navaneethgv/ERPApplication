using ERP.Application.DTOs;
using ERP.Domain.Entities;

namespace ERP.Application.Interfaces.Services;

public interface INotificationService
{
    List<NotificationDto> GetAll(string? role = null, int limit = 20);
    int GetUnreadCount(string? role = null);
    NotificationDto CreateOrderNotification(Sale sale, string customerName);
    bool MarkAsRead(int notificationId);
    int MarkAllAsRead(string? role = null);
}
