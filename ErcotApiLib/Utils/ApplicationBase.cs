using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;


namespace ErcotAPILib.Utils
{
    public class ApplicationBase
    {

        protected Logger Log { get; private set; }

        protected ApplicationBase(Type declaringType)
        {
            Log = LogManager.GetLogger(declaringType.FullName);

        }


        protected void LogDebug(string s)
        {
            Log.Debug(s);
        }

        protected void LogDebug(object o)
        {
            Log.Debug(o.ToString());
        }


        protected void LogTrace(string s)
        {
            Log.Trace(s);
        }

        protected void LogTrace(object o)
        {
            Log.Trace(o.ToString());
        }


        protected void LogWarn(string s)
        {
            Log.Warn(s);
        }

        protected void LogWarn(object o)
        {
            Log.Warn(o.ToString());
        }

        protected void LogError(string s)
        {
            Log.Error(s);
        }

        protected void LogError(object o)
        {
            Log.Error(o.ToString());
        }

        protected void LogInfo(object o)
        {
            Log.Info(o.ToString());
        }

        protected void LogInfo(string s)
        {
            Log.Info(s);
        }


    }
}
