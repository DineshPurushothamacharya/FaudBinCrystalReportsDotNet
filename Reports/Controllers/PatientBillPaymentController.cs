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
using static Reports.BusinessLogic.PatientBillPayment;

namespace Reports.Controllers
{
    public class PatientBillPaymentController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Route("api/PatientBillPayment/GetPaymentBillNo")]
        public PatientBiillInfoList GetPaymentBillNo(String HospitalID, string RegCode, string ScheduleId)
        {
            log.Info("Begin GetPaymentBillNo Controller");
            PatientBillPayment objPayment = new PatientBillPayment();
            PatientBillList objRequest = new PatientBillList();
            objRequest.HospitalId = int.Parse(HospitalID); // 1;
            objRequest.RegCode = RegCode; // "PFBS.0000363921";
            objRequest.ScheduleID = ScheduleId; // "5891";

            var objPatientBillInfoList = objPayment.GettingPatientList(objRequest);
            log.Info("End GetPaymentBillNo Controller");

            return objPatientBillInfoList;
        }
    }
}
