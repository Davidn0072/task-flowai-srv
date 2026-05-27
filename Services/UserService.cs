using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TaskFlowAISrv.Data;
using TaskFlowAISrv.Models;

namespace TaskFlowAISrv.Services;

public record LoginResponse(User User, string Token);

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<UserValidationResult> CreateAsync(UserSaveRequest request);
    Task<UserValidationResult> UpdateAsync(UserSaveRequest request);
    Task DeleteAsync(int id);
    Task<LoginResponse?> LoginAsync(string email, string password);
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

    public async Task<List<User>> GetAllAsync() =>
        await _context.Users.ToListAsync();

    public async Task<User?> GetByIdAsync(int id) =>
        await _context.Users.FindAsync(id);

    public async Task<UserValidationResult> CreateAsync(UserSaveRequest request)
    {
        var result = await ValidateAsync(request);
        if (!result.IsValid) return result;

        request.User.CreatedAt = DateTime.UtcNow;
        _context.Users.Add(request.User);
        await _context.SaveChangesAsync();

        result.SavedUser = request.User;
        return result;
    }

    public async Task<UserValidationResult> UpdateAsync(UserSaveRequest request)
    {
        var result = await ValidateAsync(request);
        if (!result.IsValid) return result;

        var existing = await _context.Users.FindAsync(request.User.Id);
        if (existing == null)
        {
            result.Errors.Add("User not found.");
            return result;
        }

        existing.Username = request.User.Username;
        existing.Email = request.User.Email;

        if (!string.IsNullOrEmpty(request.User.Password))
            existing.Password = request.User.Password;

        await _context.SaveChangesAsync();

        result.SavedUser = existing;
        return result;
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null && user.Password == password)
            return new LoginResponse(user, _jwtTokenService.GenerateToken(user));
        return null;
    }

    // ─── Validation ───────────────────────────────────────────────────────────

    private async Task<UserValidationResult> ValidateAsync(UserSaveRequest request)
    {
        var result = new UserValidationResult();
        var user = request.User;
        bool isUpdate = user.Id > 0;

        if (!IsEmailValid(user.Email))
            result.Errors.Add("Invalid email address.");

        if (await _context.Users.AnyAsync(u => u.Username == user.Username && u.Id != user.Id))
            result.Errors.Add("Username already exists.");

        if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.Id != user.Id))
            result.Errors.Add("Email already exists.");

        if (isUpdate)
        {
            var existing = await _context.Users.FindAsync(user.Id);
            if (existing == null || existing.Password != request.OldPassword)
                result.Errors.Add("Old password is incorrect.");

            if (!string.IsNullOrEmpty(user.Password) && user.Password != request.ConfirmPassword)
                result.Errors.Add("New password and confirmation do not match.");
        }
        else
        {
            if (user.Password != request.ConfirmPassword)
                result.Errors.Add("Password and confirmation do not match.");
        }

        return result;
    }

    private static bool IsEmailValid(string email) =>
        Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
}
