using Microsoft.EntityFrameworkCore;
using TaskFlowAISrv.Data;
using TaskFlowAISrv.Models;

namespace TaskFlowAISrv.Services;

public interface IUserService
{
    System.Threading.Tasks.Task<List<User>> GetAllAsync();
    System.Threading.Tasks.Task<User?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<User> CreateAsync(User user);
    System.Threading.Tasks.Task<User> UpdateAsync(User user);
    System.Threading.Tasks.Task DeleteAsync(int id);
}

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<List<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async System.Threading.Tasks.Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async System.Threading.Tasks.Task<User> CreateAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async System.Threading.Tasks.Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async System.Threading.Tasks.Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
