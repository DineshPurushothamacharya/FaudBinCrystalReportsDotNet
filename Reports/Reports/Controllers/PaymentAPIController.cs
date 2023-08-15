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

namespace Reports.Controllers
{
    public class PaymentAPIController : ApiController
    {
        [Route("api/paymentapi/payment11")]
        public string Payment()
        {
            PaymentsClass objPayment = new PaymentsClass();
            objPayment.GetRootPatientIdPerformance("PFBS.0000363921", 1, 1);
            
            return string.Empty;
        }
    }
}
