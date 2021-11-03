using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErcotAPILib.ErcotNodalService;
using ErcotAPILib.Utils;
using ErcotAPILib.Exceptions;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Web.Services3;
using Microsoft.Web.Services3.Security.Tokens;
using Microsoft.Web.Services3.Security;
using System.Net;
using System.Xml;

namespace ErcotAPILib.MarketInfo
{
    public class MarketInfo : ApplicationBase
    {


        /***********************************************************************************************************
         * Constants
         * *********************************************************************************************************/
        private readonly  string CLIENT_CERT_PATH = Environment.CurrentDirectory + @"\" +
                                                    Properties.Settings.Default.ClientCertificateDirectory + @"\" +
                                                    Properties.Settings.Default.ClientCertificateName;

        private readonly string CLIENT_CERT_PASSWORD = Properties.Settings.Default.ClientCertificatePassword;

        private readonly string CLIENT_CERT_USER_ID = Properties.Settings.Default.ClientCertificateUserID;

        private readonly string SOURCE = Properties.Settings.Default.Source;

       
        private readonly double LMP_REPORT_STARTTIME_OFFSET_MINUTES = Properties.Settings.Default.LMP_REPORT_STARTTIME_OFFSET_MINUTES;
        private readonly double LMP_REPORT_ENDTIME_OFFSET_MINUTES = Properties.Settings.Default.LMP_REPORT_ENDTIME_OFFSET_MINUTES;





        /*********************************************************************************************************
         * Fields
         * ******************************************************************************************************/
        private NodalService _ercotClient;
        private X509Certificate2 _clientCert;
      
        private SoapContext _context;
        private X509SecurityToken _token;
      
        private List<Lmp> _lmpList;

        private enum MarketInfoCallType { LMPs, Report };  // add more market call types in the future as required
        private MarketInfoCallType _marketCallType;



        /// <summary>
        /// Constructor
        /// </summary>
        public MarketInfo() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {

            LogDebug("Initializing Service Proxy");
            this.InitializeServiceProxy();

        }

        /// <summary>
        /// InitializeServiceProxy
        /// </summary>
        private void InitializeServiceProxy()
        {

            // Create NodalService proxy reference
            _ercotClient = new NodalService();

            // Load cert and sign SOAP message
           
            LogDebug(CLIENT_CERT_PATH);

            try
            {
                _clientCert = new X509Certificate2(CLIENT_CERT_PATH, CLIENT_CERT_PASSWORD, X509KeyStorageFlags.MachineKeySet);
            }catch(Exception e)
            {
                LogError(e);
                throw new ErcotCertException("Problem creating X509Certificate. Verify certificates and check certificate path.", e);
            }

            try
            {
                LogDebug("adding _clientCert");
                _ercotClient.ClientCertificates.Add(_clientCert);

                LogDebug("here.");
                _context = _ercotClient.RequestSoapContext;
                LogDebug("here..");
                _token = new X509SecurityToken(_clientCert);

                //These are obsolete but necessary since they use old technology
                LogDebug("here...");
                _context.Security.Tokens.Add(_token);


               // LogDebug("here....");
               _context.Security.Elements.Add(new MessageSignature(_token));


                //Set TLS protocol
                LogDebug("here.....");
                // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
                // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
                // ServicePointManager.SecurityProtocol =  SecurityProtocolType.Tls11;


                //LogDebug("here......");
                // Handle service reply with a callback method
                // _ercotClient.MarketInfoCompleted += this.ErcotClient_MarketInfoCompleted;
            }
            catch (Exception e)
            {
                LogError(e);
            }

            LogDebug("Finished Initializing Service Proxy");

        }


        /// <summary>
        /// ErcotClient_MarketInfoCompleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErcotClient_MarketInfoCompleted(object sender, MarketInfoCompletedEventArgs e)
        {
            LogDebug("MarketInfoAsync call completed...");
            
            try
            {
                LogDebug("Reading MarketInfo ReplyCode...");
                string reply = e.Result.Reply.ReplyCode;
                LogDebug("Ercot Get " + e.Result.Header.Noun + " Response: " + reply);
                LogDebug("Payload Format:" + e.Result.Payload.format);
                LogDebug("Processing MarketInfo response payload...");
               
                //If we get this far, reply must be valid object!
                if (e.Result.Payload.format.Equals("XML"))
                {

                    switch (_marketCallType)
                    {
                        case MarketInfoCallType.LMPs:
                            {
                                ProcessLMPsCallResults(e.Result.Payload);
                                break;
                            }
                        default: break;
                    }


                   
                }

            }
            catch (Exception)
            {

                LogError("Ercot Error Response: " + e.Error);

            }

        }


        /// <summary>
        /// GetRtmLmps
        /// </summary>
        /// <returns></returns>
        public List<Lmp> GetRtmLmps()
        {
            LogDebug("Calling GetRtmLmps() method...");
            _marketCallType = MarketInfoCallType.LMPs;
            try
            {
                RequestMessageBuilder requestBuilder = new RequestMessageBuilder(SOURCE, CLIENT_CERT_USER_ID);
                LogDebug("Source: " +  SOURCE + " Client Cert UserID: " + CLIENT_CERT_USER_ID);

                /*******************************************************
                 StartTime and EndTime must be in Central Standard Time.
                 The conversion will automatically account for Daylight
                 Savings Time.
                *******************************************************/
                RequestMessage requestmsg = requestBuilder.GetRtmLMPs(
                    TimeZoneInfo.ConvertTime(DateTime.Now.AddMinutes(LMP_REPORT_STARTTIME_OFFSET_MINUTES), TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")), 
                    TimeZoneInfo.ConvertTime(DateTime.Now.AddMinutes(LMP_REPORT_ENDTIME_OFFSET_MINUTES), TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")), true, true);

                //This is where the rubber meets the road. Calling the service asynchronously with the help of a Task
                LogDebug("Attempting to call Ercot Get LMPs service...");
                // Task serviceTask = new Task(() => _ercotClient.MarketInfoAsync(requestmsg));
                // serviceTask.Start();
                // LogDebug("Ercot Get LMPs service task started...");
                LogDebug("RequestMessage: " + requestmsg);
                ResponseMessage response = _ercotClient.MarketInfo(requestmsg);
                 LogDebug("Returning response.Payload");
                 return ProcessLMPsCallResults(response.Payload);
                
               

            }
            catch (System.Web.Services.Protocols.SoapHeaderException she)
            {
                LogError(she);
                LogDebug(she);
                throw new Exception($"From {System.Reflection.MethodBase.GetCurrentMethod().ToString()}: SOAP Header Exception...");
            }

            catch (System.Reflection.TargetInvocationException te)
            {
                LogError(te);
                LogDebug(te);
                throw new Exception($"From {System.Reflection.MethodBase.GetCurrentMethod().ToString()}: SOAP Header Exception...");
            }

            catch (Exception ex)
            {
                LogError(ex);
                LogDebug(ex);
                throw new Exception($"From {System.Reflection.MethodBase.GetCurrentMethod().ToString()}: SOAP Header Exception...");
            }

          
           
        } // end GetRtmLmps() method


        public string GetReport(string reportTypeID)
        {

            LogDebug("Calling GetReport() method...");
            _marketCallType = MarketInfoCallType.Report;
            try
            {
                RequestMessageBuilder requestBuilder = new RequestMessageBuilder(SOURCE, CLIENT_CERT_USER_ID);
                RequestMessage requestmsg = requestBuilder.GetReports(null, null, reportTypeID);

             
                LogDebug("Attempting to call Ercot GetReports service...");
              
                ResponseMessage response = _ercotClient.MarketInfo(requestmsg);
                LogDebug("Calling ExtractReportFromResponsePayload() method...");
                Report report = ExtractReportFromResponsePayload(response.Payload);
                LogDebug("Creating ScreenScraper object...");
                ScreenScraper sc = new ScreenScraper();
                LogDebug("Extracting csv report contents from zip file...");
                string csv_report_contents = sc.ExtractCsvFileForReportInMemory(report);
                return csv_report_contents;
              


            }
            catch (System.Web.Services.Protocols.SoapHeaderException she)
            {
                LogError(she);
                LogDebug(she);
                throw new Exception($"From {System.Reflection.MethodBase.GetCurrentMethod().ToString()}: SOAP Header Exception...");
            }

            catch (System.Reflection.TargetInvocationException te)
            {
                LogError(te);
                LogDebug(te);
                throw new Exception($"From {System.Reflection.MethodBase.GetCurrentMethod().ToString()}: SOAP Header Exception...");
            }

            catch (Exception ex)
            {
                LogError(ex);
                LogDebug(ex);
                throw new Exception($"From {System.Reflection.MethodBase.GetCurrentMethod().ToString()}: SOAP Header Exception...");
            }


        }



        /// <summary>
        /// ProcessLMPsCallResults
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private List<Lmp> ProcessLMPsCallResults(PayloadType payload)
        {
            _lmpList = new List<Lmp>();

            XmlElement element = (XmlElement)payload.Items[0];
            XmlNodeList LmpNodes = element.GetElementsByTagName("ns1:LMP");


            foreach (XmlNode xn in LmpNodes)
            {
                Lmp lmp = new Lmp(xn.ChildNodes[0].InnerText, xn.ChildNodes[1].InnerText, xn.ChildNodes[2].InnerText);
                _lmpList.Add(lmp);
            }

            return _lmpList;
        } // end ProcessLMPsCallResults





        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns>string representing the report's download URL</returns>
        private string ExtractReportDownloadUrlFromResponsePayload(PayloadType payload)
        {
            Report report = null;

            XmlElement element = (XmlElement)payload.Items[0];
            XmlNodeList LmpNodes = element.GetElementsByTagName("ns1:Report");

            LogDebug("Extracting report node...");
            foreach (XmlNode xn in LmpNodes)
            {
                 report = new Report(xn);
                 break;
            }
            LogDebug(report.ToString());
            return report.Url;

        }


        private Report ExtractReportFromResponsePayload(PayloadType payload)
        {
            Report report = null;

            XmlElement element = (XmlElement)payload.Items[0];
            XmlNodeList LmpNodes = element.GetElementsByTagName("ns1:Report");

            LogDebug("Extracting report node...");
            foreach (XmlNode xn in LmpNodes)
            {
                report = new Report(xn);
                break;
            }
            LogDebug(report.ToString());
            return report;

        }



      


    }
}
