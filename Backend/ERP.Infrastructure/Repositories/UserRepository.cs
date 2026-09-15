
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;
public class UserRepository : IUserRepository
{
    private readonly ErpDbContext _context;

    public UserRepository(ErpDbContext context)
    {
        _context = context;
    }

    public List<User> GetAll()
    {
        return _context.Users.AsNoTracking().ToList();
    }

    public User? GetById(int id)
    {
        return _context.Users.AsNoTracking().FirstOrDefault(u => u.UserId == id);
    }

    public User? GetByUsername(string username)
    {
        var lowerUsername = username.ToLower();
        return _context.Users.AsNoTracking().FirstOrDefault(u => u.Username.ToLower() == lowerUsername);
    }

    public User? GetByEmployeeId(int employeeId)
    {
        return _context.Users.AsNoTracking().FirstOrDefault(u => u.EmployeeId == employeeId);
    }

    public User? GetByCustomerId(int customerId)
    {
        return _context.Users.AsNoTracking().FirstOrDefault(u => u.CustomerId == customerId);
    }

    public User Add(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }

    public void Update(User user)
    {
        var existing = _context.Users.Find(user.UserId);
        if (existing != null)
        {
            existing.Username = user.Username;
            existing.PasswordHash = user.PasswordHash;
            existing.Role = user.Role;
            existing.EmployeeId = user.EmployeeId;
            existing.CustomerId = user.CustomerId;
            existing.FullName = user.FullName;
            existing.Status = user.Status;
            existing.PasswordChangedAt = user.PasswordChangedAt;
            existing.FailedLoginAttempts = user.FailedLoginAttempts;
            existing.LockoutEnd = user.LockoutEnd;
            _context.SaveChanges();
        }
    }

    public void Delete(int id)
    {
        var existing = _context.Users.Find(id);
        if (existing != null)
        {
            _context.Users.Remove(existing);
            _context.SaveChanges();
        }
    }
}


