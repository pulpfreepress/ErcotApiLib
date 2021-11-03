/*****************************************************************************************
 * Class: ScreenScraper
 * Date:  17 February 2018
 * Author: Rick Miller
 * 
 * Change Log:
 * Date          Coder              Comments
 * ---------------------------------------------------------------------------------------
 * 17Feb18      Rick Miller         Initial version
 * 29Apr18      Rick Miller         Improved performance ExtratCsvFileForReport() methods 
 *                                  by doing in-memory processing with MemoryStreams vs.
 *                                  FileStreams. 
 * 
 * 
 * 
 * Description:
 * ERCOT LMP Report Screen Scrape Sequence

1: http://mis.ercot.com/misapp/servlets/IceMktDocListWS?reportTypeId=12300
   
   This returns an XML document with a list of LMP reports:

   <ns0:ListDocsByRptTypeRes xmlns:ns0="http://www.ercot.com/schema/2012-10/nodal/ListDocsByRptTypeRes">
    <ns0:DocumentList>
        <ns0:Document>
            <ns0:ExpiredDate>2018-02-15T23:59:59-06:00</ns0:ExpiredDate>
            <ns0:ILMStatus>EXT</ns0:ILMStatus>
            <ns0:SecurityStatus>P</ns0:SecurityStatus>
            <ns0:ContentSize>3335</ns0:ContentSize>
            <ns0:Extension>zip</ns0:Extension>
            <ns0:FileName/>
            <ns0:ReportTypeID>12300</ns0:ReportTypeID>
            <ns0:Prefix>cdr</ns0:Prefix>
            <ns0:FriendlyName>LMPSROSNODENP6788_20180210_070013_csv</ns0:FriendlyName>
            <ns0:ConstructedName>cdr.00012300.0000000000000000.20180210.070017176.LMPSROSNODENP6788_20180210_070013_csv.zip</ns0:ConstructedName>
            <ns0:DocID>598257277</ns0:DocID>
            <ns0:PublishDate>2018-02-10T07:00:16-06:00</ns0:PublishDate>
            <ns0:ReportName>LMPs by Resource Nodes, Load Zones and Trading Hubs</ns0:ReportName>
            <ns0:DUNS>0000000000000000</ns0:DUNS>
            <ns0:DocCount>0</ns0:DocCount>
        </ns0:Document>
        <ns0:Document>
            <ns0:ExpiredDate>2018-02-15T23:59:59-06:00</ns0:ExpiredDate>
            <ns0:ILMStatus>EXT</ns0:ILMStatus>
            <ns0:SecurityStatus>P</ns0:SecurityStatus>
            <ns0:ContentSize>3801</ns0:ContentSize>
            <ns0:Extension>zip</ns0:Extension>
            <ns0:FileName/>
            <ns0:ReportTypeID>12300</ns0:ReportTypeID>
            <ns0:Prefix>cdr</ns0:Prefix>
            <ns0:FriendlyName>LMPSROSNODENP6788_20180210_070013_xml</ns0:FriendlyName>
            <ns0:ConstructedName>cdr.00012300.0000000000000000.20180210.070017093.LMPSROSNODENP6788_20180210_070013_xml.zip</ns0:ConstructedName>
            <ns0:DocID>598257275</ns0:DocID>
            <ns0:PublishDate>2018-02-10T07:00:16-06:00</ns0:PublishDate>
            <ns0:ReportName>LMPs by Resource Nodes, Load Zones and Trading Hubs</ns0:ReportName>
            <ns0:DUNS>0000000000000000</ns0:DUNS>
            <ns0:DocCount>0</ns0:DocCount>
        </ns0:Document>
            …

       Extract the DocID element from the first <Document> node. In this example it would be 598257277
       Use this value to call the next URL

       

2: http://mis.ercot.com/misdownload/servlets/mirDownload?doclookupId=598257277

   This will download the requested report in .ZIP format.
   Need to unzip this file, extract the CSV LMP report, and extract HB_NORTH and HB_HOUSTON node information.



   — General Screen Scrape Algorithm:
 
   START (On Signal)
     var _lastDocID == String.Empty
     while(true)
        Query URL -> http://mis.ercot.com/misapp/servlets/IceMktDocListWS?reportTypeId=12300
        Extract DocID from first node
        if(!_lastDocID.Equals(DocID))
	    QueryURL -> http://mis.ercot.com/misdownload/servlets/mirDownload?doclookupId=DocID
            ExtractAndDisplay();
            _lastDocID = DocID;
   STOP (On Signal)
       
     
   Notes: There's a time difference between when a report is run and when a report is published on the order or seconds:
     
      -- The report published time is located in the  <ns0:PublishDate></ns0:PublishDate> tags like so:
            <ns0:PublishDate>2018-02-10T07:00:16-06:00</ns0:PublishDate>
      -- The report runtime is contained within each report
     

 * 
 * 
 * ******************************************************************************/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Xml;
using System.IO;
using ErcotAPILib.MarketInfo;
using Syroot.Windows.IO;
using System.Runtime.InteropServices;


namespace ErcotAPILib.Utils
{
    /// <summary>
    /// Provides means to download and extract reports directly from ERCOT's website without having
    /// to make a SOAP call.
    /// </summary>
    public class ScreenScraper : ApplicationBase
    {

        /****************************************
         * Constants
         * *************************************/
        private const string DOC_LIST_URL = @"http://mis.ercot.com/misapp/servlets/IceMktDocListWS?reportTypeId=";
        private const string DOC_DOWNLOAD_URL = @"http://mis.ercot.com/misdownload/servlets/mirDownload?doclookupId=";
      
        private const string HTTP_REQUEST_METHOD = "GET";
        private const string DOC_ID_ELEMENT = "<ns0:DocID>";
        private const string DOC_ID_END_ELEMENT = "</ns0:DocID>";
        private const int DOC_ID_LENGTH = 9;
        private const int REPORT_RUNTIME_POSITION = 0;
        private const int BUS_POSITION = 2;
        private const int PRICE_POSITION = 3;

        private const string LMPS_BY_NODES_ZONES_HUBS = "12300";
        private const string SHORT_TERM_SYSTEM_ADEQUACY_REPORT = "12315";
        private const string RTOR_PRICE_ADDER_REPORT = "13221";

      


       

        

       
        public ScreenScraper() : base(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
        {
            
            LogDebug("Screen Scraper instance created!");
        }



        /***************************************
         * Methods
         * *************************************/


        /// <summary>
        /// Convenience Method - Gets the latest DocID for LMPs report 12300
        /// </summary>
        /// <returns></returns>
        public string GetLatestLMPsReportDocID()
        {

            return GetLatestDocID(LMPS_BY_NODES_ZONES_HUBS);

        } // GetLatestDocID



        /// <summary>
        /// Convenience Method - Get the latest DocID for Short Term System Adequacy Report 12315
        /// </summary>
        /// <returns></returns>
        public string GetLatestShortTermSystemAdequacyReportDocID()
        {
            return GetLatestDocID(SHORT_TERM_SYSTEM_ADEQUACY_REPORT);
        }


        public string GetLatestRTORPriceAdderDocID()
        {
            return GetLatestDocID(RTOR_PRICE_ADDER_REPORT);
        }

        /// <summary>
        /// Gets the latest DocID for report type
        /// </summary>
        /// <param name="reportTypeID">ReportTypeID from ERCOT API Guide </param>
        /// <returns></returns>
        public string GetLatestDocID(string reportTypeID)
        {
            WebResponse response = null;
            StreamReader reader = null;
            Stream dataStream = null;
            string docID = string.Empty;
            string server_response = string.Empty;
            try
            {
                LogDebug("Submitting web request...");
                WebRequest request = WebRequest.CreateHttp(DOC_LIST_URL + reportTypeID);
                LogDebug("Web request returned successfully...");

                request.Method = HTTP_REQUEST_METHOD;
                response = request.GetResponse();
                dataStream = response.GetResponseStream();

                reader = new StreamReader(dataStream);
                server_response = reader.ReadToEnd();
                int start_doc_id_element_position = server_response.IndexOf(DOC_ID_ELEMENT); // go to the first doc_id tag occurrence 
                int end_doc_id_element_position = server_response.IndexOf(DOC_ID_END_ELEMENT);
                docID = server_response.Substring(start_doc_id_element_position + DOC_ID_ELEMENT.Length, (end_doc_id_element_position - (start_doc_id_element_position + DOC_ID_ELEMENT.Length) )); // get the docID
                
               

                LogDebug("Retrieved DocID " + docID);

            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw new Exception("Problem extracting DocID!", e);
            }
            finally
            {
                if (response != null) response.Close();
                if (reader != null) reader.Close();
                if (dataStream != null) dataStream.Close();
            }

            return docID;
        }


        /// <summary>
        /// Extracts the LMP csv report file from downloaded zip file
        /// </summary>
        /// <param name="docID">Valid document ID</param>
        /// <returns>CSV file contents string</returns>
        public string ExtractLmpsReportCsvFile(string docID)
        {
            return ExtractCsvFileForReportInMemory(docID);
        }


        /// <summary>
        /// Extracts the Short Term Adequacy Report CSV file
        /// </summary>
        /// <param name="docID">Valid DocumentID</param>
        /// <returns>Extracted CSV file contents string</returns>
        public string ExtractShortTermSystemAdequacyReportCsvFile(string docID)
        {
            return ExtractCsvFileForReportInMemory(docID);
        }


        public string ExtractPTORPriceAdderCsvFile(string docID)
        {
            return ExtractCsvFileForReportInMemory(docID);
        }



      /// <summary>
      /// Downloads report, saves to disk, and extracts CSV file. Lots of disk I/O.
      /// This method is retained for historic reasons.
      /// </summary>
      /// <param name="report"></param>
      /// <returns></returns>
        public string ExtractCsvFileForReportToDisk(Report report)
        {

            if ((report == null) || (report.Url == null) || (report.Url == string.Empty) || (report.FileName == null) || (report.FileName == string.Empty))
            {
                throw new ArgumentException("Report object missing Url or Filename fields.");
            }

            KnownFolder knownFolder = new KnownFolder(KnownFolderType.Downloads);
            FileStream fs = null;
            ZipFile zipFile = null;
            string csv_file_contents = string.Empty;
            FileStream fs1 = null;
            StreamReader reader = null;
            string fullZipToPath = string.Empty;

            try
            {
                // download the zip file and save it in user's Downloads folder
                using (var client = new WebClient())
                {
                    client.DownloadFile(report.Url, knownFolder.Path + @"\" + report.FileName);

                }

                fs = File.OpenRead(knownFolder.Path + @"\" + report.FileName);
                zipFile = new ZipFile(fs);

                foreach (ZipEntry zipEntry in zipFile)
                {

                    string entryFileName = zipEntry.Name;
                    byte[] buffer = new byte[4096];

                    Stream zipStream = zipFile.GetInputStream(zipEntry);
                    fullZipToPath = Path.Combine(knownFolder.Path, entryFileName);

                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                        streamWriter.Close();
                    }

                    fs1 = File.OpenRead(fullZipToPath);
                    reader = new StreamReader(fs1);
                    reader = new StreamReader(zipStream);
                    csv_file_contents = reader.ReadToEnd();
                }

            }
            catch (Exception e)
            {
                LogError(e.ToString());

                throw new Exception("Problem extracting csv from zip file!", e);
            }
            finally
            {
                if (zipFile != null)
                {
                    zipFile.IsStreamOwner = true;
                    zipFile.Close();
                }
                if (fs1 != null)
                {
                    fs1.Close();
                    reader.Close();
                }
            }

            LogDebug(csv_file_contents);
            return csv_file_contents;

        }

        /// <summary>
        /// Downloads report to MemoryStream and extracts CSV file contents.
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        public string ExtractCsvFileForReportInMemory(Report report)
        {

            if ((report == null) || (report.Url == null) || (report.Url == string.Empty) || (report.FileName == null) || (report.FileName == string.Empty))
            {
                throw new ArgumentException("Report object missing Url or Filename fields.");
            }

            ZipFile zipFile = null;
            string csv_file_contents = string.Empty;
            MemoryStream ms = null;
            StreamReader reader = null;


            try
            {
                // download the zip file into MemoryStream
                using (var client = new WebClient())
                {
                    ms = new MemoryStream(client.DownloadData(report.Url));
                }

                zipFile = new ZipFile(ms);
                foreach (ZipEntry zipEntry in zipFile)
                {
                    Stream zipStream = zipFile.GetInputStream(zipEntry);
                    reader = new StreamReader(zipStream);
                    csv_file_contents = reader.ReadToEnd();
                }

            }
            catch (Exception e)
            {
                LogError(e.ToString());

                throw new Exception("Problem extracting csv from zip file!", e);
            }
            finally
            {
                if (zipFile != null)
                {
                    zipFile.IsStreamOwner = true;
                    zipFile.Close();
                    zipFile = null;
                }
                if (ms != null)
                {
                    ms.Dispose();
                    ms = null;
                    reader.Close();
                    reader = null;
                }
            }

            return csv_file_contents;
        } // end ()

  
        /// <summary>
        /// Overloaded version of previous method. 
        /// </summary>
        /// <param name="docID">Valid DocumentID </param>
        /// <returns></returns>
        public string ExtractCsvFileForReportInMemory(string docID)
        {
            if ((docID == null) || (docID == String.Empty))
            {
                throw new ArgumentException("Invalid docID argument.");
            }

            ZipFile zipFile = null;
            string csv_file_contents = string.Empty;
            MemoryStream ms = null;
            StreamReader reader = null;

            try
            {
                // download the zip file into MemoryStream
                using (var client = new WebClient())
                {
                    ms = new MemoryStream(client.DownloadData(DOC_DOWNLOAD_URL + docID));
                }

                zipFile = new ZipFile(ms);
                foreach (ZipEntry zipEntry in zipFile)
                {
                    Stream zipStream = zipFile.GetInputStream(zipEntry);
                    reader = new StreamReader(zipStream);
                    csv_file_contents = reader.ReadToEnd();
                }

            }
            catch (Exception e)
            {
                LogError(e.ToString());
                throw new Exception("Problem extracting csv from zip file!", e);
            }
            finally
            {
                if (zipFile != null)
                {
                    zipFile.IsStreamOwner = true;
                    zipFile.Close();
                }
                if (ms != null)
                {
                    ms.Dispose();
                    ms = null;
                    reader.Close();
                    reader = null;
                }
            }

            return csv_file_contents;
        } // end ()




        /// <summary>
        /// Extracts node price data for HB_HOUSTON and HB_NORTH (any nodes given as arguments, really)
        /// </summary>
        /// <param name="nodes">Nodes whose price you want to extract</param>
        /// <param name="lmp_csv_report">CSV file contents extracted from zip file</param>
        /// <returns></returns>
        public List<Lmp> ExtractLmpsNodeData(string[] nodes, string lmp_csv_report)
        {
            if ((nodes.Length < 1) || (lmp_csv_report == null) || (lmp_csv_report == string.Empty)) throw new ArgumentException("Invalid node name!");

            List<Lmp> nodeList = new List<Lmp>();
            string node_line = string.Empty;

            foreach (string s in nodes)
            {
                int node_location = lmp_csv_report.IndexOf(s);
                node_line = lmp_csv_report.Substring(node_location - 23, node_location - (node_location - 24) + s.Length + 12);
                string[] node_line_tokens = node_line.Split(',');
                Lmp lmp = new Lmp(node_line_tokens[REPORT_RUNTIME_POSITION], node_line_tokens[PRICE_POSITION], node_line_tokens[BUS_POSITION]);
                nodeList.Add(lmp);
            }
            
            return nodeList;
        } // end ()


        /// <summary>
        /// Extracts range of nodes from Short Term Adequacy CSV File
        /// </summary>
        /// <param name="startRow">Strting row</param>
        /// <param name="endRow">Ending Row</param>
        /// <param name="shortTermAdequacyCsvFile">Valid CSV file</param>
        /// <returns></returns>
        public string[] ExtractShortTermAdequacyReportRows(int startRow, int endRow, string shortTermAdequacyCsvFile)
        {
            StringBuilder sb = new StringBuilder();
            string[] rows = shortTermAdequacyCsvFile.Split('\n');
            for (int i = startRow; i < endRow; i++)
            {
                sb.AppendLine(rows[i]);
            }

            return sb.ToString().Split('\n');
        } // end ()



        /// <summary>
        /// Returns the value of the 5th column of the second line of the extracted csv file.
        /// This represents Column F -> RTORPA (Real Time Online Resource Price Adder)
        /// </summary>
        /// <param name="csvFile"></param>
        /// <returns></returns>
        public string[] ExtractPriceAdderValueFromCsvFile(string csvFile)
        {
            string line2 = csvFile.Split('\n')[1];
            string[] columns = line2.Split(',');
            string[] retVals = { columns[1], columns[5] };
            return retVals;
        }



    } // end class definition
} // end namespace
