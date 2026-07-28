using OpenAI.Responses;
using System.ClientModel;


DotNetEnv.Env.TraversePath().Load();
string endpoint = "https://ai-dev-103-foundry.services.ai.azure.com/openai/v1";
string deploymentName = "gpt-5-mini";
string apiKey = Environment.GetEnvironmentVariable("AZURE_AI_API_KEY") ?? "";

#pragma warning disable OPENAI001

ResponsesClient client = new(
    credential: new ApiKeyCredential(apiKey),
    options: new ResponsesClientOptions()
    {
        Endpoint = new Uri($"{endpoint}"),
    });
CreateResponseOptions options = new()
{
    Model = deploymentName,
    InputItems =
    {
        ResponseItem.CreateUserMessageItem("What's the weather like today for my current location?"),
    },
};

ResponseResult response = client.CreateResponse(options);

Console.WriteLine($"[ASSISTANT]: {response.GetOutputText()}");
Console.ReadKey();