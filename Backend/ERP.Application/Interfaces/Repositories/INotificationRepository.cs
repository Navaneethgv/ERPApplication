using ERP.Domain.Entities;

namespace ERP.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    List<Notification> GetAll(string? role = null);
    Notification? GetById(int id);
    int GetUnreadCount(string? role = null);
    Notification Add(Notification notification);
    bool MarkAsRead(int notificationId);
    int MarkAllAsRead(string? role = null);
}
