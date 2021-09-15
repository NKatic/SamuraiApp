using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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


    }
}
