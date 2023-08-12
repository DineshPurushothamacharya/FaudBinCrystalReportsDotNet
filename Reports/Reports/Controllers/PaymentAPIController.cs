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

namespace Reports.Controllers
{
    public class PaymentAPIController : ApiController
    {
        [Route("api/paymentapi/payment")]
        public string Payment()
        {

            return string.Empty;
        }
    }
}
