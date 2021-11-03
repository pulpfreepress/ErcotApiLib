using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErcotAPILib.MarketInfo
{
    /// <summary>
    /// Location Marginal Price (LMP) 
    /// </summary>
    public class Lmp
    {

        public string ReportRunTime { get; set; }
        public string Price { get; set;  }
        public string Bus { get; set;  }


        public Lmp(string reportRunTime, string price, string bus)
        {
            ReportRunTime = reportRunTime;
            Price = price;
            Bus = bus;
          
        }


        public override string ToString()
        {
            return ReportRunTime + "\t\t" + Price + "\t\t" + Bus;
        }


    }
}
