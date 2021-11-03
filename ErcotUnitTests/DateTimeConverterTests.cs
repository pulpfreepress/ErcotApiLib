/******************************************************************************************
* Filename:      DateTimeConverterTests.cs
* Author:        Rick Miller
* Date:          28 April 2018
* Description:   Tests ERCOT DateTime conversion utilities.
* Change Log:
* Date            Coder             Comments
* -----------------------------------------------------------------------------------------
* 28Apr18         Rick Miller       Conceived and brought into this world!
*
*******************************************************************************************/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ErcotAPILib.Utils;

namespace ErcotUnitTests
{
    [TestClass]
    public class DateTimeConverterTests
    {
        [TestMethod]
        public void DateTimeUTCFromStringTest()
        {
            DateTime ComparisonDateTime_280418_050505_UTC = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2018, 04, 28, 5, 5, 5));
            string ercotReportRuntime = "04/28/2018 05:05:05";

            DateTime convertedDate = DateTimeConverter.DateTimeUTCFromString(ercotReportRuntime);
            Assert.AreEqual(ComparisonDateTime_280418_050505_UTC, convertedDate);
        }



        [TestMethod]
        public void BadReportStringFormatTest()
        {
            Assert.ThrowsException<ArgumentException>(new Action(() => RunBadFormatTest()), "Properly formatted string!"); 
        }


        private void RunBadFormatTest()
        {
            string ercotReportRuntime = "04-28-2018 05:05:05"; //should be 04/28/2018
            DateTimeConverter.DateTimeUTCFromString(ercotReportRuntime); //should throw exception

        }
    }
}
