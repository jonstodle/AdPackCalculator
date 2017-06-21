using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdPackCalculator
{
    public class AdPack
    {
        public AdPack() { }

        public AdPack(int duration)
        {
            Tickets = duration;
        }

        public AdPack(AdPackInfo adPackInfo, int duration) : this(duration)
        {
            BuyDate = adPackInfo.BuyDate;
        }

        public DateTimeOffset BuyDate { get; set; }
        public int Tickets { get; set; }

        public void TakeTicket() => Tickets -= 1;
    }
}
