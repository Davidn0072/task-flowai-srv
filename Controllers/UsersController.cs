using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskFlowAISrv.Models;
using TaskFlowAISrv.Services;

namespace TaskFlowAISrv.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<User>>> GetAll()
    {
        return await _userService.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return user;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<User>> Create(UserSaveRequest request)
    {
        var result = await _userService.CreateAsync(request);
        if (!result.IsValid) return BadRequest(result.Errors);
        return CreatedAtAction(nameof(GetById), new { id = result.SavedUser!.Id }, result.SavedUser);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UserSaveRequest request)
    {
        if (id != request.User.Id) return BadRequest();
        var result = await _userService.UpdateAsync(request);
        if (!result.IsValid) return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
}
