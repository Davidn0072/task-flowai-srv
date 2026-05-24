using Microsoft.AspNetCore.Mvc;
using TaskFlowAISrv.Models;
using TaskFlowAISrv.Services;

namespace TaskFlowAISrv.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ITaskSubItemService _subItemService;
    private readonly IAIService _aiService;

    public TasksController(ITaskService taskService, ITaskSubItemService subItemService, IAIService aiService)
    {
        _taskService = taskService;
        _subItemService = subItemService;
        _aiService = aiService;
    }

    [HttpGet]
    public async System.Threading.Tasks.Task<ActionResult<List<TaskItem>>> GetAll()
    {
        return await _taskService.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async System.Threading.Tasks.Task<ActionResult<TaskItem>> GetById(int id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null) return NotFound();
        return task;
    }

    [HttpGet("user/{userId}")]
    public async System.Threading.Tasks.Task<ActionResult<List<TaskItem>>> GetByUserId(int userId)
    {
        return await _taskService.GetByUserIdAsync(userId);
    }

    [HttpPost]
    public async System.Threading.Tasks.Task<ActionResult<TaskItem>> Create(TaskItem task)
    {
        return CreatedAtAction(nameof(GetById), new { id = task.Id },
            await _taskService.CreateAsync(task));
    }

    [HttpPut("{id}")]
    public async System.Threading.Tasks.Task<IActionResult> Update(int id, TaskItem task)
    {
        if (id != task.Id) return BadRequest();
        await _taskService.UpdateAsync(task);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async System.Threading.Tasks.Task<IActionResult> Delete(int id)
    {
        await _taskService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{taskId}/generate-subtasks")]
    public async System.Threading.Tasks.Task<ActionResult<List<TaskSubItem>>> GenerateSubtasks(int taskId)
    {
        var task = await _taskService.GetByIdAsync(taskId);
        if (task == null) return NotFound();

        try
        {
            var subtasks = await _aiService.GenerateSubtasksAsync(task.Title, task.Description);
            var createdSubItems = new List<TaskSubItem>();

            for (int i = 0; i < subtasks.Count; i++)
            {
                var subItem = new TaskSubItem
                {
                    TaskId = taskId,
                    Title = subtasks[i],
                    IsDone = false,
                    OrderIndex = i,
                    CreatedAt = DateTime.UtcNow
                };
                var created = await _subItemService.CreateAsync(subItem);
                createdSubItems.Add(created);
            }

            return Ok(createdSubItems);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
