/******************************************************************************************
* Filename:      RequestMessageBuilder.cs
* Author:        Rick Miller
* Date:          November 2017
* Description:   Utility class to build Ercot SOAP message headers
* Change Log:
* Date            Coder             Comments
* -----------------------------------------------------------------------------------------
* 01Nov18       Rick Miller       Created.
* 29Apr18       Rick Miller       Reviewed, added comments.
*
*******************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErcotAPILib.ErcotNodalService;


namespace ErcotAPILib.Utils
{
    /// <summary>
    /// Utility class used to build ERCOT SOAP request message headers.
    /// </summary>
    public class RequestMessageBuilder : ApplicationBase
    {
        /**************************************
         * Constants
         * ************************************/
        private const string SYSTEMSTATUS = "SystemStatus";
        private const string LMPS = "LMPs";
        private const string REPORTS = "Reports";


        /**************************************
         * Properties
         * ***********************************/
         public string UserID { get; set; }
         public string Source { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RequestMessageBuilder():this(Properties.Settings.Default.Source, Properties.Settings.Default.ClientCertificateUserID)
        {
            
        }

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="source">The Client Certificate Name</param>
        /// <param name="userID">The Client Certificate User ID </param>
        public RequestMessageBuilder(string source, string userID):base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {
            this.Source = source;
            this.UserID = userID;
            LogInfo("RequestMessageBuilder instance created...");
        }

        /// <summary>
        /// Builds the basic RequestMessage structure with populated ReplayDetection elements. 
        /// This method can be called outright and used internal to this class by specific market
        /// request builder methods.
        /// </summary>
        /// <returns></returns>
        public RequestMessage NewRequestMessageStructure(string source, string userID )
        {
            RequestMessage requestmsg = new RequestMessage();
            requestmsg.Header = new HeaderType();
            requestmsg.Header.ReplayDetection = new ReplayDetectionType();
            requestmsg.Header.ReplayDetection.Nonce = new EncodedString();
            requestmsg.Header.ReplayDetection.Nonce.Value = Guid.NewGuid().ToString("N");
            requestmsg.Header.ReplayDetection.Created = new AttributedDateTime();
            requestmsg.Header.ReplayDetection.Created.Value = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            requestmsg.Header.MessageID = ((int)((new Random()).NextDouble() * 1000000)).ToString();
            requestmsg.Header.Source = source;
            requestmsg.Header.UserID = userID;
            requestmsg.Request = new RequestType();
            requestmsg.Request.MarketType = new RequestTypeMarketType();
            requestmsg.Payload = new PayloadType();

            return requestmsg;
        } // end ()



        public RequestMessage GetSystemStatusRequestMessage()
        {
            RequestMessage requestmsg = this.NewRequestMessageStructure(this.Source, this.UserID);
            requestmsg.Header.Verb = HeaderTypeVerb.get;
            requestmsg.Header.Noun = SYSTEMSTATUS;

            return requestmsg;
        } // end ()



        /// <summary>
        /// Builds the RequestMessage object for the get LMPs for Realtime Markets (RTM)
        /// </summary>
        /// <param name="startTime">Report start time</param>
        /// <param name="endTime">Report end time</param>
        /// <returns>Populated MarketInfo.RequestMessage object</returns>

        public RequestMessage GetRtmLMPs(DateTime startTime, DateTime endTime, bool startTimeSpecified = true, bool endTimeSpecified = true)
        {
            if ((startTime == null) || (endTime == null)) throw new ArgumentException("Both startTime and endTime cannot be null");
            RequestMessage requestmsg = this.NewRequestMessageStructure(this.Source, this.UserID);
            requestmsg.Header.Verb = HeaderTypeVerb.get;
            requestmsg.Header.Noun = LMPS;
            if(startTimeSpecified) requestmsg.Request.StartTime = startTime;
            
            requestmsg.Request.StartTimeSpecified = startTimeSpecified;
            if(endTimeSpecified) requestmsg.Request.EndTime = endTime;

            requestmsg.Request.EndTimeSpecified = endTimeSpecified;
            requestmsg.Request.MarketType = RequestTypeMarketType.RTM;
            requestmsg.Request.MarketTypeSpecified = true;
            return requestmsg;
        } // end ()


        /// <summary>
        /// Builds the RequestMessage object for get Reports.
        /// Usage: 
        ///    -- reportID + (startTime & endTime) == All reports for reportID and time duration
        ///    -- reportID + startTime == All reports for reportID from startTime till now
        ///    -- reportID + endTime == All reports for reportID till endTime for the request
        ///    -- reportID only = - All reports for reportID
        /// </summary>
        /// <param name="startTime"> Optional - Can be null</param>
        /// <param name="endTime"> Optional - Can be null</param>
        /// <param name="reportID"> Optional - Can be null</param>
        /// <returns>Populated MarketInfo.RequestMessage object</returns>
        public RequestMessage GetReports(DateTime? startTime, DateTime? endTime, string reportID)
        {
            RequestMessage requestmsg = this.NewRequestMessageStructure(this.Source, this.UserID);
            requestmsg.Header.Verb = HeaderTypeVerb.get;
            requestmsg.Header.Noun = REPORTS;
            requestmsg.Request.StartTimeSpecified = false;
            requestmsg.Request.EndTimeSpecified = false;
            if (startTime != null)
            {
                requestmsg.Request.StartTime = (DateTime)startTime;
                requestmsg.Request.StartTimeSpecified = true;
            }
            if (endTime != null)
            {
                requestmsg.Request.EndTime = (DateTime)endTime;
                requestmsg.Request.EndTimeSpecified = true;
            }
            requestmsg.Request.Option = reportID;


            return requestmsg;
        } // end ()

    } // end class
}
