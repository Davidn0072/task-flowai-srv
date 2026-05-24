using Microsoft.AspNetCore.Mvc;
using TaskFlowAISrv.Models;
using TaskFlowAISrv.Services;

namespace TaskFlowAISrv.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskSubItemsController : ControllerBase
{
    private readonly ITaskSubItemService _subItemService;

    public TaskSubItemsController(ITaskSubItemService subItemService)
    {
        _subItemService = subItemService;
    }

    [HttpGet("task/{taskId}")]
    public async System.Threading.Tasks.Task<ActionResult<List<TaskSubItem>>> GetByTaskId(int taskId)
    {
        return await _subItemService.GetByTaskIdAsync(taskId);
    }

    [HttpGet("{id}")]
    public async System.Threading.Tasks.Task<ActionResult<TaskSubItem>> GetById(int id)
    {
        var subItem = await _subItemService.GetByIdAsync(id);
        if (subItem == null) return NotFound();
        return subItem;
    }

    [HttpPost]
    public async System.Threading.Tasks.Task<ActionResult<TaskSubItem>> Create(TaskSubItem subItem)
    {
        return CreatedAtAction(nameof(GetById), new { id = subItem.Id },
            await _subItemService.CreateAsync(subItem));
    }

    [HttpPut("{id}")]
    public async System.Threading.Tasks.Task<IActionResult> Update(int id, TaskSubItem subItem)
    {
        if (id != subItem.Id) return BadRequest();
        await _subItemService.UpdateAsync(subItem);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async System.Threading.Tasks.Task<IActionResult> Delete(int id)
    {
        await _subItemService.DeleteAsync(id);
        return NoContent();
    }

    [HttpDelete("task/{taskId}")]
    public async System.Threading.Tasks.Task<IActionResult> DeleteByTaskId(int taskId)
    {
        await _subItemService.DeleteByTaskIdAsync(taskId);
        return NoContent();
    }
}
