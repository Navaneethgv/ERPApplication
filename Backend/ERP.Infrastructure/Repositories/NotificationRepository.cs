
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;
public class NotificationRepository : INotificationRepository
{
    private readonly ErpDbContext _context;

    public NotificationRepository(ErpDbContext context)
    {
        _context = context;
    }

    public List<Notification> GetAll(string? role = null)
    {
        var query = _context.Notifications.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(role))
        {
            var lowerRole = role.ToLower();
            query = query.Where(n =>
                n.TargetRole.ToLower() == lowerRole ||
                n.TargetRole.ToLower() == "all" ||
                string.IsNullOrEmpty(n.TargetRole));
        }

        return query.OrderByDescending(n => n.NotificationId).ToList();
    }

    public Notification? GetById(int id)
    {
        return _context.Notifications.AsNoTracking().FirstOrDefault(n => n.NotificationId == id);
    }

    public int GetUnreadCount(string? role = null)
    {
        var query = _context.Notifications.AsNoTracking().Where(n => !n.IsRead);

        if (!string.IsNullOrEmpty(role))
        {
            var lowerRole = role.ToLower();
            query = query.Where(n =>
                n.TargetRole.ToLower() == lowerRole ||
                n.TargetRole.ToLower() == "all" ||
                string.IsNullOrEmpty(n.TargetRole));
        }

        return query.Count();
    }

    public Notification Add(Notification notification)
    {
        _context.Notifications.Add(notification);
        _context.SaveChanges();
        return notification;
    }

    public bool MarkAsRead(int notificationId)
    {
        var notif = _context.Notifications.Find(notificationId);
        if (notif == null) return false;

        notif.IsRead = true;
        _context.SaveChanges();
        return true;
    }

    public int MarkAllAsRead(string? role = null)
    {
        var query = _context.Notifications.Where(n => !n.IsRead);

        if (!string.IsNullOrEmpty(role))
        {
            var lowerRole = role.ToLower();
            query = query.Where(n =>
                n.TargetRole.ToLower() == lowerRole ||
                n.TargetRole.ToLower() == "all" ||
                string.IsNullOrEmpty(n.TargetRole));
        }

        var unread = query.ToList();
        foreach (var item in unread)
        {
            item.IsRead = true;
        }

        _context.SaveChanges();
        return unread.Count;
    }
}


