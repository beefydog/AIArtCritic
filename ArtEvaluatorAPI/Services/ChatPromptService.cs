using Serilog;

namespace ArtEvaluatorAPI.Services;

public class ChatPromptService : IChatPromptService
{
    public string ChatPrompt { get; }

    public ChatPromptService()
    {
        string chatPromptFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChatPrompt.txt");

        if (!File.Exists(chatPromptFilePath))
        {
            string ErrMsg = $"ChatPrompt.txt file not found at path \"{chatPromptFilePath}\". Please ensure the file is in the same directory as the executable.";
            Log.Error(ErrMsg);
            throw new FileNotFoundException(ErrMsg);
        }

        ChatPrompt = File.ReadAllText(chatPromptFilePath);
    }
}
