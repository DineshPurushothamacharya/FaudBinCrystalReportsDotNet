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
        //https://localhost:44351/api/reportapi?TestOrderID=1426994&UHID=PFBS.0000397737&isExternal=true
        //http://172.16.16.53/api/reportapi?TestOrderID=1358599&UHID=PFBS.0000397737&isExternal=true
        // GET api/<controller>/5
        //[Route("api/reportapi/TestOrderID")]
        public string Get(string TestOrderID, string UHID, bool isExternal)
        {
            DataSet ds = new DataSet();
            DataSet dsResultNew = new DataSet();
            DataSet dsHeader = new DataSet();

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


    }
}