/******************************************************************************************
* Filename:   
* Author:        Rick Miller
* Date:       
* Description:
* Change Log:
* Date            Coder             Comments
* -----------------------------------------------------------------------------------------
*
*
*******************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ErcotAPILib.MarketInfo
{
    public class Report
    {


        /*****************************************************
        * Constants
        ******************************************************/

        private const int OPERATING_DATE = 0;
        private const int REPORT_GROUP = 1;
        private const int FILE_NAME = 2;
        private const int CREATED = 3;
        private const int SIZE = 4;
        private const int FORMAT = 5;
        private const int URL = 6;



        /*****************************************************
        * Properties
        ******************************************************/

        public string OperatingDate
        {
            get;
            set;
        }

        public string ReportGroup
        {
            get;
            set;
        }

        public string FileName
        {
            get;
            set;
        }

        public string Created
        {
            get;
            set;
        }

        public string Size
        {
            get;
            set;
        }

        public string Format
        {
            get;
            set;
        }

        public string Url
        {
            get;
            set;
        }

        /*****************************************************
        * Constructors
        ******************************************************/
        public Report(string operatingDate, string reportGroup, string fileName, string created, string size, string format, string url)
        {
            OperatingDate = operatingDate;
            ReportGroup = reportGroup;
            FileName = fileName;
            Created = created;
            Size = size;
            Format = format;
            Url = url;
        }

        public Report(XmlNode reportNode)
        {
            OperatingDate = reportNode.ChildNodes[OPERATING_DATE].InnerText;
            ReportGroup = reportNode.ChildNodes[REPORT_GROUP].InnerText;
            FileName = reportNode.ChildNodes[FILE_NAME].InnerText;
            Created = reportNode.ChildNodes[CREATED].InnerText;
            Size = reportNode.ChildNodes[SIZE].InnerText;
            Format = reportNode.ChildNodes[FORMAT].InnerText;
            Url = reportNode.ChildNodes[URL].InnerText;

        }


        /******************************************************
        * Methods
        ******************************************************/

        public override string ToString()
        {
            return OperatingDate + " " + ReportGroup + " " + FileName + " " + Created + " " + Size + " " + Format + " " + Url;

        }


    }
}
