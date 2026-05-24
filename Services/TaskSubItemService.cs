using Microsoft.EntityFrameworkCore;
using TaskFlowAISrv.Data;
using TaskFlowAISrv.Models;

namespace TaskFlowAISrv.Services;

public interface ITaskSubItemService
{
    System.Threading.Tasks.Task<List<TaskSubItem>> GetByTaskIdAsync(int taskId);
    System.Threading.Tasks.Task<TaskSubItem?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<TaskSubItem> CreateAsync(TaskSubItem subItem);
    System.Threading.Tasks.Task<TaskSubItem> UpdateAsync(TaskSubItem subItem);
    System.Threading.Tasks.Task DeleteAsync(int id);
    System.Threading.Tasks.Task DeleteByTaskIdAsync(int taskId);
}

public class TaskSubItemService : ITaskSubItemService
{
    private readonly ApplicationDbContext _context;

    public TaskSubItemService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<List<TaskSubItem>> GetByTaskIdAsync(int taskId)
    {
        return await _context.TaskSubItems
            .Where(s => s.TaskId == taskId)
            .OrderBy(s => s.OrderIndex)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<TaskSubItem?> GetByIdAsync(int id)
    {
        return await _context.TaskSubItems.FindAsync(id);
    }

    public async System.Threading.Tasks.Task<TaskSubItem> CreateAsync(TaskSubItem subItem)
    {
        subItem.CreatedAt = DateTime.UtcNow;
        _context.TaskSubItems.Add(subItem);
        await _context.SaveChangesAsync();
        return subItem;
    }

    public async System.Threading.Tasks.Task<TaskSubItem> UpdateAsync(TaskSubItem subItem)
    {
        _context.TaskSubItems.Update(subItem);
        await _context.SaveChangesAsync();
        return subItem;
    }

    public async System.Threading.Tasks.Task DeleteAsync(int id)
    {
        var subItem = await _context.TaskSubItems.FindAsync(id);
        if (subItem != null)
        {
            _context.TaskSubItems.Remove(subItem);
            await _context.SaveChangesAsync();
        }
    }

    public async System.Threading.Tasks.Task DeleteByTaskIdAsync(int taskId)
    {
        var subItems = await _context.TaskSubItems
            .Where(s => s.TaskId == taskId)
            .ToListAsync();
        _context.TaskSubItems.RemoveRange(subItems);
        await _context.SaveChangesAsync();
    }
}
