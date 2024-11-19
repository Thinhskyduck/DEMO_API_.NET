using Microsoft.EntityFrameworkCore;

namespace test_api_ver_2.Models;

public class UserContext : DbContext
{
    public UserContext(DbContextOptions<UserContext> options)
        : base(options)
    {
    }

    public DbSet<User> User { get; set; } = null!;
}
