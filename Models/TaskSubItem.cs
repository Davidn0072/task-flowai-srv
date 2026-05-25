using System.Text.Json.Serialization;

namespace TaskFlowAISrv.Models;

public class TaskSubItem
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public required string Title { get; set; }
    public bool IsDone { get; set; }
    public int? OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public TaskItem? Task { get; set; }
}
