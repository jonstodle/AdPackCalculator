﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdPackCalculator
{
    public class StateObject
    {
        public string AddDate { get; set; }
        public string AddAmount { get; set; }
        public IEnumerable<AdPackInfo> AdPackInfos { get; set; } = Enumerable.Empty<AdPackInfo>();
        public string CalculateDate { get; set; }
        public SettingsObject Settings { get; set; } = new SettingsObject();
    }
}
