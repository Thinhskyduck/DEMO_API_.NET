using Microsoft.EntityFrameworkCore;

namespace test_api_ver_2.Models;

public class MovieContext : DbContext
{
    public MovieContext(DbContextOptions<MovieContext> options)  
        : base(options) 
    {  
    }

    public DbSet<Movie> Movies { get; set; } = null!;
}
