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
    }
}