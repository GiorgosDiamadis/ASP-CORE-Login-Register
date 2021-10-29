using Microsoft.EntityFrameworkCore;

namespace WebApplication.Database
{
    public class WebApplicationContext : DbContext
    {
        public WebApplicationContext (DbContextOptions<WebApplicationContext> options)
            : base(options)
        {
        }

        public DbSet<WebApplication.Models.User> User { get; set; }
    }
}
