using ArtEvaluatorAPI.Configurations;
using ArtEvaluatorAPI.Models;
using ArtEvaluatorAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Serilog;
using System.Security.Cryptography;
using System.Text;

namespace ArtEvaluatorAPI.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController(IOptions<AppSettings> appSettings, IChatPromptService chatPromptService, IDummyResponseService dummyResponseService, IOptions<OpenAIOptions> openAIOptions, AppDbContext dbContext) : ControllerBase, IChatController
{
    private readonly IChatPromptService _chatPromptService = chatPromptService;
    private readonly IDummyResponseService _dummyResponseService = dummyResponseService;
    private readonly OpenAIOptions _openAIOptions = openAIOptions.Value;
    private readonly AppSettings _settings = appSettings.Value;
    private readonly AppDbContext _dbContext = dbContext;

    [HttpPost("processimage")]
    public async Task<IActionResult> ProcessImage([FromForm] ImageProcessingRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            if (request.ImageFile == null || request.ImageFile.Length == 0)
            {
                Log.Error("No image file provided.");
                return BadRequest("No image file provided.");
            }

            // Hash the image file
            using var md5 = MD5.Create();
            using var stream = request.ImageFile.OpenReadStream();
            var imageFileHash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();

            string chatPrompt = _chatPromptService.ChatPrompt;

            string additionalInfo = $"{(string.IsNullOrEmpty(request.MediaType) ? "" : $"Media Type: {request.MediaType}\n")}" +
                                    $"{(string.IsNullOrEmpty(request.Properties) ? "" : $"Properties: {request.Properties}\n")}" +
                                    $"{(string.IsNullOrEmpty(request.Dimensions) ? "" : $"Dimensions: {request.Dimensions}\n")}" +
                                    $"{(string.IsNullOrEmpty(request.Artist) ? "" : $"Artist: {request.Artist}\n")}" +
                                    $"{(request.Year == null ? "" : $"Year: {request.Year}\n")}" +
                                    $"{(string.IsNullOrEmpty(request.Title) ? "" : $"Title: {request.Title}\n")}" +
                                    $"{(string.IsNullOrEmpty(request.Location) ? "" : $"Location: {request.Location}\n")}";

            string userKey = Request.Headers["userkey"].ToString();
            var existingRequest = await _dbContext.PostRequests
                .FirstOrDefaultAsync(pr => pr.ImageFileHash == imageFileHash && pr.AdditionalInfo == additionalInfo && pr.UserKey == userKey);

            if (existingRequest != null)
            {
                Log.Information("Duplicate request detected, returning cached response.");
                return Ok(new { AssistantResponse = existingRequest.APIResponse });
            }

            ChatClient client = new("gpt-4o", _openAIOptions.ApiKey!);

            using Stream imageStream = request.ImageFile.OpenReadStream();
            BinaryData imageBytes = BinaryData.FromStream(imageStream);

            List<ChatMessage> messages =
            [
                new UserChatMessage(
                    ChatMessageContentPart.CreateTextMessageContentPart(chatPrompt),
                    ChatMessageContentPart.CreateImageMessageContentPart(imageBytes, "image/png"))
            ];

            if (_settings.TestMode)
            {
                // BYPASS FOR TESTING WITHOUT CALLING OPENAI
                Log.Information("Dummy chat response mode.");
                var newRequest = new PostRequest
                {
                    AdditionalInfo = additionalInfo,
                    ImageFileHash = imageFileHash,
                    UserKey = userKey,
                    APIResponse = _dummyResponseService.DummyResponse
                };
                _dbContext.PostRequests.Add(newRequest);
                await _dbContext.SaveChangesAsync();

                return Ok(_dummyResponseService.DummyResponse);
            }
            else
            {
                ChatCompletion chatCompletion = await client.CompleteChatAsync(messages);

                Log.Information("Chat completion successful.");

                var newRequest = new PostRequest
                {
                    AdditionalInfo = additionalInfo,
                    ImageFileHash = imageFileHash,
                    UserKey = userKey,
                    APIResponse = chatCompletion.Content[0].Text
                };

                _dbContext.PostRequests.Add(newRequest);
                await _dbContext.SaveChangesAsync();

                return Ok(new { AssistantResponse = chatCompletion.Content[0].Text });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while processing the image.");
            return StatusCode(500, "An internal server error occurred. Please try again later.");
        }
    }
}
