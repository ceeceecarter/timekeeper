using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NM.Web.WebApplication.Timesheets.United;
using NM.Web.WebApplication.Timesheets.United.BusinessModel;
using NM.Web.WebApplication.Timesheets.United.Concrete;

namespace UnitedTestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            List<TimeDateRange> tdr = new List<TimeDateRange>();
            DateTime dateStart = DateTime.Parse("2/7/2016");
            DateTime dateEnd = DateTime.Parse("2/13/2016");

            var test = new Repository();
            var result = test.GetTimeDateRange(dateStart, dateEnd);

            foreach (var item in result)
            {
                Console.WriteLine(item.dtmDate);
            }
        }
    }
}
