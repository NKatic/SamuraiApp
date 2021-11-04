using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamuraiApp.Domain
{
    /// <summary>
    /// Using keyless entities to map to views
    /// </summary>
    public class SamuraiBattleStat
    {
        public string Name { get; set; }
        public int? NumberOfBattles { get; set; }
        public string EarliestBattle { get; set; }
    }
}
