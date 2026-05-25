using Microsoft.EntityFrameworkCore;
using TaskFlowAISrv.Data;
using TaskFlowAISrv.Models;

namespace TaskFlowAISrv.Services;

public record LoginResponse(User User, string Token);

public interface IUserService
{
    System.Threading.Tasks.Task<List<User>> GetAllAsync();
    System.Threading.Tasks.Task<User?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<User> CreateAsync(User user);
    System.Threading.Tasks.Task<User> UpdateAsync(User user);
    System.Threading.Tasks.Task DeleteAsync(int id);
    System.Threading.Tasks.Task<LoginResponse?> LoginAsync(string email, string password);
}

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public UserService(ApplicationDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
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
        var existingUser = await _context.Users.FindAsync(user.Id);
        if (existingUser == null)
        {
            return user;
        }

        existingUser.Username = user.Username;
        existingUser.Email = user.Email;
        existingUser.Password = user.Password;
        await _context.SaveChangesAsync();
        return existingUser;
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

    public async System.Threading.Tasks.Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null && user.Password == password)
        {
            var token = _jwtTokenService.GenerateToken(user);
            return new LoginResponse(user, token);
        }
        return null;
    }
}
