using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdPackCalculator
{
    public class AdPack
    {
        public DateTimeOffset BuyDate { get; set; }
        public int Tickets { get; set; } = 120;

        public void TakeTicket() => Tickets -= 1;
    }
}
