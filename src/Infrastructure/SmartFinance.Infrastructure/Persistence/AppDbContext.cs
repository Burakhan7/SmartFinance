using Microsoft.EntityFrameworkCore;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<ConsentRecord> ConsentRecords => Set<ConsentRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.FullName).IsRequired().HasMaxLength(200);
            e.Property(c => c.Email).IsRequired().HasMaxLength(256);
            e.HasIndex(c => c.Email).IsUnique();

            e.HasMany(c => c.Cards)
             .WithOne(card => card.Customer)
             .HasForeignKey(card => card.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(c => c.Offers)
             .WithOne(o => o.Customer)
             .HasForeignKey(o => o.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Card>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.MaskedNumber).IsRequired().HasMaxLength(25);

            e.HasMany(c => c.Transactions)
             .WithOne(t => t.Card)
             .HasForeignKey(t => t.CardId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Transaction>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            e.Property(t => t.Currency).IsRequired().HasMaxLength(3);
            e.Property(t => t.MerchantName).IsRequired().HasMaxLength(200);
            e.HasIndex(t => new { t.CardId, t.OccurredAt });
        });

        modelBuilder.Entity<Offer>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Title).IsRequired().HasMaxLength(200);
            e.Property(o => o.Description).IsRequired().HasMaxLength(1000);
        });

        modelBuilder.Entity<ConsentRecord>(e =>
        {
            e.HasKey(c => c.Id);
            e.Ignore(c => c.IsActive); // hesaplanan property, DB'ye yazılmaz
        });
    }
}