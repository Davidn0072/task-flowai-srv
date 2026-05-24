using System.Text.Json;

namespace TaskFlowAISrv.Services;

public interface IAIService
{
    System.Threading.Tasks.Task<List<string>> GenerateSubtasksAsync(string taskTitle, string? taskDescription);
}

public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIService> _logger;

    public AIService(HttpClient httpClient, IConfiguration configuration, ILogger<AIService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<List<string>> GenerateSubtasksAsync(string taskTitle, string? taskDescription)
    {
        try
        {
            var vercelGatewayUrl = _configuration["VercelGateway:Url"];
            var apiKey = _configuration["VercelGateway:ApiKey"];

            if (string.IsNullOrEmpty(vercelGatewayUrl) || string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("VercelGateway configuration is missing");
            }

            var prompt = $@"You are a senior software engineering assistant.

Your task is to break down a given task into small, clear, actionable subtasks that a developer can execute step by step.

Rules:
- Output ONLY a JSON array of subtasks.
- Each subtask must be a short actionable sentence.
- Do not include explanations, headers, or extra text.
- Do not repeat the original task.
- Do not number the items.
- Keep subtasks atomic (one action per subtask).
- Order them logically from start to finish.

Task:
""{taskTitle}""

Optional context:
""{taskDescription}""

Output format example:
[
  ""Create project structure"",
  ""Implement authentication API"",
  ""Add database models"",
  ""Connect frontend to backend""
]";

            var request = new
            {
                prompt = prompt,
                model = "claude-3-5-sonnet-20241022",
                max_tokens = 1024
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, vercelGatewayUrl)
            {
                Content = content
            };

            httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");

            _logger.LogInformation("Sending request to Vercel Gateway for subtask generation");

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responseText = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Vercel Gateway response: {responseText}");

            var subtasks = ParseSubtasksFromResponse(responseText);
            return subtasks;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calling Vercel Gateway: {ex.Message}");
            throw;
        }
    }

    private List<string> ParseSubtasksFromResponse(string response)
    {
        try
        {
            using (JsonDocument doc = JsonDocument.Parse(response))
            {
                var root = doc.RootElement;

                // Try to find the content or text field
                if (root.TryGetProperty("content", out var contentElement))
                {
                    if (contentElement.ValueKind == JsonValueKind.Array && contentElement.GetArrayLength() > 0)
                    {
                        var firstContent = contentElement[0];
                        if (firstContent.TryGetProperty("text", out var textElement))
                        {
                            return ParseJsonArray(textElement.GetString() ?? "");
                        }
                    }
                }

                if (root.TryGetProperty("text", out var textElement2))
                {
                    return ParseJsonArray(textElement2.GetString() ?? "");
                }

                if (root.TryGetProperty("response", out var responseElement))
                {
                    return ParseJsonArray(responseElement.GetString() ?? "");
                }

                // Fallback: try to parse the entire response as JSON
                return ParseJsonArray(response);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to parse Vercel Gateway response: {ex.Message}");
        }
    }

    private List<string> ParseJsonArray(string jsonText)
    {
        var result = new List<string>();

        // Find JSON array in the text
        var startIdx = jsonText.IndexOf('[');
        var endIdx = jsonText.LastIndexOf(']');

        if (startIdx >= 0 && endIdx > startIdx)
        {
            var jsonStr = jsonText.Substring(startIdx, endIdx - startIdx + 1);
            using (JsonDocument doc = JsonDocument.Parse(jsonStr))
            {
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in root.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            result.Add(item.GetString() ?? "");
                        }
                    }
                }
            }
        }

        return result;
    }
}
