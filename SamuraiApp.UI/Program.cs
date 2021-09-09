using System;
using System.Collections.Generic;
using System.Linq;
using SamuraiApp.Data;
using SamuraiApp.Domain;

namespace SamuraiApp.UI
{
    class Program
    {
        private static SamuraiContext _context = new SamuraiContext();

        static void Main(string[] args)
        {
            _context.Database.EnsureCreated();
            ListSamurais("Before add");
            AddSamurai();
            ListSamurais("After add");
        }

        private static void AddSamurai()
        {
            Samurai samurai = new Samurai { Name = "Sampson" };
            _context.Samurais.Add(samurai);
            _context.SaveChanges();
        }

        private static void ListSamurais(string text)
        {
            System.Collections.Generic.List<Samurai> samurais = _context.Samurais.ToList();
            Console.WriteLine($"{text}: Samurai count is {samurais.Count}");

            foreach (Samurai samurai in samurais)
            {
                Console.WriteLine(samurai.Name);
            }
        }
    }
}
