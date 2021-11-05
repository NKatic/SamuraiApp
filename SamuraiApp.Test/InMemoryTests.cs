using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SamuraiApp.Data;
using SamuraiApp.Domain;

namespace SamuraiApp.Test
{
    [TestClass]
    public class InMemoryTests
    {
        [TestMethod]
        public void CanInsertSamuraiIntoDatabase()
        {
            DbContextOptionsBuilder builder = new();
            builder.UseInMemoryDatabase("CanInsertSamuraiIntoDatabase");
            
            using SamuraiContext context = new(builder.Options);
            Samurai samurai = new();
            context.Samurais.Add(samurai);
            Assert.AreEqual(EntityState.Added, context.Entry(samurai).State);
        }
    }
}