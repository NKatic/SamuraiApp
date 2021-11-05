using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamuraiApp.Domain;

namespace SamuraiApp.Data
{
    public class SamuraiContext : DbContext
    {
        public SamuraiContext(DbContextOptions<SamuraiContext> options) : base(options)
        {

        }

        public DbSet<Samurai> Samurais { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Battle> Battles { get; set;  }
        public DbSet<SamuraiBattleStat> SamuraiBattleStats { get; set; }

        // !! Enable for Console app, disable for ASP.NET app !!

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    string someConnectionString = "Data Source=.; Initial Catalog = SamuraiAppData; Trusted_Connection=true; Integrated Security=true";
        //    optionsBuilder.UseSqlServer(someConnectionString, options => options.MaxBatchSize(100))
        //                  .LogTo(Console.WriteLine, LogLevel.Information)
        //                  //.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name });
        //                  //.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);
        //                  .EnableSensitiveDataLogging();

        //    // if you want to log to a file:
        //    //StreamWriter _writer = new StreamWriter("EFCoreLog.txt", append: true);
        //    //optionsBuilder.LogTo(_writer.WriteLine);

        //    // if you want to log to a debug window:
        //    //optionsBuilder.LogTo(log => Debug.WriteLog(log));
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Entity Samurai...
            modelBuilder.Entity<Samurai>()
                  // ...has many Battles...
                 .HasMany(s => s.Battles)
                 // ... with many samurais.
                 .WithMany(b => b.Samurais)
                 // Sada EFCore zna da pričamo o vezi Samurai-Battle. Dalje mu treba reći koju tablicu da koristi za tu many-to-many vezu, inače će sam pretpostaviti join tablicu.
                 // Osim definiranja same tablice, potrebno je definirati vezu između BattleSamurai join tablice i dvije tablice koje ona povezuje. To radimo s HasOne/WithMany, zato jer su
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

            // Ako želimo imati naziv klase drugačiji od naziva tablice u bazi, moramo definirati koja klasa se mapira u koju tablicu
            // modelBuilder.Entity<XYZBattleSamurai>().ToTable("BattleSamurai");

            // Trenutno se tablica za konje u bazi zove Horse, zato jer nemamo dbSet u kontekstu za klasu Horse i zato EfCore koristi konvenciju i kreira tablicu koja se zove jednako kao i klasa. To nam ne odgovara
            // jer želimo da se tablice zovu u množini pa koristimo ToTable metodu
            modelBuilder.Entity<Horse>().ToTable("Horses");

            // If we want to use a keyless entity to map to a view, we need to explicitly tell EFCore that the class is keyless.
            // !! EFCore will NEVER track entities marked with HasNoKey() !!
            modelBuilder.Entity<SamuraiBattleStat>().HasNoKey();
            // Also, we need to tell EFCore that the DbSet maps to a view, otherwise it will try to create a new table.
            modelBuilder.Entity<SamuraiBattleStat>().ToView("SamuraiBattleStats");
        }
    }
}