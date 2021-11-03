using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErcotAPILib.Exceptions
{
    public class ErcotCertException : Exception
    {

        public ErcotCertException() : base("Problem creating X509Certificate. Check certs or ensure certs are in expected location.") { }

        public ErcotCertException(string message) : base(message) { }

        public ErcotCertException(string message, Exception innerException) : base(message, innerException) { }

    }
}
