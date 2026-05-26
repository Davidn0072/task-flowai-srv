namespace TaskFlowAISrv.Models;

public class SearchFilter
{
    public string? Employee { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public string? SearchText { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class AiSearchRequest
{
    public required string Query { get; set; }
}
