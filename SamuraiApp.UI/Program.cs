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
            AddSamurai("Marko");
            ListSamurais("After add");
        }

        private static void AddSamurais(params string[] samuraiNames)
        {
            foreach (string samuraiName in samuraiNames)
            {
                AddSamurai(samuraiName, false);
            }
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

        private static void ListSamurais(string text)
        {
            List<Samurai> samurais = _context.Samurais.ToList();
            Console.WriteLine($"{text}: Samurai count is {samurais.Count}");

            foreach (Samurai samurai in samurais)
            {
                Console.WriteLine(samurai.Name);
            }
        }
    }
}
