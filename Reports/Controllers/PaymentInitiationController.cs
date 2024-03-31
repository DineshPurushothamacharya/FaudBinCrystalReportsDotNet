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
    public class PaymentInitiation : ApiController
    {
        [Route("api/PaymentInitiation/PaymentInitiate")]
        public string PaymentInitiate(PayementIntitatedList paymentInitiatet)
        {
            PaymentProcess objPaymentProcess = new PaymentProcess();
            objPaymentProcess.FetchingPaymentIntitatedList(paymentInitiatet);

            return string.Empty;
        }
    }
   
 
}