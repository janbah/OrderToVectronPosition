using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Order2VPos.Core.Common;
using System.IO;

namespace Order2VPos.Core.Models
{
    public class CoreDbContext : DbContext
    {
        public CoreDbContext()
        {
        }

        public static CoreDbContext GetContext()
        {
            CoreDbContext dbContext = new CoreDbContext();
            dbContext.Database.Migrate();
            return dbContext;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source = {Path.Combine(AppSettings.AppDataFolder, "CommonData.db")};");
        }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Category> Categories { get; set; }

    }
}
