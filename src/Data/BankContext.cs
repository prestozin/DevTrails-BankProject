using DevTrails___BankProject.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DevTrails___BankProject.Data
{
    public class BankContext : IdentityDbContext<User>
    {
        public BankContext(DbContextOptions<BankContext> options) : base(options) 
        {
            
        }
        public DbSet<Client> Clients { get; set; } 
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            //client

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.CPF)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .Property(c => c.Name)
                .IsRequired();

            modelBuilder.Entity<Client>()
                .HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<Client>(c => c.UserId)
                .IsRequired();

            //account

            modelBuilder.Entity<Account>()
                .HasDiscriminator<string>("AccountType") 
                .HasValue<CheckingAccount>("Checking")  
                .HasValue<SavingsAccount>("Savings");

            modelBuilder.Entity<CheckingAccount>()
                .Property(c => c.MonthlyFee)
                .HasPrecision(18, 2); 

            modelBuilder.Entity<SavingsAccount>()
                .Property(s => s.InterestRate)
                .HasPrecision(18, 4);


            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Number)
                .IsUnique();

            modelBuilder.Entity<Account>()
                .Property(a=> a.AccountStatus)
                .IsRequired()
                .HasConversion<string>();

            modelBuilder.Entity<Account>()
                .HasOne(a=> a.Client)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Account>()
                .Property(a => a.RowVersion)
                .IsRowVersion();

            //transaction

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18,2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Type)       
                .IsRequired()
                .HasConversion<string>();

            modelBuilder.Entity<Transaction>()
                 .HasOne(t => t.FromAccount)        
                 .WithMany(a => a.SentTransactions) 
                 .HasForeignKey(t => t.FromAccountId) 
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ToAccount)          
                .WithMany(a => a.ReceivedTransactions) 
                .HasForeignKey(t => t.ToAccountId)   
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
