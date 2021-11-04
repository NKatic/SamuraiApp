using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SamuraiApp.Data;
using SamuraiApp.Domain;

namespace SamuraiApp.UI
{
    internal class Program
    {
        private static SamuraiContext _context = new SamuraiContext();

        private static void Main(string[] args)
        {
            _context.Database.EnsureCreated();
            //InsertNewSamuraiWithAQuote();
            //InsertNewSamuraiWithManyQuotes();
            //AddQuoteToExistingSamuraiWhileTracked();
            //AddQuoteToExistingSamuraiNotTracked(2);
        }

        private static void AddSamurai(string samuraiName, bool saveChanges = true)
        {
            Samurai samurai = new Samurai { Name = samuraiName };
            _context.Samurais.Add(samurai);
            if (saveChanges)
            {
                _context.SaveChanges();
            }
        }

        private static void AddSamurais(params string[] samuraiNames)
        {
            foreach (string samuraiName in samuraiNames)
            {
                AddSamurai(samuraiName, false);
            }
        }

        private static void ListSamurais(string text)
        {
            List<Samurai> samurais = _context.Samurais.ToList();
            Console.WriteLine($"{text}: Samurai count is {samurais.Count}");

            foreach (Samurai samurai in samurais)
            {
                Console.WriteLine(samurai.Name);
            }
        }

        private static void QueryFilters()
        {
            List<Samurai> samurais = _context.Samurais.Where(s => s.Name == "Sampson").ToList();
            IQueryable<Samurai> s = _context.Samurais.Where(s => EF.Functions.Like(s.Name, "%pson"));
        }

        private static void QueryAggregates()
        {
            string name = "Sampson";
            Samurai samurai = _context.Samurais.FirstOrDefault(s => s.Name == name);
            Samurai samuraiByKey = _context.Samurais.Find(2);
        }

        private static void RetrieveAndUpdateSamurai()
        {
            Samurai samurai = _context.Samurais.First();
            samurai.Name += "San";
            _context.SaveChanges();
        }

        private static void RetrieveAndUpdateMultipleSamurais()
        {
            List<Samurai> samurais = _context.Samurais.Skip(1).Take(4).ToList();
            samurais.ForEach(s => s.Name += "San");
            _context.SaveChanges();
        }

        private static void RetrieveAndDeleteSamurai()
        {
            Samurai samurai = _context.Samurais.Find(2);
            _context.Samurais.Remove(samurai);
            _context.SaveChanges();
        }

        // Warning: disconnected update updates all properties, whether they were edited or not
        private static void QueryAndUpdateBattlesDisconnected()
        {
            List<Battle> disconnectedBattles;
            using (var context1 = new SamuraiContext())
            {
                disconnectedBattles = _context.Battles.ToList();
            }
            disconnectedBattles.ForEach(b =>
            {
                b.StartDate = new DateTime(1570, 1, 1);
                b.EndDate = new DateTime(1570, 12, 1);
            });
            using (var context2 = new SamuraiContext())
            {
                context2.UpdateRange(disconnectedBattles);
                context2.SaveChanges();
            }
        }

        // No Tracking Queries
        private static void AsNoTrackingQuery()
        {
            var samurai = _context.Samurais.AsNoTracking().FirstOrDefault();
        }

        // No Tracking DbContext
        private class SamuraiContextNoTracking : SamuraiContext
        {
            public SamuraiContextNoTracking()
            {
                ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
        }

        // Inserting Related Data
        private static void InsertNewSamuraiWithAQuote()
        {
            Samurai samurai = new Samurai()
            {
                Name = "Kambei Shimada",
                Quotes = new List<Quote>()
                {
                    new Quote {Text = "I've come to save you!"}
                }
            };
            _context.Samurais.Add(samurai);
            _context.SaveChanges();
        }

        // Inserting Related Data
        private static void InsertNewSamuraiWithManyQuotes()
        {
            Samurai samurai = new Samurai
            {
                Name = "Kyuzo",
                Quotes = new List<Quote>
                {
                    new Quote { Text = "Watch out for my sharp word!" },
                    new Quote { Text = "I told you to watch out for the sharp sword! Oh well!" }
                }
            };
            _context.Samurais.Add(samurai);
            _context.SaveChanges();
        }

        // Inserting Related Data
        private static void AddQuoteToExistingSamuraiWhileTracked()
        {
            Samurai samurai = _context.Samurais.FirstOrDefault();
            samurai.Quotes.Add(new Quote
            {
                Text = "I bet you're happy that I've saved you!"
            });
            _context.SaveChanges();
        }

        // Inserting related data
        // **This one is bad because EFCore will run an update command on Samurai, even though we didn't
        // change anything...
        private static void AddQuoteToExistingSamuraiNotTracked(int samuraiId)
        {
            var samurai = _context.Samurais.Find(samuraiId);
            samurai.Quotes.Add(new Quote
            {
                Text = "Now that I saved you, will you feed me dinner?"
            });
            using (SamuraiContext context = new SamuraiContext())
            {
                // **The solution is to use the Attach method (Samurais.Attach(samurai)), that will
                // connect the object but set its state to unmodified. EFCore will see the missing
                // key and missing foreign key in quote and insert that, but it will leave the
                // Samurai alone
                context.Samurais.Update(samurai);
                context.SaveChanges();
            }
        }

        // Inserting related data
        private static void Simpler_AddQuoteToExistingSamuraiNotTracked(int samuraiId)
        {
            var quote = new Quote { Text = "Thanks for dinner!", SamuraiId = samuraiId };
            using SamuraiContext newContext = new SamuraiContext();
            newContext.Quotes.Add(quote);
            newContext.SaveChanges();
        }

        // Eager Loading Related Data Samurais LEFT JOIN Quotes
        private static void EagerLoadSamuraiWithQuotes()
        {
            List<Samurai> samuraiWithQuotes = _context.Samurais.Include(s => s.Quotes).ToList();
            // Default is LEFT JOIN, AsSplitQuery will get samurais first then INNER JOIN them with the quotes
            //List<Samurai> samuraiWithQuotes = _context.Samurais.AsSplitQuery().Include(s => s.Quotes).ToList();
        }

        private static void EagerLoadSamuraiWithQuotesIncludeFilter()
        {
            List<Samurai> filteredInclude = _context.Samurais.Where(s => s.Name.Contains("Sampson"))
                                                             .Include(s => s.Quotes.Where(q => q.Text.Contains("Thanks")))
                                                             .ToList();
        }

        private struct IdAndName
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public IdAndName(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        private static void ProjectSomeProperties()
        {
            var someProperties = _context.Samurais.Select(s => new { s.Id, s.Name }).ToList();
            var idAndNames = _context.Samurais.Select(s => new IdAndName(s.Id, s.Name)).ToList();
        }

        private static void ProjectSamuraisWithQuotes()
        {
            var somePropsWithQuotes = _context.Samurais.Select(s => new { s.Id, s.Name, s.Quotes })
                                                       .ToList();
        }

        private static void ProjectSamuraisWithQuotesFiltered()
        {
            var somePropsWithQuotes = _context.Samurais.Select(s => new { s.Id, s.Name, HappyQuotes = s.Quotes.Where(q => q.Text.Contains("happy")) })
                                                       .ToList();
        }

        // Projecting full entity objects while filtering the related objects that are also returned - 
        private static void ProjectSamuraisWithQuotesReturnSamuraiWithFilledNavigationProperty()
        {
            var somePropsWithQuotes = _context.Samurais.Select(s => new { s, s.Quotes }).ToList();
            // somePropsWithQuotes[0].s.Quotes is filled because EFCore tracks objects and knows how to connect corresponding properties
        }

        // ALL entities recognized by the model will be tracked - EFCore marks the changed object by setting it to Modified status
        private static void ProjectSamuraisWithQuotesEditSamuraiAfterQuery()
        {
            var samuraisAndQuotes = _context.Samurais.Select(s => new { Samurai = s, Happyquotes = s.Quotes.Where(q => q.Text.Contains("happy")) })
                                                     .ToList();
            var firstSamurai = samuraisAndQuotes[0].Samurai.Name += " The Happiest";
        }

        // Loading related data for objects already in memory
        private static void AddNewHorse()
        {
            _context.Set<Horse>().Add(new Horse { SamuraiId = 1, Name = "Mr. Ed" });
            _context.SaveChanges();
        }

        // You can only load from a single object!
        private static void ExplicitLoad()
        {
            var samurai = _context.Samurais.Find(1);
            _context.Entry(samurai).Collection(s => s.Quotes).Load();
            _context.Entry(samurai).Reference(s => s.Horse).Load();
        }

        private static void FilterLoadedDataUsingQueryMethod()
        {
            var samurai = _context.Samurais.Find(1);
            var happyQuotes = _context.Entry(samurai)
                                      .Collection(e => e.Quotes)
                                      .Query()
                                      .Where(s => s.Text.Contains("happy"))
                                      .ToList();
        }

        // Using related data to filter objects

        // Get only samurais but filter over quote
        private static void FilteringWithRelatedData()
        {
            var saumrais = _context.Samurais.Where(s => s.Quotes.Any(q => q.Text.Contains("happy")))
                                            .ToList();
        }

        // Modifying related data

        private static void ModifyingRelatedDataWhenTracked()
        {
            var samurai = _context.Samurais.Include(s => s.Quotes).FirstOrDefault(s => s.Id == 2);
            samurai.Quotes[0].Text = "Did you hear that?";
            _context.SaveChanges();
        }

        // In this example EFCore will create a query to update all quotes, even though we're only updating one!
        // This is because the samurai object has all 4 quotes attached
        private static void ModifyingRelatedDataWhenNotTrackedWrong()
        {
            var samurai = _context.Samurais.Include(s => s.Quotes).FirstOrDefault(s => s.Id == 2);
            var quote = samurai.Quotes[0];
            quote.Text += "Did you hear that again?";

            using var newContext = new SamuraiContext();
            newContext.Quotes.Update(quote);
            newContext.SaveChanges();
        }

        // Use Entry method that focues specifically on the object you pass in the method and it will ignore anything else that may be attached to it
        private static void ModifyingRelatedDataWhenNotTracked()
        {
            var samurai = _context.Samurais.Include(s => s.Quotes)
                                           .FirstOrDefault(s => s.Id == 2);
            var quote = samurai.Quotes[0];
            quote.Text += " Did you hear that again?";

            using var newContext = new SamuraiContext();
            newContext.Entry(quote).State = EntityState.Modified;
            newContext.SaveChanges();
        }

        // Removes an entity from a many to many relationship (takes a samurai out of a battle)
        private static void RemoveSamuraiFromABattle()
        {
            // Get a battle with attached samurai we want to remove. It's important to have the navigation graph filled.
            Battle battleWithSamurai = _context.Battles.Include(e => e.Samurais.Where(s => s.Id == 12))
                                                    .Single(e => e.BattleId == 1);
            Samurai samurai = battleWithSamurai.Samurais[0];

            // Remove the samurai from the battle's navigation property
            battleWithSamurai.Samurais.Remove(samurai);
            _context.SaveChanges();

            // There's no mention of any join tables or simmilar, it's enough just to remove one entity from the navigation property of the other one.
        }
        // Example of a failed attempt at removing an entity from a many to many relationship.
        // We never create a link via the navigation properties so the context is not aware of the relationship between the samurai and the battle.
        private static void FailedRemoveSamuraiFromABattle()
        {
            Battle battle = _context.Battles.First();
            Samurai samurai = _context.Samurais.Find(12);
            battle.Samurais.Remove(samurai);
            _context.SaveChanges(); // the relationship is not being tracked
        }

        private static void RemoveSamuraiFromABattleExplicit()
        {
            var b_s = _context.Set<BattleSamurai>()
                              .SingleOrDefault(bs => bs.BattleId == 1 && bs.SamuraiId == 10);
            if(b_s != null)
            {
                _context.Remove(b_s);
                _context.SaveChanges();
            }
        }

        /// Persisting data in One-to-One relationships
        // Db behavior is the same as when adding a samurai with a quote (one-to-many): EfCore sends an insert statement to insert a samurai 
        // with a query to return the database generated id. EfCore then takes that id, integrates it into an insert statement to insert a new horse.
        private static void AddNewSamuraiWithHorse()
        {
            var samurai = new Samurai { Name = "Jina Ujichika" };
            samurai.Horse = new Horse { Name = "Silver" };
            _context.Samurais.Add(samurai);
            _context.SaveChanges();
        }

        // Add a new horse to a samurai that exists but doesn't already have a horse, using samurai id (without samurai object)
        private static void AddNewHorseToSamuraiUsingId()
        {
            var horse = new Horse { Name = "Scout", SamuraiId = 2 };
            _context.Add(horse);
            _context.SaveChanges();
        }

        // Add a new horse to a samurai that exists but doesn't already have a horse, using a samurai object.
        private static void AddNewHorseToSamuraiObject()
        {
            var samurai = _context.Samurais.Find(12);
            samurai.Horse = new Horse { Name = "Black Beauty" };
            _context.SaveChanges();
        }

        // Add a new horse to samurai detached
        private static void AddNewHorseToDisconnectedSamuraiObject()
        {
            var samurai = _context.Samurais.AsNoTracking().FirstOrDefault(s => s.Id == 5);
            samurai.Horse = new Horse { Name = "Mr. Ed" };

            using var newContext = new SamuraiContext();
            newContext.Samurais.Attach(samurai);
            newContext.SaveChanges();
        }

        /// Changing the child of an existing parent
        // EfCore will delete the old horse from the database before inserting a new horse
        private static void ReplaceAHorse()
        {
            var samurai = _context.Samurais.Include(e => e.Horse).FirstOrDefault(s => s.Id == 5);
            samurai.Horse = new Horse { Name = "Trigger" };
            _context.SaveChanges();
        }

        /// Querying a one-to-one relationship
        // Gets a samurai with his horse. Straightforward
        private static void GetSamuraiWithHorse()
        {
            var samurais = _context.Samurais.Include(s => s.Horse).ToList();
        }

        // Gets a horse with a samurai. We don't have a dbSet for horses in the context! One solution is to use _context.Set<> and then some creative linq to get a samurai.
        // But a better solution is to start with a samurai and drill down to the horse property
        private static void GetHorsesWithSamurai()
        {
            Horse horseOnly = _context.Set<Horse>().Find(3);

            Samurai horseWithSamurai = _context.Samurais.Include(s => s.Horse)
                                                    .FirstOrDefault(s => s.Horse.Id == 3);

            // We can also select a horse-samurai pair
            var horseSamuraiPairs = _context.Samurais.Where(s => s.Horse != null)
                                                     .Select(s => new { Horse = s.Horse, Samurai = s })
                                                     .ToList();
        }

        /// Module 7: Working with Views and Stored Procedures and Raw SQL
        
    }
}