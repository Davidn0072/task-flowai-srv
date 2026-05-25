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
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ITaskSubItemService subItemService, IAIService aiService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _subItemService = subItemService;
        _aiService = aiService;
        _logger = logger;
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
    public async System.Threading.Tasks.Task<ActionResult> GenerateSubtasks(int taskId)
    {
        _logger.LogInformation($"[GENERATE SUBTASKS] Starting for Task ID: {taskId}");

        var task = await _taskService.GetByIdAsync(taskId);
        if (task == null)
        {
            _logger.LogWarning($"[GENERATE SUBTASKS] Task {taskId} not found");
            return NotFound();
        }

        _logger.LogInformation($"[GENERATE SUBTASKS] Task found: '{task.Title}' | Description: '{task.Description}'");

        try
        {
            _logger.LogInformation($"[GENERATE SUBTASKS] Calling AI Service...");
            var subtasks = await _aiService.GenerateSubtasksAsync(task.Title, task.Description);

            _logger.LogInformation($"[GENERATE SUBTASKS] AI returned {subtasks.Count} subtasks");
            foreach (var (index, subtask) in subtasks.Select((s, i) => (i, s)))
            {
                _logger.LogInformation($"[GENERATE SUBTASKS] [{index + 1}] {subtask}");
            }

            var createdSubItems = new List<object>();

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
                _logger.LogInformation($"[GENERATE SUBTASKS] Created SubItem: ID={created.Id}, Title='{created.Title}'");
                createdSubItems.Add(new { created.Id, created.TaskId, created.Title, created.IsDone, created.OrderIndex, created.CreatedAt });
            }

            _logger.LogInformation($"[GENERATE SUBTASKS] Successfully created {createdSubItems.Count} subtasks in database");
            return Ok(createdSubItems);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[GENERATE SUBTASKS] Error: {ex.Message} | StackTrace: {ex.StackTrace}");
            return BadRequest(new { error = ex.Message });
        }
    }
}
