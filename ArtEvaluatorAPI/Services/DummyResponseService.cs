using Serilog;

namespace ArtEvaluatorAPI.Services;

public class DummyResponseService : IDummyResponseService
{
    public string DummyResponse { get; }

    public DummyResponseService()
    {
        string DummyResponseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DummyResponse.txt");

        if (!File.Exists(DummyResponseFilePath))
        {
            string ErrMsg = $"DummyResponse.txt file not found at path \"{DummyResponseFilePath}\". Please ensure the file is in the same directory as the executable.";
            Log.Error(ErrMsg);
            throw new FileNotFoundException(ErrMsg);
        }

        DummyResponse = File.ReadAllText(DummyResponseFilePath);
    }
}
