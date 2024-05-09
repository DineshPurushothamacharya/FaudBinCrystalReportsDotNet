using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Reports.Common;
using Reports.BusinessLogic;
using static Reports.BusinessLogic.PaymentProcess;

namespace Reports.Controllers
{
    public class PaymentProcessController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        [Route("api/paymentprocess/PaymentProcessList")]
        public FetchingPaymentProcessList PaymentProcessList(PaymentProcessList objPaymentProcessList)
        {
            log.Info("Begin: PaymentProcessList");
            PaymentProcess objPaymentProcess = new PaymentProcess();
            var result = objPaymentProcess.FetchingPaymentProcessList(objPaymentProcessList);
            log.Info("End: PaymentProcessList");

            return result;
        }

    }
}
