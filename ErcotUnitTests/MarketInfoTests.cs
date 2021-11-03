/******************************************************************************************
* Filename:      MarketInfoTests.cs
* Author:        Rick Miller
* Date:          3 November 2021
* Description:   Tests ERCOT MarketInfo SOAP Service.
* Change Log:
* Date            Coder             Comments
* -----------------------------------------------------------------------------------------
* 03Nov2021       Rick Miller       Created
*
*******************************************************************************************/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ErcotAPILib.MarketInfo;
using System.Collections.Generic;

namespace ErcotUnitTests
{
    [TestClass]
    public class MarketInfoTests
    {
        [TestMethod]
        public void GetLMPs()
        {
            MarketInfo _marketInfo = new MarketInfo();
            List<Lmp> lmpList = _marketInfo.GetRtmLmps();
            Assert.AreNotEqual(lmpList.Count, 0);
        }
    }
}
