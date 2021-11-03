/******************************************************************************************
* Filename:      DateTimeConverter
* Author:        Rick Miller
* Date:          28 April 2018
* Description:   Contains utility methods to covert ERCOT unique report runtime strings to 
*                DateTime objects.
* Change Log:
* Date            Coder             Comments
* -----------------------------------------------------------------------------------------
* 28Apr18         Rick Miller       Conceived and brought into this world!
*
*******************************************************************************************/


using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErcotAPILib.Utils
{
    public static class DateTimeConverter
    {
        

        private const string ERCOT_LMP_SCREENSCRAPER_REPORT_RUNTIME_FORMAT =
                "\\b(?<month>\\d{1,2})/(?<day>\\d{1,2})/(?<year>\\d{2,4}) (?<hour>\\d{1,2}):(?<minute>\\d{1,2}):(?<second>\\d{1,2})\\b";
        private const string ERCOT_SOAP_SCREENSCRAPER_REPORT_RUNTIME_FORMAT =
             "\\b(?<year>\\d{2,4})-(?<month>\\d{1,2})-(?<day>\\d{1,2})T(?<hour>\\d{1,2}):(?<minute>\\d{1,2}):(?<second>\\d{1,2})-(?<timezonehour>\\d{1,2}):(?<timezoneminute>\\d{1,2})\\b";

        /// <summary>
        /// Converts an ERCOT report runtime string to a DateTime object. 
        /// 
        /// </summary>
        /// <param name="reportRuntimeString">Takes the format: "mm/dd/yyyy hh:mm:ss"</param>
        /// <returns>DateTime object whose value represents the report runtime in Coordinated Universal Time (UTC)</returns>
        public static DateTime DateTimeUTCFromString(string reportRuntimeString)
        {
            if (!Regex.IsMatch(reportRuntimeString, ERCOT_LMP_SCREENSCRAPER_REPORT_RUNTIME_FORMAT)){
                throw new ArgumentException("Input report runtime string must match the following regex pattern: {0}",
                                             ERCOT_LMP_SCREENSCRAPER_REPORT_RUNTIME_FORMAT);
            }
            DateTime returnVal = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
            try
            {
               
                returnVal = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(reportRuntimeString));
            }
            catch (Exception e)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, e.ToString());
            }

            return returnVal;

        }


        public static DateTime DateTimeUTCFromSoapReportRuntimeString(string soapReportRuntimeString)
        {
            if (!Regex.IsMatch(soapReportRuntimeString, ERCOT_SOAP_SCREENSCRAPER_REPORT_RUNTIME_FORMAT))
            {
                throw new ArgumentException("Input report runtime string must match the following regex pattern: {0}",
                                             ERCOT_SOAP_SCREENSCRAPER_REPORT_RUNTIME_FORMAT);
            }
            DateTime returnVal = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
            try
            {
               
                returnVal = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(soapReportRuntimeString));
            }
            catch (Exception e)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, e.ToString());
            }

            return returnVal;

        }


        public static TimeSpan NextReportTimeSpan(DateTime lastReportTime, int reportFrequencyMinutes)
        {
            return new TimeSpan( lastReportTime.ToLocalTime().AddMinutes(reportFrequencyMinutes).Ticks - lastReportTime.ToLocalTime().Ticks).Duration();
        }




    }
}
