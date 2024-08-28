using AzStorageAccountPrivareEndP.Models;
using Microsoft.EntityFrameworkCore;

namespace AzStorageAccountPrivareEndP
{
    public class AppDatabaseContext: DbContext
    {
        public AppDatabaseContext(DbContextOptions<AppDatabaseContext> options) : base(options) // base to call parent class DbContext
        {

        }
        public DbSet<DatabaseModel> FileDetails { get; set; }
    }
}
