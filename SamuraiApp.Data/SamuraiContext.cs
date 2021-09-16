using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamuraiApp.Domain;

namespace SamuraiApp.Data
{
    public class SamuraiContext : DbContext
    {
        public DbSet<Samurai> Samurais { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Battle> Battles { get; set;  }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string someConnectionString = "Data Source=.; Initial Catalog = SamuraiAppData; Trusted_Connection=true; Integrated Security=true";
            optionsBuilder.UseSqlServer(someConnectionString, options => options.MaxBatchSize(100))
                          .LogTo(Console.WriteLine, LogLevel.Information)
                          //.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name });
                          //.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
                          .EnableSensitiveDataLogging();

            // if you want to log to a file:
            //StreamWriter _writer = new StreamWriter("EFCoreLog.txt", append: true);
            //optionsBuilder.LogTo(_writer.WriteLine);

            // if you want to log to a debug window:
            //optionsBuilder.LogTo(log => Debug.WriteLog(log));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Entity Samurai...
            modelBuilder.Entity<Samurai>()
                  // ...has many Battles...
                 .HasMany(s => s.Battles)
                 // ... with many samurais.
                 .WithMany(b => b.Samurais)
                 // Sada EFCore zna da pričamo o vezi Samurai-Battle. Dalje mu treba reći koju tablicu da koristi za tu many-to-many vezu, inače će sam pretpostaviti join tablicu.
                 // Osim definiranja same tablice, potrebno je definirati vezu između BattleSamurai tablice i dvije tablice koje ona povezuje. To radimo s HasOne/WithMany, zato jer su
                 // veze Battle-BattleSamurai i Samurai-BattleSamurai one-to-many.
                 .UsingEntity<BattleSamurai>(
                bs => bs.HasOne<Battle>().WithMany(),
                bs => bs.HasOne<Samurai>().WithMany())
                 // Postavljamo default vrijednost DateJoined propertyja (T-SQL metoda getdate).
                 .Property(bs => bs.DateJoined).HasDefaultValueSql("getdate()");

            // Ako želimo zadržati stare nazive stupaca u BattleSamurai tablici, koje smo imali prije nego smo dodali eksplicitnu many to many tablicu,
            // moramo definirati koji property se mapira na koji naziv stupca
            //modelBuilder.Entity<BattleSamurai>()
            //    .Property(bs => bs.SamuraiId).HasColumnName("SamuraisId");
            //modelBuilder.Entity<BattleSamurai>()
            //    .Property(bs => bs.BattleId).HasColumnName("BattlesBattleId");
        }
    }
}