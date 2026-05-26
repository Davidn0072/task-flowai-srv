using Microsoft.EntityFrameworkCore;
using TaskFlowAISrv.Data;
using TaskFlowAISrv.Models;

namespace TaskFlowAISrv.Services;

public interface ITaskService
{
    System.Threading.Tasks.Task<List<TaskItem>> GetAllAsync();
    System.Threading.Tasks.Task<TaskItem?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<List<TaskItem>> GetByUserIdAsync(int userId);
    System.Threading.Tasks.Task<List<TaskItem>> SearchAsync(string q);
    System.Threading.Tasks.Task<List<TaskItem>> FilterAsync(SearchFilter filter);
    System.Threading.Tasks.Task<TaskItem> CreateAsync(TaskItem task);
    System.Threading.Tasks.Task<TaskItem> UpdateAsync(TaskItem task);
    System.Threading.Tasks.Task DeleteAsync(int id);
}

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;

    public TaskService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<List<TaskItem>> GetAllAsync()
    {
        return await _context.TaskItems
            .Include(t => t.User)
            .Include(t => t.SubItems)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _context.TaskItems
            .Include(t => t.User)
            .Include(t => t.SubItems)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async System.Threading.Tasks.Task<List<TaskItem>> GetByUserIdAsync(int userId)
    {
        return await _context.TaskItems
            .Where(t => t.UserId == userId)
            .Include(t => t.User)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<List<TaskItem>> SearchAsync(string q)
    {
        return await _context.TaskItems
            .Include(t => t.User)
            .Include(t => t.SubItems)
            .Where(t => t.Title.Contains(q) ||
                        (t.Description != null && t.Description.Contains(q)))
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<List<TaskItem>> FilterAsync(SearchFilter filter)
    {
        var query = _context.TaskItems
            .Include(t => t.User)
            .Include(t => t.SubItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Employee))
            query = query.Where(t => t.User != null && t.User.Username.Contains(filter.Employee));

        if (!string.IsNullOrWhiteSpace(filter.Priority))
            query = query.Where(t => t.Priority == filter.Priority);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(t => t.Status == filter.Status);

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
            query = query.Where(t => t.Title.Contains(filter.SearchText) ||
                                     (t.Description != null && t.Description.Contains(filter.SearchText)));

        if (filter.DateFrom.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.DateTo.Value);

        return await query.ToListAsync();
    }

    public async System.Threading.Tasks.Task<TaskItem> CreateAsync(TaskItem task)
    {
        task.CreatedAt = DateTime.UtcNow;
        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async System.Threading.Tasks.Task<TaskItem> UpdateAsync(TaskItem task)
    {
        var existingTask = await _context.TaskItems.FindAsync(task.Id);
        if (existingTask != null)
        {
            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.Status = task.Status;
            existingTask.Priority = task.Priority;
            existingTask.DueDate = task.DueDate;
            existingTask.UserId = task.UserId;
            await _context.SaveChangesAsync();
            return existingTask;
        }
        return task;
    }

    public async System.Threading.Tasks.Task DeleteAsync(int id)
    {
        var task = await _context.TaskItems.FindAsync(id);
        if (task != null)
        {
            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();
        }
    }
}
