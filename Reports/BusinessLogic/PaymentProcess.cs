using System;
using HISDataAccess;
using Reports.AdminServiceClient;
using Reports.BillingCalculationServiceClient;
using Reports.BillingFacadeServiceClient;
using Reports.Common;
using Reports.CommonSearchServiceClient;
using Reports.ContractMgmtServiceClient;
using Reports.FrontOfficeServiceClient;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Reflection;
using System.Resources;
using Reports.ARServiceReferenceClient;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

namespace Reports.BusinessLogic
{
    class PaymentProcess
    {
        const int DEFAULTWORKSTATION = 0;
        static String strConnString = ConfigurationManager.AppSettings["DBConnectionStringMasters"].ToString();
        static String strDefWorkstationId = ConfigurationManager.AppSettings["DefaultWorkstationId"].ToString();
        static String strDefaultUserId = ConfigurationManager.AppSettings["DefaultUserId"].ToString();
        static String strDefaultHospitalId = ConfigurationManager.AppSettings["DefaultHospitalId"].ToString();
        string json = "";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal enum Database
        {
            Master = 1,
            Transaction = 2
        }
        internal enum ProcessStatus
        {
            Fail = 1,
            Success = 2
        }
        DataTable dtMultiPayment = new DataTable();

        public FetchingPaymentIntitatedList FetchingPaymentIntitatedList(PayementIntitatedList PaymentIntitatedList)
        {
            DataHelper objDataHelper = new DataHelper(DEFAULTWORKSTATION, (int)Database.Master);
            FetchingPaymentIntitatedList objPayIntitatedList = new FetchingPaymentIntitatedList();
            try
            {
                log.Debug("Begin FetchingPaymentIntitatedList");
                FetchingPaymentIntitatedListN obj = new FetchingPaymentIntitatedListN();
                obj.SCHEDULEID = PaymentIntitatedList.ScheduleID;
                obj.PaymentStatus = "Pending";

                objPayIntitatedList.PaymentIntitatedList = new List<FetchingPaymentIntitatedListN>();
                objPayIntitatedList.PaymentIntitatedList.Add(obj);

                List<IDbDataParameter> objIDbDataParameters = new List<IDbDataParameter>();
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@ScheduleId", PaymentIntitatedList.ScheduleID, DbType.Int32, ParameterDirection.Input));
                int intRes = objDataHelper.RunSP("PR_PaymentInitiate_MAPI", objIDbDataParameters.ToArray());
                if (intRes == -1 || intRes > 0)
                {
                    if (objPayIntitatedList.PaymentIntitatedList.Count > 0)
                    {
                        objPayIntitatedList.Code = (int)ProcessStatus.Success;
                        objPayIntitatedList.Status = ProcessStatus.Success.ToString();
                        objPayIntitatedList.Message = "";
                        objPayIntitatedList.Message2L = "";
                    }
                    else
                    {
                        objPayIntitatedList.Code =(int) ProcessStatus.Success;
                        objPayIntitatedList.Status = ProcessStatus.Success.ToString();
                        //objPayIntitatedList.Message = Resources.English.ResourceManager.GetString("NoRecordsFound");
                        //objPayIntitatedList.Message2L = Resources.Arabic.ResourceManager.GetString("NoRecordsFound");
                    }
                }
                else
                {
                    objPayIntitatedList.Code = (int)ProcessStatus.Fail;
                    objPayIntitatedList.Status = ProcessStatus.Fail.ToString();
                    objPayIntitatedList.Message = "";


                }


            }
            catch (Exception ex)
            {
                log.Debug("Exception while FetchingPaymentIntitatedList", ex);
                objPayIntitatedList.Code =(int) ProcessStatus.Fail;
                objPayIntitatedList.Status = ProcessStatus.Fail.ToString();
                objPayIntitatedList.Message = ex.Message;
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in FetchingPaymentIntitatedList", "");
            }
            finally
            {
                objDataHelper = null;
            }

            return objPayIntitatedList;
        }
        public FetchingPaymentProcessList FetchingPaymentProcessList(PaymentProcessList PaymentProcessList) {
            DataHelper objDataHelper = new DataHelper(DEFAULTWORKSTATION, (int)Database.Master);

            var paymentString = PaymentProcessList.PaymentString;

            strDefaultHospitalId = PaymentProcessList.HospitalId.ToString();
            log.Debug("Begin FetchingPaymentProcessList");

            FetchingPaymentProcessList objPayProcessList = new FetchingPaymentProcessList();
            string CardNo = string.Empty;
            string BankId = ""; string CardId = string.Empty; string ValidDate = string.Empty; decimal Amount; string CardType = string.Empty;
            try
            {
                FetchingPaymentProcessListN obj = new FetchingPaymentProcessListN();
                json = paymentString;
                string jsonText = @"{" + json + "}";
                Newtonsoft.Json.Linq.JObject rss = Newtonsoft.Json.Linq.JObject.Parse(jsonText);


                List<IDbDataParameter> objIDbDataParameters = new List<IDbDataParameter>();
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@ScheduleId", Convert.ToInt32(rss["pt_invoice_id"]), DbType.Int32, ParameterDirection.Input));
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@ResonseMsg", paymentString, DbType.String, ParameterDirection.Input));
                int intRes = objDataHelper.RunSP("PR_PaymentProcessingDetails_MAPI", objIDbDataParameters.ToArray());

                if (intRes == -1 || intRes > 0)
                {

                    #region for gettnig credit card details  

                    string PaymentDetails = PaymentCheck(rss["transaction_id"].ToString());
                    string PaymentInfo = PaymentDetails;
                    string PaymentInfoText = @"" + PaymentInfo + "";
                    dynamic data = JObject.Parse(PaymentInfoText);
                    string PaymentLast = @"{" + ((Newtonsoft.Json.Linq.JContainer)data).Last + "}";
                    Newtonsoft.Json.Linq.JObject PaymentInfoNN = Newtonsoft.Json.Linq.JObject.Parse(PaymentLast);
                    var dataS = (JObject)JsonConvert.DeserializeObject(PaymentInfoNN.ToString());
                    //CardNo = dataS["payment_info"]["payment_description"].ToString();
                    //CardId = dataS["payment_info"]["card_scheme"].ToString();
                    //CardType = dataS["payment_info"]["card_type"].ToString();
                    CardNo = data["source"]["number"].ToString();
                    CardId = data["source"]["company"].ToString();
                    CardType = data["source"]["type"].ToString();
                    var details = JObject.Parse(PaymentInfoText);
                    Amount = Convert.ToDecimal(details["amount"].ToString());

                    #endregion

                    string PayTransNo = string.Empty;
                    string IssueDate = string.Empty;
                    string EntryDate = DateTime.Now.Date.ToString();
                    string EntryNo = string.Empty;

                    string strBankname = string.Empty;
                    string strCardtype = string.Empty;
                    int BlockStatus = 1;//     Blocked for OP;                  
                    string TransationNo = " "; int customerID = -1; int IsXmL = 1; int CID = 0; int CType = 0;
                    PaymentProcessList pay = new PaymentProcessList();
                    DataSet dsCards = FetchCardMasters("56,58", 0, 0, 0, pay);
                    if (dsCards.Tables[1].Rows.Count > 0)
                    {
                        DataRow[] drF = dsCards.Tables[1].Select("Names =  '" + (CardId+" card").ToUpper().Trim() + "'");
                        if (drF.Length > 0)
                            CID = Convert.ToInt32(drF[0]["ID"]);
                    }
                    if (dsCards.Tables[0].Rows.Count > 0)
                    {
                        // DataRow[] drF = dsCards.Tables[0].Select("Names=" + CardType);
                        //DataRow[] drF = dsCards.Tables[0].Select("Names like '" + CardType + " [[]%'");
                        DataTable dtN = dsCards.Tables[0].Copy();
                        dtN.AcceptChanges();
                        var results = dtN.AsEnumerable().Where(dr => dr.Field<string>("Names").ToUpper().Contains(CardType.ToUpper()));
                        DataTable dtFinalInvest = results.Any() ? results.CopyToDataTable() : dtN;
                        if (dtFinalInvest.Rows.Count > 0)
                            CType = Convert.ToInt32(dtFinalInvest.Rows[0]["ID"]);
                    }

                    List<IDbDataParameter> objIDbDataParametersP = new List<IDbDataParameter>();
                    
                    AssignPayments(pay, CardNo, CardId, Amount, CID);
                    string strXMLPaymentDetails = Utilities.ConvertDTToXML("PAYMENTS", "ITEM", dtMultiPayment);
                    objIDbDataParametersP.Add(CreateParam1(objDataHelper, "@EntryID", null, 100, DbType.Int32, ParameterDirection.Output));
                    objIDbDataParametersP.Add(CreateParam1(objDataHelper, "@EntryNo", null, 100, DbType.String, ParameterDirection.Output));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@EntryDate", EntryDate.ToString() == "" ? null : EntryDate, DbType.DateTime, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@PatientID", 0, DbType.String, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@PaymentModeID", CID, DbType.Int32, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@Amount", Amount, DbType.Decimal, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@TransationNo", TransationNo, DbType.String, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@CardNo", CardNo, DbType.String, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@CardID", CType, DbType.Int32, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@BankID", BankId, DbType.String, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@CustomerID", customerID, DbType.Int32, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@IssueDate", IssueDate.ToString() == "" ? null : IssueDate, DbType.DateTime, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@ValidTill", ValidDate.ToString() == "" ? null : ValidDate, DbType.DateTime, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@Contact", null, DbType.String, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@Remarks", null, DbType.String, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@BlockID", BlockStatus, DbType.Int32, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@BlockedFor", BlockStatus, DbType.Int32, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@UserID", Convert.ToInt32(strDefaultUserId), DbType.Int32, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@WorkStationID", Convert.ToInt32(strDefWorkstationId), DbType.Int32, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@Error", 0, DbType.Int32, ParameterDirection.Output));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@ISXML", IsXmL, DbType.Int32, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@payments", strXMLPaymentDetails, DbType.String, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@currencyid", 5, DbType.Int32, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@CurrencyRate", 1, DbType.Decimal, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@CurrencyAmount", Amount, DbType.Decimal, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@NottoPost", 0, DbType.Boolean, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@ScheduleId", Convert.ToInt32(rss["pt_invoice_id"].ToString()), DbType.Int32, ParameterDirection.Input));
                    objIDbDataParametersP.Add(CreateParam(objDataHelper, "@UHID", null, DbType.String, ParameterDirection.Input));
                    int intRes1 = objDataHelper.RunSP("Pr_SaveAccountDeposits_MAPI", objIDbDataParametersP.ToArray());
                    string sDepositNo = string.Empty;
                    if (int.Parse(objIDbDataParametersP[0].Value.ToString()) > 0)
                    {
                        sDepositNo = Convert.ToString(objIDbDataParametersP[1].Value.ToString());
                        obj.DepositNO = sDepositNo;
                        objPayProcessList.PaymentProcessList = new List<FetchingPaymentProcessListN>();
                        objPayProcessList.PaymentProcessList.Add(obj);
                        //objPayProcessList.Code = ProcessStatus.Success;
                        //objPayProcessList.Status = ProcessStatus.Success.ToString();
                        //objPayProcessList.Message = "Deposit Number :" + sDepositNo + " Generated Successfully";
                    }
                    if (objPayProcessList.PaymentProcessList.Count > 0)
                    {
                        objPayProcessList.Code = (int) ProcessStatus.Success;
                        objPayProcessList.Status = ProcessStatus.Success.ToString();
                        objPayProcessList.Message = "Deposit Number :" + sDepositNo + " Generated Successfully";
                        objPayProcessList.Message2L = "";
                    }
                    else
                    {
                        objPayProcessList.Code = (int) ProcessStatus.Success;
                        objPayProcessList.Status = ProcessStatus.Success.ToString();
                        //objPayProcessList.Message = Resources.English.ResourceManager.GetString("NoRecordsFound");
                        //objPayProcessList.Message2L = Resources.Arabic.ResourceManager.GetString("NoRecordsFound");
                        objPayProcessList.Message = System.Configuration.ConfigurationManager.AppSettings["NoRecordsFound"].ToString();
                        objPayProcessList.Message2L = System.Configuration.ConfigurationManager.AppSettings["NoRecordsFound"].ToString();
                    }
                }
                else
                {
                    objPayProcessList.Code = (int) ProcessStatus.Fail;
                    objPayProcessList.Status = ProcessStatus.Fail.ToString();
                    objPayProcessList.Message = "";
                }



            }
            catch (Exception ex)
            {
                objPayProcessList.Code =(int) ProcessStatus.Fail;
                objPayProcessList.Status = ProcessStatus.Fail.ToString();
                objPayProcessList.Message = ex.Message;
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in FetchingPaymentProcessList", "");
            }

            return objPayProcessList;
        }

        private void AssignPayments(PaymentProcessList paymentProcessList, string cardNo, string cardId, decimal amount, int cid)
        {

            dtMultiPayment.Columns.Add("BID", System.Type.GetType("System.Int32"));
            dtMultiPayment.Columns.Add("CID", System.Type.GetType("System.String"));
            dtMultiPayment.Columns.Add("PMID", System.Type.GetType("System.Int32"));
            dtMultiPayment.Columns.Add("EDATE", System.Type.GetType("System.String"));
            dtMultiPayment.Columns.Add("EID", System.Type.GetType("System.Int32"));
            dtMultiPayment.Columns.Add("ET", System.Type.GetType("System.String"));
            dtMultiPayment.Columns.Add("AMT", System.Type.GetType("System.Decimal"));
            dtMultiPayment.Columns.Add("TNO", System.Type.GetType("System.String"));
            dtMultiPayment.Columns.Add("CNO", System.Type.GetType("System.String"));
            dtMultiPayment.Columns.Add("TDATE", System.Type.GetType("System.String"));
            dtMultiPayment.Columns.Add("VTO", System.Type.GetType("System.String"));
            dtMultiPayment.Columns.Add("CON", System.Type.GetType("System.String"));
            dtMultiPayment.Columns.Add("REM", System.Type.GetType("System.String"));
            dtMultiPayment.Columns.Add("ORINT", System.Type.GetType("System.Int32"));
            dtMultiPayment.Columns.Add("CTYP", System.Type.GetType("System.Int32"));
            dtMultiPayment.Columns.Add("VID", System.Type.GetType("System.Int32"));
            dtMultiPayment.Columns.Add("VNAME", System.Type.GetType("System.String"));
            dtMultiPayment.Columns.Add("CHOL", System.Type.GetType("System.String"));
            dtMultiPayment.Columns.Add("EDCM", System.Type.GetType("System.String"));
            dtMultiPayment.Columns.Add("CUID", System.Type.GetType("System.Int32"));
            dtMultiPayment.Columns.Add("CURS", System.Type.GetType("System.Decimal"));
            dtMultiPayment.Columns.Add("CAMT", System.Type.GetType("System.Decimal"));



            DateTime Edate = new DateTime(1900, 01, 01);
            DataRow drMulti = dtMultiPayment.NewRow();
            drMulti["PMID"] = "2";  //2 -credit 3 -- debit
            drMulti["CID"] = cid;
            drMulti["BID"] = DBNull.Value;
            drMulti["EDATE"] = "";//Edate.ToString("dd-MMM-yyyy");
            drMulti["EID"] = DBNull.Value;
            drMulti["ET"] = "DI";
            drMulti["AMT"] = amount.ToString();
            drMulti["TNO"] = " ";
            drMulti["CNO"] = cardNo.ToString();
            drMulti["TDATE"] = "";//Edate.ToString("dd-MMM-yyyy");
            drMulti["VTO"] = DBNull.Value;//Card Validity
            drMulti["CON"] = " ";
            drMulti["REM"] = " ";
            drMulti["ORINT"] = 0;
            drMulti["CTYP"] = 0;
            drMulti["VID"] = 0;
            drMulti["VNAME"] = " ";
            drMulti["CHOL"] = " ";
            drMulti["EDCM"] = " ";
            drMulti["CUID"] = 5;
            drMulti["CURS"] = 1;
            drMulti["CAMT"] = amount.ToString();
            dtMultiPayment.Rows.Add(drMulti);
        }

        private DataSet FetchCardMasters(string StrTableIds, int UserId, int intError, int inttagid, PaymentProcessList paymentProcessList)
        {
            DataHelper objDataHelper = new DataHelper(DEFAULTWORKSTATION, (int)Database.Master);
            DataSet dsSpecConfig = new DataSet();
            try
            {
                List<IDbDataParameter> objIDbDataParameters = new List<IDbDataParameter>();
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@Demographic", StrTableIds.ToString(), DbType.String, ParameterDirection.Input));
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@UserID", UserId, DbType.Int32, ParameterDirection.Input));
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@Error", intError, DbType.Int32, ParameterDirection.Input));
                dsSpecConfig = objDataHelper.RunSPReturnDS("Pr_FetchDemoGraphics", objIDbDataParameters.ToArray());

                return dsSpecConfig;
            }
            finally
            {

            }
        }

        private string PaymentCheck(string TranscationNumber)
        {
            //string strURL = "https://secure.paytabs.sa/payment/query";
            //string json3 = string.Empty;
            //string userName = ConfigurationManager.AppSettings["PaymentUser"].ToString();//"syed@alhammadi.com";
            //string passWord = ConfigurationManager.AppSettings["PaymentPassword"].ToString();//"Ahh@123321";
            //PaymentListN PaymentListNA = new PaymentListN();
            //PaymentListNA.profile_id = ConfigurationManager.AppSettings["ProfileID"].ToString();//"53146";
            //PaymentListNA.tran_ref = TranscationNumber;//"TST2100500034089";
            //string output = JsonConvert.SerializeObject(PaymentListNA);
            //var httpWebRequest = (HttpWebRequest)WebRequest.Create(strURL);
            //Encoding encoding = new UTF8Encoding();
            //httpWebRequest.Method = "POST";
            //httpWebRequest.AllowAutoRedirect = true;
            //ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            ////ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            //ServicePointManager.ServerCertificateValidationCallback = (snder, cert, chain, error) => true;
            //httpWebRequest.ContentType = "application/json";
            //string _auth = string.Format("{0}:{1}", userName, passWord);
            //string _enc = Convert.ToBase64String(Encoding.ASCII.GetBytes(_auth));
            //string _cred = string.Format("{0} {1}", "Basic", _enc);
            //httpWebRequest.Headers[HttpRequestHeader.Authorization] = ConfigurationManager.AppSettings["ServerKey"].ToString();//"SNJNML6MBK-HZWRKDHJ6H-9LRKDRKTLK";
            //byte[] data = encoding.GetBytes(output);
            //httpWebRequest.ContentLength = data.Length;
            //Stream stream = httpWebRequest.GetRequestStream();
            //stream.Write(data, 0, data.Length);
            //stream.Close();
            //HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
            //string s = response.ToString();
            //StreamReader reader = new StreamReader(response.GetResponseStream());
            //String jsonresponse = "";
            //String temp = null;
            //while ((temp = reader.ReadLine()) != null)
            //{
            //    jsonresponse += temp;
            //}

            var client = new HttpClient();
            var strUrl = "https://api.moyasar.com/v1/payments/";
            strUrl = strUrl+ TranscationNumber;
            var request = new HttpRequestMessage(HttpMethod.Get, strUrl);//098b3711-4cdd-4928-bf5d-ada700db6b7c");
            request.Headers.Add("Authorization", ConfigurationManager.AppSettings["mayosarAuth"].ToString() );
            var response = client.SendAsync(request).Result;
            var result = response.Content.ReadAsStringAsync().Result;

            return result;
        }

        private IDbDataParameter CreateParam1(DataHelper objDataHelper, string paramName, object paramVal, int size, DbType paramType, ParameterDirection paramDirection)
        {
            IDbDataParameter objIDbDataParameter = objDataHelper.CreateDataParameter();
            objIDbDataParameter.ParameterName = paramName;
            objIDbDataParameter.Value = paramVal;
            objIDbDataParameter.DbType = paramType;
            objIDbDataParameter.Direction = paramDirection;
            objIDbDataParameter.Size = size;

            return objIDbDataParameter;
        }

        private IDbDataParameter CreateParam(DataHelper objDataHelper, string paramName, object paramVal, DbType paramType, ParameterDirection paramDirection)
        {
            IDbDataParameter objIDbDataParameter = objDataHelper.CreateDataParameter();
            objIDbDataParameter.ParameterName = paramName;
            objIDbDataParameter.Value = paramVal;
            objIDbDataParameter.DbType = paramType;
            objIDbDataParameter.Direction = paramDirection;

            return objIDbDataParameter;
        }

    }

    internal class PaymentListN
    {
        public string profile_id { get; set; }
        public string tran_ref { get; set; }
    }

    public class PaymentProcessList
    {
        public  int HospitalId { get; set; } 

        public  string PaymentString { get; set; }  

    }

    public class FetchingPaymentProcessList
    {
        public int Code { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string Message2L { get; set; }
        public List<FetchingPaymentProcessListN> PaymentProcessList;
    }

    public class FetchingPaymentProcessListN
    {
        public string DepositNO { get; internal set; }
    }

    public class PayementIntitatedList
    {
        public int ScheduleID { get; set; }
    }

    public class FetchingPaymentIntitatedListN
    {
        public int SCHEDULEID { get; set; }       
                        
        public string PaymentStatus { get; set; }
    }

    public class FetchingPaymentIntitatedList
    {
        public int Code { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string Message2L { get; set; }
        public List<FetchingPaymentIntitatedListN> PaymentIntitatedList;
    }
}