using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdPackCalculator
{
    public struct AlertInfo
    {
        public AlertInfo(AlertType type, string caption, string message)
        {
            Type = type;
            Caption = caption;
            Message = message;
        }

        public AlertType Type { get; private set; }
        public string Caption { get; private set; }
        public string Message { get; private set; }
    }
}
