using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ArtEvaluatorAPI.Models;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArtEvaluatorAPI.Middleware;

public partial class UserKeyValidationMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        string ip = context.Connection.RemoteIpAddress == null ? "" : context.Connection.RemoteIpAddress.ToString();
        try
        {
            if (!context.Request.Headers.TryGetValue("userkey", out var userkeyValues))
            {
                Log.Warning("userkey header missing");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync($"Unauthorized: {ip}");
                return;
            }

            var userkey = userkeyValues.ToString() + "==";

            if (!IsValidGuidBase64(userkey, out var guid))
            {
                Log.Warning("Invalid userkey format");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync($"Forbidden: {ip}");
                return;
            }

            var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
            if (!await dbContext.CheckUserKeyAsync(guid.ToString()))
            {
                Log.Warning("Invalid userkey");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync($"Forbidden: {ip}");
                return;
            }
        }
        catch (Exception ex)
        {
            Log.Error("Error in userkey validation: " + ex.Message);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Internal Server Error");
            return;
        }

        await _next(context);
    }

    private static bool IsValidGuidBase64(string base64, out Guid guid)
    {
        try
        {
            var bytes = Convert.FromBase64String(base64);
            guid = new Guid(bytes);
            return RegexGUID().IsMatch(guid.ToString());
        }
        catch
        {
            guid = Guid.Empty;
            return false;
        }
    }

    [GeneratedRegex(@"^[{(]?[0-9a-fA-F]{8}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{12}[)}]?$")]
    private static partial Regex RegexGUID();
}
