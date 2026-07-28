using System.ClientModel;
using OpenAI.Responses;

DotNetEnv.Env.TraversePath().Load();

const string endpoint = "https://ai-dev-103-foundry.services.ai.azure.com/openai/v1";
const string deploymentName = "gpt-5-mini";

string? apiKey = Environment.GetEnvironmentVariable("AZURE_AI_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("Missing AZURE_AI_API_KEY. Set it in your environment or .env file.");
    return 1;
}

#pragma warning disable OPENAI001 // Responses API is experimental in OpenAI 2.12.0.
var client = new ResponsesClient(
    credential: new ApiKeyCredential(apiKey),
    options: new ResponsesClientOptions { Endpoint = new Uri(endpoint) });

const string imagePath = "./image.png"; // point this at a real image file
byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
BinaryData imageData = BinaryData.FromBytes(imageBytes, "image/png");

var options = new CreateResponseOptions(
    model: deploymentName,
    inputItems:
    [
        ResponseItem.CreateUserMessageItem(
        [
            ResponseContentPart.CreateInputTextPart("Explain what's in this image in detail."),
            ResponseContentPart.CreateInputImagePart(imageData, ResponseImageDetailLevel.Auto),
        ]),
    ]);

ClientResult<ResponseResult> response = await client.CreateResponseAsync(options);

Console.WriteLine($"answer: {response.Value.GetOutputText()}");
return 0;
#pragma warning restore OPENAI001
