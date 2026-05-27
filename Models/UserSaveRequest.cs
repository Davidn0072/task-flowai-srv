namespace TaskFlowAISrv.Models;

public class UserSaveRequest
{
    public required User User { get; set; }
    public string? OldPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}

public class UserValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; set; } = [];
    public User? SavedUser { get; set; }
}
