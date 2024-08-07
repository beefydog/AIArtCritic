namespace APIKeyGenerator;

internal class Program
{
    static void Main(string[] args)
    {
        Guid guid = Guid.NewGuid();

        Console.WriteLine("guid: " + guid.ToString());
        Console.WriteLine("api key: " + GenerateApiKeyFromGuid(guid));
    }

    public static string GenerateApiKeyFromGuid(Guid guid)
    {
        var base64 = Convert.ToBase64String(guid.ToByteArray());
        return base64.TrimEnd('=');
    }

    /*
     Guid guid = Guid.NewGuid();


     */
}
