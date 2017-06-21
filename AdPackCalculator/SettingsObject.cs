using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdPackCalculator
{
    public class SettingsObject
    {
        public double AdPackCost { get; set; } = 50;
        public int AdPackDuration { get; set; } = 120;
        public double AdPackIncomePerDay { get; set; } = .5;
        public double ReservePercentage { get; set; } = 5;
    }
}
