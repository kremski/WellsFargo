using Microsoft.EntityFrameworkCore;
using WellsFargo.DAL.Model;

namespace WellsFargo.DAL
{
    public class WellsFargoDbContext : DbContext
    {
        private DbContextOptions<WellsFargoDbContext> options;
        public WellsFargoDbContext(DbContextOptions<WellsFargoDbContext> options) : base(options) { }

        public DbSet<Oms> Oms { get; set; }
        public DbSet<Portfolio> Portfolio { get; set; }
        public DbSet<Security> Security { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<TransactionType> TransactionType { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Oms>()
                .HasKey(x => x.OmsId);
            modelBuilder.Entity<Portfolio>()
                .HasKey(x => x.PortfolioId);
            modelBuilder.Entity<Security>()
                .HasKey(x => x.SecurityId);
            modelBuilder.Entity<TransactionType>()
                .HasKey(x => x.TransactionTypeId);
            modelBuilder.Entity<Transaction>()
                .HasKey(x => x.TransactionId);

            modelBuilder.Entity<Transaction>()
                .HasOne(e => e.Oms)
                .WithMany(e => e.Transactions)
                .HasForeignKey(e => e.OmsId)
                .IsRequired();
            modelBuilder.Entity<Transaction>()
                .Property(b => b.Nominal).HasColumnType("decimal");
            modelBuilder.Entity<Transaction>()
                .HasOne(e => e.Portfolio)
                .WithMany(e => e.Transactions)
                .HasForeignKey(e => e.PortfolioId)
                .IsRequired();
            modelBuilder.Entity<Transaction>()
                .HasOne(e => e.TransactionType)
                .WithMany(e => e.Transactions)
                .HasForeignKey(e => e.TransactionTypeId)
                .IsRequired();
            modelBuilder.Entity<Transaction>()
                .HasIndex(b => b.OmsId);


            modelBuilder.Entity<Oms>()
                .HasData(
                    new Oms { OmsId = 1, OmsCode = "AAA" },
                    new Oms { OmsId = 2, OmsCode = "BBB" },
                    new Oms { OmsId = 3, OmsCode = "CCC" }
                    );
            modelBuilder.Entity<TransactionType>()
                .HasData(
                    new TransactionType { TransactionTypeId = 1, TransactionTypeCode = "BUY" },
                    new TransactionType { TransactionTypeId = 2, TransactionTypeCode = "SELL" }
                    );
        }
    }
}
