using System.Text.Json;
using System.Text.Json.Serialization;

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
            var apiKey = _configuration["VercelGateway:ApiKey"];
            const string vercelGatewayUrl = "https://ai-gateway.vercel.sh/v1/chat/completions";

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("VercelGateway:ApiKey is missing in configuration");
            }

            var prompt = $@"You are a senior software engineering assistant.

Your task is to break down a given task into small, clear, actionable subtasks that a developer can execute step by step.

Rules:
- Output ONLY a JSON array of subtasks.
- Maximum 10 subtasks total.
- Each subtask must be a short actionable sentence (max 75 characters).
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
                model = "anthropic/claude-opus-4.7",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                stream = false
            };

            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, vercelGatewayUrl)
            {
                Content = content
            };

            httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");

            _logger.LogInformation("Sending request to Vercel AI Gateway for subtask generation");

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

                // OpenAI format: { choices: [{ message: { content: "..." } }] }
                if (root.TryGetProperty("choices", out var choicesElement))
                {
                    if (choicesElement.ValueKind == JsonValueKind.Array && choicesElement.GetArrayLength() > 0)
                    {
                        var firstChoice = choicesElement[0];
                        if (firstChoice.TryGetProperty("message", out var messageElement))
                        {
                            if (messageElement.TryGetProperty("content", out var contentElement))
                            {
                                var content = contentElement.GetString() ?? "";
                                return ParseJsonArray(content);
                            }
                        }
                    }
                }

                throw new Exception("Unexpected response format from Vercel Gateway");
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

        try
        {
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
                                var task = item.GetString();
                                if (!string.IsNullOrWhiteSpace(task) && task.Length <= 75)
                                {
                                    result.Add(task);
                                    // Stop at 10 subtasks max
                                    if (result.Count >= 10)
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error parsing JSON array: {ex.Message}");
        }

        return result;
    }
}
