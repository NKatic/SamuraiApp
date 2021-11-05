using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SamuraiApp.Domain;

namespace SamuraiApp.Data
{
    public class BusinessDataLogic
    {
        private SamuraiContext _context;
        public BusinessDataLogic(SamuraiContext context)
        {
            _context = context;
        }
        public BusinessDataLogic()
        {
            _context = new SamuraiContext();
        }

        public int AddSamuraisByName(params string[] names)
        {
            foreach (string name in names)
            {
                _context.Samurais.Add(new Domain.Samurai { Name = name });
            }
            var dbResult = _context.SaveChanges();
            return dbResult;
        }

        public int InsertNewSamurai(Samurai samurai)
        {
            _context.Samurais.Add(samurai);
            var dbResult = _context.SaveChanges();
            return dbResult;
        }

        public Samurai GetSamuraiWithQuotes(int samuraiId)
        {
            var samuraiWithQuotes = _context.Samurais.Where(s => s.Id == samuraiId).Include(s => s.Quotes).FirstOrDefault();
            return samuraiWithQuotes;
        }
    }
}
