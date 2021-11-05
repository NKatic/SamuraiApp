using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SamuraiApp.Data;
using SamuraiApp.Domain;

namespace SamuraiApp.Test
{
    [TestClass]
    public class BizDataTests
    {
        [TestMethod]
        public void CanAddSamuraiByName()
        {
            DbContextOptionsBuilder builder = new DbContextOptionsBuilder();
            builder.UseInMemoryDatabase("CanAddSamuraisByName");

            using SamuraiContext context = new SamuraiContext(builder.Options);
            BusinessDataLogic bizLogic = new BusinessDataLogic(context);

            var nameList = new string[] { "Kikuchiyo", "Kyuzo", "Rikchi" };
            var result = bizLogic.AddSamuraisByName(nameList);
            Assert.AreEqual(nameList.Length, result);
        }

        [TestMethod]
        public void CanInsertSingleSamurai()
        {
            DbContextOptionsBuilder builder = new DbContextOptionsBuilder();
            builder.UseInMemoryDatabase("CanAddSamuraisByName");

            using (SamuraiContext context = new SamuraiContext(builder.Options))
            {
                BusinessDataLogic bizLogic = new BusinessDataLogic(context);
                bizLogic.InsertNewSamurai(new Samurai());
            }

            using (SamuraiContext context2 = new SamuraiContext(builder.Options))
            {
                Assert.AreEqual(1, context2.Samurais.Count());
            }
        }

        [TestMethod]
        public void CanInsertSamuraiWithQuotes()
        {
            Samurai samuraiGraph = new Samurai
            {
                Name = "Kyuzo",
                Quotes = new List<Quote>
                {
                    new Quote { Text = "Watch out for my sharp sword!" },
                    new Quote { Text = "I told you to watch out for the sharp sword! Oh well!" }
                }
            };
            var builder = new DbContextOptionsBuilder();
            builder.UseInMemoryDatabase("CanInsertSamuraiWithQuotes");
            using (var context = new SamuraiContext(builder.Options))
            {
                var bizLogic = new BusinessDataLogic(context);
                var result = bizLogic.InsertNewSamurai(samuraiGraph);
            }
            using (var context2 = new SamuraiContext(builder.Options))
            {
                var samuraiWithQuotes = context2.Samurais.Include(s => s.Quotes).FirstOrDefault();
                Assert.AreEqual(samuraiWithQuotes.Quotes.Count, 2);
            }
        }

        [TestMethod, TestCategory("SamuraiWithQuotes")]
        public void CanGetSamuraiWithQuotes()
        {
            int samuraiId;
            var builder = new DbContextOptionsBuilder();
            builder.UseInMemoryDatabase("SamuraiWithQuotes");
            using (var seedContext = new SamuraiContext(builder.Options))
            {
                Samurai samuraiGraph = new Samurai
                {
                    Name = "Kyuzo",
                    Quotes = new List<Quote>
                    {
                    new Quote { Text = "Watch out for my sharp sword!" },
                    new Quote { Text = "I told you to watch out for the sharp sword! Oh well!" }
                    }
                };
                seedContext.Samurais.Add(samuraiGraph);
                seedContext.SaveChanges();
                samuraiId = samuraiGraph.Id;
            }

            using (var getContext = new SamuraiContext(builder.Options))
            {
                var bizLogic = new BusinessDataLogic(getContext);
                var samuraiWithQuotes = bizLogic.GetSamuraiWithQuotes(samuraiId);
                Assert.AreEqual(samuraiWithQuotes.Quotes.Count, 2);
            }
        }
    }
}