using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
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
    public class ReportAPIController : ApiController
    {
        public object ExportFormatType { get; private set; }
        public object CrFormatTypeOptions { get; private set; }

        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }


        //https://localhost:44351/api/reportapi/5
        //https://localhost:44351/api/reportapi?TestOrderID=test
        //https://localhost:44351/api/reportapi?TestOrderID=test&UHID=123
        //https://localhost:44351/api/reportapi?TestOrderID=1358599&UHID=PFBS.0000397737&isExternal=true
        //https://localhost:44351/api/reportapi?TestOrderID=1463197&UHID=PFBS.0000070775&isExternal=true
        //https://localhost:44351/api/reportapi?TestOrderID=1447060&UHID=PFBS.0000397737&isExternal=true

        //http://172.16.16.53/api/reportapi?TestOrderID=1358599&UHID=PFBS.0000397737&isExternal=true
        // GET api/<controller>/5
        //[Route("api/reportapi/TestOrderID")]
        public string Get(string TestOrderID, string UHID, bool isExternal)
        {
            DataSet ds = new DataSet();
            DataSet dsResultNew = new DataSet();
            DataSet dsHeader = new DataSet();

            //FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://172.16.16.40/PatientPhoto/93f95769-6c20-4686-a616-039f7a118052LabTemp_488-15.pdf");
            //request.Method = WebRequestMethods.Ftp.DownloadFile;

            //using (Stream ftpstream = request.GetResponse().GetResponseStream())
            //using (Stream filestream = File.Create("C:\\Users\\Swathi\\Sangamesh\\Reports\\PDF\\FileFile.pdf"))
            //{
            //    ftpstream.CopyTo(filestream);
            //}

            try
            {
                string connectionstring = System.Configuration.ConfigurationManager.AppSettings["ConnectionString"];
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = new SqlCommand("Pr_FetchVerifiedTestOrderDetails_MAPI", con))
                    {
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                        sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@UHID", UHID);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@TestOrderId", TestOrderID);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@HospitalID", 1);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@Type", 1);
                        sqlDataAdapter.Fill(dsResultNew);
                        con.Close();
                    }

                    if (dsResultNew.Tables[0].Rows.Count == 0)
                    {
                       return "Failure";
                    }

                    using (SqlCommand cmd = new SqlCommand("Pr_FetchTestResults_MAPI", con))
                    {
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                        sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@TestOrderID", TestOrderID);
                        //1358599
                        //1463197
                        //1473739
                        //1447060
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@TestOrderItemID", DBNull.Value);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@TestID", DBNull.Value);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@PatientID", DBNull.Value);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@PatientType", DBNull.Value);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@TBL", 0);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@UserID", 702);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@WorkStationID", 30);
                        SqlParameter @out = cmd.Parameters.AddWithValue("@Error", String.Empty);
                        @out.Direction = ParameterDirection.Output;
                        sqlDataAdapter.Fill(ds);
                        con.Close();
                        //con.Open();
                        //var DataReader = cmd.ExecuteReader();
                    }
                    using (SqlCommand cmd = new SqlCommand("Pr_FetchLabDeptConfigurations_MAPI", con))
                    {
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                        sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@Type", 1);
                        sqlDataAdapter.Fill(dsHeader);
                        con.Close();
                    }

                }

                

                foreach (DataRow IndivRow in ds.Tables[0].Rows)
                {
                    if (IndivRow["IsExternal"].ToString() == "1")
                    {
                        string pdfFileName = IndivRow["Value"].ToString();
                        if (!string.IsNullOrEmpty(pdfFileName))
                        {
                            try
                            {
                                string ftpserver = System.Configuration.ConfigurationManager.AppSettings["ftpserver"];
                                string ftppath = System.Configuration.ConfigurationManager.AppSettings["ftppath"];
                                string ReportLocation = System.Configuration.ConfigurationManager.AppSettings["ReportLocation"];
                                GetFTPReport(ftpserver, ftppath, TestOrderID, ReportLocation, pdfFileName);
                                return "Success";
                            }
                            catch (Exception ex)
                            {
                               return "Failure " + "Please check the report with the hospital";
                            }
                                
                        }

                    }
                }
                

                DataSet ds1 = CreateDataSet();

                if (dsHeader.Tables[0] != null && dsHeader.Tables[0].Rows.Count >= 1)
                {
                    DataRow workRow = ds1.Tables[0].NewRow();
                    workRow["ReportHeading"] = dsHeader.Tables[0].Rows[0]["ReportHeading"].ToString();
                    workRow["RefRange"] = ParseToInt(dsHeader.Tables[0].Rows[0]["RefRange"].ToString());
                    workRow["DisplayNote"] = ParseToBool(dsHeader.Tables[0].Rows[0]["DisplayNote"].ToString());
                    workRow["ModuleID"] = dsHeader.Tables[0].Rows[0]["ModuleID"].ToString();
                    workRow["FootNote"] = dsHeader.Tables[0].Rows[0]["FootNote"].ToString();
                    workRow["GroupPrinting"] = ParseToBool(dsHeader.Tables[0].Rows[0]["GroupPrinting"].ToString());
                    workRow["GPShowTestName"] = ParseToBool(dsHeader.Tables[0].Rows[0]["GPShowTestName"].ToString());
                    workRow["ShowSpecimen"] = ParseToBool(dsHeader.Tables[0].Rows[0]["ShowSpecimen"].ToString());
                    workRow["ResultsNotVerified"] = ParseToBool(dsHeader.Tables[0].Rows[0]["ResultsNotVerified"].ToString());
                    ds1.Tables[0].Rows.Add(workRow);
                }

                foreach (DataRow IndivRow in ds.Tables[2].Rows)
                {

                    DataRow DoctorData = ds1.Tables[1].NewRow();
                    DoctorData["DoctorID1"] = ParseToInt(IndivRow["DoctorID"].ToString());
                    DoctorData["Doctor1"] = IndivRow["DoctorName"].ToString();
                    DoctorData["Signature1"] = IndivRow["SignaturePath"].ToString();
                    DoctorData["Sign1"] = false;
                    DoctorData["TestOrderItemID"] = IndivRow["TestOrderItemId"].ToString();
                    DoctorData["DoctorNameWithDesignation"] = IndivRow["DoctorName2l"].ToString();
                    ds1.Tables[1].Rows.Add(DoctorData);
                }



                foreach (DataRow IndivRow in ds.Tables[0].Rows)
                {
                    DataRow LabReport = ds1.Tables[2].NewRow();

                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count >= 1)
                    {
                        LabReport["TestID"] = ParseToInt(IndivRow["TestID"].ToString());
                        LabReport["TestName"] = IndivRow["TestName"].ToString();
                        LabReport["ParamID"] = IndivRow["ParameterId"].ToString();
                        LabReport["ParamName"] = IndivRow["ParameterName"].ToString();

                        if (LabReport["ParamName"].ToString() == "Troponin I")
                        {
                            string temp = string.Empty;
                        }

                        LabReport["UnitType"] = IndivRow["UnitType"].ToString();

                        LabReport["F1Result"] = IndivRow["Value"].ToString();
                        LabReport["F1Unit"] = IndivRow["UOM"].ToString();
                        // trucate 0 from the value
                        string strSIUnit = string.Empty;
                        //LabReport["F1ReferenceRange"] = IndivRow["minlimit"].ToString() + " - " + IndivRow["maxlimit"].ToString();
                        LabReport["F1ReferenceRange"] = ReferenceRange(IndivRow["MinLimit"].ToString(), IndivRow["MaxLimit"].ToString(), IndivRow["HasUoM"] == DBNull.Value ? false : (IndivRow["HasUoM"].ToString().ToLower() == "true" || IndivRow["HasUoM"].ToString().ToLower() == "false" ? Convert.ToBoolean(IndivRow["HasUoM"]) : Convert.ToBoolean(int.Parse(IndivRow["HasUoM"].ToString()))), "", IndivRow["IsKeywordPrefix"] == DBNull.Value ? false : (IndivRow["IsKeywordPrefix"].ToString().ToLower() == "true" || IndivRow["IsKeywordPrefix"].ToString().ToLower() == "false" ? Convert.ToBoolean(IndivRow["IsKeywordPrefix"]) : Convert.ToBoolean(int.Parse(IndivRow["IsKeywordPrefix"].ToString()))), IndivRow["Keyword"].ToString(), strSIUnit);
                        LabReport["Sequence"] = IndivRow["Sequence"].ToString();
                        LabReport["Slno"] = ParseToInt(IndivRow["Seq"].ToString());
                        LabReport["Remarks"] = IndivRow["ResultRemarks"].ToString();
                        LabReport["ReportDate"] = IndivRow["ModDate"].ToString(); //
                        LabReport["Serial"] = ParseToInt(IndivRow["Seq"].ToString());

                        LabReport["TestOrderItemID"] = IndivRow["TestOrderItemId"].ToString();
                        LabReport["FormatType"] = IndivRow["ParameterType"].ToString();
                        string ParameterType = IndivRow["ParameterType"].ToString();
                        string FormatType = string.Empty;
                        //F2ParamDesc should be blank
                        LabReport["F2ParamDesc"] = "";
                        switch (ParameterType)
                        {
                            case "1":
                                FormatType = "F7";
                                break;
                            case "2":
                                if (IndivRow["Value"] != null && IndivRow["Value"].ToString().Trim() != string.Empty)
                                {
                                    FormatType = "F1";
                                }
                                break;
                            case "3":
                                if (IndivRow["Value"] != null && IndivRow["Value"].ToString().Trim() != string.Empty)
                                {
                                    if (LabReport["F1ReferenceRange"].ToString() != "") FormatType = "F1";
                                    else if (LabReport["F1Unit"].ToString() != "") FormatType = "F8";
                                    else FormatType = "F4";
                                }
                                break;

                            case "4":

                                LabReport["F2ParamDesc"] = IndivRow["Value"].ToString();
                                if (IndivRow["Value"] != null && IndivRow["Value"].ToString().Trim() != string.Empty)
                                {
                                    FormatType = "F2";
                                }
                                break;
                            case "5":
                                if (IndivRow["Value"] != null && IndivRow["Value"].ToString().Trim() != string.Empty)
                                {
                                    FormatType = "F1";
                                }
                                break;
                            case "6":
                                if (IndivRow["Value"] != null && IndivRow["Value"].ToString().Trim() != string.Empty)
                                {
                                    FormatType = "F4";
                                }
                                break;
                            case "7":
                                string[] strTemp = IndivRow["Value"].ToString().Split(Convert.ToChar("#"));

                                if (strTemp.Length > 1)

                                {

                                    LabReport["F1Result"] = strTemp[0].ToString();

                                    FormatType = "F4";

                                }
                                break;
                            case "8":
                                StringBuilder strResult = new StringBuilder("");

                                string strMultiSelect = IndivRow["Value"].ToString();

                                if (strMultiSelect.Trim().Length > 0)
                                {
                                    DataSet dsMultiSelect = new DataSet("MultiSelect");
                                    System.IO.StringReader xmlSR = new StringReader(strMultiSelect);
                                    string[] strSplit = strMultiSelect.Split(new char[] { ';' });
                                    if (strSplit.Length > 0)
                                    {
                                        foreach (string str in strSplit)
                                        {
                                            if (str.Trim() != string.Empty)
                                                strResult.Append(str.Trim() + "\n");
                                        }
                                    }
                                }
                                LabReport["F1Result"] = strResult.ToString();
                                FormatType = "F4";
                                break;
                            case "9":
                                if (IndivRow["Value"].ToString().Trim().Length > 0)
                                {

                                    string strpp = IndivRow["Value"].ToString();
                                    //drPreview["F2ParamDesc"]= StripTags(strpp);Commented by Nagaraju to avoid replacing html tags
                                    LabReport["F2ParamDesc"] = strpp;
                                    // drPreview["F2ParamDesc"] = strpp;
                                    if (IndivRow["Value"] != null && IndivRow["Value"].ToString().Trim() != string.Empty)
                                    {
                                        FormatType = "F2";
                                    }
                                }
                                break;
                            case "10":
                            case "11":
                                FormatType = "";
                                break;
                            case "12":
                            case "13":
                            case "14":
                            case "15":
                                FormatType = "F4";
                                break;
                            case "17":
                                string strResultsXML = IndivRow["ExtValue"].ToString();
                                System.IO.StringReader xmlSROrg = new StringReader(strResultsXML);
                                DataSet dsCulture = new DataSet();
                                dsCulture.ReadXml(xmlSROrg);
                                DataRow[] drCulture = dsCulture.Tables[0].Select("", "Organism");

                                FormatType = "F5";
                                LabReport["F1Result"] = "";
                                string Organism = string.Empty;
                                foreach (var Indiv in drCulture)
                                {
                                    DataRow AntibioticRow = ds1.Tables[5].NewRow();
                                    AntibioticRow["TestID"] = LabReport["TestID"];
                                    AntibioticRow["TestOrderItemID"] = LabReport["TestOrderItemID"];
                                    AntibioticRow["Antibiotic"] = Indiv["Antibiotic"].ToString();
                                    AntibioticRow["Organism"] = Indiv["Organism"].ToString();
                                    AntibioticRow["Code"] = Indiv["Code"].ToString();
                                    Organism = Indiv["Organism"].ToString();
                                    AntibioticRow["Susceptibility1"] = Indiv["Susceptibility"].ToString();
                                    ds1.Tables[5].Rows.Add(AntibioticRow);



                                }


                                DataRow OrganismRow = ds1.Tables[6].NewRow();
                                OrganismRow["TestID"] = LabReport["TestID"];
                                OrganismRow["TestOrderItemID"] = LabReport["TestOrderItemID"];
                                OrganismRow["Organism1"] = Organism;
                                ds1.Tables[6].Rows.Add(OrganismRow);

                                //DataRow OrganismRow = ds1.Tables[6].NewRow();
                                //OrganismRow["TestID"] = LabReport["TestID"];
                                //OrganismRow["TestOrderItemID"] = LabReport["TestOrderItemID"];
                                //OrganismRow["Organism1"] = Organism;
                                //ds1.Tables[6].Rows.Add(OrganismRow);


                                break;
                            case "21":
                                FormatType = "F2";
                                LabReport["F1Result"] = "htmlform";
                                LabReport["F2ParamDesc"] = IndivRow["Value"].ToString();
                                break;


                        }
                        LabReport["FormatType"] = FormatType;

                    }

                    if (ds.Tables[1] != null && ds.Tables[1].Rows.Count >= 1)
                    {
                        LabReport["CollectedDate"] = ds.Tables[1].Rows[0]["SampleCollectedAt"].ToString();
                        LabReport["ResultEntryDate"] = ds.Tables[1].Rows[0]["ResultEnteredAt"].ToString();
                    }

                    if (ds.Tables[2] != null && ds.Tables[2].Rows.Count >= 1)
                    {


                        LabReport["DoctorID"] = ParseToInt(ds.Tables[2].Rows[0]["DoctorId"].ToString());

                    }

                    foreach (DataRow ResultRow in ds.Tables[5].Rows)
                    {
                        if (ResultRow["TestID"].ToString() == LabReport["TestID"].ToString() && ResultRow["ParameterId"].ToString() == LabReport["ParamID"].ToString())
                        {
                            LabReport["F1SIResult"] = ResultRow["Value"].ToString();
                            LabReport["F1SIUnit"] = ResultRow["SIUOM"].ToString();
                            LabReport["F1SIReferenceRange"] = ResultRow["minlimit"].ToString() + " - " + ResultRow["maxlimit"].ToString();
                            break;
                        }
                    }


                    ds1.Tables[2].Rows.Add(LabReport);

                }


                DataRow PatientRow = ds1.Tables[3].NewRow();

                if (dsResultNew.Tables[0].Rows.Count >= 1)
                {
                    DataRow Patient = dsResultNew.Tables[0].Rows[0];

                    PatientRow["Department"] = Patient["Specialisation"].ToString(); // Pr_FetchTestOrderDetails_MAPI[0].
                    PatientRow["OrderNo"] = Patient["OrderSlNo"].ToString();
                    PatientRow["IPNo"] = Patient["IpNo"].ToString();
                    PatientRow["UHID"] = Patient["RegCode"].ToString(); //CustomerRow["UHID"] = "1";
                    PatientRow["BillNo"] = Patient["BillNo"].ToString();
                    PatientRow["BillRemarks"] = Patient["BillRemarks"].ToString();
                    PatientRow["Name"] = Patient["PatientName"].ToString();
                    PatientRow["AgeGender"] = Patient["AgeGender"].ToString();
                    PatientRow["OrderDate"] = Patient["OrderDate"].ToString();
                    //CustomerRow["IPBillNoColName"] = "1";
                    //ReportDate	Pr_FetchTestResults_MAPI[1].ResultEnteredAt	minimum value from the tablet
                    PatientRow["PatientID"] = ParseToInt(Patient["PatientId"].ToString());
                    PatientRow["PatPhone"] = Patient["PatPhone"].ToString();
                    PatientRow["DocPhone"] = Patient["DocPhone"].ToString();
                    PatientRow["Nationality"] = Patient["Nationality"].ToString();
                    PatientRow["WardCaption"] = Patient["WardCaption"].ToString();
                    PatientRow["Ward"] = Patient["Ward"].ToString();
                    PatientRow["Doctor"] = Patient["DoctorName"].ToString();
                    PatientRow["PayerName"] = Patient["Payername"].ToString();
                    PatientRow["GradeName"] = Patient["GradeName"].ToString();
                    PatientRow["MRNO"] = Patient["RegCode"].ToString();
                    PatientRow["EmpID"] = Patient["EmpId"].ToString();

                    PatientRow["CompanyCode"] = Patient["CompanyCode"].ToString();
                    PatientRow["CompanyType"] = Patient["CompanyType"].ToString();
                    PatientRow["PatientType"] = 1;



                }


                if (ds.Tables[1] != null && ds.Tables[1].Rows.Count >= 1)
                {
                    PatientRow["ReportDate"] = ds.Tables[1].Rows[0]["ResultEnteredAt"].ToString();
                    PatientRow["ResultEntryDate"] = ds.Tables[1].Rows[0]["ResultEnteredAt"].ToString();
                    PatientRow["CollectionDate"] = ds.Tables[1].Rows[0]["SampleCollectedAt"].ToString();
                    PatientRow["AcknowledgeDate"] = ds.Tables[1].Rows[0]["SampleAckAT"].ToString();
                    PatientRow["Remarks"] = ds.Tables[1].Rows[0]["Remarks"].ToString();
                    PatientRow["AccessionNumber"] = ds.Tables[1].Rows[0]["SampleNumber"].ToString();
                    PatientRow["ResultEnteredByID"] = ds.Tables[1].Rows[0]["ResultEnteredbyEmpid"].ToString();
                    PatientRow["ResultEnteredBy"] = ds.Tables[1].Rows[0]["ResultEnteredBy"].ToString();
                    PatientRow["ResultVerifiedBy"] = ds.Tables[1].Rows[0]["ResultVerifyedBy"].ToString();
                }

                //if (ds.Tables[2] != null && ds.Tables[2].Rows.Count >= 1)
                //{
                //    PatientRow["Doctor"] = ds.Tables[2].Rows[0]["DoctorName"].ToString();
                //}


                ds1.Tables[3].Rows.Add(PatientRow);

                foreach (DataRow IndivRow in ds.Tables[0].Rows)
                {

                    DataRow TestResult = ds1.Tables[4].NewRow();
                    string TestOrderItemID = string.Empty;
                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count >= 1)
                    {
                        string strTestID = IndivRow["TestId"].ToString();
                        bool isPresent = false;
                        foreach (DataRow temp in ds1.Tables[4].Rows)
                        {
                            if (temp["TestID"].ToString() == strTestID)
                            {
                                isPresent = true;
                                break;
                            }
                        }
                        if (isPresent == true)
                            continue;

                        TestResult["TestID"] = IndivRow["TestId"].ToString();
                        TestResult["TestName"] = IndivRow["TestName"].ToString();
                        TestResult["Specimen"] = IndivRow["SpecimenName"].ToString();
                        TestResult["Slno"] = IndivRow["Sequence"].ToString();
                        TestResult["EquipmentName"] = IndivRow["EquipmentName"].ToString();
                        TestResult["Specialisation"] = IndivRow["Specialisation"].ToString();
                        TestOrderItemID = IndivRow["TestOrderItemID"].ToString();
                    }

                    foreach (DataRow ResultRow in ds.Tables[1].Rows)
                    {
                        if (TestOrderItemID == ResultRow["TestOrderItemID"].ToString())
                        {
                            TestResult["Method"] = ResultRow["MethodName"].ToString();
                            TestResult["ProfileName"] = ResultRow["ProfileName"].ToString();
                            TestResult["SampleNumber"] = ResultRow["SampleNumber"].ToString();
                            //TestResult["Specialisation"] = "1";
                            TestResult["IsPanic"] = ParseToBool(ResultRow["Ispanic"].ToString());
                            break;
                        }


                    }


                    //DataTable dt = ds1.Tables[4].Clone().


                    ds1.Tables[4].Rows.Add(TestResult);
                }

                ReportDocument cryRpt = new ReportDocument();
                cryRpt.Load(System.Configuration.ConfigurationManager.AppSettings["ReportPath"] + "LabReportGroupPrint.rpt");
                cryRpt.SetDataSource(ds1);
                cryRpt.Refresh();
                cryRpt.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, System.Configuration.ConfigurationManager.AppSettings["ReportLocation"] + TestOrderID.ToString() + ".pdf");

                //return System.Configuration.ConfigurationManager.AppSettings["ReportLocation"]  + TestOrderID.ToString() + ".pdf";
                return "Success";
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return "Failure " + message;
                //return "Failure";
            }

        }


        //https://localhost:44351/api/reportapi/Radiology?TestOrderID=1424957&UHID=PFBS.0000397737&isExternal=true
        //https://localhost:44351/api/reportapi/Radiology?TestOrderID=1424957&TestOrderItemID=5517950&UHID=PFBS.0000397737&isExternal=true
        //https://localhost:44351/api/reportapi/Radiology?TestOrderID=410658&TestOrderItemID=1398600&UHID=PFBS.0000037559&isExternal=true
        [Route("api/reportapi/Radiology")]
        public string GetRadiology(string TestOrderID, string TestOrderItemID, string UHID)
        {
            string strPath = String.Empty;


            DataSet ds = new DataSet();
            DataSet dsResult = new DataSet();
            DataSet dsResultNew = new DataSet();
            DataSet dsHeader = new DataSet();
            DataSet ds1 = CreateDataSetForRadiology();

            try
            {
                string connectionstring = System.Configuration.ConfigurationManager.AppSettings["ConnectionString"];
                using (SqlConnection con = new SqlConnection(connectionstring))
                {

                    using (SqlCommand cmd = new SqlCommand("Pr_FetchVerifiedRadioLogyOrderDetails_MAPI", con))
                    {
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                        sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@UHID", UHID);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@TestOrderId", TestOrderID);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@TestOrderItemId", TestOrderItemID);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@HospitalID", 1);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@Type", 0);
                        sqlDataAdapter.Fill(dsResultNew);
                        con.Close();
                    }

                    if (dsResultNew.Tables[0].Rows.Count == 0)
                    {
                        return "Failure";
                    }

                    using (SqlCommand cmd = new SqlCommand("PR_FetchRadiologyResults_MAPI", con))
                    {
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                        sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@TestOrderID", TestOrderID);
                        //1358599
                        //1463197
                        //1473739
                        //1447060
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@TestOrderItemID", TestOrderItemID);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@TestID", DBNull.Value);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@PatientID", DBNull.Value);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@PatientType", DBNull.Value);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@TBL", 0);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@UserID", 702);
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@WorkStationID", 30);
                        SqlParameter @out = cmd.Parameters.AddWithValue("@Error", String.Empty);
                        @out.Direction = ParameterDirection.Output;
                        sqlDataAdapter.Fill(ds);
                        con.Close();
                        //con.Open();
                        //var DataReader = cmd.ExecuteReader();
                    }



                    
                }

                bool isInsert = false;
                foreach (DataRow IndivRow in ds.Tables[2].Rows)
                {
                    if (!isInsert)
                    {
                        DataRow DoctorData = ds1.Tables[0].NewRow();
                        DoctorData["DoctorID1"] = ParseToInt(IndivRow["DoctorID"].ToString());
                        DoctorData["Doctor1"] = IndivRow["DoctorName"].ToString();
                        DoctorData["Signature1"] = IndivRow["SignaturePath"].ToString();
                        DoctorData["Sign1"] = false;
                        DoctorData["TestOrderItemID"] = IndivRow["TestOrderItemId"].ToString();
                        DoctorData["DoctorNameWithDesignation"] = IndivRow["DoctorName2l"].ToString();
                        ds1.Tables[0].Rows.Add(DoctorData);
                        isInsert = true;
                    }
                }



                DataRow PatientRow = ds1.Tables[1].NewRow();

                if (dsResultNew.Tables[0].Rows.Count >= 1)
                {
                    DataRow Patient = dsResultNew.Tables[0].Rows[0];

                    //PatientRow["Department"] = Patient["Specialisation"].ToString(); // Pr_FetchTestOrderDetails_MAPI[0].
                    PatientRow["OrderNo"] = Patient["OrderSlNo"].ToString();
                    PatientRow["IPNo"] = Patient["IpNo"].ToString();
                    PatientRow["UHID"] = Patient["RegCode"].ToString(); //CustomerRow["UHID"] = "1";
                    PatientRow["BillNo"] = Patient["BillNo"].ToString();
                    PatientRow["BillRemarks"] = Patient["BillRemarks"].ToString();
                    PatientRow["Name"] = Patient["PatientName"].ToString();
                    PatientRow["AgeGender"] = Patient["AgeGender"].ToString();
                    PatientRow["OrderDate"] = Patient["OrderDate"].ToString();
                    //CustomerRow["IPBillNoColName"] = "1";
                    //ReportDate	Pr_FetchTestResults_MAPI[1].ResultEnteredAt	minimum value from the tablet
                    PatientRow["PatientID"] = ParseToInt(Patient["PatientId"].ToString());
                    PatientRow["PatPhone"] = Patient["PatPhone"].ToString();
                    PatientRow["DocPhone"] = Patient["DocPhone"].ToString();
                    PatientRow["Nationality"] = Patient["Nationality"].ToString();
                    PatientRow["WardCaption"] = Patient["WardCaption"].ToString();
                    PatientRow["Ward"] = Patient["Ward"].ToString();
                    PatientRow["Doctor"] = Patient["DoctorName"].ToString();
                    PatientRow["PayerName"] = Patient["Payername"].ToString();
                    PatientRow["GradeName"] = Patient["GradeName"].ToString();
                    PatientRow["MRNO"] = Patient["RegCode"].ToString();
                    PatientRow["EmpID"] = Patient["EmpId"].ToString();

                    PatientRow["CompanyCode"] = Patient["CompanyCode"].ToString();
                    PatientRow["CompanyType"] = Patient["CompanyType"].ToString();
                    PatientRow["PatientType"] = 1;

                }



                if (ds.Tables[1] != null && ds.Tables[1].Rows.Count >= 1)
                {
                    PatientRow["ReportDate"] = ds.Tables[1].Rows[0]["ResultEnteredAt"].ToString();
                    PatientRow["ResultEntryDate"] = ds.Tables[1].Rows[0]["ResultEnteredAt"].ToString();
                    PatientRow["CollectionDate"] = ds.Tables[1].Rows[0]["SampleCollectedAt"].ToString();
                    PatientRow["AcknowledgeDate"] = ds.Tables[1].Rows[0]["SampleAckAT"].ToString();
                    PatientRow["Remarks"] = ds.Tables[1].Rows[0]["Remarks"].ToString();
                    PatientRow["AccessionNumber"] = ds.Tables[1].Rows[0]["SampleNumber"].ToString();
                    PatientRow["ResultEnteredByID"] = ds.Tables[1].Rows[0]["ResultEnteredbyEmpid"].ToString();
                    PatientRow["ResultEnteredBy"] = ds.Tables[1].Rows[0]["ResultEnteredBy"].ToString();
                    PatientRow["ResultVerifiedBy"] = ds.Tables[1].Rows[0]["ResultVerifyedBy"].ToString();
                }

                foreach (DataRow IndivRow in ds.Tables[0].Rows)
                {
                    DataRow ProcReport = ds1.Tables[2].NewRow();

                    ProcReport["ProcID"] = ParseToInt(IndivRow["TestOrderItemID"].ToString());
                    ProcReport["ProcName"] = IndivRow["TestName"].ToString();
                    ProcReport["ParamID"] = ParseToInt(IndivRow["ParameterID"].ToString());
                    ProcReport["ParamName"] = IndivRow["ParameterName"].ToString();
                    ProcReport["F1Unit"] = IndivRow["UOM"].ToString();
                    
                    PatientRow["Department"] = IndivRow["Specialisation"].ToString();

                    if (IndivRow["Value"].ToString().ToString().Trim().Length > 0)
                    {
                        ProcReport["F1Result"] = "htmlform"; 
                        ProcReport["F2ParamDesc"] = IndivRow["Value"].ToString();
                        ProcReport["FormatType"] = "F2"; 
                    }

                    ds1.Tables[2].Rows.Add(ProcReport);
                }
                ds1.Tables[1].Rows.Add(PatientRow);

                ReportDocument cryRpt = new ReportDocument();
                cryRpt.Load(System.Configuration.ConfigurationManager.AppSettings["ReportPath"] + "ResultEntryView.rpt");
                cryRpt.SetDataSource(ds1);
                cryRpt.Refresh();
                cryRpt.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, System.Configuration.ConfigurationManager.AppSettings["ReportLocation"] + TestOrderID.ToString() + ".pdf");
                return "Success";

            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return "Failure " + message;
                //return "Failure";
            }
        }

        //https://localhost:44351/api/reportapi/PrescriptionNew?AdmissionId=2449626
        [Route("api/reportapi/PrescriptionNew")]
        public string GetPrescriptionNew(int AdmissionId)
        {
            DataSet ds1 = CreateDatasetForPrescriptionNew();

            DataSet dsResultNew = new DataSet();

            try
            {
                string connectionstring = System.Configuration.ConfigurationManager.AppSettings["ConnectionString"];
                using (SqlConnection con = new SqlConnection(connectionstring))
                {

                    using (SqlCommand cmd = new SqlCommand("pr_FetchPatientDetailsForPrescription_MAPI", con))
                    {
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                        sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                        sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@AdmissionId", AdmissionId);
                        sqlDataAdapter.Fill(dsResultNew);
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                return "Failure :" + ex.Message;
            }

            if (dsResultNew.Tables[12].Rows.Count >= 1)
            {
                foreach (DataRow dr in dsResultNew.Tables[12].Rows)
                {
                    DataRow AdviceRow = ds1.Tables[0].NewRow();

                    AdviceRow["TBL"] = dr["TBL"].ToString();
                    AdviceRow["MonitorID"] = ParseToInt(dr["MonitorID"].ToString());
                    AdviceRow["MonitorDate"] = ParseToDate(dr["MonitorDate"].ToString());
                    AdviceRow["PatientType"] = ParseToInt(dr["PatientType"].ToString());
                    AdviceRow["FollowUpType"] = ParseToInt(dr["FollowUpType"].ToString());
                    AdviceRow["Advice"] = dr["Advice"].ToString();
                    AdviceRow["ReferralOrderID"] = ParseToInt(dr["ReferralOrderID"].ToString());
                    AdviceRow["FollowAfter"] = ParseToInt(dr["FollowAfter"].ToString());
                    AdviceRow["FollowUpOn"] = ParseToDate(dr["FollowUpOn"].ToString());
                    AdviceRow["IsInternalReferral"] = false;
                    AdviceRow["RefDoctorID"] = ParseToInt(dr["RefDoctorID"].ToString());
                    AdviceRow["PatientID"] = ParseToInt(dr["PatientID"].ToString());
                    AdviceRow["AdmissionID"] = ParseToInt(dr["AdmissionID"].ToString());
                    AdviceRow["AdmissionNumber"] = dr["AdmissionNumber"].ToString();
                    AdviceRow["RefDoctorName"] = dr["RefDoctorName"].ToString();
                    AdviceRow["ReasonForAdm"] = dr["ReasonForAdm"].ToString();
                    AdviceRow["UserName"] = dr["UserName"].ToString();
                    AdviceRow["CreateDate"] = ParseToDate(dr["CreateDate"].ToString());
                    AdviceRow["MODDATE"] = ParseToDate(dr["MODDATE"].ToString());
                    AdviceRow["ProcedureId"] = ParseToInt(dr["ProcedureId"].ToString());
                    AdviceRow["ProcedureName"] = dr["ProcedureName"].ToString();
                    AdviceRow["USERID"] = ParseToInt(dr["USERID"].ToString());
                    AdviceRow["LengthOfStay"] = ParseToInt(dr["LengthOfStay"].ToString());
                    AdviceRow["Adviceid"] = ParseToInt(dr["Adviceid"].ToString());
                    AdviceRow["TreatmentPlanID"] = ParseToInt(dr["TreatmentPlanID"].ToString());
                    AdviceRow["TreatmentPlanName"] = dr["TreatmentPlanName"].ToString();
                    AdviceRow["IsPfVisible"] = ParseToInt(dr["IsPfVisible"].ToString());
                    AdviceRow["DietTypeID"] = ParseToInt(dr["DietTypeID"].ToString());
                    AdviceRow["DietType"] = dr["DietType"].ToString();
                    AdviceRow["IPID"] = ParseToInt(dr["IPID"].ToString());
                    AdviceRow["DoctorID"] = ParseToInt(dr["DoctorID"].ToString());
                    AdviceRow["IsSpecialityTreatmentApplicable"] = false;

                    ds1.Tables[0].Rows.Add(AdviceRow);
                }
            }

            if (dsResultNew.Tables[4].Rows.Count >= 1)
            {
                foreach (DataRow dr in dsResultNew.Tables[4].Rows)
                {
                    DataRow DiagonsisRow = ds1.Tables[1].NewRow();

                    DiagonsisRow["TBL"] = dr["TBL"].ToString();
                    DiagonsisRow["MonitorID"] = ParseToInt(dr["MonitorID"].ToString());
                    DiagonsisRow["MonitorDate"] = dr["MonitorDate"].ToString();
                    DiagonsisRow["DiseaseID"] = ParseToInt(dr["DiseaseID"].ToString());
                    DiagonsisRow["DiagnosisType"] = ParseToInt(dr["DiagnosisType"].ToString());
                    DiagonsisRow["DiseaseName"] = dr["DiseaseName"].ToString();
                    DiagonsisRow["Code"] = dr["Code"].ToString();
                    DiagonsisRow["PatientID"] = ParseToInt(dr["PatientID"].ToString());
                    DiagonsisRow["AdmissionID"] = ParseToInt(dr["AdmissionID"].ToString());
                    DiagonsisRow["CreateDate"] = dr["CreateDate"].ToString();
                    DiagonsisRow["MODDATE"] = dr["MODDATE"].ToString();
                    DiagonsisRow["ProblemPointID"] = ParseToInt(dr["ProblemPointID"].ToString());
                    DiagonsisRow["ProblemPointName"] = dr["ProblemPointName"].ToString();
                    DiagonsisRow["PointValue"] = ParseToInt(dr["PointValue"].ToString());
                    DiagonsisRow["Username"] = dr["Username"].ToString();
                    DiagonsisRow["AssessmentID"] = ParseToInt(dr["AssessmentID"].ToString());
                    DiagonsisRow["userid"] = ParseToInt(dr["userid"].ToString());
                    DiagonsisRow["DiagonosisTypeID"] = ParseToInt(dr["DiagonosisTypeID"].ToString());
                    DiagonsisRow["IsPsd"] = ParseToInt(dr["IsPsd"].ToString());
                    DiagonsisRow["Blocked"] = ParseToInt(dr["Blocked"].ToString());
                    DiagonsisRow["DiagonosisType"] = dr["DiagonosisType"].ToString();
                    DiagonsisRow["IsPfVisible"] = ParseToInt(dr["IsPfVisible"].ToString());
                    DiagonsisRow["SignificantData"] = false;
                    DiagonsisRow["Remarks"] = dr["Remarks"].ToString();
                    DiagonsisRow["IsAdmitDisease"] = false;

                    ds1.Tables[1].Rows.Add(DiagonsisRow);
                }
            }

            if (dsResultNew.Tables[9].Rows.Count >= 1)
            {
                foreach (DataRow dr in dsResultNew.Tables[9].Rows)
                {
                    DataRow DrugAllergiesRow = ds1.Tables[2].NewRow();

                    DrugAllergiesRow["TBL"] = dr["TBL"].ToString();
                    DrugAllergiesRow["GenericID"] = ParseToInt(dr["GenericID"].ToString());
                    DrugAllergiesRow["Remark"] = dr["Remark"].ToString();
                    DrugAllergiesRow["GenericName"] = dr["GenericName"].ToString();
                    DrugAllergiesRow["GenericName2L"] = dr["GenericName2L"].ToString();
                    DrugAllergiesRow["FROMdate"] = ParseToDate(dr["FROMdate"].ToString());
                    DrugAllergiesRow["todate"] = ParseToDate(dr["todate"].ToString());
                    DrugAllergiesRow["Description"] = dr["Description"].ToString();
                    DrugAllergiesRow["IsNotActive"] = false;
                    DrugAllergiesRow["CreateDate"] = ParseToDate(dr["CreateDate"].ToString());
                    DrugAllergiesRow["IPID"] = ParseToInt(dr["IPID"].ToString());
                    DrugAllergiesRow["AllergieTypes"] = dr["AllergieTypes"].ToString();
                    DrugAllergiesRow["Doctorid"] = ParseToInt(dr["Doctorid"].ToString());
                    DrugAllergiesRow["IsPfVisible"] = false;
                    DrugAllergiesRow["Status"] = ParseToInt(dr["Status"].ToString());

                    ds1.Tables[2].Rows.Add(DrugAllergiesRow);
                }
            }
            if (dsResultNew.Tables[6].Rows.Count >= 1)
            {
                foreach (DataRow dr in dsResultNew.Tables[6].Rows)
                {

                    DataRow DurationOfIllenessRow = ds1.Tables[3].NewRow();

                    DurationOfIllenessRow["TBL"] = dr["TBL"].ToString();
                    DurationOfIllenessRow["EpisodeName"] = dr["EpisodeName"].ToString();
                    DurationOfIllenessRow["DurationOfIllness"] = dr["DurationOfIllness"].ToString();
                    DurationOfIllenessRow["Height"] = 1.0;
                    DurationOfIllenessRow["Weight"] = 1.0;
                    DurationOfIllenessRow["PainScoreID"] = ParseToInt(dr["PainScoreID"].ToString());
                    DurationOfIllenessRow["IsEducated"] = false;
                    DurationOfIllenessRow["IsSmoke"] = false;
                    DurationOfIllenessRow["IsPregnent"] = false;
                    DurationOfIllenessRow["ExpectedDeliveryDate"] = ParseToDate(dr["ExpectedDeliveryDate"].ToString());
                    DurationOfIllenessRow["IsPatientDrugAlleric"] = false;
                    DurationOfIllenessRow["CreateDate"] = dr["CreateDate"].ToString();
                    DurationOfIllenessRow["BodyMassID"] = ParseToInt(dr["BodyMassID"].ToString());
                    DurationOfIllenessRow["DoctorName"] = dr["DoctorName"].ToString();
                    DurationOfIllenessRow["PainScore"] = dr["PainScore"].ToString();
                    DurationOfIllenessRow["HeadCircumference"] = 1.0;
                    DurationOfIllenessRow["IsoldVisit"] = false;

                    ds1.Tables[3].Rows.Add(DurationOfIllenessRow);
                }
            }

            if (dsResultNew.Tables[2].Rows.Count >= 1)
            {
                foreach (DataRow dr in dsResultNew.Tables[2].Rows)
                {
                    DataRow InvistigationRow = ds1.Tables[4].NewRow();

                    InvistigationRow["TBL"] = dr["TBL"].ToString();
                    InvistigationRow["ServiceTypeID"] = ParseToInt(dr["ServiceTypeID"].ToString());
                    InvistigationRow["PrescriptionID"] = ParseToInt(dr["PrescriptionID"].ToString());
                    InvistigationRow["MonitorID"] = ParseToInt(dr["MonitorID"].ToString());
                    InvistigationRow["PrescriptionDate"] = dr["PrescriptionDate"].ToString();
                    InvistigationRow["DoctorID"] = ParseToInt(dr["DoctorID"].ToString());
                    InvistigationRow["Doctorname"] = dr["Doctorname"].ToString(); 
                    InvistigationRow["ItemSequence"] = ParseToInt(dr["ItemSequence"].ToString());
                    InvistigationRow["ItemID"] = ParseToInt(dr["ItemID"].ToString());
                    InvistigationRow["ItemName"] = dr["ItemName"].ToString();
                    InvistigationRow["Dose"] = ParseToInt(dr["Dose"].ToString());
                    InvistigationRow["DoseID"] = ParseToInt(dr["DoseID"].ToString());
                    InvistigationRow["DoseUoM"] = ParseToInt(dr["DoseUoM"].ToString());
                    InvistigationRow["FrequencyID"] = ParseToInt(dr["FrequencyID"].ToString());
                    InvistigationRow["Frequency"] = ParseToInt(dr["Frequency"].ToString());
                    InvistigationRow["Duration"] = ParseToInt(dr["Duration"].ToString());
                    InvistigationRow["DurationID"] = ParseToInt(dr["DurationID"].ToString());
                    InvistigationRow["DurationUOM"] = ParseToInt(dr["DurationUOM"].ToString());
                    InvistigationRow["StartFrom"] = ParseToInt(dr["StartFrom"].ToString());
                    InvistigationRow["Remarks"] = dr["Remarks"].ToString(); 
                    InvistigationRow["SpecimenID"] = ParseToInt(dr["SpecimenID"].ToString());
                    InvistigationRow["SpecimenName"] = dr["SpecimenName"].ToString();
                    InvistigationRow["Status"] = ParseToInt(dr["Status"].ToString());
                    InvistigationRow["Quantity"] = ParseToInt(dr["Quantity"].ToString());
                    InvistigationRow["UserName"] = dr["UserName"].ToString();
                    InvistigationRow["CreateDate"] = dr["CreateDate"].ToString();
                    InvistigationRow["MODDATE"] = dr["MODDATE"].ToString();
                    InvistigationRow["UCAFApproval"] = false;
                    InvistigationRow["USERID"] = ParseToInt(dr["USERID"].ToString());
                    InvistigationRow["Itemstatus"] = ParseToInt(dr["Itemstatus"].ToString());
                    InvistigationRow["admissionid"] = ParseToInt(dr["admissionid"].ToString());
                    InvistigationRow["MonitorDate"] = dr["MonitorDate"].ToString();
                    InvistigationRow["TestOrderItemID"] = ParseToInt(dr["TestOrderItemID"].ToString());
                    InvistigationRow["TestOrderID"] = ParseToInt(dr["TestOrderID"].ToString());
                    InvistigationRow["SpecialiseID"] = ParseToInt(dr["SpecialiseID"].ToString());
                    InvistigationRow["Specialisation"] = dr["Specialisation"].ToString();
                    InvistigationRow["ResultStatus"] = ParseToInt(dr["ResultStatus"].ToString());
                    InvistigationRow["ResultStatusName"] = dr["ResultStatusName"].ToString();
                    InvistigationRow["IsPfVisible"] = false;
                    InvistigationRow["ItemCode"] = dr["ItemCode"].ToString();

                    ds1.Tables[4].Rows.Add(InvistigationRow);
    
                }
            }


            
            if (dsResultNew.Tables[1].Rows.Count >= 1)
            {
                foreach (DataRow dr in dsResultNew.Tables[1].Rows)
                {
                    DataRow MedicationDataRow = ds1.Tables[5].NewRow();

                    MedicationDataRow["TBL"] = dr["TBL"].ToString();
                    MedicationDataRow["PrescriptionID"] = ParseToInt(dr["PrescriptionID"].ToString());
                    MedicationDataRow["MonitorID"] = ParseToInt(dr["MonitorID"].ToString());
                    MedicationDataRow["DoctorID"] = ParseToInt(dr["DoctorID"].ToString());
                    MedicationDataRow["DoctorName"] = dr["DoctorName"].ToString();
                    MedicationDataRow["ItemSequence"] = ParseToInt(dr["ItemSequence"].ToString());
                    MedicationDataRow["ItemID"] = ParseToInt(dr["ItemID"].ToString());
                    MedicationDataRow["ItemName"] = dr["ItemName"].ToString();
                    MedicationDataRow["Dose"] = 1.0;
                    MedicationDataRow["DoseID"] = ParseToInt(dr["DoseID"].ToString());
                    MedicationDataRow["DoseUoM"] = dr["DoseUoM"].ToString();
                    MedicationDataRow["FrequencyID"] = ParseToInt(dr["FrequencyID"].ToString());
                    MedicationDataRow["Frequency"] = dr["Frequency"].ToString();

                    MedicationDataRow["Duration"] = ParseToInt(dr["Duration"].ToString());
                    MedicationDataRow["DurationID"] = ParseToInt(dr["DurationID"].ToString());
                    MedicationDataRow["DurationUOM"] = dr["DurationUOM"].ToString();
                    MedicationDataRow["StartFrom"] = dr["StartFrom"].ToString();
                    MedicationDataRow["Remarks"] = dr["Remarks"].ToString();
                    MedicationDataRow["SpecimenID"] = ParseToInt(dr["FrequencyID"].ToString());
                    MedicationDataRow["SpecimenName"] = dr["FrequencyID"].ToString();
                    MedicationDataRow["Status"] = ParseToInt(dr["Status"].ToString());
                    MedicationDataRow["AdmRouteID"] = ParseToInt(dr["AdmRouteID"].ToString());
                    MedicationDataRow["AdmRoute"] = dr["AdmRoute"].ToString();
                    MedicationDataRow["UserName"] = dr["UserName"].ToString();
                    MedicationDataRow["CreateDate"] = dr["CreateDate"].ToString();
                    MedicationDataRow["ModDate"] = dr["ModDate"].ToString();
                    MedicationDataRow["blocked"] = ParseToInt(dr["blocked"].ToString());
                    MedicationDataRow["CustomizedFlag"] = false;
                    MedicationDataRow["CustDrugDetails"] = dr["CustDrugDetails"].ToString();
                    MedicationDataRow["FrequencyQTY"] = 1.0;
                    MedicationDataRow["IssueUOMValue"] = ParseToInt(dr["IssueUOMValue"].ToString());
                    MedicationDataRow["IsNarcotic"] = ParseToInt(dr["IsNarcotic"].ToString());
                    MedicationDataRow["IsAntibiotic"] = false;
                    MedicationDataRow["IsControledDrug"] = false;
                    MedicationDataRow["IsDisPrescription"] = false;
                    MedicationDataRow["UCAFApproval"] = false;
                    MedicationDataRow["PrescriptionNo"] = dr["PrescriptionNo"].ToString();
                    MedicationDataRow["ENDDatetime"] = DateTime.Now;
                    MedicationDataRow["ENDDate"] = DateTime.Now;
                    MedicationDataRow["MonitorDate"] = DateTime.Now;
                    MedicationDataRow["IsPfvisible"] = false;
                    MedicationDataRow["OrderStatus"] = ParseToInt(dr["OrderStatus"].ToString());
                    MedicationDataRow["PrescriptionStatusID"] = false;

                    ds1.Tables[5].Rows.Add(MedicationDataRow);
                }
            }

            

            if (dsResultNew.Tables[0].Rows.Count >= 1)
            {
                foreach (DataRow dr in dsResultNew.Tables[0].Rows)
                {
                    DataRow PatientDataRow = ds1.Tables[6].NewRow();

                    PatientDataRow["RegCode"] = dr["RegCode"].ToString();
                    PatientDataRow["PatientName"] = dr["PatientName"].ToString();
                    PatientDataRow["Gender"] = dr["Gender"].ToString();
                    PatientDataRow["MobileNo"] = dr["MobileNo"].ToString();
                    PatientDataRow["HospitalName"] = dr["HospitalName"].ToString();
                    PatientDataRow["FullAge"] = dr["FullAge"].ToString();
                    PatientDataRow["Nationality"] = dr["Nationality"].ToString();
                    PatientDataRow["FamilyHeadId"] = dr["FamilyHeadId"].ToString();
                    PatientDataRow["IsContrastAllergic"] = false;
                    PatientDataRow["DoctorName"] = dr["DoctorName"].ToString();
                    PatientDataRow["Admitdate"] = dr["Admitdate"].ToString(); 
                    PatientDataRow["CompanyName"] = dr["CompanyName"].ToString();
                    PatientDataRow["DocSpecDepartment"] = dr["DocSpecDepartment"].ToString();
                    PatientDataRow["DocSign"] = dr["DocSign"].ToString();
                    PatientDataRow["DocCode"] = dr["DoctorCode"].ToString();

                    ds1.Tables[6].Rows.Add(PatientDataRow);

                }
            }

            if (dsResultNew.Tables[3].Rows.Count >= 1)
            {
                foreach (DataRow dr in dsResultNew.Tables[3].Rows)
                {

                    DataRow ProcedureRow = ds1.Tables[7].NewRow();

                    ProcedureRow["Tbl"] = dr["Tbl"].ToString();
                    ProcedureRow["ServiceTypeID"] = ParseToInt(dr["ServiceTypeID"].ToString());
                    ProcedureRow["PrescriptionID"] = ParseToInt(dr["PrescriptionID"].ToString());
                    ProcedureRow["MonitorID"] = ParseToInt(dr["MonitorID"].ToString());
                    ProcedureRow["PrescriptionDate"] = dr["PrescriptionDate"].ToString();
                    ProcedureRow["DoctorID"] = ParseToInt(dr["DoctorID"].ToString());
                    ProcedureRow["Doctorname"] = dr["Doctorname"].ToString();
                    ProcedureRow["ItemSequence"] = ParseToInt(dr["ItemSequence"].ToString());
                    ProcedureRow["ItemID"] = ParseToInt(dr["ItemID"].ToString());
                    ProcedureRow["ItemName"] = dr["ItemName"].ToString();
                    ProcedureRow["Dose"] = ParseToInt(dr["Dose"].ToString());
                    ProcedureRow["DoseID"] = ParseToInt(dr["DoseID"].ToString());
                    ProcedureRow["DoseUoM"] = ParseToInt(dr["DoseUoM"].ToString());
                    ProcedureRow["FrequencyID"] = ParseToInt(dr["FrequencyID"].ToString());
                    ProcedureRow["Frequency"] = ParseToInt(dr["Frequency"].ToString());
                    ProcedureRow["Duration"] = ParseToInt(dr["Duration"].ToString());
                    ProcedureRow["DurationID"] = ParseToInt(dr["DurationID"].ToString());
                    ProcedureRow["DurationUOM"] = ParseToInt(dr["DurationUOM"].ToString()); 
                    ProcedureRow["StartFrom"] = ParseToInt(dr["StartFrom"].ToString());
                    ProcedureRow["Remarks"] = dr["Remarks"].ToString();
                    ProcedureRow["SpecimenID"] = ParseToInt(dr["SpecimenID"].ToString());
                    ProcedureRow["SpecimenName"] = dr["SpecimenName"].ToString();
                    ProcedureRow["Status"] = ParseToInt(dr["Status"].ToString());
                    ProcedureRow["Quantity"] = ParseToInt(dr["Quantity"].ToString());
                    ProcedureRow["UserName"] = dr["UserName"].ToString();
                    ProcedureRow["CreateDate"] = dr["CreateDate"].ToString();
                    ProcedureRow["MODDATE"] = dr["MODDATE"].ToString();
                    ProcedureRow["UCAFApproval"] = false;
                    ProcedureRow["USERID"] = ParseToInt(dr["USERID"].ToString());
                    ProcedureRow["SpecialiseID"] = ParseToInt(dr["SpecialiseID"].ToString());
                    ProcedureRow["Specialisation"] = dr["Specialisation"].ToString();
                    ProcedureRow["WorkstationName"] = dr["WorkstationName"].ToString();
                    ProcedureRow["MonitorDate"] = dr["MonitorDate"].ToString();
                    ProcedureRow["ItemStatus"] = ParseToInt(dr["ItemStatus"].ToString());
                    ProcedureRow["TestOrderItemID"] = ParseToInt(dr["TestOrderItemID"].ToString());
                    ProcedureRow["TestOrderID"] = ParseToInt(dr["TestOrderID"].ToString());
                    ProcedureRow["ResultStatus"] = ParseToInt(dr["ResultStatus"].ToString());
                    ProcedureRow["ResultStatusName"] = dr["ResultStatusName"].ToString();
                    ProcedureRow["IsPfVisible"] = false;
                    ProcedureRow["ItemCode"] = dr["ItemCode"].ToString();

                    ds1.Tables[7].Rows.Add(ProcedureRow);
                }
            }

            if (dsResultNew.Tables[10].Rows.Count >= 1)
            {
                foreach (DataRow dr in dsResultNew.Tables[10].Rows)
                {
                    DataRow SurgeryRow = ds1.Tables[8].NewRow();

                    SurgeryRow["TBL"] = dr["TBL"].ToString();
                    SurgeryRow["ProcedureID"] = ParseToInt(dr["ProcedureID"].ToString());
                    SurgeryRow["ProcedureName"] = dr["ProcedureName"].ToString();

                    ds1.Tables[8].Rows.Add(SurgeryRow);
                }
            }

            
            ds1.Tables[9].Rows.Add(ds1.Tables[9].NewRow());

            if (dsResultNew.Tables[5].Rows.Count >= 1)
            {
                foreach (DataRow dr in dsResultNew.Tables[5].Rows)
                {

                    DataRow VitalsRow = ds1.Tables[10].NewRow();

                    VitalsRow["TBL"] = dr["TBL"].ToString();
                    VitalsRow["Value"] = dr["Value"].ToString();
                    VitalsRow["Datetime"] = DateTime.Now;
                    VitalsRow["Vital"] = dr["Vital"].ToString();
                    VitalsRow["UOM"] = dr["UOM"].ToString();
                    VitalsRow["MINVALUE"] = 1.0;
                    VitalsRow["MAXVALUE"] = 1.0;
                    VitalsRow["AssessmentID"] = ParseToInt(dr["AssessmentID"].ToString());
                    VitalsRow["UserName"] = "1";
                    VitalsRow["USERID"] = ParseToInt(dr["USERID"].ToString());
                    VitalsRow["IsPsd"] = ParseToInt(dr["IsPsd"].ToString());
                    VitalsRow["DoctorID"] = ParseToInt(dr["DoctorID"].ToString());
                    VitalsRow["MonitorID"] = ParseToInt(dr["MonitorID"].ToString());
                    VitalsRow["MonitorDate"] = DateTime.Now;
                    VitalsRow["Comments"] = dr["Comments"].ToString();
                    VitalsRow["ArchiveModdate"] = DateTime.Now;
                    VitalsRow["Status"] = ParseToInt(dr["Status"].ToString());
                    VitalsRow["IsPfVisible"] = ParseToInt(dr["IsPfVisible"].ToString());

                    ds1.Tables[10].Rows.Add(VitalsRow);

                }
            }
            ds1.Tables[11].Rows.Add(ds1.Tables[11].NewRow());
            if (dsResultNew.Tables[8].Rows.Count >= 1)
            {
                foreach (DataRow dr in dsResultNew.Tables[8].Rows)
                {
                    DataRow FoodAllergiesRow = ds1.Tables[12].NewRow();

                    FoodAllergiesRow["TBL"] = dr["TBL"].ToString();
                    FoodAllergiesRow["FoodID"] = ParseToInt(dr["IsPsd"].ToString());
                    FoodAllergiesRow["Remark"] = dr["Remark"].ToString();
                    FoodAllergiesRow["FdIngrName"] = dr["Remark"].ToString();
                    FoodAllergiesRow["FdIngrName2L"] = dr["Remark"].ToString();
                    FoodAllergiesRow["FROMdate"] = ParseToDate(dr["FROMdate"].ToString());
                    FoodAllergiesRow["todate"] = ParseToDate(dr["todate"].ToString());
                    FoodAllergiesRow["Description"] = dr["Description"].ToString();
                    FoodAllergiesRow["IsNotActive"] = false;
                    FoodAllergiesRow["CreateDate"] = ParseToDate(dr["CreateDate"].ToString());
                    FoodAllergiesRow["IPID"] = ParseToInt(dr["IPID"].ToString());
                    FoodAllergiesRow["IsPfVisible"] = false;
                    FoodAllergiesRow["AllergieTypes"] = dr["AllergieTypes"].ToString();
                    FoodAllergiesRow["Doctorid"] = ParseToInt(dr["Doctorid"].ToString());
                    FoodAllergiesRow["Status"] = ParseToInt(dr["Status"].ToString());

                    ds1.Tables[12].Rows.Add(FoodAllergiesRow);
                }
            }


            

            if (dsResultNew.Tables[7].Rows.Count >= 1)
            {
                foreach (DataRow dr in dsResultNew.Tables[7].Rows)
                {
                    DataRow OtherAllergiesRow = ds1.Tables[13].NewRow();

                    OtherAllergiesRow["TBL"] = dr["TBL"].ToString();
                    OtherAllergiesRow["ID"] = ParseToInt(dr["ID"].ToString());
                    OtherAllergiesRow["Allergy"] = dr["Allergy"].ToString();
                    OtherAllergiesRow["Allergy2L"] = dr["Allergy2L"].ToString();
                    OtherAllergiesRow["FROMdate"] = ParseToDate(dr["FROMdate"].ToString());
                    OtherAllergiesRow["todate"] = ParseToDate(dr["todate"].ToString());
                    OtherAllergiesRow["Description"] = dr["Description"].ToString();
                    OtherAllergiesRow["IsNotActive"] = false;
                    OtherAllergiesRow["CreateDate"] = dr["CreateDate"].ToString();
                    OtherAllergiesRow["IPID"] = ParseToInt(dr["IPID"].ToString());
                    OtherAllergiesRow["AllergieTypes"] = dr["AllergieTypes"].ToString();
                    OtherAllergiesRow["Doctorid"] = ParseToInt(dr["Doctorid"].ToString());
                    OtherAllergiesRow["IsPfVisible"] = false;
                    OtherAllergiesRow["Status"] = ParseToInt(dr["Doctorid"].ToString());

                    ds1.Tables[13].Rows.Add(OtherAllergiesRow);
                }
            }

            if (dsResultNew.Tables[11].Rows.Count >= 1)
            {
                foreach (DataRow dr in dsResultNew.Tables[11].Rows)
                {
                    DataRow ReferralRow = ds1.Tables[14].NewRow();

                    ReferralRow["TBL"] = dr["TBL"].ToString();
                    ReferralRow["ReferralOrderID"] = ParseToInt(dr["ReferralOrderID"].ToString());
                    ReferralRow["ReferralTypeID"] = ParseToInt(dr["ReferralTypeID"].ToString());
                    ReferralRow["ReferralType"] = dr["ReferralType"].ToString();
                    ReferralRow["SpecialiseID"] = ParseToInt(dr["SpecialiseID"].ToString());
                    ReferralRow["Specialisation"] = dr["Specialisation"].ToString();
                    ReferralRow["FromDoctorID"] = ParseToInt(dr["FromDoctorID"].ToString());
                    ReferralRow["Priority"] = ParseToInt(dr["Priority"].ToString());
                    ReferralRow["Remarks"] = dr["Remarks"].ToString();
                    ReferralRow["DoctorID"] = ParseToInt(dr["DoctorID"].ToString());
                    ReferralRow["DoctornameName"] = dr["DoctornameName"].ToString();
                    ReferralRow["AdmissionID"] = ParseToInt(dr["AdmissionID"].ToString());
                    ReferralRow["MonitorID"] = ParseToInt(dr["MonitorID"].ToString());
                    ReferralRow["HospitalID"] = ParseToInt(dr["HospitalID"].ToString());
                    ReferralRow["IsDirect"] = false;
                    ReferralRow["ExReferDoctorID"] = ParseToInt(dr["ExReferDoctorID"].ToString());
                    ReferralRow["ExReferDoctor"] = dr["ExReferDoctor"].ToString();
                    ReferralRow["adviceid"] = ParseToInt(dr["adviceid"].ToString());
                    ReferralRow["Feedback"] = dr["Feedback"].ToString();
                    ReferralRow["refusalremarks"] = dr["refusalremarks"].ToString();
                    ReferralRow["Reasonid"] = ParseToInt(dr["Reasonid"].ToString());
                    ReferralRow["Reason"] = dr["Reason"].ToString();
                    ReferralRow["duration"] = ParseToInt(dr["duration"].ToString());
                    ReferralRow["IsInternalReferral"] = false;
                    ReferralRow["Blocked"] = ParseToInt(dr["Blocked"].ToString());
                    ReferralRow["Status"] = ParseToInt(dr["Status"].ToString());
                    ReferralRow["MonitorDate"] = ParseToDate(dr["Status"].ToString());
                    ReferralRow["IPID"] = ParseToInt(dr["IPID"].ToString());
                    ReferralRow["DoctorID1"] = ParseToInt(dr["DoctorID1"].ToString());

                    ds1.Tables[14].Rows.Add(ReferralRow);
                }
            }

            ReportDocument cryRpt = new ReportDocument();
            cryRpt.Load(System.Configuration.ConfigurationManager.AppSettings["ReportPath"] + "CaseRecord.rpt");
            cryRpt.SetDataSource(ds1);
            cryRpt.Refresh();
            cryRpt.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, System.Configuration.ConfigurationManager.AppSettings["ReportLocation"] + AdmissionId.ToString() + ".pdf");
            return "Success";

        }



        //https://localhost:44351/api/reportapi/PrescriptionBegin?AdmissionId=2449626
        [Route("api/reportapi/PrescriptionBegin")]
        public string GetPrescriptionBegin(string AdmissionId)
        {
            DataSet ds1 = CreateDatasetForPrescriptionNew();


            DataRow AdviceRow = ds1.Tables[0].NewRow();
            ds1.Tables[0].Rows.Add(AdviceRow);

            DataRow DiagnosisRow = ds1.Tables[1].NewRow();
            ds1.Tables[1].Rows.Add(DiagnosisRow);

            DataRow DrugAllergiesRow = ds1.Tables[2].NewRow();
            ds1.Tables[2].Rows.Add(DrugAllergiesRow);

            DataRow DurationOfIllenessRow = ds1.Tables[3].NewRow();
            ds1.Tables[3].Rows.Add(DurationOfIllenessRow);

            DataRow InvestigationsRow = ds1.Tables[4].NewRow();
            ds1.Tables[4].Rows.Add(InvestigationsRow);

            DataRow MedicationDataRow = ds1.Tables[5].NewRow();
            ds1.Tables[5].Rows.Add(MedicationDataRow);

            DataRow PatientDataRow = ds1.Tables[6].NewRow();

            PatientDataRow["RegCode"] = "1";
            PatientDataRow["PatientName"] = "1";
            PatientDataRow["Gender"] = "1";
            PatientDataRow["MobileNo"] = "1";
            PatientDataRow["HospitalName"] = "1";
            PatientDataRow["FullAge"] = "1";
            PatientDataRow["Nationality"] = "1";
            // PatientDataRow["FamilyHeadId"] = "1";
            // PatientDataRow["IsContrastAllergic"] = false;
            PatientDataRow["DoctorName"] = "1";
            PatientDataRow["Admitdate"] = "1";
            PatientDataRow["CompanyName"] = "1";
            PatientDataRow["DocSpecDepartment"] = "1";
            PatientDataRow["DocSign"] = "1";
            PatientDataRow["DocCode"] = "1";

            ds1.Tables[6].Rows.Add(PatientDataRow);

            DataRow ProceduresRow = ds1.Tables[7].NewRow();
            ds1.Tables[7].Rows.Add(ProceduresRow);

            DataRow SurgeryRow = ds1.Tables[8].NewRow();
            ds1.Tables[8].Rows.Add(SurgeryRow);

            DataRow TemplateDataRow = ds1.Tables[9].NewRow();
            ds1.Tables[9].Rows.Add(TemplateDataRow);

            DataRow VitalsRow = ds1.Tables[10].NewRow();
            ds1.Tables[10].Rows.Add(VitalsRow);

            DataRow PateintBloodOrdersRow = ds1.Tables[11].NewRow();
            ds1.Tables[11].Rows.Add(PateintBloodOrdersRow);

            DataRow FoodAllergiesRow = ds1.Tables[12].NewRow();
            ds1.Tables[12].Rows.Add(FoodAllergiesRow);

            DataRow OtherAlleriesRow = ds1.Tables[13].NewRow();
            ds1.Tables[13].Rows.Add(OtherAlleriesRow);

            DataRow ReferalRow = ds1.Tables[14].NewRow();
            ds1.Tables[14].Rows.Add(ReferalRow);

            ReportDocument cryRpt = new ReportDocument();
            cryRpt.Load(System.Configuration.ConfigurationManager.AppSettings["ReportPath"] + "CaseRecord.rpt");
            cryRpt.SetDataSource(ds1);
            cryRpt.Refresh();
            cryRpt.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, System.Configuration.ConfigurationManager.AppSettings["ReportLocation"] + 1.ToString() + ".pdf");
            return "Success";
        }

        private string ReferenceRange(string strMinVal, string strMaxVal, bool blnUnits, string strUnit, bool blnPrefix, string strKeyword, string StrSIUnit)
        {
            try
            {
                string strRefRange = "";

                //if (strMinVal != "0" && strMinVal != "")//Commented by Sattiraju For Issue No: 78571
                if (strMinVal != "")
                {
                    strMinVal = strMinVal.Replace("0", " ").Trim().Replace(" ", "0").ToString().Trim();
                    strMinVal = strMinVal == "." || strMinVal == "" ? "0" : Convert.ToDecimal(strMinVal).ToString(); //Modified by Sattiraju For Issue No: 78571
                    //Added by anandm for BUG ID 85307

                    if (StrSIUnit == "1" || StrSIUnit == "2")
                    {
                        var result = strMinVal;
                        strMinVal = Math.Round(Double.Parse(result.ToString()), 1).ToString();
                    }

                    //End of Anandm
                }
                //if (strMaxVal != "0" && strMaxVal != "") //Commented by Sattiraju For Issue No: 78571
                if (strMaxVal != "")
                {
                    strMaxVal = strMaxVal.Replace("0", " ").Trim().Replace(" ", "0").ToString().Trim();
                    strMaxVal = strMaxVal == "." || strMaxVal == "" ? "0" : Convert.ToDecimal(strMaxVal).ToString(); //Modified by Sattiraju For Issue No: 78571

                    //Added by anandm for BUG ID 85307

                    if (StrSIUnit == "1" || StrSIUnit == "2")
                    {
                        var result = strMaxVal;
                        strMaxVal = Math.Round(Double.Parse(result.ToString()), 1).ToString();
                    }

                    //End of Anandm

                }

                if (strMaxVal != "0" || strMinVal != "0")
                {
                    if (strMinVal.Length > 0)
                    { strRefRange = strMinVal; }

                    if (strMaxVal != "0" && strMaxVal.Length > 0)
                    { strRefRange = strRefRange + " - " + strMaxVal; }

                    if (strMinVal.Length > 0 && strMaxVal.Length == 0)
                    { strRefRange = strMinVal; }

                    if (strMaxVal.Length > 0 && strMinVal.Length == 0)
                    { strRefRange = strMaxVal; }

                    if (blnUnits)
                    {
                        if (strRefRange.Length > 0)
                        { strRefRange = strRefRange + " " + strUnit; }
                        else
                        { strRefRange = strUnit; }
                    }

                    if (strRefRange.Length > 0)
                    {
                        if (!blnPrefix)
                        { strRefRange = strRefRange + " " + strKeyword; }
                        else
                        { strRefRange = strKeyword + " " + strRefRange; }
                    }
                    else
                    {
                        //strRefRange ="\n"+strKeyword;
                        strRefRange = strKeyword;
                    }
                }
                return strRefRange;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        private int ParseToInt(string value)
        {
            int Numeric;
            int.TryParse(value, out Numeric);
            return Numeric;
        }

        private DateTime ParseToDate(string value)
        {
            DateTime dt;
            DateTime.TryParse(value, out dt);
            return dt;
        }

        private bool ParseToBool(string value)
        {
            bool boolValue;
            bool.TryParse(value, out boolValue);
            return boolValue;
        }

        private DataSet CreateDataSet()
        {
            DataSet ds1 = new DataSet();

            ds1.Tables.Add("Config");
            ds1.Tables.Add("Doctors");
            ds1.Tables.Add("LabReport");
            ds1.Tables.Add("Patient");
            ds1.Tables.Add("Test");
            ds1.Tables.Add("Antibiotics");
            ds1.Tables.Add("Organisms");

            ds1.Tables[0].Columns.Add("ReportHeading", typeof(string));
            ds1.Tables[0].Columns.Add("RefRange", typeof(int));
            ds1.Tables[0].Columns.Add("DisplayNote", typeof(Boolean));
            ds1.Tables[0].Columns.Add("ModuleID", typeof(string));
            ds1.Tables[0].Columns.Add("FootNote", typeof(string));
            ds1.Tables[0].Columns.Add("GroupPrinting", typeof(Boolean));
            ds1.Tables[0].Columns.Add("GPShowTestName", typeof(Boolean));
            ds1.Tables[0].Columns.Add("ShowSpecimen", typeof(Boolean));
            ds1.Tables[0].Columns.Add("ResultsNotVerified", typeof(Boolean));

            ds1.Tables[1].Columns.Add("DoctorID1", typeof(int));
            ds1.Tables[1].Columns.Add("Doctor1", typeof(string));
            ds1.Tables[1].Columns.Add("Signature1", typeof(string));
            ds1.Tables[1].Columns.Add("Sign1", typeof(bool));
            ds1.Tables[1].Columns.Add("TestOrderItemID", typeof(int));
            ds1.Tables[1].Columns.Add("DoctorNameWithDesignation", typeof(string));

            ds1.Tables[2].Columns.Add("TestID", typeof(int));
            ds1.Tables[2].Columns.Add("TestName", typeof(string));
            ds1.Tables[2].Columns.Add("ParamID", typeof(int));
            ds1.Tables[2].Columns.Add("ParamName", typeof(string));
            ds1.Tables[2].Columns.Add("FormatType", typeof(string));
            ds1.Tables[2].Columns.Add("F1Result", typeof(string));
            ds1.Tables[2].Columns.Add("F1Unit", typeof(string));
            ds1.Tables[2].Columns.Add("F1ReferenceRange", typeof(string));
            ds1.Tables[2].Columns.Add("F1SIResult", typeof(string));
            ds1.Tables[2].Columns.Add("F1SIUnit", typeof(string));
            ds1.Tables[2].Columns.Add("F1SIReferenceRange", typeof(string));
            ds1.Tables[2].Columns.Add("UnitType", typeof(string));
            ds1.Tables[2].Columns.Add("F2ParamDesc", typeof(string));
            ds1.Tables[2].Columns.Add("F3ImagePath", typeof(string));
            ds1.Tables[2].Columns.Add("Observation", typeof(string));
            ds1.Tables[2].Columns.Add("MObservation", typeof(string));
            ds1.Tables[2].Columns.Add("Sequence", typeof(string));
            ds1.Tables[2].Columns.Add("Slno", typeof(int));
            ds1.Tables[2].Columns.Add("TestOrderItemID", typeof(int));
            ds1.Tables[2].Columns.Add("DoctorID", typeof(int));
            ds1.Tables[2].Columns.Add("Remarks", typeof(string));
            ds1.Tables[2].Columns.Add("CollectedDate", typeof(string));
            ds1.Tables[2].Columns.Add("ResultEntryDate", typeof(string));
            ds1.Tables[2].Columns.Add("ReportDate", typeof(string));
            ds1.Tables[2].Columns.Add("Serial", typeof(int));


            ds1.Tables[3].Columns.Add("Department", typeof(string));
            ds1.Tables[3].Columns.Add("OrderNo", typeof(string));
            ds1.Tables[3].Columns.Add("IPNo", typeof(string));
            ds1.Tables[3].Columns.Add("UHID", typeof(string));
            ds1.Tables[3].Columns.Add("BillNo", typeof(string));
            ds1.Tables[3].Columns.Add("BillRemarks", typeof(string));
            ds1.Tables[3].Columns.Add("Name", typeof(string));
            ds1.Tables[3].Columns.Add("AgeGender", typeof(string));
            ds1.Tables[3].Columns.Add("OrderDate", typeof(string));
            ds1.Tables[3].Columns.Add("ReportDate", typeof(string));
            ds1.Tables[3].Columns.Add("BedNo", typeof(string));
            ds1.Tables[3].Columns.Add("Doctor", typeof(string));
            ds1.Tables[3].Columns.Add("IPBillNoColName", typeof(string));
            ds1.Tables[3].Columns.Add("PatientID", typeof(long));
            ds1.Tables[3].Columns.Add("PatPhone", typeof(string));
            ds1.Tables[3].Columns.Add("DocPhone", typeof(string));
            ds1.Tables[3].Columns.Add("Nationality", typeof(string));
            ds1.Tables[3].Columns.Add("WardCaption", typeof(string));
            ds1.Tables[3].Columns.Add("Ward", typeof(string));
            ds1.Tables[3].Columns.Add("CollectionDate", typeof(string));
            ds1.Tables[3].Columns.Add("PatientType", typeof(int));
            ds1.Tables[3].Columns.Add("AcknowledgeDate", typeof(string));
            ds1.Tables[3].Columns.Add("Status", typeof(int));
            ds1.Tables[3].Columns.Add("Remarks", typeof(string));
            ds1.Tables[3].Columns.Add("AccessionNumber", typeof(string));
            ds1.Tables[3].Columns.Add("ResultEnteredByID", typeof(string));
            ds1.Tables[3].Columns.Add("ResultEnteredBy", typeof(string));
            ds1.Tables[3].Columns.Add("PayerName", typeof(string));
            ds1.Tables[3].Columns.Add("GradeName", typeof(string));
            ds1.Tables[3].Columns.Add("MRNO", typeof(string));
            ds1.Tables[3].Columns.Add("EmpID", typeof(string));
            ds1.Tables[3].Columns.Add("InsuranceID", typeof(string));
            ds1.Tables[3].Columns.Add("CurrentPrintingUser", typeof(string));
            ds1.Tables[3].Columns.Add("ResultEntryDate", typeof(string));
            ds1.Tables[3].Columns.Add("ResultVerifiedBy", typeof(string));
            ds1.Tables[3].Columns.Add("CompanyCode", typeof(string));
            ds1.Tables[3].Columns.Add("CompanyType", typeof(string));
            ds1.Tables[3].Columns.Add("Qualification", typeof(string));
            ds1.Tables[3].Columns.Add("DesignationName", typeof(string));

            ds1.Tables[4].Columns.Add("TestID", typeof(string));
            ds1.Tables[4].Columns.Add("TestName", typeof(string));
            ds1.Tables[4].Columns.Add("Specimen", typeof(string));
            ds1.Tables[4].Columns.Add("Method", typeof(string));
            ds1.Tables[4].Columns.Add("ProfileName", typeof(string));
            ds1.Tables[4].Columns.Add("SampleNumber", typeof(string));
            ds1.Tables[4].Columns.Add("Specialisation", typeof(string));

            ds1.Tables[4].Columns.Add("Slno", typeof(int));
            ds1.Tables[4].Columns.Add("EquipmentName", typeof(string));
            ds1.Tables[4].Columns.Add("IsPanic", typeof(Boolean));

            ds1.Tables[5].Columns.Add("TestID", typeof(int));
            ds1.Tables[5].Columns.Add("Organism", typeof(string));
            ds1.Tables[5].Columns.Add("Antibiotic", typeof(string));
            ds1.Tables[5].Columns.Add("Susceptibility1", typeof(string));
            ds1.Tables[5].Columns.Add("Susceptibility2", typeof(string));
            ds1.Tables[5].Columns.Add("Susceptibility3", typeof(string));
            ds1.Tables[5].Columns.Add("Susceptibility4", typeof(string));
            ds1.Tables[5].Columns.Add("Susceptibility5", typeof(string));
            ds1.Tables[5].Columns.Add("Observation", typeof(string));
            ds1.Tables[5].Columns.Add("Sequence", typeof(string));
            ds1.Tables[5].Columns.Add("Code", typeof(string));
            ds1.Tables[5].Columns.Add("SENSITIVE", typeof(string));
            ds1.Tables[5].Columns.Add("INTERMEDIATE", typeof(string));
            ds1.Tables[5].Columns.Add("RESISTANT", typeof(string));
            ds1.Tables[5].Columns.Add("TestOrderItemID", typeof(int));

            ds1.Tables[6].Columns.Add("TestID", typeof(int));
            ds1.Tables[6].Columns.Add("Organism1", typeof(string));
            ds1.Tables[6].Columns.Add("Organism2", typeof(string));
            ds1.Tables[6].Columns.Add("Organism3", typeof(string));
            ds1.Tables[6].Columns.Add("Organism4", typeof(string));
            ds1.Tables[6].Columns.Add("Organism5", typeof(string));
            ds1.Tables[6].Columns.Add("Observation", typeof(string));
            ds1.Tables[6].Columns.Add("Note", typeof(string));
            ds1.Tables[6].Columns.Add("Sequence", typeof(string));
            ds1.Tables[6].Columns.Add("TestOrderItemID", typeof(int));

            return ds1;
        }

        private DataSet CreateDataSetForRadiology()
        {
            DataSet ds1 = new DataSet();


            ds1.Tables.Add("Doctors");
            ds1.Tables.Add("Patient");
            ds1.Tables.Add("ProcReport");




            ds1.Tables[0].Columns.Add("DoctorID1", typeof(int));
            ds1.Tables[0].Columns.Add("Doctor1", typeof(string));
            ds1.Tables[0].Columns.Add("Signature1", typeof(string));
            ds1.Tables[0].Columns.Add("Sign1", typeof(bool));
            ds1.Tables[0].Columns.Add("TestOrderItemID", typeof(int));
            ds1.Tables[0].Columns.Add("DoctorNameWithDesignation", typeof(string));

            ds1.Tables[1].Columns.Add("Department", typeof(string));
            ds1.Tables[1].Columns.Add("OrderNo", typeof(string));
            ds1.Tables[1].Columns.Add("IPNo", typeof(string));
            ds1.Tables[1].Columns.Add("UHID", typeof(string));
            ds1.Tables[1].Columns.Add("BillNo", typeof(string));
            ds1.Tables[1].Columns.Add("BillRemarks", typeof(string));
            ds1.Tables[1].Columns.Add("Name", typeof(string));
            ds1.Tables[1].Columns.Add("AgeGender", typeof(string));
            ds1.Tables[1].Columns.Add("OrderDate", typeof(string));
            ds1.Tables[1].Columns.Add("ReportDate", typeof(string));
            ds1.Tables[1].Columns.Add("BedNo", typeof(string));
            ds1.Tables[1].Columns.Add("Doctor", typeof(string));
            ds1.Tables[1].Columns.Add("IPBillNoColName", typeof(string));
            ds1.Tables[1].Columns.Add("PatientID", typeof(long));
            ds1.Tables[1].Columns.Add("PatPhone", typeof(string));
            ds1.Tables[1].Columns.Add("DocPhone", typeof(string));
            ds1.Tables[1].Columns.Add("Nationality", typeof(string));
            ds1.Tables[1].Columns.Add("WardCaption", typeof(string));
            ds1.Tables[1].Columns.Add("Ward", typeof(string));
            ds1.Tables[1].Columns.Add("CollectionDate", typeof(string));
            ds1.Tables[1].Columns.Add("PatientType", typeof(int));
            ds1.Tables[1].Columns.Add("AcknowledgeDate", typeof(string));
            ds1.Tables[1].Columns.Add("Status", typeof(int));
            ds1.Tables[1].Columns.Add("Remarks", typeof(string));
            ds1.Tables[1].Columns.Add("AccessionNumber", typeof(string));
            ds1.Tables[1].Columns.Add("ResultEnteredByID", typeof(string));
            ds1.Tables[1].Columns.Add("ResultEnteredBy", typeof(string));
            ds1.Tables[1].Columns.Add("PayerName", typeof(string));
            ds1.Tables[1].Columns.Add("GradeName", typeof(string));
            ds1.Tables[1].Columns.Add("MRNO", typeof(string));
            ds1.Tables[1].Columns.Add("EmpID", typeof(string));
            ds1.Tables[1].Columns.Add("InsuranceID", typeof(string));
            ds1.Tables[1].Columns.Add("CurrentPrintingUser", typeof(string));
            ds1.Tables[1].Columns.Add("ResultEntryDate", typeof(string));
            ds1.Tables[1].Columns.Add("ResultVerifiedBy", typeof(string));
            ds1.Tables[1].Columns.Add("CompanyCode", typeof(string));
            ds1.Tables[1].Columns.Add("CompanyType", typeof(string));
            ds1.Tables[1].Columns.Add("Qualification", typeof(string));
            ds1.Tables[1].Columns.Add("DesignationName", typeof(string));


            ds1.Tables[2].Columns.Add("ProcID", typeof(int));
            ds1.Tables[2].Columns.Add("ProcName", typeof(string));
            ds1.Tables[2].Columns.Add("ParamID", typeof(int));
            ds1.Tables[2].Columns.Add("ParamName", typeof(string));
            ds1.Tables[2].Columns.Add("FormatType", typeof(string));
            ds1.Tables[2].Columns.Add("F1Result", typeof(string));
            ds1.Tables[2].Columns.Add("F1Unit", typeof(string));
            ds1.Tables[2].Columns.Add("F1ReferenceRange", typeof(string));
            ds1.Tables[2].Columns.Add("F2ParamDesc", typeof(string));
            ds1.Tables[2].Columns.Add("F3ImagePath", typeof(string));

            return ds1;
        }


        private DataSet CreateDataSetForPrescription()
        {
            DataSet ds1 = new DataSet();


            ds1.Tables.Add("Advice");
            ds1.Tables.Add("Diagnosis");
            ds1.Tables.Add("DrugAllergies");
            ds1.Tables.Add("DurationOfIllness");
            ds1.Tables.Add("Investigations");
            ds1.Tables.Add("MedicationData");
            ds1.Tables.Add("PatientData");
            ds1.Tables.Add("Procedures");
            ds1.Tables.Add("Surgery");
            ds1.Tables.Add("TemplateData");
            ds1.Tables.Add("Vitals");
            ds1.Tables.Add("PateintBloodOrders");
            ds1.Tables.Add("FoodAllergies");
            ds1.Tables.Add("OtherAllergies");
            ds1.Tables.Add("Referal");


            ds1.Tables[0].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[0].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[0].Columns.Add("PatientType", typeof(int));
            ds1.Tables[0].Columns.Add("FollowUpType", typeof(int));
            ds1.Tables[0].Columns.Add("Advice", typeof(string));
            ds1.Tables[0].Columns.Add("ReferralOrderID", typeof(int));
            ds1.Tables[0].Columns.Add("FollowAfter", typeof(int));
            ds1.Tables[0].Columns.Add("FollowUpOn", typeof(DateTime));
            ds1.Tables[0].Columns.Add("IsInternalReferral", typeof(Boolean));
            ds1.Tables[0].Columns.Add("RefDoctorID", typeof(int));
            ds1.Tables[0].Columns.Add("PatientID", typeof(int));
            ds1.Tables[0].Columns.Add("AdmissionID", typeof(int));
            ds1.Tables[0].Columns.Add("AdmissionNumber", typeof(string));
            ds1.Tables[0].Columns.Add("RefDoctorName", typeof(string));
            ds1.Tables[0].Columns.Add("ReasonForAdm", typeof(string));
            ds1.Tables[0].Columns.Add("UserName", typeof(string));
            ds1.Tables[0].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[0].Columns.Add("MODDATE", typeof(DateTime));
            ds1.Tables[0].Columns.Add("ProcedureId", typeof(int));
            ds1.Tables[0].Columns.Add("ProcedureName", typeof(string));
            ds1.Tables[0].Columns.Add("USERID", typeof(int));
            ds1.Tables[0].Columns.Add("LengthOfStay", typeof(int));
            ds1.Tables[0].Columns.Add("Adviceid", typeof(int));
            ds1.Tables[0].Columns.Add("TreatmentPlanID", typeof(int));
            ds1.Tables[0].Columns.Add("TreatmentPlanName", typeof(string));
            ds1.Tables[0].Columns.Add("IsPfVisible", typeof(int));
            ds1.Tables[0].Columns.Add("DietTypeID", typeof(int));
            ds1.Tables[0].Columns.Add("DietType", typeof(string));
            ds1.Tables[0].Columns.Add("IPID", typeof(int));
            ds1.Tables[0].Columns.Add("DoctorID", typeof(string));
            ds1.Tables[0].Columns.Add("IsSpecialityTreatmentApplicable", typeof(Boolean));
            ds1.Tables[0].Columns.Add("FollowUpCount", typeof(int));
            ds1.Tables[0].Columns.Add("Followupdays", typeof(int));
            ds1.Tables[0].Columns.Add("Remarks", typeof(string));
            ds1.Tables[0].Columns.Add("PrimaryDocID", typeof(int));
            ds1.Tables[0].Columns.Add("PrimaryDocName", typeof(string));
            ds1.Tables[0].Columns.Add("PatientTemplateid", typeof(int));
            ds1.Tables[0].Columns.Add("AdmissionTypeID", typeof(int));
            ds1.Tables[0].Columns.Add("AdmissionType", typeof(string));
            ds1.Tables[0].Columns.Add("IsVitual", typeof(int));


            ds1.Tables[1].Columns.Add("TBL", typeof(string));
            ds1.Tables[1].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[1].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[1].Columns.Add("DiseaseID", typeof(int));
            ds1.Tables[1].Columns.Add("DiagnosisType", typeof(int));
            ds1.Tables[1].Columns.Add("DiseaseName", typeof(string));
            ds1.Tables[1].Columns.Add("Code", typeof(string));
            ds1.Tables[1].Columns.Add("PatientID", typeof(int));
            ds1.Tables[1].Columns.Add("AdmissionID", typeof(int));
            ds1.Tables[1].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[1].Columns.Add("MODDATE", typeof(DateTime));
            ds1.Tables[1].Columns.Add("ProblemPointID", typeof(int));
            ds1.Tables[1].Columns.Add("ProblemPointName", typeof(string));
            ds1.Tables[1].Columns.Add("PointValue", typeof(int));
            ds1.Tables[1].Columns.Add("Username", typeof(string));
            ds1.Tables[1].Columns.Add("AssessmentID", typeof(int));
            ds1.Tables[1].Columns.Add("userid", typeof(int));
            ds1.Tables[1].Columns.Add("DiagonosisTypeID", typeof(int));
            ds1.Tables[1].Columns.Add("IsPsd", typeof(int));
            ds1.Tables[1].Columns.Add("Blocked", typeof(int));
            ds1.Tables[1].Columns.Add("DiagonosisType", typeof(string));
            ds1.Tables[1].Columns.Add("IsPfVisible", typeof(int));
            ds1.Tables[1].Columns.Add("SignificantData", typeof(bool));
            ds1.Tables[1].Columns.Add("Remarks", typeof(string));
            ds1.Tables[1].Columns.Add("IsAdmitDisease", typeof(bool));
            ds1.Tables[1].Columns.Add("ExpectedLengthOfStay", typeof(int));
            ds1.Tables[1].Columns.Add("DoctorId", typeof(int));
            ds1.Tables[1].Columns.Add("DiagnosisStatus", typeof(string));


            ds1.Tables[2].Columns.Add("TBL", typeof(string));
            ds1.Tables[2].Columns.Add("GenericID", typeof(int));
            ds1.Tables[2].Columns.Add("Remark", typeof(string));
            ds1.Tables[2].Columns.Add("GenericName", typeof(string));
            ds1.Tables[2].Columns.Add("GenericName2L", typeof(string));
            ds1.Tables[2].Columns.Add("FROMdate", typeof(DateTime));
            ds1.Tables[2].Columns.Add("todate", typeof(DateTime));
            ds1.Tables[2].Columns.Add("Description", typeof(string));
            ds1.Tables[2].Columns.Add("IsNotActive", typeof(bool));
            ds1.Tables[2].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[2].Columns.Add("IPID", typeof(int));
            ds1.Tables[2].Columns.Add("AllergieTypes", typeof(string));
            ds1.Tables[2].Columns.Add("Doctorid", typeof(int));
            ds1.Tables[2].Columns.Add("IsPfVisible", typeof(Boolean));
            ds1.Tables[2].Columns.Add("Status", typeof(int));


            ds1.Tables[3].Columns.Add("TBL", typeof(string));
            ds1.Tables[3].Columns.Add("EpisodeName", typeof(string));
            ds1.Tables[3].Columns.Add("DurationOfIllness", typeof(string));
            ds1.Tables[3].Columns.Add("Height", typeof(Decimal));
            ds1.Tables[3].Columns.Add("Weight", typeof(Decimal));
            ds1.Tables[3].Columns.Add("PainScoreID", typeof(int));
            ds1.Tables[3].Columns.Add("IsEducated", typeof(bool));
            ds1.Tables[3].Columns.Add("IsSmoke", typeof(bool));
            ds1.Tables[3].Columns.Add("IsPregnent", typeof(Boolean));
            ds1.Tables[3].Columns.Add("ExpectedDeliveryDate", typeof(DateTime));
            ds1.Tables[3].Columns.Add("IsPatientDrugAlleric", typeof(bool));
            ds1.Tables[3].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[3].Columns.Add("BodyMassID", typeof(string));
            ds1.Tables[3].Columns.Add("DoctorName", typeof(string));
            ds1.Tables[3].Columns.Add("PainScore", typeof(string));
            ds1.Tables[3].Columns.Add("HeadCircumference", typeof(Decimal));
            ds1.Tables[3].Columns.Add("IsoldVisit", typeof(Boolean));
            ds1.Tables[3].Columns.Add("Pregnancy", typeof(Boolean));
            ds1.Tables[3].Columns.Add("Lactation", typeof(Boolean));
            ds1.Tables[3].Columns.Add("TriSemester", typeof(Boolean));
            ds1.Tables[3].Columns.Add("PlannedDischargeDate", typeof(DateTime));
            ds1.Tables[3].Columns.Add("LMP", typeof(DateTime));
            ds1.Tables[3].Columns.Add("CTASScoreColorID", typeof(int));
            ds1.Tables[3].Columns.Add("Score", typeof(string));
            ds1.Tables[3].Columns.Add("Color", typeof(string));
            ds1.Tables[3].Columns.Add("CTASDescription", typeof(string));

            ds1.Tables[4].Columns.Add("TBL", typeof(string));
            ds1.Tables[4].Columns.Add("ServiceTypeID", typeof(int));
            ds1.Tables[4].Columns.Add("PrescriptionID", typeof(int));
            ds1.Tables[4].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[4].Columns.Add("PrescriptionDate", typeof(DateTime));
            ds1.Tables[4].Columns.Add("DoctorID", typeof(int));
            ds1.Tables[4].Columns.Add("Doctorname", typeof(string));
            ds1.Tables[4].Columns.Add("ItemSequence", typeof(int));
            ds1.Tables[4].Columns.Add("ItemID", typeof(int));
            ds1.Tables[4].Columns.Add("ItemName", typeof(string));
            ds1.Tables[4].Columns.Add("Dose", typeof(int));
            ds1.Tables[4].Columns.Add("DoseID", typeof(int));
            ds1.Tables[4].Columns.Add("DoseUoM", typeof(int));
            ds1.Tables[4].Columns.Add("FrequencyID", typeof(int));
            ds1.Tables[4].Columns.Add("Frequency", typeof(int));
            ds1.Tables[4].Columns.Add("Duration", typeof(int));
            ds1.Tables[4].Columns.Add("DurationID", typeof(int));
            ds1.Tables[4].Columns.Add("DurationUOM", typeof(int));
            ds1.Tables[4].Columns.Add("StartFrom", typeof(int));
            ds1.Tables[4].Columns.Add("Remarks", typeof(string));
            ds1.Tables[4].Columns.Add("SpecimenID", typeof(int));
            ds1.Tables[4].Columns.Add("SpecimenName", typeof(string));
            ds1.Tables[4].Columns.Add("Status", typeof(int));
            ds1.Tables[4].Columns.Add("Quantity", typeof(int));
            ds1.Tables[4].Columns.Add("UserName", typeof(string));
            ds1.Tables[4].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[4].Columns.Add("MODDATE", typeof(DateTime));
            ds1.Tables[4].Columns.Add("UCAFApproval", typeof(Boolean));
            ds1.Tables[4].Columns.Add("USERID", typeof(int));
            ds1.Tables[4].Columns.Add("Itemstatus", typeof(int));
            ds1.Tables[4].Columns.Add("admissionid", typeof(int));
            ds1.Tables[4].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[4].Columns.Add("TestOrderItemID", typeof(int));
            ds1.Tables[4].Columns.Add("TestOrderID", typeof(int));
            ds1.Tables[4].Columns.Add("SpecialiseID", typeof(int));
            ds1.Tables[4].Columns.Add("Specialisation", typeof(string));
            ds1.Tables[4].Columns.Add("ResultStatus", typeof(int));
            ds1.Tables[4].Columns.Add("ResultStatusName", typeof(string));
            ds1.Tables[4].Columns.Add("IsPfVisible", typeof(Boolean));
            ds1.Tables[4].Columns.Add("ItemCode", typeof(string));
            ds1.Tables[4].Columns.Add("Isresult", typeof(bool));
            ds1.Tables[4].Columns.Add("OrderType", typeof(string));


            ds1.Tables[5].Columns.Add("TBL", typeof(string));
            ds1.Tables[5].Columns.Add("ServiceTypeID", typeof(int));
            ds1.Tables[5].Columns.Add("PrescriptionID", typeof(int));
            ds1.Tables[5].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[5].Columns.Add("PrescriptionDate", typeof(DateTime));
            ds1.Tables[5].Columns.Add("DoctorID", typeof(int));
            ds1.Tables[5].Columns.Add("DoctorName", typeof(string));
            ds1.Tables[5].Columns.Add("ItemSequence", typeof(int));
            ds1.Tables[5].Columns.Add("ItemID", typeof(int));
            ds1.Tables[5].Columns.Add("ItemName", typeof(string));
            ds1.Tables[5].Columns.Add("Dose", typeof(Decimal));
            ds1.Tables[5].Columns.Add("DoseID", typeof(int));
            ds1.Tables[5].Columns.Add("DoseUoM", typeof(string));
            ds1.Tables[5].Columns.Add("FrequencyID", typeof(int));
            ds1.Tables[5].Columns.Add("Frequency", typeof(string));
            ds1.Tables[5].Columns.Add("Duration", typeof(int));
            ds1.Tables[5].Columns.Add("DurationID", typeof(int));
            ds1.Tables[5].Columns.Add("DurationUOM", typeof(string));
            ds1.Tables[5].Columns.Add("StartFrom", typeof(DateTime));
            ds1.Tables[5].Columns.Add("Remarks", typeof(string));
            ds1.Tables[5].Columns.Add("SpecimenID", typeof(int));
            ds1.Tables[5].Columns.Add("SpecimenName", typeof(int));
            ds1.Tables[5].Columns.Add("Status", typeof(int));
            ds1.Tables[5].Columns.Add("AdmissionID", typeof(int));
            ds1.Tables[5].Columns.Add("AdmissionNumber", typeof(string));
            ds1.Tables[5].Columns.Add("AdmRouteID", typeof(int));
            ds1.Tables[5].Columns.Add("AdmRoute", typeof(string));
            ds1.Tables[5].Columns.Add("UserName", typeof(string));
            ds1.Tables[5].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[5].Columns.Add("ModDate", typeof(DateTime));
            ds1.Tables[5].Columns.Add("blocked", typeof(int));
            ds1.Tables[5].Columns.Add("CustomizedFlag", typeof(Boolean));
            ds1.Tables[5].Columns.Add("CustDrugDetails", typeof(string));
            ds1.Tables[5].Columns.Add("FrequencyQTY", typeof(Decimal));
            ds1.Tables[5].Columns.Add("IssueUOMValue", typeof(int));
            ds1.Tables[5].Columns.Add("IsNarcotic", typeof(int));
            ds1.Tables[5].Columns.Add("IsAntibiotic", typeof(int));
            ds1.Tables[5].Columns.Add("IsControledDrug", typeof(int));
            ds1.Tables[5].Columns.Add("IsDisPrescription", typeof(Boolean));
            ds1.Tables[5].Columns.Add("UCAFApproval", typeof(Boolean));
            ds1.Tables[5].Columns.Add("GenericID", typeof(int));
            ds1.Tables[5].Columns.Add("GenericName", typeof(string));
            ds1.Tables[5].Columns.Add("Strength", typeof(Decimal));
            ds1.Tables[5].Columns.Add("StrengthUOM", typeof(string));
            ds1.Tables[5].Columns.Add("ScheduleTime", typeof(string));
            ds1.Tables[5].Columns.Add("PrescriptionNo", typeof(string));
            ds1.Tables[5].Columns.Add("Remarks1", typeof(string));
            ds1.Tables[5].Columns.Add("StrengthUoMID", typeof(int));
            ds1.Tables[5].Columns.Add("ENDDatetime", typeof(DateTime));
            ds1.Tables[5].Columns.Add("DiscontinuingRemarks", typeof(string));
            ds1.Tables[5].Columns.Add("IsAdverseDrug", typeof(int));
            ds1.Tables[5].Columns.Add("AdverseDrugDescription", typeof(string));
            ds1.Tables[5].Columns.Add("ENDDate", typeof(DateTime));
            ds1.Tables[5].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[5].Columns.Add("PatientInstructions", typeof(string));
            ds1.Tables[5].Columns.Add("IsPfvisible", typeof(Boolean));
            ds1.Tables[5].Columns.Add("OrderStatus", typeof(int));
            ds1.Tables[5].Columns.Add("PrescriptionStatusID", typeof(int));
            ds1.Tables[5].Columns.Add("ItemCode", typeof(string));
            ds1.Tables[5].Columns.Add("PrescriptionStatus", typeof(int));
            ds1.Tables[5].Columns.Add("PrescriptionModDate", typeof(DateTime));
            ds1.Tables[5].Columns.Add("TOTIssuingQty", typeof(int));
            ds1.Tables[5].Columns.Add("TotIssuingQtyUOM", typeof(string));

            ds1.Tables[6].Columns.Add("RegCode", typeof(string));
            ds1.Tables[6].Columns.Add("PatientName", typeof(string));
            ds1.Tables[6].Columns.Add("Gender", typeof(string));
            ds1.Tables[6].Columns.Add("MobileNo", typeof(string));
            ds1.Tables[6].Columns.Add("HospitalName", typeof(string));
            ds1.Tables[6].Columns.Add("FullAge", typeof(string));
            ds1.Tables[6].Columns.Add("Nationality", typeof(string));
            ds1.Tables[6].Columns.Add("FamilyHeadId", typeof(string));
            ds1.Tables[6].Columns.Add("IsContrastAllergic", typeof(Boolean));
            ds1.Tables[6].Columns.Add("DoctorName", typeof(string));
            ds1.Tables[6].Columns.Add("Admitdate", typeof(string));
            ds1.Tables[6].Columns.Add("CompanyName", typeof(string));
            ds1.Tables[6].Columns.Add("DocSpecDepartment", typeof(string));
            ds1.Tables[6].Columns.Add("DocSign", typeof(string));
            ds1.Tables[6].Columns.Add("DocCode", typeof(string));

            ds1.Tables[7].Columns.Add("TBL", typeof(string));
            ds1.Tables[7].Columns.Add("ServiceTypeID", typeof(int));
            ds1.Tables[7].Columns.Add("PrescriptionID", typeof(int));
            ds1.Tables[7].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[7].Columns.Add("PrescriptionDate", typeof(DateTime));
            ds1.Tables[7].Columns.Add("DoctorID", typeof(int));
            ds1.Tables[7].Columns.Add("Doctorname", typeof(string));
            ds1.Tables[7].Columns.Add("ItemSequence", typeof(int));
            ds1.Tables[7].Columns.Add("ItemID", typeof(int));
            ds1.Tables[7].Columns.Add("ItemName", typeof(string));
            ds1.Tables[7].Columns.Add("Dose", typeof(int));
            ds1.Tables[7].Columns.Add("DoseID", typeof(int));
            ds1.Tables[7].Columns.Add("DoseUoM", typeof(int));
            ds1.Tables[7].Columns.Add("FrequencyID", typeof(int));
            ds1.Tables[7].Columns.Add("Frequency", typeof(int));
            ds1.Tables[7].Columns.Add("Duration", typeof(int));
            ds1.Tables[7].Columns.Add("DurationID", typeof(int));
            ds1.Tables[7].Columns.Add("DurationUOM", typeof(int));
            ds1.Tables[7].Columns.Add("StartFrom", typeof(int));
            ds1.Tables[7].Columns.Add("Remarks", typeof(string));
            ds1.Tables[7].Columns.Add("SpecimenID", typeof(int));
            ds1.Tables[7].Columns.Add("SpecimenName", typeof(string));
            ds1.Tables[7].Columns.Add("Status", typeof(int));
            ds1.Tables[7].Columns.Add("Quantity", typeof(int));
            ds1.Tables[7].Columns.Add("UserName", typeof(string));
            ds1.Tables[7].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[7].Columns.Add("MODDATE", typeof(DateTime));
            ds1.Tables[7].Columns.Add("UCAFApproval", typeof(bool));
            ds1.Tables[7].Columns.Add("USERID", typeof(int));
            ds1.Tables[7].Columns.Add("SpecialiseID", typeof(int));
            ds1.Tables[7].Columns.Add("Specialisation", typeof(string));
            ds1.Tables[7].Columns.Add("WorkstationName", typeof(string));
            ds1.Tables[7].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[7].Columns.Add("ItemStatus", typeof(int));
            ds1.Tables[7].Columns.Add("TestOrderItemID", typeof(int));
            ds1.Tables[7].Columns.Add("TestOrderID", typeof(int));
            ds1.Tables[7].Columns.Add("ResultStatus", typeof(int));
            ds1.Tables[7].Columns.Add("ResultStatusName", typeof(string));
            ds1.Tables[7].Columns.Add("IsPfVisible", typeof(bool));
            ds1.Tables[7].Columns.Add("ItemCode", typeof(string));
            ds1.Tables[7].Columns.Add("AdmissionID", typeof(int));
            ds1.Tables[7].Columns.Add("IsTreatmentplanApplicable", typeof(bool));
            ds1.Tables[7].Columns.Add("OrderType", typeof(string));

            ds1.Tables[8].Columns.Add("TBL", typeof(string));
            ds1.Tables[8].Columns.Add("SurgeryRequestId", typeof(int));
            ds1.Tables[8].Columns.Add("PatientId", typeof(int));
            ds1.Tables[8].Columns.Add("PatientName", typeof(string));
            ds1.Tables[8].Columns.Add("RegCode", typeof(string));
            ds1.Tables[8].Columns.Add("Age", typeof(string));
            ds1.Tables[8].Columns.Add("Gender", typeof(string));
            ds1.Tables[8].Columns.Add("Age_x002F_Gender", typeof(string));
            ds1.Tables[8].Columns.Add("SurgeryRequesteDby", typeof(int));
            ds1.Tables[8].Columns.Add("ProcedureID", typeof(int));
            ds1.Tables[8].Columns.Add("ProcedureName", typeof(string));
            ds1.Tables[8].Columns.Add("payerid", typeof(int));
            ds1.Tables[8].Columns.Add("ReceiverID", typeof(int));
            ds1.Tables[8].Columns.Add("GradeID", typeof(int));
            ds1.Tables[8].Columns.Add("GradeName", typeof(string));
            ds1.Tables[8].Columns.Add("TaskStatus", typeof(int));
            ds1.Tables[8].Columns.Add("StatusName", typeof(string));
            ds1.Tables[8].Columns.Add("StatusColor", typeof(int));
            ds1.Tables[8].Columns.Add("MonitorId", typeof(int));
            ds1.Tables[8].Columns.Add("VisitId", typeof(int));
            ds1.Tables[8].Columns.Add("IpId", typeof(int));
            ds1.Tables[8].Columns.Add("Remarks", typeof(string));
            ds1.Tables[8].Columns.Add("ScheduleId", typeof(int));
            ds1.Tables[8].Columns.Add("OrderId", typeof(int));
            ds1.Tables[8].Columns.Add("ServiceId", typeof(int));
            ds1.Tables[8].Columns.Add("Reqconsentteken", typeof(Boolean));
            ds1.Tables[8].Columns.Add("InstructionType", typeof(int));
            ds1.Tables[8].Columns.Add("ProcedureTypeid", typeof(int));
            ds1.Tables[8].Columns.Add("ReqconsenttekEndate", typeof(DateTime));
            ds1.Tables[8].Columns.Add("Consenttaken", typeof(Boolean));
            ds1.Tables[8].Columns.Add("ConsenttakEndate", typeof(DateTime));
            ds1.Tables[8].Columns.Add("SurgeryType", typeof(int));
            ds1.Tables[8].Columns.Add("Tentativesurgeryplaned", typeof(DateTime));
            ds1.Tables[8].Columns.Add("TentavtiveStartTime", typeof(string));
            ds1.Tables[8].Columns.Add("Estimatedtimeduration", typeof(string));
            ds1.Tables[8].Columns.Add("Anesthesiologistinstructions", typeof(string));
            ds1.Tables[8].Columns.Add("Otinstructions", typeof(string));
            ds1.Tables[8].Columns.Add("WardsInstructions", typeof(string));
            ds1.Tables[8].Columns.Add("Anesthesiologist", typeof(int));
            ds1.Tables[8].Columns.Add("EquipmentsurgerYId", typeof(int));
            ds1.Tables[8].Columns.Add("Implantsurgeryid", typeof(int));
            ds1.Tables[8].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[8].Columns.Add("ModDate", typeof(DateTime));
            ds1.Tables[8].Columns.Add("EndDate", typeof(DateTime));
            ds1.Tables[8].Columns.Add("Blocked", typeof(int));
            ds1.Tables[8].Columns.Add("Routid", typeof(int));
            ds1.Tables[8].Columns.Add("Userid", typeof(int));
            ds1.Tables[8].Columns.Add("Workstationid", typeof(int));
            ds1.Tables[8].Columns.Add("Status", typeof(int));
            ds1.Tables[8].Columns.Add("Rowseq", typeof(string));
            ds1.Tables[8].Columns.Add("Isprimary", typeof(int));
            ds1.Tables[8].Columns.Add("Priority", typeof(int));
            ds1.Tables[8].Columns.Add("Doctorname", typeof(string));
            ds1.Tables[8].Columns.Add("DoctorQualification", typeof(string));
            ds1.Tables[8].Columns.Add("DoctorDesignation", typeof(string));
            ds1.Tables[8].Columns.Add("Surgeon", typeof(string));
            ds1.Tables[8].Columns.Add("SurgeonQualification", typeof(string));
            ds1.Tables[8].Columns.Add("SurgeonSignature", typeof(string));
            ds1.Tables[8].Columns.Add("SurgeonDesignation", typeof(string));
            ds1.Tables[8].Columns.Add("Anestheologistname", typeof(string));
            ds1.Tables[8].Columns.Add("AnestheologistQualification", typeof(string));
            ds1.Tables[8].Columns.Add("AnestheologistSignature", typeof(string));
            ds1.Tables[8].Columns.Add("AnestheologistDesignation", typeof(string));
            ds1.Tables[8].Columns.Add("PACDateTime", typeof(DateTime));
            ds1.Tables[8].Columns.Add("Facilityid", typeof(int));
            ds1.Tables[8].Columns.Add("ScheduleDate", typeof(string));
            ds1.Tables[8].Columns.Add("IsInfected", typeof(Boolean));
            ds1.Tables[8].Columns.Add("InfectionDetails", typeof(string));
            ds1.Tables[8].Columns.Add("CancelRemarks", typeof(string));
            ds1.Tables[8].Columns.Add("DoctorID", typeof(int));
            ds1.Tables[8].Columns.Add("ISPatientFit", typeof(Boolean));
            ds1.Tables[8].Columns.Add("SurgeonID", typeof(int));
            ds1.Tables[8].Columns.Add("IsApprovedByBilling", typeof(Boolean));
            ds1.Tables[8].Columns.Add("BillingRemarks", typeof(string));
            ds1.Tables[8].Columns.Add("BillingRemarksEnteredDate", typeof(DateTime));
            ds1.Tables[8].Columns.Add("StartTime", typeof(string));
            ds1.Tables[8].Columns.Add("ENDTime", typeof(string));
            ds1.Tables[8].Columns.Add("FROMSlot", typeof(int));
            ds1.Tables[8].Columns.Add("ToSlot", typeof(int));
            ds1.Tables[8].Columns.Add("Primarydoctorname", typeof(string));
            ds1.Tables[8].Columns.Add("OTReceptionFacilityId", typeof(int));
            ds1.Tables[8].Columns.Add("OTReceptionFacility", typeof(string));
            ds1.Tables[8].Columns.Add("ProcedureCode", typeof(string));


            ds1.Tables[9].Columns.Add("TBL", typeof(string));
            ds1.Tables[9].Columns.Add("ComponentID", typeof(int));
            ds1.Tables[9].Columns.Add("Value", typeof(string));
            ds1.Tables[9].Columns.Add("ComponentName", typeof(string));
            ds1.Tables[9].Columns.Add("AssessmentID", typeof(int));
            ds1.Tables[9].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[9].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[9].Columns.Add("AdmissionID", typeof(int));
            ds1.Tables[9].Columns.Add("PreviewHTML", typeof(string));
            ds1.Tables[9].Columns.Add("CSTemplateID", typeof(int));

            ds1.Tables[10].Columns.Add("TBL", typeof(string));
            ds1.Tables[10].Columns.Add("Value", typeof(string));
            ds1.Tables[10].Columns.Add("Datetime", typeof(DateTime));
            ds1.Tables[10].Columns.Add("Vital", typeof(string));
            ds1.Tables[10].Columns.Add("UOM", typeof(string));
            ds1.Tables[10].Columns.Add("MINVALUE", typeof(Decimal));
            ds1.Tables[10].Columns.Add("MAXVALUE", typeof(Decimal));
            ds1.Tables[10].Columns.Add("AssessmentID", typeof(int));
            ds1.Tables[10].Columns.Add("UserName", typeof(string));
            ds1.Tables[10].Columns.Add("USERID", typeof(int));
            ds1.Tables[10].Columns.Add("IsPsd", typeof(int));
            ds1.Tables[10].Columns.Add("DoctorID", typeof(int));
            ds1.Tables[10].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[10].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[10].Columns.Add("Comments", typeof(string));
            ds1.Tables[10].Columns.Add("ArchiveModdate", typeof(DateTime));
            ds1.Tables[10].Columns.Add("Status", typeof(int));
            ds1.Tables[10].Columns.Add("IsPfVisible", typeof(int));
            ds1.Tables[10].Columns.Add("VITALSIGNID", typeof(int));
            ds1.Tables[10].Columns.Add("Sequence", typeof(bool));
            ds1.Tables[10].Columns.Add("VitalSignDate", typeof(DateTime));

            ds1.Tables[11].Columns.Add("PatientID", typeof(int));
            ds1.Tables[11].Columns.Add("IPID", typeof(int));
            ds1.Tables[11].Columns.Add("PatientName", typeof(string));
            ds1.Tables[11].Columns.Add("AdmissionNumber", typeof(string));
            ds1.Tables[11].Columns.Add("BloodorderID", typeof(int));
            ds1.Tables[11].Columns.Add("ComponentID", typeof(int));
            ds1.Tables[11].Columns.Add("Component", typeof(string));
            ds1.Tables[11].Columns.Add("Quantity", typeof(int));
            ds1.Tables[11].Columns.Add("RequiredDate", typeof(DateTime));
            ds1.Tables[11].Columns.Add("Remarks", typeof(string));
            ds1.Tables[11].Columns.Add("Volume", typeof(int));
            ds1.Tables[11].Columns.Add("Type", typeof(int));
            ds1.Tables[11].Columns.Add("Componentcode", typeof(string));

            ds1.Tables[12].Columns.Add("TBL", typeof(string));
            ds1.Tables[12].Columns.Add("FoodID", typeof(int));
            ds1.Tables[12].Columns.Add("Remark", typeof(string));
            ds1.Tables[12].Columns.Add("FdIngrName", typeof(string));
            ds1.Tables[12].Columns.Add("FdIngrName2L", typeof(string));
            ds1.Tables[12].Columns.Add("FROMdate", typeof(DateTime));
            ds1.Tables[12].Columns.Add("todate", typeof(DateTime));
            ds1.Tables[12].Columns.Add("Description", typeof(string));
            ds1.Tables[12].Columns.Add("IsNotActive", typeof(bool));
            ds1.Tables[12].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[12].Columns.Add("IPID", typeof(int));
            ds1.Tables[12].Columns.Add("IsPfVisible", typeof(Boolean));
            ds1.Tables[12].Columns.Add("AllergieTypes", typeof(string));
            ds1.Tables[12].Columns.Add("Doctorid", typeof(int));
            ds1.Tables[12].Columns.Add("Status", typeof(int));


            ds1.Tables[13].Columns.Add("TBL", typeof(string));
            ds1.Tables[13].Columns.Add("ID", typeof(int));
            ds1.Tables[13].Columns.Add("Allergy", typeof(string));
            ds1.Tables[13].Columns.Add("Allergy2L", typeof(string));
            ds1.Tables[13].Columns.Add("FROMdate", typeof(DateTime));
            ds1.Tables[13].Columns.Add("todate", typeof(DateTime));
            ds1.Tables[13].Columns.Add("Description", typeof(string));
            ds1.Tables[13].Columns.Add("IsNotActive", typeof(Boolean));
            ds1.Tables[13].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[13].Columns.Add("IPID", typeof(int));
            ds1.Tables[13].Columns.Add("AllergieTypes", typeof(string));
            ds1.Tables[13].Columns.Add("Doctorid", typeof(int));
            ds1.Tables[13].Columns.Add("IsPfVisible", typeof(Boolean));
            ds1.Tables[13].Columns.Add("Status", typeof(int));

            ds1.Tables[14].Columns.Add("TBL", typeof(string));
            ds1.Tables[14].Columns.Add("ReferralOrderID", typeof(int));
            ds1.Tables[14].Columns.Add("ReferralTypeID", typeof(int));
            ds1.Tables[14].Columns.Add("ReferralType", typeof(string));
            ds1.Tables[14].Columns.Add("SpecialiseID", typeof(int));
            ds1.Tables[14].Columns.Add("Specialisation", typeof(string));
            ds1.Tables[14].Columns.Add("FromDoctorID", typeof(int));
            ds1.Tables[14].Columns.Add("Priority", typeof(int));
            ds1.Tables[14].Columns.Add("Remarks", typeof(string));
            ds1.Tables[14].Columns.Add("DoctorID", typeof(int));
            ds1.Tables[14].Columns.Add("DoctornameName", typeof(string));
            ds1.Tables[14].Columns.Add("AdmissionID", typeof(int));
            ds1.Tables[14].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[14].Columns.Add("HospitalID", typeof(int));
            ds1.Tables[14].Columns.Add("IsDirect", typeof(Boolean));
            ds1.Tables[14].Columns.Add("ExReferDoctorID", typeof(int));
            ds1.Tables[14].Columns.Add("ExReferDoctor", typeof(string));
            ds1.Tables[14].Columns.Add("adviceid", typeof(int));
            ds1.Tables[14].Columns.Add("Feedback", typeof(string));
            ds1.Tables[14].Columns.Add("refusalremarks", typeof(string));
            ds1.Tables[14].Columns.Add("Reasonid", typeof(int));
            ds1.Tables[14].Columns.Add("Reason", typeof(string));
            ds1.Tables[14].Columns.Add("duration", typeof(int));
            ds1.Tables[14].Columns.Add("IsInternalReferral", typeof(Boolean));
            ds1.Tables[14].Columns.Add("Blocked", typeof(int));
            ds1.Tables[14].Columns.Add("Status", typeof(int));
            ds1.Tables[14].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[14].Columns.Add("IPID", typeof(int));
            ds1.Tables[14].Columns.Add("DoctorID1", typeof(int));

            
            return ds1;
        }


        private DataSet CreateDatasetForPrescriptionNew()
        {
            DataSet ds1 = new DataSet();

            ds1.Tables.Add("Advice");
            ds1.Tables.Add("Diagnosis");
            ds1.Tables.Add("DrugAllergies");
            ds1.Tables.Add("DurationOfIllness");
            ds1.Tables.Add("Investigations");
            ds1.Tables.Add("MedicationData");
            ds1.Tables.Add("PatientData");
            ds1.Tables.Add("Procedures");
            ds1.Tables.Add("Surgery");
            ds1.Tables.Add("TemplateData");
            ds1.Tables.Add("Vitals");
            ds1.Tables.Add("PateintBloodOrders");
            ds1.Tables.Add("FoodAllergies");
            ds1.Tables.Add("OtherAllergies");
            ds1.Tables.Add("Referal");
            ds1.Tables.Add("Table8");

            ds1.Tables[0].Columns.Add("TBL", typeof(string));
            ds1.Tables[0].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[0].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[0].Columns.Add("PatientType", typeof(int));
            ds1.Tables[0].Columns.Add("FollowUpType", typeof(int));
            ds1.Tables[0].Columns.Add("Advice", typeof(string));
            ds1.Tables[0].Columns.Add("ReferralOrderID", typeof(int));
            ds1.Tables[0].Columns.Add("FollowAfter", typeof(int));
            ds1.Tables[0].Columns.Add("FollowUpOn", typeof(DateTime));
            ds1.Tables[0].Columns.Add("IsInternalReferral", typeof(Boolean));
            ds1.Tables[0].Columns.Add("RefDoctorID", typeof(int));
            ds1.Tables[0].Columns.Add("PatientID", typeof(int));
            ds1.Tables[0].Columns.Add("AdmissionID", typeof(int));
            ds1.Tables[0].Columns.Add("AdmissionNumber", typeof(string));
            ds1.Tables[0].Columns.Add("RefDoctorName", typeof(string));
            ds1.Tables[0].Columns.Add("ReasonForAdm", typeof(string));
            ds1.Tables[0].Columns.Add("UserName", typeof(string));
            ds1.Tables[0].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[0].Columns.Add("MODDATE", typeof(DateTime));
            ds1.Tables[0].Columns.Add("ProcedureId", typeof(int));
            ds1.Tables[0].Columns.Add("ProcedureName", typeof(string));
            ds1.Tables[0].Columns.Add("USERID", typeof(int));
            ds1.Tables[0].Columns.Add("LengthOfStay", typeof(int));
            ds1.Tables[0].Columns.Add("Adviceid", typeof(int));
            ds1.Tables[0].Columns.Add("TreatmentPlanID", typeof(int));
            ds1.Tables[0].Columns.Add("TreatmentPlanName", typeof(string));
            ds1.Tables[0].Columns.Add("IsPfVisible", typeof(int));
            ds1.Tables[0].Columns.Add("DietTypeID", typeof(int));
            ds1.Tables[0].Columns.Add("DietType", typeof(string));
            ds1.Tables[0].Columns.Add("IPID", typeof(int));
            ds1.Tables[0].Columns.Add("DoctorID", typeof(string));
            ds1.Tables[0].Columns.Add("IsSpecialityTreatmentApplicable", typeof(Boolean));
            ds1.Tables[0].Columns.Add("FollowUpCount", typeof(int));
            ds1.Tables[0].Columns.Add("Followupdays", typeof(int));
            ds1.Tables[0].Columns.Add("Remarks", typeof(string));
            ds1.Tables[0].Columns.Add("PrimaryDocID", typeof(int));
            ds1.Tables[0].Columns.Add("PrimaryDocName", typeof(string));
            ds1.Tables[0].Columns.Add("PatientTemplateid", typeof(int));
            ds1.Tables[0].Columns.Add("AdmissionTypeID", typeof(int));
            ds1.Tables[0].Columns.Add("AdmissionType", typeof(string));
            ds1.Tables[0].Columns.Add("IsVitual", typeof(int));

            ds1.Tables[1].Columns.Add("TBL", typeof(string));
            ds1.Tables[1].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[1].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[1].Columns.Add("DiseaseID", typeof(int));
            ds1.Tables[1].Columns.Add("DiagnosisType", typeof(int));
            ds1.Tables[1].Columns.Add("DiseaseName", typeof(string));
            ds1.Tables[1].Columns.Add("Code", typeof(string));
            ds1.Tables[1].Columns.Add("PatientID", typeof(int));
            ds1.Tables[1].Columns.Add("AdmissionID", typeof(int));
            ds1.Tables[1].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[1].Columns.Add("MODDATE", typeof(DateTime));
            ds1.Tables[1].Columns.Add("ProblemPointID", typeof(int));
            ds1.Tables[1].Columns.Add("ProblemPointName", typeof(string));
            ds1.Tables[1].Columns.Add("PointValue", typeof(int));
            ds1.Tables[1].Columns.Add("Username", typeof(string));
            ds1.Tables[1].Columns.Add("AssessmentID", typeof(int));
            ds1.Tables[1].Columns.Add("userid", typeof(int));
            ds1.Tables[1].Columns.Add("DiagonosisTypeID", typeof(int));
            ds1.Tables[1].Columns.Add("IsPsd", typeof(int));
            ds1.Tables[1].Columns.Add("Blocked", typeof(int));
            ds1.Tables[1].Columns.Add("DiagonosisType", typeof(string));
            ds1.Tables[1].Columns.Add("IsPfVisible", typeof(int));
            ds1.Tables[1].Columns.Add("SignificantData", typeof(bool));
            ds1.Tables[1].Columns.Add("Remarks", typeof(string));
            ds1.Tables[1].Columns.Add("IsAdmitDisease", typeof(bool));
            ds1.Tables[1].Columns.Add("ExpectedLengthOfStay", typeof(int));
            ds1.Tables[1].Columns.Add("DoctorId", typeof(int));
            ds1.Tables[1].Columns.Add("DiagnosisStatus", typeof(string));

            ds1.Tables[2].Columns.Add("TBL", typeof(string));
            ds1.Tables[2].Columns.Add("GenericID", typeof(int));
            ds1.Tables[2].Columns.Add("Remark", typeof(string));
            ds1.Tables[2].Columns.Add("GenericName", typeof(string));
            ds1.Tables[2].Columns.Add("GenericName2L", typeof(string));
            ds1.Tables[2].Columns.Add("FROMdate", typeof(DateTime));
            ds1.Tables[2].Columns.Add("todate", typeof(DateTime));
            ds1.Tables[2].Columns.Add("Description", typeof(string));
            ds1.Tables[2].Columns.Add("IsNotActive", typeof(bool));
            ds1.Tables[2].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[2].Columns.Add("IPID", typeof(int));
            ds1.Tables[2].Columns.Add("AllergieTypes", typeof(string));
            ds1.Tables[2].Columns.Add("Doctorid", typeof(int));
            ds1.Tables[2].Columns.Add("IsPfVisible", typeof(Boolean));
            ds1.Tables[2].Columns.Add("Status", typeof(int));


            ds1.Tables[3].Columns.Add("TBL", typeof(string));
            ds1.Tables[3].Columns.Add("EpisodeName", typeof(string));
            ds1.Tables[3].Columns.Add("DurationOfIllness", typeof(string));
            ds1.Tables[3].Columns.Add("Height", typeof(Decimal));
            ds1.Tables[3].Columns.Add("Weight", typeof(Decimal));
            ds1.Tables[3].Columns.Add("PainScoreID", typeof(int));
            ds1.Tables[3].Columns.Add("IsEducated", typeof(bool));
            ds1.Tables[3].Columns.Add("IsSmoke", typeof(bool));
            ds1.Tables[3].Columns.Add("IsPregnent", typeof(Boolean));
            ds1.Tables[3].Columns.Add("ExpectedDeliveryDate", typeof(DateTime));
            ds1.Tables[3].Columns.Add("IsPatientDrugAlleric", typeof(bool));
            ds1.Tables[3].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[3].Columns.Add("BodyMassID", typeof(string));
            ds1.Tables[3].Columns.Add("DoctorName", typeof(string));
            ds1.Tables[3].Columns.Add("PainScore", typeof(string));
            ds1.Tables[3].Columns.Add("HeadCircumference", typeof(Decimal));
            ds1.Tables[3].Columns.Add("IsoldVisit", typeof(Boolean));
            ds1.Tables[3].Columns.Add("Pregnancy", typeof(Boolean));
            ds1.Tables[3].Columns.Add("Lactation", typeof(Boolean));
            ds1.Tables[3].Columns.Add("TriSemester", typeof(Boolean));
            ds1.Tables[3].Columns.Add("PlannedDischargeDate", typeof(DateTime));
            ds1.Tables[3].Columns.Add("LMP", typeof(DateTime));
            ds1.Tables[3].Columns.Add("CTASScoreColorID", typeof(int));
            ds1.Tables[3].Columns.Add("Score", typeof(string));
            ds1.Tables[3].Columns.Add("Color", typeof(string));
            ds1.Tables[3].Columns.Add("CTASDescription", typeof(string));

            ds1.Tables[4].Columns.Add("TBL", typeof(string));
            ds1.Tables[4].Columns.Add("ServiceTypeID", typeof(int));
            ds1.Tables[4].Columns.Add("PrescriptionID", typeof(int));
            ds1.Tables[4].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[4].Columns.Add("PrescriptionDate", typeof(DateTime));
            ds1.Tables[4].Columns.Add("DoctorID", typeof(int));
            ds1.Tables[4].Columns.Add("Doctorname", typeof(string));
            ds1.Tables[4].Columns.Add("ItemSequence", typeof(int));
            ds1.Tables[4].Columns.Add("ItemID", typeof(int));
            ds1.Tables[4].Columns.Add("ItemName", typeof(string));
            ds1.Tables[4].Columns.Add("Dose", typeof(int));
            ds1.Tables[4].Columns.Add("DoseID", typeof(int));
            ds1.Tables[4].Columns.Add("DoseUoM", typeof(int));
            ds1.Tables[4].Columns.Add("FrequencyID", typeof(int));
            ds1.Tables[4].Columns.Add("Frequency", typeof(int));
            ds1.Tables[4].Columns.Add("Duration", typeof(int));
            ds1.Tables[4].Columns.Add("DurationID", typeof(int));
            ds1.Tables[4].Columns.Add("DurationUOM", typeof(int));
            ds1.Tables[4].Columns.Add("StartFrom", typeof(int));
            ds1.Tables[4].Columns.Add("Remarks", typeof(string));
            ds1.Tables[4].Columns.Add("SpecimenID", typeof(int));
            ds1.Tables[4].Columns.Add("SpecimenName", typeof(string));
            ds1.Tables[4].Columns.Add("Status", typeof(int));
            ds1.Tables[4].Columns.Add("Quantity", typeof(int));
            ds1.Tables[4].Columns.Add("UserName", typeof(string));
            ds1.Tables[4].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[4].Columns.Add("MODDATE", typeof(DateTime));
            ds1.Tables[4].Columns.Add("UCAFApproval", typeof(Boolean));
            ds1.Tables[4].Columns.Add("USERID", typeof(int));
            ds1.Tables[4].Columns.Add("Itemstatus", typeof(int));
            ds1.Tables[4].Columns.Add("admissionid", typeof(int));
            ds1.Tables[4].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[4].Columns.Add("TestOrderItemID", typeof(int));
            ds1.Tables[4].Columns.Add("TestOrderID", typeof(int));
            ds1.Tables[4].Columns.Add("SpecialiseID", typeof(int));
            ds1.Tables[4].Columns.Add("Specialisation", typeof(string));
            ds1.Tables[4].Columns.Add("ResultStatus", typeof(int));
            ds1.Tables[4].Columns.Add("ResultStatusName", typeof(string));
            ds1.Tables[4].Columns.Add("IsPfVisible", typeof(Boolean));
            ds1.Tables[4].Columns.Add("ItemCode", typeof(string));
            ds1.Tables[4].Columns.Add("Isresult", typeof(bool));
            ds1.Tables[4].Columns.Add("OrderType", typeof(string));

            ds1.Tables[5].Columns.Add("TBL", typeof(string));
            ds1.Tables[5].Columns.Add("ServiceTypeID", typeof(int));
            ds1.Tables[5].Columns.Add("PrescriptionID", typeof(int));
            ds1.Tables[5].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[5].Columns.Add("PrescriptionDate", typeof(DateTime));
            ds1.Tables[5].Columns.Add("DoctorID", typeof(int));
            ds1.Tables[5].Columns.Add("DoctorName", typeof(string));
            ds1.Tables[5].Columns.Add("ItemSequence", typeof(int));
            ds1.Tables[5].Columns.Add("ItemID", typeof(int));
            ds1.Tables[5].Columns.Add("ItemName", typeof(string));
            ds1.Tables[5].Columns.Add("Dose", typeof(Decimal));
            ds1.Tables[5].Columns.Add("DoseID", typeof(int));
            ds1.Tables[5].Columns.Add("DoseUoM", typeof(string));
            ds1.Tables[5].Columns.Add("FrequencyID", typeof(int));
            ds1.Tables[5].Columns.Add("Frequency", typeof(string));
            ds1.Tables[5].Columns.Add("Duration", typeof(int));
            ds1.Tables[5].Columns.Add("DurationID", typeof(int));
            ds1.Tables[5].Columns.Add("DurationUOM", typeof(string));
            ds1.Tables[5].Columns.Add("StartFrom", typeof(DateTime));
            ds1.Tables[5].Columns.Add("Remarks", typeof(string));
            ds1.Tables[5].Columns.Add("SpecimenID", typeof(int));
            ds1.Tables[5].Columns.Add("SpecimenName", typeof(int));
            ds1.Tables[5].Columns.Add("Status", typeof(int));
            ds1.Tables[5].Columns.Add("AdmissionID", typeof(int));
            ds1.Tables[5].Columns.Add("AdmissionNumber", typeof(string));
            ds1.Tables[5].Columns.Add("AdmRouteID", typeof(int));
            ds1.Tables[5].Columns.Add("AdmRoute", typeof(string));
            ds1.Tables[5].Columns.Add("UserName", typeof(string));
            ds1.Tables[5].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[5].Columns.Add("ModDate", typeof(DateTime));
            ds1.Tables[5].Columns.Add("blocked", typeof(int));
            ds1.Tables[5].Columns.Add("CustomizedFlag", typeof(Boolean));
            ds1.Tables[5].Columns.Add("CustDrugDetails", typeof(string));
            ds1.Tables[5].Columns.Add("FrequencyQTY", typeof(Decimal));
            ds1.Tables[5].Columns.Add("IssueUOMValue", typeof(int));
            ds1.Tables[5].Columns.Add("IsNarcotic", typeof(int));
            ds1.Tables[5].Columns.Add("IsAntibiotic", typeof(int));
            ds1.Tables[5].Columns.Add("IsControledDrug", typeof(int));
            ds1.Tables[5].Columns.Add("IsDisPrescription", typeof(Boolean));
            ds1.Tables[5].Columns.Add("UCAFApproval", typeof(Boolean));
            ds1.Tables[5].Columns.Add("GenericID", typeof(int));
            ds1.Tables[5].Columns.Add("GenericName", typeof(string));
            ds1.Tables[5].Columns.Add("Strength", typeof(Decimal));
            ds1.Tables[5].Columns.Add("StrengthUOM", typeof(string));
            ds1.Tables[5].Columns.Add("ScheduleTime", typeof(string));
            ds1.Tables[5].Columns.Add("PrescriptionNo", typeof(string));
            ds1.Tables[5].Columns.Add("Remarks1", typeof(string));
            ds1.Tables[5].Columns.Add("StrengthUoMID", typeof(int));
            ds1.Tables[5].Columns.Add("ENDDatetime", typeof(DateTime));
            ds1.Tables[5].Columns.Add("DiscontinuingRemarks", typeof(string));
            ds1.Tables[5].Columns.Add("IsAdverseDrug", typeof(int));
            ds1.Tables[5].Columns.Add("AdverseDrugDescription", typeof(string));
            ds1.Tables[5].Columns.Add("ENDDate", typeof(DateTime));
            ds1.Tables[5].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[5].Columns.Add("PatientInstructions", typeof(string));
            ds1.Tables[5].Columns.Add("IsPfvisible", typeof(Boolean));
            ds1.Tables[5].Columns.Add("OrderStatus", typeof(int));
            ds1.Tables[5].Columns.Add("PrescriptionStatusID", typeof(int));
            ds1.Tables[5].Columns.Add("ItemCode", typeof(string));
            ds1.Tables[5].Columns.Add("PrescriptionStatus", typeof(int));
            ds1.Tables[5].Columns.Add("PrescriptionModDate", typeof(DateTime));
            ds1.Tables[5].Columns.Add("TOTIssuingQty", typeof(int));
            ds1.Tables[5].Columns.Add("TotIssuingQtyUOM", typeof(string));

            ds1.Tables[6].Columns.Add("RegCode", typeof(string));
            ds1.Tables[6].Columns.Add("PatientName", typeof(string));
            ds1.Tables[6].Columns.Add("Gender", typeof(string));
            ds1.Tables[6].Columns.Add("MobileNo", typeof(string));
            ds1.Tables[6].Columns.Add("HospitalName", typeof(string));
            ds1.Tables[6].Columns.Add("FullAge", typeof(string));
            ds1.Tables[6].Columns.Add("Nationality", typeof(string));
            ds1.Tables[6].Columns.Add("FamilyHeadId", typeof(string));
            ds1.Tables[6].Columns.Add("IsContrastAllergic", typeof(Boolean));
            ds1.Tables[6].Columns.Add("DoctorName", typeof(string));
            ds1.Tables[6].Columns.Add("Admitdate", typeof(string));
            ds1.Tables[6].Columns.Add("CompanyName", typeof(string));
            ds1.Tables[6].Columns.Add("DocSpecDepartment", typeof(string));
            ds1.Tables[6].Columns.Add("DocSign", typeof(string));
            ds1.Tables[6].Columns.Add("DocCode", typeof(string));


            ds1.Tables[7].Columns.Add("TBL", typeof(string));
            ds1.Tables[7].Columns.Add("ServiceTypeID", typeof(int));
            ds1.Tables[7].Columns.Add("PrescriptionID", typeof(int));
            ds1.Tables[7].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[7].Columns.Add("PrescriptionDate", typeof(DateTime));
            ds1.Tables[7].Columns.Add("DoctorID", typeof(int));
            ds1.Tables[7].Columns.Add("Doctorname", typeof(string));
            ds1.Tables[7].Columns.Add("ItemSequence", typeof(int));
            ds1.Tables[7].Columns.Add("ItemID", typeof(int));
            ds1.Tables[7].Columns.Add("ItemName", typeof(string));
            ds1.Tables[7].Columns.Add("Dose", typeof(int));
            ds1.Tables[7].Columns.Add("DoseID", typeof(int));
            ds1.Tables[7].Columns.Add("DoseUoM", typeof(int));
            ds1.Tables[7].Columns.Add("FrequencyID", typeof(int));
            ds1.Tables[7].Columns.Add("Frequency", typeof(int));
            ds1.Tables[7].Columns.Add("Duration", typeof(int));
            ds1.Tables[7].Columns.Add("DurationID", typeof(int));
            ds1.Tables[7].Columns.Add("DurationUOM", typeof(int));
            ds1.Tables[7].Columns.Add("StartFrom", typeof(int));
            ds1.Tables[7].Columns.Add("Remarks", typeof(string));
            ds1.Tables[7].Columns.Add("SpecimenID", typeof(int));
            ds1.Tables[7].Columns.Add("SpecimenName", typeof(string));
            ds1.Tables[7].Columns.Add("Status", typeof(int));
            ds1.Tables[7].Columns.Add("Quantity", typeof(int));
            ds1.Tables[7].Columns.Add("UserName", typeof(string));
            ds1.Tables[7].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[7].Columns.Add("MODDATE", typeof(DateTime));
            ds1.Tables[7].Columns.Add("UCAFApproval", typeof(bool));
            ds1.Tables[7].Columns.Add("USERID", typeof(int));
            ds1.Tables[7].Columns.Add("SpecialiseID", typeof(int));
            ds1.Tables[7].Columns.Add("Specialisation", typeof(string));
            ds1.Tables[7].Columns.Add("WorkstationName", typeof(string));
            ds1.Tables[7].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[7].Columns.Add("ItemStatus", typeof(int));
            ds1.Tables[7].Columns.Add("TestOrderItemID", typeof(int));
            ds1.Tables[7].Columns.Add("TestOrderID", typeof(int));
            ds1.Tables[7].Columns.Add("ResultStatus", typeof(int));
            ds1.Tables[7].Columns.Add("ResultStatusName", typeof(string));
            ds1.Tables[7].Columns.Add("IsPfVisible", typeof(bool));
            ds1.Tables[7].Columns.Add("ItemCode", typeof(string));
            ds1.Tables[7].Columns.Add("AdmissionID", typeof(int));
            ds1.Tables[7].Columns.Add("IsTreatmentplanApplicable", typeof(bool));
            ds1.Tables[7].Columns.Add("OrderType", typeof(string));

            ds1.Tables[8].Columns.Add("TBL", typeof(string));
            ds1.Tables[8].Columns.Add("SurgeryRequestId", typeof(int));
            ds1.Tables[8].Columns.Add("PatientId", typeof(int));
            ds1.Tables[8].Columns.Add("PatientName", typeof(string));
            ds1.Tables[8].Columns.Add("RegCode", typeof(string));
            ds1.Tables[8].Columns.Add("Age", typeof(string));
            ds1.Tables[8].Columns.Add("Gender", typeof(string));
            ds1.Tables[8].Columns.Add("Age_x002F_Gender", typeof(string));
            ds1.Tables[8].Columns.Add("SurgeryRequesteDby", typeof(int));
            ds1.Tables[8].Columns.Add("ProcedureID", typeof(int));
            ds1.Tables[8].Columns.Add("ProcedureName", typeof(string));
            ds1.Tables[8].Columns.Add("payerid", typeof(int));
            ds1.Tables[8].Columns.Add("ReceiverID", typeof(int));
            ds1.Tables[8].Columns.Add("GradeID", typeof(int));
            ds1.Tables[8].Columns.Add("GradeName", typeof(string));
            ds1.Tables[8].Columns.Add("TaskStatus", typeof(int));
            ds1.Tables[8].Columns.Add("StatusName", typeof(string));
            ds1.Tables[8].Columns.Add("StatusColor", typeof(int));
            ds1.Tables[8].Columns.Add("MonitorId", typeof(int));
            ds1.Tables[8].Columns.Add("VisitId", typeof(int));
            ds1.Tables[8].Columns.Add("IpId", typeof(int));
            ds1.Tables[8].Columns.Add("Remarks", typeof(string));
            ds1.Tables[8].Columns.Add("ScheduleId", typeof(int));
            ds1.Tables[8].Columns.Add("OrderId", typeof(int));
            ds1.Tables[8].Columns.Add("ServiceId", typeof(int));
            ds1.Tables[8].Columns.Add("Reqconsentteken", typeof(Boolean));
            ds1.Tables[8].Columns.Add("InstructionType", typeof(int));
            ds1.Tables[8].Columns.Add("ProcedureTypeid", typeof(int));
            ds1.Tables[8].Columns.Add("ReqconsenttekEndate", typeof(DateTime));
            ds1.Tables[8].Columns.Add("Consenttaken", typeof(Boolean));
            ds1.Tables[8].Columns.Add("ConsenttakEndate", typeof(DateTime));
            ds1.Tables[8].Columns.Add("SurgeryType", typeof(int));
            ds1.Tables[8].Columns.Add("Tentativesurgeryplaned", typeof(DateTime));
            ds1.Tables[8].Columns.Add("TentavtiveStartTime", typeof(string));
            ds1.Tables[8].Columns.Add("Estimatedtimeduration", typeof(string));
            ds1.Tables[8].Columns.Add("Anesthesiologistinstructions", typeof(string));
            ds1.Tables[8].Columns.Add("Otinstructions", typeof(string));
            ds1.Tables[8].Columns.Add("WardsInstructions", typeof(string));
            ds1.Tables[8].Columns.Add("Anesthesiologist", typeof(int));
            ds1.Tables[8].Columns.Add("EquipmentsurgerYId", typeof(int));
            ds1.Tables[8].Columns.Add("Implantsurgeryid", typeof(int));
            ds1.Tables[8].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[8].Columns.Add("ModDate", typeof(DateTime));
            ds1.Tables[8].Columns.Add("EndDate", typeof(DateTime));
            ds1.Tables[8].Columns.Add("Blocked", typeof(int));
            ds1.Tables[8].Columns.Add("Routid", typeof(int));
            ds1.Tables[8].Columns.Add("Userid", typeof(int));
            ds1.Tables[8].Columns.Add("Workstationid", typeof(int));
            ds1.Tables[8].Columns.Add("Status", typeof(int));
            ds1.Tables[8].Columns.Add("Rowseq", typeof(string));
            ds1.Tables[8].Columns.Add("Isprimary", typeof(int));
            ds1.Tables[8].Columns.Add("Priority", typeof(int));
            ds1.Tables[8].Columns.Add("Doctorname", typeof(string));
            ds1.Tables[8].Columns.Add("DoctorQualification", typeof(string));
            ds1.Tables[8].Columns.Add("DoctorDesignation", typeof(string));
            ds1.Tables[8].Columns.Add("Surgeon", typeof(string));
            ds1.Tables[8].Columns.Add("SurgeonQualification", typeof(string));
            ds1.Tables[8].Columns.Add("SurgeonSignature", typeof(string));
            ds1.Tables[8].Columns.Add("SurgeonDesignation", typeof(string));
            ds1.Tables[8].Columns.Add("Anestheologistname", typeof(string));
            ds1.Tables[8].Columns.Add("AnestheologistQualification", typeof(string));
            ds1.Tables[8].Columns.Add("AnestheologistSignature", typeof(string));
            ds1.Tables[8].Columns.Add("AnestheologistDesignation", typeof(string));
            ds1.Tables[8].Columns.Add("PACDateTime", typeof(DateTime));
            ds1.Tables[8].Columns.Add("Facilityid", typeof(int));
            ds1.Tables[8].Columns.Add("ScheduleDate", typeof(string));
            ds1.Tables[8].Columns.Add("IsInfected", typeof(Boolean));
            ds1.Tables[8].Columns.Add("InfectionDetails", typeof(string));
            ds1.Tables[8].Columns.Add("CancelRemarks", typeof(string));
            ds1.Tables[8].Columns.Add("DoctorID", typeof(int));
            ds1.Tables[8].Columns.Add("ISPatientFit", typeof(Boolean));
            ds1.Tables[8].Columns.Add("SurgeonID", typeof(int));
            ds1.Tables[8].Columns.Add("IsApprovedByBilling", typeof(Boolean));
            ds1.Tables[8].Columns.Add("BillingRemarks", typeof(string));
            ds1.Tables[8].Columns.Add("BillingRemarksEnteredDate", typeof(DateTime));
            ds1.Tables[8].Columns.Add("StartTime", typeof(string));
            ds1.Tables[8].Columns.Add("ENDTime", typeof(string));
            ds1.Tables[8].Columns.Add("FROMSlot", typeof(int));
            ds1.Tables[8].Columns.Add("ToSlot", typeof(int));
            ds1.Tables[8].Columns.Add("Primarydoctorname", typeof(string));
            ds1.Tables[8].Columns.Add("OTReceptionFacilityId", typeof(int));
            ds1.Tables[8].Columns.Add("OTReceptionFacility", typeof(string));
            ds1.Tables[8].Columns.Add("ProcedureCode", typeof(string));

            ds1.Tables[9].Columns.Add("TBL", typeof(string));
            ds1.Tables[9].Columns.Add("ComponentID", typeof(int));
            ds1.Tables[9].Columns.Add("Value", typeof(string));
            ds1.Tables[9].Columns.Add("ComponentName", typeof(string));
            ds1.Tables[9].Columns.Add("AssessmentID", typeof(int));
            ds1.Tables[9].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[9].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[9].Columns.Add("AdmissionID", typeof(int));
            ds1.Tables[9].Columns.Add("PreviewHTML", typeof(string));
            ds1.Tables[9].Columns.Add("CSTemplateID", typeof(int));

            ds1.Tables[10].Columns.Add("TBL", typeof(string));
            ds1.Tables[10].Columns.Add("Value", typeof(string));
            ds1.Tables[10].Columns.Add("Datetime", typeof(DateTime));
            ds1.Tables[10].Columns.Add("Vital", typeof(string));
            ds1.Tables[10].Columns.Add("UOM", typeof(string));
            ds1.Tables[10].Columns.Add("MINVALUE", typeof(Decimal));
            ds1.Tables[10].Columns.Add("MAXVALUE", typeof(Decimal));
            ds1.Tables[10].Columns.Add("AssessmentID", typeof(int));
            ds1.Tables[10].Columns.Add("UserName", typeof(string));
            ds1.Tables[10].Columns.Add("USERID", typeof(int));
            ds1.Tables[10].Columns.Add("IsPsd", typeof(int));
            ds1.Tables[10].Columns.Add("DoctorID", typeof(int));
            ds1.Tables[10].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[10].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[10].Columns.Add("Comments", typeof(string));
            ds1.Tables[10].Columns.Add("ArchiveModdate", typeof(DateTime));
            ds1.Tables[10].Columns.Add("Status", typeof(int));
            ds1.Tables[10].Columns.Add("IsPfVisible", typeof(int));
            ds1.Tables[10].Columns.Add("VITALSIGNID", typeof(int));
            ds1.Tables[10].Columns.Add("Sequence", typeof(bool));
            ds1.Tables[10].Columns.Add("VitalSignDate", typeof(DateTime));

            ds1.Tables[11].Columns.Add("PatientID", typeof(int));
            ds1.Tables[11].Columns.Add("IPID", typeof(int));
            ds1.Tables[11].Columns.Add("PatientName", typeof(string));
            ds1.Tables[11].Columns.Add("AdmissionNumber", typeof(string));
            ds1.Tables[11].Columns.Add("BloodorderID", typeof(int));
            ds1.Tables[11].Columns.Add("ComponentID", typeof(int));
            ds1.Tables[11].Columns.Add("Component", typeof(string));
            ds1.Tables[11].Columns.Add("Quantity", typeof(int));
            ds1.Tables[11].Columns.Add("RequiredDate", typeof(DateTime));
            ds1.Tables[11].Columns.Add("Remarks", typeof(string));
            ds1.Tables[11].Columns.Add("Volume", typeof(int));
            ds1.Tables[11].Columns.Add("Type", typeof(int));
            ds1.Tables[11].Columns.Add("Componentcode", typeof(string));


            ds1.Tables[12].Columns.Add("TBL", typeof(string));
            ds1.Tables[12].Columns.Add("FoodID", typeof(int));
            ds1.Tables[12].Columns.Add("Remark", typeof(string));
            ds1.Tables[12].Columns.Add("FdIngrName", typeof(string));
            ds1.Tables[12].Columns.Add("FdIngrName2L", typeof(string));
            ds1.Tables[12].Columns.Add("FROMdate", typeof(DateTime));
            ds1.Tables[12].Columns.Add("todate", typeof(DateTime));
            ds1.Tables[12].Columns.Add("Description", typeof(string));
            ds1.Tables[12].Columns.Add("IsNotActive", typeof(bool));
            ds1.Tables[12].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[12].Columns.Add("IPID", typeof(int));
            ds1.Tables[12].Columns.Add("IsPfVisible", typeof(Boolean));
            ds1.Tables[12].Columns.Add("AllergieTypes", typeof(string));
            ds1.Tables[12].Columns.Add("Doctorid", typeof(int));
            ds1.Tables[12].Columns.Add("Status", typeof(int));

            ds1.Tables[13].Columns.Add("TBL", typeof(string));
            ds1.Tables[13].Columns.Add("ID", typeof(int));
            ds1.Tables[13].Columns.Add("Allergy", typeof(string));
            ds1.Tables[13].Columns.Add("Allergy2L", typeof(string));
            ds1.Tables[13].Columns.Add("FROMdate", typeof(DateTime));
            ds1.Tables[13].Columns.Add("todate", typeof(DateTime));
            ds1.Tables[13].Columns.Add("Description", typeof(string));
            ds1.Tables[13].Columns.Add("IsNotActive", typeof(Boolean));
            ds1.Tables[13].Columns.Add("CreateDate", typeof(DateTime));
            ds1.Tables[13].Columns.Add("IPID", typeof(int));
            ds1.Tables[13].Columns.Add("AllergieTypes", typeof(string));
            ds1.Tables[13].Columns.Add("Doctorid", typeof(int));
            ds1.Tables[13].Columns.Add("IsPfVisible", typeof(Boolean));
            ds1.Tables[13].Columns.Add("Status", typeof(int));

            ds1.Tables[14].Columns.Add("TBL", typeof(string));
            ds1.Tables[14].Columns.Add("ReferralOrderID", typeof(int));
            ds1.Tables[14].Columns.Add("ReferralTypeID", typeof(int));
            ds1.Tables[14].Columns.Add("ReferralType", typeof(string));
            ds1.Tables[14].Columns.Add("SpecialiseID", typeof(int));
            ds1.Tables[14].Columns.Add("Specialisation", typeof(string));
            ds1.Tables[14].Columns.Add("FromDoctorID", typeof(int));
            ds1.Tables[14].Columns.Add("Priority", typeof(int));
            ds1.Tables[14].Columns.Add("Remarks", typeof(string));
            ds1.Tables[14].Columns.Add("DoctorID", typeof(int));
            ds1.Tables[14].Columns.Add("DoctornameName", typeof(string));
            ds1.Tables[14].Columns.Add("AdmissionID", typeof(int));
            ds1.Tables[14].Columns.Add("MonitorID", typeof(int));
            ds1.Tables[14].Columns.Add("HospitalID", typeof(int));
            ds1.Tables[14].Columns.Add("IsDirect", typeof(Boolean));
            ds1.Tables[14].Columns.Add("ExReferDoctorID", typeof(int));
            ds1.Tables[14].Columns.Add("ExReferDoctor", typeof(string));
            ds1.Tables[14].Columns.Add("adviceid", typeof(int));
            ds1.Tables[14].Columns.Add("Feedback", typeof(string));
            ds1.Tables[14].Columns.Add("refusalremarks", typeof(string));
            ds1.Tables[14].Columns.Add("Reasonid", typeof(int));
            ds1.Tables[14].Columns.Add("Reason", typeof(string));
            ds1.Tables[14].Columns.Add("duration", typeof(int));
            ds1.Tables[14].Columns.Add("IsInternalReferral", typeof(Boolean));
            ds1.Tables[14].Columns.Add("Blocked", typeof(int));
            ds1.Tables[14].Columns.Add("Status", typeof(int));
            ds1.Tables[14].Columns.Add("MonitorDate", typeof(DateTime));
            ds1.Tables[14].Columns.Add("IPID", typeof(int));
            ds1.Tables[14].Columns.Add("DoctorID1", typeof(int));

            return ds1;
        }

        private string GetFTPReport(string FTPServer, string FTPFolder, string TestOrderId, string ReportLocation, string pdfName)
        {
            pdfName = pdfName.Substring(1, pdfName.Length - 1);
           
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + FTPServer + "/" + FTPFolder + "/" + pdfName);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            using (Stream ftpstream = request.GetResponse().GetResponseStream())
            using (Stream filestream = File.Create(ReportLocation + TestOrderId + ".pdf"))
            {
                ftpstream.CopyTo(filestream);
            }
            return "";
        }

    }
}