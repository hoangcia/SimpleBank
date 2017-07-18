using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using SimpleBank.DataService.Entity;

namespace SimpleBank.DataService.Data
{
    public class SimpleBankContext: DbContext
    {                        
        public SimpleBankContext(DbContextOptions<SimpleBankContext> options) : base(options)
        {

        }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Account>()
                .Property(p => p.RowVersion)
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();
            modelBuilder.Entity<Account>().ToTable("Account");

            modelBuilder.Entity<Account>().HasIndex(p => p.Email).IsUnique();
            modelBuilder.Entity<Account>().HasIndex(p => p.Number).IsUnique();
            
            modelBuilder.Entity<Transaction>().ToTable("Transaction");

            base.OnModelCreating(modelBuilder);
        }        
    }

    public class SimpleBankDbContextFactory : IDbContextFactory<SimpleBankContext>
    {
        public SimpleBankContext Create()
        {
            var environmentName = Environment.GetEnvironmentVariable("Hosting:Environment");

            var basePath = AppContext.BaseDirectory;

            return Create(basePath, environmentName);
        }

        public SimpleBankContext Create(DbContextFactoryOptions options)
        {
            return Create(options.ContentRootPath, options.EnvironmentName);
        }

        private SimpleBankContext Create(string basePath, string environmentName)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json", true)
            .AddEnvironmentVariables();

            var config = builder.Build();

            var connstr = config.GetConnectionString("DefaultConnection");

            if (String.IsNullOrWhiteSpace(connstr) == true)
            {
                throw new InvalidOperationException(
                "Could not find a connection string named 'DefaultConnection'.");
            }
            else
            {
                return Create(connstr);
            }
        }
        private SimpleBankContext Create(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException(
                $"{nameof(connectionString)} is null or empty.",
                nameof(connectionString));

            var optionsBuilder =
             new DbContextOptionsBuilder<SimpleBankContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return new SimpleBankContext(optionsBuilder.Options);
        }

    }
}
