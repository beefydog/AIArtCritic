using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace ArtEvaluatorAPI.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserKey> UserKeys { get; set; }

    public DbSet<PostRequest> PostRequests { get; set; }

    public async Task<bool> CheckUserKeyAsync(string userKey)
    {
        var userKeyParam = new SqlParameter("@UserKey", userKey);
        var result = await Database.SqlQueryRaw<CheckUserKeyResult>("EXEC CheckUserKey @UserKey", userKeyParam).ToListAsync();

        return result.FirstOrDefault()?.UserKeyExists == 1;
    }
}
