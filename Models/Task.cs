using System.ComponentModel.DataAnnotations.Schema;

namespace TaskFlowAISrv.Models;

public class TaskItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }

    public User? User { get; set; }
    public ICollection<TaskSubItem> SubItems { get; set; } = [];

    [NotMapped]
    public int SubItemsCount => SubItems?.Count ?? 0;
}
