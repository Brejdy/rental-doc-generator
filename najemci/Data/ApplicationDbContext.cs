using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using najemci.Models;

namespace najemci.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Console.WriteLine($"Connection String: {this.Database.GetConnectionString()}");
            Console.WriteLine($"Database: {this.Database.GetDbConnection().Database}");
            Console.WriteLine($"Data Source: {this.Database.GetDbConnection().DataSource}");
        }

        public DbSet<Nemovitost> Nemovitosti { get; set; }
        public DbSet<Byt> Byty { get; set; }
        public DbSet<Najemnik> Najemnici { get; set; }
    }
}
