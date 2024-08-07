
namespace ArtEvaluatorAPI.Middleware;

public interface IUserKeyValidationMiddleware
{
    Task InvokeAsync(HttpContext context);
}