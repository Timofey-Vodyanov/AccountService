using Microsoft.EntityFrameworkCore;
using AccountService.Models;

namespace AccountService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка индексов и отношений (опционально)
        modelBuilder.Entity<Account>()
            .HasIndex(a => a.OwnerId);

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.AccountId);

        // Указываем точность для decimal, чтобы избежать предупреждений в PostgreSQL
        modelBuilder.Entity<Account>()
            .Property(a => a.Balance)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Account>()
            .Property(a => a.InterestRate)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);
    }
}