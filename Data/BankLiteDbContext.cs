using BankLite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BankLite.Api.Data
{
    public class BankLiteDbContext : DbContext
    {
        public BankLiteDbContext(DbContextOptions<BankLiteDbContext> opt) : base(opt) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transfer> Transfers => Set<Transfer>();
        public DbSet<TermDeposit> TermDeposits => Set<TermDeposit>();
        public DbSet<Receipt> Receipts => Set<Receipt>();
        public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
        public DbSet<Alert> Alerts => Set<Alert>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<User>()
                .HasIndex(x => x.Username)
                .IsUnique();

            mb.Entity<Account>()
                .HasIndex(x => x.Iban)
                .IsUnique();

            mb.Entity<Account>()
                .HasOne(a => a.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.UserId);

            mb.Entity<Account>()
                .Property(a => a.Balance)
                .HasPrecision(18, 2);

            mb.Entity<Transfer>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            mb.Entity<TermDeposit>()
                .Property(td => td.Principal)
                .HasPrecision(18, 2);

            mb.Entity<TermDeposit>()
                .Property(td => td.Rate)
                .HasPrecision(5, 2);

            mb.Entity<TermDeposit>()
                .Property(td => td.PayoutAmount)
                .HasPrecision(18, 2);

            mb.Entity<Beneficiary>()
                .HasOne(b => b.User)
                .WithMany(u => u.Beneficiaries)
                .HasForeignKey(b => b.UserId);

            mb.Entity<Alert>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId);

            mb.Entity<Transfer>()
                .HasOne(t => t.FromAccount)
                .WithMany()
                .HasForeignKey(t => t.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            mb.Entity<Receipt>()
                .HasOne(r => r.Transfer)
                .WithOne(t => t.Receipt)
                .HasForeignKey<Receipt>(r => r.TransferId);

            base.OnModelCreating(mb);
        }
    }
}
