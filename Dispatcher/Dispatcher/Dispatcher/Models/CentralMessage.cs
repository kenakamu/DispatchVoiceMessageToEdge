using System;
using System.Collections.Generic;
using System.Text;

namespace Dispatcher.Models
{
    public class CentralMessage
    {
        public List<string> Locations { get; set; }
        public string Message { get; set; }
    }
}
