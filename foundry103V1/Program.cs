using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        DotNetEnv.Env.TraversePath().Load();
        string endpoint = "https://ai-dev-103-foundry.services.ai.azure.com/openai/v1";
        string deploymentName = "gpt-5-mini";
        string apiKey = Environment.GetEnvironmentVariable("AZURE_AI_API_KEY") ?? "";

        using var http = new HttpClient();
        http.Timeout = TimeSpan.FromMinutes(2);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var requestBody = new
        {
            model = deploymentName,
            instructions = "You are a helpful assistant.",
            input = "Who are you?",
            max_output_tokens = 1000,
            tools = new[]
            {
                new { type = "web_search_preview" }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        Console.WriteLine("Sending request with web search tool...");
        var response = await http.PostAsync($"{endpoint}/responses", content);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error: {response.StatusCode}");
            Console.WriteLine(responseText);
            return;
        }

        // Parse the Responses API output
        using var doc = JsonDocument.Parse(responseText);
        var root = doc.RootElement;

        if (root.TryGetProperty("output", out var output))
        {
            foreach (var item in output.EnumerateArray())
            {
                var type = item.GetProperty("type").GetString();

                // Show web search results if present
                if (type == "web_search_call")
                {
                    Console.WriteLine("[Web search performed]");
                }

                // Show the assistant's message
                if (type == "message")
                {
                    var contentArr = item.GetProperty("content");
                    foreach (var part in contentArr.EnumerateArray())
                    {
                        if (part.TryGetProperty("text", out var text))
                        {
                            Console.WriteLine($"answer: {text.GetString()}");
                        }
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("Unexpected response:");
            Console.WriteLine(responseText);
        }
    }
}
