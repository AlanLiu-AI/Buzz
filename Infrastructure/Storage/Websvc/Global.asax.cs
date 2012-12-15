using System;
using System.Web;
using Runner.Base.Util;

namespace Websvc
{
    public class Global : HttpApplication
    {
        public const string ServiceName = "Test.Websvc";
        private readonly static log4net.ILog Log = log4net.LogManager.GetLogger(ServiceName);

        protected void Application_Start(object sender, EventArgs e)
        {
            Log4netUtil.EmbbededInitial(ServiceName+".log");
            Runner.Base.Global.ConnectionString = "user id=sa;password=sa123;server=tcp:localhost, 1432; database=AQ30R4Db2;MultipleActiveResultSets=True;";
            Runner.Base.Global.DbType = Runner.Base.Db.SqlType.MsSql;
            Log.Info("Application_Start called.");
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            Log.Info("Application_End called.");
        }
    }
}