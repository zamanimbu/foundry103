using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

DotNetEnv.Env.TraversePath().Load();

const string endpoint = "https://ai-dev-103-foundry.services.ai.azure.com/openai/v1";
const string deploymentName = "gpt-5-mini";

string? apiKey = Environment.GetEnvironmentVariable("AZURE_AI_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("Missing AZURE_AI_API_KEY. Set it in your environment or .env file.");
    return 1;
}

using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

var request = new
{
    model = deploymentName,
    instructions = "You are a helpful assistant.",
    input = "Who are you?",
    max_output_tokens = 1000,
    tools = new[] { new { type = "web_search_preview" } },
};

Console.WriteLine("Sending request with web search tool...");
using var response = await http.PostAsJsonAsync($"{endpoint}/responses", request);
string responseText = await response.Content.ReadAsStringAsync();

if (!response.IsSuccessStatusCode)
{
    Console.Error.WriteLine($"Error: {(int)response.StatusCode} {response.StatusCode}");
    Console.Error.WriteLine(responseText);
    return 1;
}

PrintResponse(responseText);
return 0;

// Parses the Responses API payload, printing web-search activity and the assistant's answer.
static void PrintResponse(string responseText)
{
    using var doc = JsonDocument.Parse(responseText);

    if (!doc.RootElement.TryGetProperty("output", out var output))
    {
        Console.WriteLine("Unexpected response:");
        Console.WriteLine(responseText);
        return;
    }

    foreach (var item in output.EnumerateArray())
    {
        switch (item.GetProperty("type").GetString())
        {
            case "web_search_call":
                Console.WriteLine("[Web search performed]");
                break;

            case "message":
                foreach (var part in item.GetProperty("content").EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text))
                        Console.WriteLine($"answer: {text.GetString()}");
                }
                break;
        }
    }
}
