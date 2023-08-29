using HISDataAccess;
using Reports.AdminServiceClient;
using Reports.BillingCalculationServiceClient;
using Reports.BillingFacadeServiceClient;
using Reports.Common;
using Reports.CommonSearchServiceClient;
using Reports.ContractMgmtServiceClient;
using Reports.FrontOfficeServiceClient;
using System;
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

namespace Reports.BusinessLogic
{
    public class PaymentsClass
    {
        #region Variables Defined
        ResourceManager rm = new ResourceManager("DateStrings", Assembly.GetExecutingAssembly());

        static string MODULE_NAME = "PatientBillPayment";
        const int DEFAULTWORKSTATION = 0;
        static String strConnString = ConfigurationManager.AppSettings["DBConnectionStringMasters"].ToString();
        static String strDefWorkstationId = ConfigurationManager.AppSettings["DefaultWorkstationId"].ToString();
        static String strDefaultUserId = ConfigurationManager.AppSettings["DefaultUserId"].ToString();
        //static String strDefaultHospitalId = ConfigurationManager.AppSettings["DefaultHospitalId"].ToString();
        static String strDefaultHospitalId = string.Empty;
        SqlConnection conn = null;
        bool blnCreditpinBlock = false; string hdnPatientType = "1";
        string ReturnMessage = string.Empty;
        DataTable dtCompanyblock = null;
        DataSet dsPayerforLOA1678 = null; DataSet dsPayerforLOA678 = null;
        DataTable dtCompanyContract; DataTable dtDocOrders = null; DataTable gdvSearchResultData = null;
        private static string strDataFormat = "dd-MMM-yyyy HH:mm";
        public static string strCurrencyFormat = "0.00";
        int intTariffID = -1; string strTariffID = string.Empty; string hdnTariffID;
        FrontOfficeServiceContractClient objFOClient = null;
        int intBillType = 1;//Default Cash
        string strBedType = "-1";// Default OPD 
        int intPriority = 0;
        string strProfChargeServiceId = "52";
        string strProfChargeInvServiceId = "56";
        string strProfChargeConsServiceId = "57";
        decimal BasePrice;
        decimal EligiblePrice;
        decimal BillablePrice;
        DataSet DsOtherOrders = new DataSet();
        DataTable dtScheduldOrders = null;
        DataTable dtReferralOrders = null;
        string hdnIPID = string.Empty; string hdnLetterIDforIPID = string.Empty;
        DataTable dtTestsProfile; DataTable TestProfile = null;
        DataTable dtTestSpecimen; DataTable DTTemp; DataTable DTTem; DataTable dtSelectedBillDetails = null;
        string hdnsCurrencyFormat = "###,###,###,##00.0000";
        string OpdocId = string.Empty;
        string hdnDocID = string.Empty;
        string hdnGradeSplzID = string.Empty;
        string hdnDocHospDeptId = string.Empty;
        string hdnProcedureID = string.Empty;
        string hdnDocSpecialiseId = string.Empty; string hdnPatientID = string.Empty; string docSpecId = string.Empty;
        bool hdnrblbilltypeCredit = false; bool hdnrblbilltypecash = false;
        bool hdnradordeTypeRoutine = true; bool hdnradordeTypeASAP = false; bool hdnradordeTypeStat = false;
        DataTable dtLOADetails = null;
        string hdnLOAApprovalID = string.Empty;
        string LOALetterID = string.Empty; string ViewStateLetterid = string.Empty;
        string VSParentLetterid = string.Empty; string hdnblnSaveDefaultLOA = string.Empty;
        string hdnLOAfollowupDays = string.Empty; DataTable dtLetter = null;
        string EMRCOPAY = string.Empty; DataTable HISCONFIG = null; string Doctorid = string.Empty;
        string strDummy, specilaize, hdnIntLetterIddatechanged;
        string hdnLOAfollowupLimit = "0";
        int IntLetterIddatechanged = 0;
        int LOAfollowupDays = 0;
        int LOAfollowupLimit = 0; string UHID = string.Empty;
        DataTable PredefinedDiscount;
        DataTable dtGradeValidation;
        string Followuplimit, CONSFollowupDAYS, CRDiscountOnCash, LatestIPID, SPLFOLLOWUPLMT, SPLFOLLOWUPDAYS, Letterid;
        static string FollowUpConsulStatus = "FollowUpConsulStatus";
        string hdnDelConfirm = string.Empty;
        DataTable dtConfollowup; int ConsBaseOrderType = 0; int BaseOrderType = 0; bool dt3 = false;
        int OrderTypeID = 0;//
        int BaseOrderTypeM = 0;
        string strGradeID = string.Empty;
        string strCompanyID = string.Empty; int TagId = 0;
        string MaxStr, Maxconsult, hdnIsFollowUp, hfConfirmNext;
        string DoctorAvailable = "NO";
        StringBuilder strMsg = new StringBuilder(""); bool HasDefaultLOA = false;
        string hdnHasDefaultLOA = string.Empty; DataTable dtPatientDetails; DataTable ViewStatedtPatientDetails; DataSet DsPatient = null;
        string strValidatePrfileAndPHCItems = string.Empty;
        string DoctorsConsultations = string.Empty; string FixedConsultation = string.Empty;
        DataTable dtConfollowupM = new DataTable(); bool CheckType; int FollowupDaysM = 0; int FollowupLimitM = 0; int intFollowupID = 0; int ParentLetterId = 0;
        int intLetterNo = 0; string WalkinBalance = string.Empty; DataTable Service = null;
        DataSet OtherOrders = null; DataTable LoaGrid = null; DataTable SchedOrders; DataTable SceduleDoctorAppointment;
        DataTable Exclusions = null;//DataTable ViewStatedtPatientDetails = null;
        DataTable dtLOAExpirydate; bool isConsReqForPHC = false; DataTable dtLOAItemStatus; DataTable dtLOAItems;
        string hdnDoctSearchName = "OPBillRefExtDocSearch";
        string hdnIsDefaultLOA = "false"; string strERExceedMSG, strERExceedMSG2L = string.Empty;
        DataTable ApprovalPendingItems = null;
        DataTable DTPatientData; string BillType = "0"; string hdnCollectableType = "0"; string hdnIsCardCollectable = "0"; int MaxCollectable = -1;
        string IPIDUHID = string.Empty; string PType = string.Empty; DataTable PatientDet = null; DataTable OPPackageOrders = null;
        DataSet dsTempOtherOrders = null; DataSet ServiceOrders = null; DataSet DsBillDetails = null;
        DataTable dtTempOthersTable; DataTable MiscDoctorEntired = null;
        DataTable dtOPPackageOrders = null; DataTable ScheduleDT = null; int CallingCardColl = 0; DataTable BedTypes = null;
        DataSet dsDeductables; DataSet DsOutputDeductable = null;
        DataTable dtDeductablesTable = null; DataTable BDPopUP = null; DataTable taxdiscount = null;
        DataTable dtDeductablesTable1 = null;
        DataTable dtDeductablesTable2 = null;
        DataTable dtDeductablesTable3 = null;
        DataTable dtDeductablesTable4 = null;
        DataTable dtBedTypes = null; string hdnConsultationMsg = string.Empty;
        DataSet DsOutPut = new DataSet(); string hdnOutputBillTpe = string.Empty; int CollectableType = 0; DataSet DsOutPutN;
        DataTable DtCompCSContribution = null;
        DataTable DtCompCRContribution = null;
        DataTable dtSummary = null;
        DataTable dtCRContribution = null; DataTable dtCashDiscounts = null; DataTable dtPackageItems = null;
        DataTable dtCompanyCashContribution = null;
        DataTable ViewStatedtCRContribution = null; 
        DataTable ViewStatedtCompanyCashContribution = null; 
        DataTable OPBAvailDisc = null;
        DataTable DtCompanyCashBLItemDetails = new DataTable();
        DataTable DtDiscountDetails = new DataTable();
        DataTable dtContDiscountDetails = new DataTable();
        DataTable DtDCOrders = new DataTable();
        DataTable BillDetails = null;
        DataSet dsDeductables_ColSplit = null;
        int intPatientType = 1;
        string hdnPVATValue = string.Empty; string hdnCVATValue = string.Empty;
        string hdnPVATValueforSP = string.Empty; string hdnMaxCollectable = string.Empty;
        string hdnVATAmount = string.Empty; DataTable dtDeductible = null;
        decimal hdnDepositAmount = 0; decimal BlanceAomunt = 0; decimal BAmount = 0; string txtamount = string.Empty;
        decimal TotalBalanceAomunt = 0; decimal TotalBalanceAomuntDeposit = 0;
        decimal patientamt = 0; bool blnLimitUtilization = false; decimal Limitbalance = 0; DataSet DsForUserMessages = new DataSet();
        string hdnCPAYAfterVAT = string.Empty; string hdnPPAYAfterVAT = string.Empty;
        DataTable BillSummary = null; DataTable ItemSplitDetails = null; DataTable AvailDiscount = null;
        string hdnordertypeID = string.Empty; string OrdertypeOtherorder = string.Empty; DataTable dtTable7;
        string hdnSPCMaternityConfig = string.Empty; string hdnEDD = string.Empty; string hdnIsPregnent = string.Empty; string hdnMaternityConfig = string.Empty;
        string PATMAXCOLL = string.Empty;
        int intConFollowupDays = 0;
        int intFollowuplimit = 0;
        int intBillMaxCollectable = -1;
        bool IsCardCollectable = false;
        bool IsDefaultLOA = false;
        DataTable dtLetterSeventh;
        DataTable dtLetterGenInfo;
        DataTable DtIncludedItems = null;
        DataSet dsRegistrationFee;
        DataTable dtItemPrices;
        DataSet ViewStatedsLOA = null; private bool blnSaveDefaultLOA = false;
        string hdnNationalityId = string.Empty; string hdnDiscountID = string.Empty;
        private DataTable dtDiscountDetails = new DataTable();
        string FilterQty = string.Empty;
        string Filter = string.Empty;
        private bool blnSkipDeptSpec = false;
        bool radPatient = false;
        bool radpayer = true;
        decimal PAmount = 0;
        decimal CAmount = 0;
        int patientAmount = 0;
        DataSet dsDiscConfig = null;
        DataTable dtServices;
        DataTable dtBillDetails = null; int ddlConsultatioType = 0; string ddlConsultatioTypeText = string.Empty;
        string hdnblnCOmpanyBlocked = string.Empty; string hdnblnCOmpanyBlockedReason = string.Empty; string hdnblnCompanyExpired = string.Empty; string hdnblnInsuranceExpired = string.Empty;
        DateTime dtpCardValid; string CompanyReturnMessage, CompanyReturnMessage2L = string.Empty; string SessionID = string.Empty;
        string OrderTypeVisit = string.Empty; string DocAvail = string.Empty; string hdnblnYes, hdnblnNo = string.Empty;
        string OrderTypeM = string.Empty; string ddlservices = "2"; bool IsAppliedProfessionalCharge = false;
        string hdnPrice = string.Empty; string Priority = string.Empty; string DocCode = string.Empty; string hdnordertypeName = string.Empty; string hdnintBillDocEmpId = string.Empty;
        DataTable invtTable = null; string hdnScheduleID = string.Empty; int ProfChargeServiceId; string ProfChargeServiceName; DataTable DTView = null; DataTable GridContents = null;
        string Servicefilter = " and ServiceId=";
        string Validation, ValidationProfile = string.Empty; DataTable MiscDoctorEntry = null;
        string OrderVisitType = string.Empty;
        DataTable dtPayments = new DataTable();
        DataTable dtMultiPayment = new DataTable(); int PatientOutstandingAmt = 0;
        DataTable dtSummaryTable = null; int intMOPoutstand = 0; decimal BillPayAmount;
        string ViewStateMultimode = string.Empty;
        string TotAmount = "0"; DataTable gdvsummary = null; string StrBillNo = string.Empty;
        string txtbillno, NewBillNo, BillNo, SPBillNO, TokenNO = string.Empty; string StrBillId = "";
        private const int intPackageID = 0;
        int intVisitType = 0;
        int intVisitId = 0;
        int intNewVisitId = 0;
        int intEpisodeId = 0; string strDiscRemarks = "";
        string hdnAge, hdnGenderID, hdAgeUomid, hdnTitleID, hdnMarStatusID, hdnDOB, hdnPhno, hdnNationalID, hdnFamilyName, lblAddress, hdnMLC = string.Empty;
        string ClaimFormNo = string.Empty; decimal decCashGiven = 0; int bitXml = 0; int DiscountTypeID = 0;
        string lblFirstName, lblMiddleName, lblLastName, hdnFirstName2L, hdnMiddleName2L, hdnLastName2L, hdnIsDirectPatient, hdnPOSTransactionNumber = string.Empty;
        string AlreadySavedLetterID, ActualPatientAmount, txtpayer = string.Empty; decimal cash = 0;
        string hdnServicePatientNumber = string.Empty;
        DataTable POSTransatctionResult = null; decimal DepositAvailed = 0; StringBuilder strbuilderBillNo = new StringBuilder();
        string TotalBalwithoutDeposit = string.Empty;
        #endregion
        //Sangamesh
        PatientBiillInfoList objPatientList = new PatientBiillInfoList();
        internal enum Database
        {
            Master = 1,
            Transaction = 2
        }
        public string FormatDate_DD_MMM_YY(string dateToFormat)
        {
            var date = dateToFormat;
            DateTime ScheduleDate = Convert.ToDateTime(date);
            string ScheduleDate_format = ScheduleDate.ToString("dd-MMM-yyyy");
            return ScheduleDate_format;
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

        public PatientBiillInfoList GettingPatientList(PatientBillList PatientBillList)
        {

            DataSet dsSearchResults = null;
            int PatId = 0; string strSearch = string.Empty;
            string strFilterCond = string.Empty; string PatientID = string.Empty; int intprev = 0; bool blnCreditpinBlock = false;
            DataSet DsPatientDetails = null; string hdnPackageUtil = string.Empty;
            string Letterid = string.Empty;
            string hdnIsCashBill = string.Empty;
            string hdnGradeFilterCond = string.Empty;
            bool HasDefaultLOA = false; string hdnHasDefaultLOA = string.Empty;
            string hdnIsfamilyLOA = string.Empty;
            int TagId = 0; string hdnPatientType = string.Empty; int intPatientType = 0; DataTable DtSch = null; DataTable DtSchRef;
            DataTable dtDtDoc = null; DataSet dsDtDoc = null;
            DataTable dtBankDetails = null; DataTable DtConsultationTypes;
            DataTable dtService;
            DataTable DtOtherOders; DataTable DtDocOrders; DataSet dsBankDetails = null; DataTable dtVisitDates;
            string hdnotheroders = string.Empty; string hdnspecialiseID = string.Empty;
            DataTable DtScheduleders;
            DataTable DtReferralOrders;
            DataTable DtSelectedSched;
            DataTable DtSelectedDocOrders;
            ArrayList intarray = new ArrayList(2);
            int intUserID = Convert.ToInt32(strDefaultUserId); int intWorkStationid = Convert.ToInt32(strDefWorkstationId);
            strDefaultHospitalId = PatientBillList.HospitalId.ToString();
            string StrFinalBillNo = string.Empty; DataTable DtBillSummary = null;
            try
            {
                UHID = Utilities.GetUHID(Utilities.GetIACode(PatientBillList.RegCode.Trim()), Utilities.GetRegistrationNo(PatientBillList.RegCode.Trim()));
                hdnScheduleID = PatientBillList.ScheduleID.ToString();
                strSearch = "OPBillRegCodeSearch";
                string HISConfigValues = System.Configuration.ConfigurationManager.AppSettings["HISConfig"].ToString();
                FetchHisConfiguation(Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), PatientBillList, HISConfigValues);
                PatId = GetRootPatientIdPerformance(UHID, Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId));
                strFilterCond = " patBlocked=0 " + "AND  PatientID = " + PatId + "";
                string strUHIDFilterCond = string.Empty; string SearchName = string.Empty; string PatientUHID = string.Empty;
                SearchName = "IPBillPatientSearch";
                string strAdmissionNumber = string.Empty;
                PatientUHID = Utilities.GetUHID(Utilities.GetIACode(PatientBillList.RegCode.Trim()), Utilities.GetRegistrationNo(PatientBillList.RegCode.Trim()));
                strUHIDFilterCond = "RegCode is not null and RegCode = '" + PatientBillList.RegCode.Trim() + "' and Status in (0,2,3) and PatientType in(2,3,4) and Blocked = 0 and HospitalID='" + PatientBillList.HospitalId + "'";

                DataSet dsUhidSearchResults = CheckPatientIPorEMR(PatientBillList.RegCode.Trim(), PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList);
                if (dsUhidSearchResults.Tables[0].Rows.Count > 0)
                {
                    if (dsUhidSearchResults.Tables[0].Rows[0]["PatienttypeID"].ToString() == "2")
                    {
                        objPatientList.Code = (int)ProcessStatus.Success;
                        objPatientList.Status = ProcessStatus.Success.ToString();
                        objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["PatientcurrentlyIP"].ToString();
                        //objPatientList.Message = Resources.English.ResourceManager.GetString("PatientcurrentlyIP");
                        objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["PatientcurrentlyIP"].ToString();
                        //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("PatientcurrentlyIP");
                        return objPatientList;
                    }
                    if (dsUhidSearchResults.Tables[0].Rows[0]["PatienttypeID"].ToString() == "3")
                    {
                        objPatientList.Code = (int)ProcessStatus.Success;
                        objPatientList.Status = ProcessStatus.Success.ToString();
                        //objPatientList.Message = Resources.English.ResourceManager.GetString("PatientcurrentlyEMR");
                        objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["PatientcurrentlyEMR"].ToString();
                        //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("PatientcurrentlyEMR");
                        objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["PatientcurrentlyEMR"].ToString();
                        return objPatientList;
                    }
                }
                PatientID = PatId.ToString();
                hdnPatientID = PatId.ToString();
                string strRoleCheck = System.Configuration.ConfigurationManager.AppSettings["PatientOutstandingAproval"].ToString();
                if (strRoleCheck.ToUpper() == "YES")
                {
                    DataTable dtOutStand = FetchOutstandingAmount(PatId, 0).Tables[0];
                    if (dtOutStand.Rows.Count > 0)
                    {
                        intprev = Convert.ToInt32(dtOutStand.Compute("sum(PBalanceReceipt)", ""));
                    }
                    if (intprev > 0)
                    {
                        objPatientList.Code = (int)ProcessStatus.Success;
                        objPatientList.Status = ProcessStatus.Success.ToString();
                        //objPatientList.Message = Resources.English.ResourceManager.GetString("PatientOutstandingAmount") + Convert.ToDecimal(intprev.ToString()) + "";
                        objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["PatientOutstandingAmount"].ToString() + Convert.ToDecimal(intprev.ToString()) + "";
                        //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("PatientOutstandingAmount") + Convert.ToDecimal(intprev.ToString()) + "";
                        objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["PatientOutstandingAmount"].ToString() + Convert.ToDecimal(intprev.ToString()) + "";
                        return objPatientList;
                    }
                }

                if (CheckPinBlock(Convert.ToInt32(PatientID), PatientBillList) == false)
                {
                    if (blnCreditpinBlock == false)
                    {
                        objPatientList.Code = (int)ProcessStatus.Success;
                        objPatientList.Status = ProcessStatus.Success.ToString();
                        objPatientList.Message = ReturnMessage;
                        objPatientList.Message2L = ReturnMessage;
                        return objPatientList;
                    }

                }
                DsPatientDetails = FetchPatientDetails(PatientID, PatientBillList.RegCode.Trim(), false, Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, 0);
                if (DsPatientDetails.Tables[0].Rows.Count > 0)
                {
                    hdnNationalityId = DsPatientDetails.Tables[0].Rows[0]["NationalityID"].ToString();



                    if (DsPatientDetails.Tables[4].Rows.Count > 0)
                    {
                        if (DsPatientDetails.Tables[4].Columns.Contains("ActiveStatus") && DsPatientDetails.Tables[4].Columns.Contains("GradeBlocked"))
                        {
                            DataRow[] dr = DsPatientDetails.Tables[4].Select("ActiveStatus =1 and GradeBlocked=1");
                            if (dr.Length > 0)
                            {
                                if ((System.Configuration.ConfigurationManager.AppSettings["AllowCreditBillingIfGradeBlocked"] != null) && (System.Configuration.ConfigurationManager.AppSettings["AllowCreditBillingIfGradeBlocked"].ToString().ToUpper() == "NO"))
                                {
                                    dr[0]["ActiveStatus"] = 0;
                                    DsPatientDetails.Tables[4].AcceptChanges();
                                }
                                objPatientList.Code = (int)ProcessStatus.Success;
                                objPatientList.Status = ProcessStatus.Success.ToString();

                                //objPatientList.Message = Resources.English.ResourceManager.GetString("GradeBlocked");
                                //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("GradeBlocked");
                                objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["GradeBlocked"].ToString();
                                objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["GradeBlocked"].ToString();
                                return objPatientList;
                            }
                        }


                        if (DsPatientDetails.Tables[4].Columns.Contains("ActiveStatus"))
                        {
                            DataRow[] dr = DsPatientDetails.Tables[4].Select("ActiveStatus =1");
                            if (dr.Length > 0)
                            {
                                strCompanyID = dr[0]["PAYERID"].ToString();
                                strGradeID = dr[0]["GradeID"].ToString(); if (dr[0]["CardValidity"] != DBNull.Value)
                                {
                                    dtpCardValid = Convert.ToDateTime(dr[0]["CardValidity"]);
                                }

                            }
                        }
                        else
                        {
                            strCompanyID = DsPatientDetails.Tables[0].Rows[0]["Companyid"].ToString();
                            strGradeID = DsPatientDetails.Tables[0].Rows[0]["GradeID"].ToString();
                            if (DsPatientDetails.Tables[0].Columns.Contains("InsuranceCardExpiry") && DsPatientDetails.Tables[0].Rows[0]["InsuranceCardExpiry"] != DBNull.Value)
                            {
                                dtpCardValid = Convert.ToDateTime(DsPatientDetails.Tables[0].Rows[0]["InsuranceCardExpiry"]);
                            }
                        }
                    }
                    else
                    {
                        strCompanyID = DsPatientDetails.Tables[0].Rows[0]["Companyid"].ToString();
                        strGradeID = DsPatientDetails.Tables[0].Rows[0]["GradeID"].ToString();
                        if (DsPatientDetails.Tables[0].Columns.Contains("InsuranceCardExpiry") && DsPatientDetails.Tables[0].Rows[0]["InsuranceCardExpiry"] != DBNull.Value)
                        {
                            dtpCardValid = Convert.ToDateTime(DsPatientDetails.Tables[0].Rows[0]["InsuranceCardExpiry"]);
                        }
                    }
                    if (DsPatientDetails.Tables[5].Rows.Count > 0)
                    {
                        if (DsPatientDetails.Tables[5].Columns.Contains("Letterid") && DsPatientDetails.Tables[5].Rows[0]["Letterid"] != null)
                        {
                            DataRow[] dr = DsPatientDetails.Tables[5].Select("blocked=0");
                            if (dr.Length > 0)
                            {
                                ViewStateLetterid = dr[0]["Letterid"].ToString();
                            }
                        }
                    }
                }
                if (DsPatientDetails.Tables[0].Rows.Count > 0)
                {
                    #region package utilization need to implment

                    #endregion
                    dtPatientDetails = DsPatientDetails.Tables[0].Copy();
                    ViewStatedtPatientDetails = dtPatientDetails.Copy();
                }
                #region CheckingDetail Calling
                // need to implement
                #endregion

                if (!string.IsNullOrEmpty(strCompanyID.Trim()))
                {
                    hdnIsCashBill = "true";
                    int HospitalID = Convert.ToInt32(strDefaultHospitalId);
                    DataSet dsCompanyBlock = FetchHospitalCompanyDetails(Convert.ToInt32(strCompanyID), "C", "1,6,7,8", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, HospitalID);
                    dtCompanyblock = dsCompanyBlock.Tables[0].Copy();
                    dsPayerforLOA1678 = null;
                    dsPayerforLOA1678 = new DataSet();
                    dsPayerforLOA1678.Tables.Add(dsCompanyBlock.Tables["table1"].Copy());

                    dsPayerforLOA1678.Tables.Add(dsCompanyBlock.Tables["table2"].Copy());
                    dsPayerforLOA1678.Tables.Add(dsCompanyBlock.Tables["table3"].Copy());
                    dsPayerforLOA1678.Tables.Add(dsCompanyBlock.Tables["table4"].Copy());

                    dsPayerforLOA1678.AcceptChanges();
                    hdnrblbilltypeCredit = true;
                    hdnrblbilltypecash = false;

                    if (dsCompanyBlock.Tables[0].Rows.Count > 0)
                    {
                        FixedConsultation = dsCompanyBlock.Tables[0].Rows[0]["FixedConsultationCharge"].ToString();
                        DoctorsConsultations = dsCompanyBlock.Tables[0].Rows[0]["DoctorConsultationsPerDay"].ToString();
                    }
                    if (DsPatientDetails.Tables[4].Rows.Count > 0)
                    {
                        if (DsPatientDetails.Tables[4].Columns.Contains("ActiveStatus"))
                        {
                            DataRow[] dr = DsPatientDetails.Tables[4].Select("ActiveStatus =1");
                            if (dr.Length > 0)
                            {
                                if (dr[0]["CardValidity"] != DBNull.Value && Convert.ToInt32(strCompanyID) > 0)
                                {
                                    dtpCardValid = Convert.ToDateTime(dr[0]["CardValidity"]);
                                }
                            }
                        }
                    }

                    if (dsPayerforLOA1678.Tables[3].Rows.Count > 0)
                    {
                        if (dsPayerforLOA1678.Tables[3].Rows[0]["Followuplimits"].ToString() != "")
                        {
                            Followuplimit = dsPayerforLOA1678.Tables[3].Rows[0]["Followuplimits"].ToString();
                        }
                        if (!string.IsNullOrEmpty(dsPayerforLOA1678.Tables[3].Rows[0]["ApprovalDays"].ToString()))
                        {
                            CONSFollowupDAYS = dsPayerforLOA1678.Tables[3].Rows[0]["ApprovalDays"].ToString();
                        }
                    }
                    if (dsPayerforLOA1678.Tables[2].Rows.Count > 0)
                    {
                        if (dsPayerforLOA1678.Tables[2].Rows[0]["IsDiscountonCashOP"].ToString() != "")
                        {
                            CRDiscountOnCash = dsPayerforLOA1678.Tables[2].Rows[0]["IsDiscountonCashOP"].ToString();
                        }
                    }
                    DoctorsConsultations = string.Empty;
                    if (dsCompanyBlock != null && dsCompanyBlock.Tables[0].Rows.Count > 0)
                    {
                        dtCompanyContract = dsCompanyBlock.Tables[0].Copy();
                        PredefinedDiscount = dsPayerforLOA1678.Tables["table2"];
                        dtGradeValidation = dsPayerforLOA1678.Tables["table2"];
                        dtCompanyContract = dsPayerforLOA1678.Tables["table1"];

                        strTariffID = dsPayerforLOA1678.Tables["table1"].Rows[0]["TariffID"].ToString();

                        if (!string.IsNullOrEmpty(strTariffID.Trim()))
                        {
                            hdnTariffID = strTariffID;
                        }
                        else
                            strTariffID = "-1";

                        if (dsPayerforLOA1678.Tables["table2"].Rows.Count > 0)
                        {
                            string strGrades = "0";
                            strGrades = "";
                            int intprevId = 0;
                            DataRow[] dr1 = dsPayerforLOA1678.Tables["table2"].Select("PatientType=1", " GradeId Asc");
                            for (int ictr = 0; ictr < dr1.Length; ictr++)
                            {
                                if (intprevId != Convert.ToInt32(dr1[ictr]["GradeID"]))
                                {
                                    strGrades = strGrades + dr1[ictr]["GradeID"] + ",";
                                    intprevId = Convert.ToInt32(dr1[ictr]["GradeID"]);
                                }
                            }
                            if (strGrades.Length > 0)
                                strGrades = strGrades.Substring(0, strGrades.Length - 1);
                            else
                                strGrades = "0";

                            hdnGradeFilterCond = " GradeID in (" + strGrades + ") and Status = 0 and blocked=0 ";
                        }
                        else
                        {

                        }
                    }
                    if (dsPayerforLOA1678.Tables["table4"].Rows.Count > 0)
                    {
                        HasDefaultLOA = false;
                        HasDefaultLOA = Convert.ToBoolean(dsPayerforLOA1678.Tables["table4"].Rows[0]["IsDefaultLOA"]);
                        if (HasDefaultLOA)
                            hdnHasDefaultLOA = "true";
                        else
                            hdnHasDefaultLOA = "false";
                        if (dsPayerforLOA1678.Tables["table4"].Columns.Contains("IsFamilyLOA"))
                        {
                            if (dsPayerforLOA1678.Tables["table4"].Rows[0]["IsFamilyLOA"] != DBNull.Value)
                                hdnIsfamilyLOA = dsPayerforLOA1678.Tables[3].Rows[0]["IsFamilyLOA"].ToString();
                        }
                    }
                    DataRow[] drrblock = dtCompanyblock.Select("Blocked=0 and ISCompanyExpired=0 and ISInsuranceExpired=0");
                    if (drrblock.Length > 0)
                    {

                    }
                    else
                    {
                        drrblock = dtCompanyblock.Select();
                        string blockedMessage = string.Empty; string blockedMessage2L = string.Empty;
                        if (Convert.ToString(drrblock[0]["Blocked"]) == "1")
                        {
                            hdnblnCOmpanyBlocked = "true";
                            

                            //blockedMessage = Resources.English.ResourceManager.GetString("CompanyPatientBlocked") + "< br/>";
                            //blockedMessage2L = Resources.Arabic.ResourceManager.GetString("CompanyPatientBlocked") + "< br/>";
                            blockedMessage = System.Configuration.ConfigurationManager.AppSettings["CompanyPatientBlocked"].ToString() + "< br/>";
                            blockedMessage2L = System.Configuration.ConfigurationManager.AppSettings["CompanyPatientBlocked"].ToString() + "< br/>";
                            if (dtCompanyblock.Rows.Count > 0 && dtCompanyblock.Rows[0]["BlockedReason"] != DBNull.Value)
                            {
                                hdnblnCOmpanyBlockedReason = dtCompanyblock.Rows[0]["BlockedReason"].ToString();
                                blockedMessage = blockedMessage + " Reason: " + dtCompanyblock.Rows[0]["BlockedReason"].ToString();
                                blockedMessage2L = blockedMessage2L + " Reason: " + dtCompanyblock.Rows[0]["BlockedReason"].ToString();
                            }
                        }
                        else if (Convert.ToString(drrblock[0]["ISCompanyExpired"]) == "1")
                        {
                            hdnblnCompanyExpired = "true";
                            hdnblnCOmpanyBlockedReason = "";
                            //blockedMessage = Resources.English.ResourceManager.GetString("CompanyPatientExpired");
                            //blockedMessage2L = Resources.Arabic.ResourceManager.GetString("CompanyPatientExpired");
                            //hdnblnCOmpanyBlockedReason = Resources.English.ResourceManager.GetString("CompanyPatientExpired");
                            blockedMessage = System.Configuration.ConfigurationManager.AppSettings["CompanyPatientBlocked"].ToString();
                            blockedMessage2L = System.Configuration.ConfigurationManager.AppSettings["CompanyPatientBlocked"].ToString();
                            hdnblnCOmpanyBlockedReason = System.Configuration.ConfigurationManager.AppSettings["CompanyPatientBlocked"].ToString();
                        }
                        else if (Convert.ToString(drrblock[0]["ISInsuranceExpired"]) == "1")
                        {
                            hdnblnInsuranceExpired = "true";
                            hdnblnCOmpanyBlockedReason = "";
                            //blockedMessage = Resources.English.ResourceManager.GetString("InsuranceCompanyPatientExpired");
                            //blockedMessage2L = Resources.Arabic.ResourceManager.GetString("InsuranceCompanyPatientExpired");
                            //hdnblnCOmpanyBlockedReason = Resources.English.ResourceManager.GetString("InsuranceCompanyPatientExpired");
                            blockedMessage = System.Configuration.ConfigurationManager.AppSettings["InsuranceCompanyPatientExpired"].ToString();
                            blockedMessage2L = System.Configuration.ConfigurationManager.AppSettings["InsuranceCompanyPatientExpired"].ToString();
                            hdnblnCOmpanyBlockedReason = System.Configuration.ConfigurationManager.AppSettings["InsuranceCompanyPatientExpired"].ToString();
                        }
                        objPatientList.Code = (int)ProcessStatus.Success;
                        objPatientList.Status = ProcessStatus.Success.ToString();
                        objPatientList.Message = blockedMessage;
                        objPatientList.Message2L = blockedMessage2L;
                        return objPatientList;

                    }
                    if (!string.IsNullOrEmpty(strGradeID.Trim()))
                    {
                        if (dsPayerforLOA1678.Tables["table2"].Columns.Contains("GradeId"))
                        {
                            DataRow[] drGradeActive = dsPayerforLOA1678.Tables["table2"].Select("GradeId=" + Convert.ToInt32(strGradeID));
                            if (drGradeActive.Length == 0)
                            {
                                if (Convert.ToInt32(strGradeID) > 0)
                                {
                                    objPatientList.Code = (int)ProcessStatus.Success;
                                    objPatientList.Status = ProcessStatus.Success.ToString();
                                    //objPatientList.Message = Resources.English.ResourceManager.GetString("GradeBlocked");
                                    //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("GradeBlocked");
                                    objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["GradeBlocked"].ToString(); 
                                    objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["GradeBlocked"].ToString(); 
                                    return objPatientList;
                                }
                            }
                        }
                    }
                    else
                    {
                        strGradeID = "0";
                    }
                    dtCompanyContract = dsPayerforLOA1678.Tables["table1"];
                    if (hdnrblbilltypeCredit == true && ValidateExpiryDate(dtCompanyContract, "ValidTo") == false)
                    {
                        objPatientList.Code = (int)ProcessStatus.Success;
                        objPatientList.Status = ProcessStatus.Success.ToString();
                        //objPatientList.Message = Resources.English.ResourceManager.GetString("CompanyContractDateExpired");
                        //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("CompanyContractDateExpired");
                        objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["CompanyContractDateExpired"].ToString(); 
                        objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["CompanyContractDateExpired"].ToString();
                        return objPatientList;
                    }

                    if (DsPatientDetails.Tables[0].Rows.Count > 0)
                    {
                        if (DsPatientDetails.Tables[0].Columns.Contains("InsuranceCardExpiry") && DsPatientDetails.Tables[0].Rows[0]["InsuranceCardExpiry"] != DBNull.Value && Convert.ToInt32(strCompanyID) > 0)
                        {
                            dtpCardValid = Convert.ToDateTime(DsPatientDetails.Tables[0].Rows[0]["InsuranceCardExpiry"]);
                            if ((Convert.ToInt32(strCompanyID) > 0) || (Convert.ToInt32(strGradeID) > 0))
                            {
                                if (Convert.ToInt32(strCompanyID) > 0 && dtpCardValid < System.DateTime.Today.Date)
                                {
                                    objPatientList.Code = (int)ProcessStatus.Success;
                                    objPatientList.Status = ProcessStatus.Success.ToString();
                                    //objPatientList.Message = Resources.English.ResourceManager.GetString("InsuranceCardExpired");
                                    //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("InsuranceCardExpired");
                                    objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["InsuranceCardExpired"].ToString();
                                    objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["InsuranceCardExpired"].ToString();
                                    return objPatientList;
                                }
                            }
                        }
                    }
                }
                else
                {
                    hdnIsCashBill = "false";
                    strCompanyID = "0";
                    strTariffID = "-1";
                    hdnrblbilltypeCredit = false;
                    hdnrblbilltypecash = true;
                }

                if (hdnPatientType != null && (!string.IsNullOrEmpty(hdnPatientType.Trim())) && hdnPatientType != "0")
                {
                    intPatientType = Convert.ToInt32(hdnPatientType);
                }
                else
                    intPatientType = 1;

                #region RegistrationFeeBinding

                /// need to implement
                /// 
                #endregion

                #region OTHER ORDERS AND SCHEDULEORDERS

                DataSet dsScheduledOrders = FetchScheduledConsultations1(Convert.ToInt32(PatientID), 0, Convert.ToInt32(strDefaultHospitalId));
                if (dsScheduledOrders.Tables[0].Rows.Count > 0)
                {
                    DataRow[] drSch = dsScheduledOrders.Tables[0].Select("ScheduleID=" + PatientBillList.ScheduleID.ToString());
                    if (drSch.Length > 0)
                    {
                        DtSch = drSch.CopyToDataTable();
                        DtSch.AcceptChanges();
                    }
                    else
                    {
                        objPatientList.Code = (int)ProcessStatus.Success;
                        objPatientList.Status = ProcessStatus.Success.ToString();
                        //objPatientList.Message = Resources.English.ResourceManager.GetString("NoAPPGBill");
                        //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("NoAPPGBill");
                        objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["NoAPPGBill"].ToString(); 
                        objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["NoAPPGBill"].ToString();
                        return objPatientList;
                    }
                }
                else
                {
                    objPatientList.Code = (int)ProcessStatus.Success;
                    objPatientList.Status = ProcessStatus.Success.ToString();
                    //objPatientList.Message = Resources.English.ResourceManager.GetString("NoAPPGBill");
                    //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("NoAPPGBill");
                    objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["NoAPPGBill"].ToString();
                    objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["NoAPPGBill"].ToString();
                    return objPatientList;
                }

                if (dsScheduledOrders.Tables.Count > 1)
                    DtSchRef = dsScheduledOrders.Tables[1];
                dsDtDoc = FetchOtherDocDetails(Convert.ToInt32(PatientID), intPatientType, Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0);
                dtDtDoc = dsDtDoc.Tables[0].Copy();

                if (dtDtDoc != null && dtDtDoc.Rows.Count > 0)
                {
                    dtDocOrders = dtDtDoc.Clone();
                    if (ConfigurationSettings.AppSettings["OPUnBilledPrescLimitinDays"] != null)
                    {
                        DataTable dtblooddetails = dtBankDetails.Copy();
                        dtBankDetails.Clear();
                        int days = Convert.ToInt32(ConfigurationSettings.AppSettings["OPUnBilledPrescLimitinDays"]);
                        DateTime dtD = DateTime.Today.AddDays(-days);
                        foreach (DataRow dr in dtDtDoc.Select("Status=0 OR UnbilledItems > 0", ""))
                        {
                            DateTime date = Convert.ToDateTime(dr["VisitDate"]);
                            if (date > dtD)
                                dtDocOrders.ImportRow(dr);
                            else if (dtDtDoc.Columns.Contains("OrderDate"))
                            {
                                DateTime date1 = Convert.ToDateTime(dr["OrderDate"]);
                                if (date1 > dtD)
                                    dtDocOrders.ImportRow(dr);
                            }
                        }
                        foreach (DataRow dr in dtblooddetails.Select())
                        {
                            DateTime date = Convert.ToDateTime(dr["VisitDate"]);
                            if (date > dtD)
                                dtBankDetails.ImportRow(dr);
                        }
                    }
                    else
                    {
                        foreach (DataRow dr in dtDtDoc.Select("Status=0 OR UnbilledItems > 0 ", ""))
                        { dtDocOrders.ImportRow(dr); }
                    }
                }
                #region OtherOrder Data loading
                DtConsultationTypes = FetchADVProcedureDetails("FetchOrderTypesAdv", "1", "ServiceTypeId=4 and serviceid=2 and Blocked=0 and patientType=1", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0);
                int BaseOrderType = 0;
                BaseOrderType = Convert.ToInt32(FetchDefaultTariff("ORD_FOLLOWUP_DAYS"));
                for (int i = DtConsultationTypes.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow dr = DtConsultationTypes.Rows[i];
                    if (dr["OrderTypeID"].ToString() == BaseOrderType.ToString())
                        DtConsultationTypes.Rows.Remove(dr);
                }
                DtConsultationTypes.AcceptChanges();
                DataRow[] drrow = DtConsultationTypes.Select("", "OrderTypeID asc");
                DataTable dt = DtConsultationTypes.Clone();

                foreach (DataRow drow in drrow)
                {
                    dt.ImportRow(drow);
                }
                if (dt.Rows.Count > 0)
                {
                    ddlConsultatioTypeText = dt.Rows[0]["OrderType"].ToString();
                    ddlConsultatioType = Convert.ToInt32(dt.Rows[0]["OrdertypeId"]);
                }

                dtService = FetchServices(2, "Blocked = 0 and PatientType in (0, 1) and IsVisible in (0,1) ", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0).Tables[0];
                for (int i = 0; i < dtService.Rows.Count; i++)
                {
                    if (dtService.Rows[i]["name"].ToString() == "Blood bank")
                    {
                        dtService.Rows[i]["IsVisible"] = 1;
                    }
                }
                dtService.AcceptChanges();
                DataTable DtOPServices = dtService.Clone();
                foreach (DataRow drOPservices in dtService.Select("Isvisible =1 or id in (52,56,57)"))
                {
                    DtOPServices.ImportRow(drOPservices);
                }
                Service = DtOPServices.Copy();
                Service.AcceptChanges();

                DataTable dtfacdetals = FetchFacilityServiceAndSpecialization(Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, Convert.ToInt32(strDefaultHospitalId), Convert.ToInt32(strDefWorkstationId), 0);

                int intNoOfDays = 0;

                DataRow[] drSchedOrders = DtSch.Select("serviceID=4", "");
                if (drSchedOrders.Length > 0)
                {
                    foreach (DataRow dritem in drSchedOrders)
                    {
                        dritem["DeptID"] = (string.IsNullOrEmpty(dritem["DeptID"].ToString())) ? 0 : dritem["DeptID"];
                        dritem["SpecialiseID"] = (string.IsNullOrEmpty(dritem["SpecialiseID"].ToString())) ? 0 : dritem["SpecialiseID"];
                        dritem["HospDeptId"] = (string.IsNullOrEmpty(dritem["HospDeptId"].ToString())) ? 0 : dritem["HospDeptId"];
                    }
                }
                if (!DtSch.Columns.Contains("IsFacilitySerSpeMapped"))
                {
                    DtSch.Columns.Add("IsFacilitySerSpeMapped");
                }
                if (dtfacdetals != null && dtfacdetals.Rows.Count > 0)
                {
                    if (DtSch.Rows.Count > 0)
                    {
                        for (int index = 0; index < DtSch.Rows.Count; index++)
                        {
                            DataRow[] drMapFacData = dtfacdetals.Select(" ServiceID=" + DtSch.Rows[index]["ServiceID"] + " and  SpecialiseID=" + DtSch.Rows[index]["SpecialiseID"]);
                            if (drMapFacData.Length > 0)
                            {
                                DtSch.Rows[index]["IsFacilitySerSpeMapped"] = 1;
                            }
                            else
                            {
                                DtSch.Rows[index]["IsFacilitySerSpeMapped"] = 2;
                            }
                        }
                    }
                }
                else
                {
                    for (int index = 0; index < DtSch.Rows.Count; index++)
                    {
                        DtSch.Rows[index]["IsFacilitySerSpeMapped"] = 0;
                    }
                }
                SchedOrders = DtSch.Copy(); SchedOrders.AcceptChanges();
                DtSch.TableName = "ScheduledOrders";
                DtSch.TableName = "ScheduledOrders";
                DataTable dtDtDoc1 = new DataTable();
                DataTable dtTemp = new DataTable();

                if (DtSch.Select("ServiceId =2").Length > 0)
                {
                    SceduleDoctorAppointment = DtSch.Select("ServiceId =2").CopyToDataTable();
                }
                else
                {
                    SceduleDoctorAppointment = null;
                }
                //Priscription Orders
                //  DataSet dsDataSet = FetchOtherDocDetails(Convert.ToInt32(PatientID.Trim()), intPatientType, Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0);
                dtDtDoc1 = dsDtDoc.Tables[2].Copy();
                int IPID = 0;
                if (!dtDtDoc1.Columns.Contains("OrderID"))
                {
                    dtDtDoc1.Columns.Add("OrderID", typeof(string));
                }
                if (!dtDtDoc1.Columns.Contains("OrderID"))
                {
                    dtDtDoc1.Columns.Add("OrderID", typeof(string));
                }
                if (!dtDtDoc1.Columns.Contains("IsFacilitySerSpeMapped"))
                {
                    dtDtDoc1.Columns.Add("IsFacilitySerSpeMapped");
                }
                if (dtfacdetals != null && dtfacdetals.Rows.Count > 0)
                {
                    if (dtDtDoc1.Rows.Count > 0)
                    {
                        for (int index = 0; index < dtDtDoc1.Rows.Count; index++)
                        {
                            DataRow[] drMapFacData = dtfacdetals.Select(" ServiceID=" + dtDtDoc1.Rows[index]["ServiceID"] + " and  SpecialiseID=" + dtDtDoc1.Rows[index]["SpecialiseID"]);
                            if (drMapFacData.Length > 0)
                            {
                                dtDtDoc1.Rows[index]["IsFacilitySerSpeMapped"] = 1;
                            }
                            else
                            {
                                dtDtDoc1.Rows[index]["IsFacilitySerSpeMapped"] = 2;
                            }
                        }
                    }
                }
                else
                {
                    for (int index = 0; index < dtDtDoc1.Rows.Count; index++)
                    {
                        dtDtDoc1.Rows[index]["IsFacilitySerSpeMapped"] = 0;
                    }
                }
                DtDocOrders = dtDtDoc1.Clone();
                if (dtDtDoc1 != null && dtDtDoc1.Rows.Count > 0)
                {
                    DtDocOrders = dtDtDoc1.Clone();
                    if (ConfigurationSettings.AppSettings["OPUnBilledPrescLimitinDays"] != null)
                    {
                        int days = Convert.ToInt32(ConfigurationSettings.AppSettings["OPUnBilledPrescLimitinDays"]);
                        DateTime dt_filter = DateTime.Today.AddDays(-days);
                        foreach (DataRow dr in dtDtDoc1.Select(""))
                        {
                            DateTime date = Convert.ToDateTime(dr["VisitDate"]);
                            if (date > dt_filter)
                                DtDocOrders.ImportRow(dr);
                            else if (dtDtDoc1.Columns.Contains("OrderDate"))
                            {
                                DateTime date1 = Convert.ToDateTime(dr["OrderDate"]);
                                if (date1 > dt_filter)
                                    DtDocOrders.ImportRow(dr);
                            }
                        }
                        dtDtDoc1.Rows.Clear();
                        foreach (DataRow dr_docorder in DtDocOrders.Rows)
                        {
                            dtDtDoc1.ImportRow(dr_docorder);
                        }
                        dtDtDoc1.AcceptChanges();
                        DtDocOrders.Rows.Clear();
                        foreach (DataRow dr in dtDtDoc1.Select("Status=0 OR UnbilledItems > 0", ""))
                        {
                            DtDocOrders.ImportRow(dr);
                        }
                    }
                    else
                    {
                        foreach (DataRow dr in dtDtDoc1.Select("Status=0 OR UnbilledItems > 0", ""))
                        {
                            DtDocOrders.ImportRow(dr);
                        }
                    }
                }
                if (dtTemp != null && dtTemp.Rows.Count > 0)
                {
                    dtTemp.AcceptChanges();
                    foreach (DataRow dr in dtTemp.Rows)
                    {
                        DtDocOrders.ImportRow(dr);
                        DtDocOrders.Rows[DtDocOrders.Rows.Count - 1]["Status"] = "0";
                        if (dtfacdetals != null && dtfacdetals.Rows.Count > 0)
                        {
                            DataRow[] drMapFacData = dtfacdetals.Select(" ServiceID=" + DtDocOrders.Rows[DtDocOrders.Rows.Count - 1]["ServiceID"]);
                            if (drMapFacData.Length > 0)
                            {
                                DtDocOrders.Rows[DtDocOrders.Rows.Count - 1]["IsFacilitySerSpeMapped"] = 1;
                            }
                            else
                            {
                                DtDocOrders.Rows[DtDocOrders.Rows.Count - 1]["IsFacilitySerSpeMapped"] = 2;
                            }
                        }
                        else
                        {
                            DtDocOrders.Rows[DtDocOrders.Rows.Count - 1]["IsFacilitySerSpeMapped"] = 0;
                        }
                        if (dr["BloodorderID"] != DBNull.Value && !string.IsNullOrEmpty(dr["BloodorderID"].ToString()))
                            DtDocOrders.Rows[DtDocOrders.Rows.Count - 1]["OrderID"] = dr["BloodorderID"].ToString();
                        if (dr["OrderTypeID"] != DBNull.Value && !string.IsNullOrEmpty(dr["OrderTypeID"].ToString()))
                            DtDocOrders.Rows[DtDocOrders.Rows.Count - 1]["OrderTypeID"] = dr["OrderTypeID"].ToString();

                        DtDocOrders.Rows[DtDocOrders.Rows.Count - 1]["UnbilledItems"] = dr["UnbilledItems"].ToString();
                    }
                    DtDocOrders.AcceptChanges();
                }

                DataRow[] drDocPresc = dtDtDoc1.Select("Status=1");
                DataTable dtPatIPID = new DataTable();

                dtPatIPID.Columns.Add("IPID");
                dtPatIPID.Columns.Add("MonitorId");
                foreach (DataRow drPresc in drDocPresc)
                {
                    DataRow dr = dtPatIPID.NewRow();
                    dr["IPID"] = dsDtDoc.Tables[0].Select("Prescriptionid = " + drPresc["Prescriptionid"].ToString())[0]["IPID"].ToString();
                    dr["MonitorId"] = dsDtDoc.Tables[0].Select("Prescriptionid = " + drPresc["Prescriptionid"].ToString())[0]["MonitorId"].ToString();
                    dtPatIPID.Rows.Add(dr);
                    dtPatIPID.AcceptChanges();
                }

                if (DtDocOrders != null && DtDocOrders.Rows.Count > 0)
                {
                    DtDocOrders.TableName = "DoctorOrders";
                    DataTable dtTempCmb = new DataTable();
                    dtTempCmb = DtDocOrders.Copy();
                    DataTable dtTempDocCmb = new DataTable();
                    dtTempDocCmb.Columns.Add("DocId", typeof(int));
                    dtTempDocCmb.Columns.Add("DocName", typeof(string));
                    int DocID = 0;
                    foreach (DataRow drDocCmb in dtTempCmb.Select(null, "DoctorID"))
                    {
                        if (Convert.ToInt32(drDocCmb["DoctorID"]) != DocID)
                        {
                            DataRow DrDoc = dtTempDocCmb.NewRow();
                            DrDoc["DocId"] = drDocCmb["DoctorID"];
                            DrDoc["DocName"] = drDocCmb["DoctorName"];
                            dtTempDocCmb.Rows.Add(DrDoc);
                        }
                        DocID = Convert.ToInt32(drDocCmb["DoctorID"]);
                    }
                }
                #endregion                
                if (DtSch != null && DtSch.Rows.Count > 0)
                {
                    hdnotheroders = DtSch.Rows[0]["OrignalID"].ToString();
                    hdnspecialiseID = DtSch.Rows[0]["SpecialiseID"].ToString();
                }
                #endregion

                #region for ok click

                DataTable dtFirstSchedOrders = null;
                DataTable dtFirstRefOrders = null;
                dtFirstSchedOrders = DtSch.Copy();//Schedule Orders
                dtFirstRefOrders = null;// dsScheduledOrders.Tables[0].Copy();//referal Orders
                intTariffID = Convert.ToInt32(strTariffID);
                DtScheduleders = CreateOtherOrderTable();
                DtScheduleders.TableName = "ScheduleOrder";
                DtReferralOrders = CreateOtherOrderTable();
                DtReferralOrders.TableName = "ReferralOrder";
                DtOtherOders = CreateOtherOrderTable();
                DtOtherOders.TableName = "DoctOrders";
                DtOtherOders.Columns.Add("ClaimStatusID", typeof(int));
                DtOtherOders.Columns.Add("StatusName", typeof(string));
                DtOtherOders.Columns.Add("StatusColour", typeof(int));
                DtOtherOders.Columns.Add("PrescribedQty", typeof(int));
                if (!DtOtherOders.Columns.Contains("BDHospDeptID"))
                    DtOtherOders.Columns.Add("BDHospDeptID", typeof(int));

                //ScheduleOrder
                intPriority = ddlConsultatioType;

                for (int i = 0; i < dtFirstSchedOrders.Rows.Count; i++)
                {
                    string strIsProfCh = dtFirstSchedOrders.Rows[i]["IsProfChargesApplicable"].ToString();
                    bool IsAppliedProfCharge = strIsProfCh == "" ? false : Convert.ToBoolean(strIsProfCh);
                    DataRow drOthers = DtScheduleders.NewRow();
                    drOthers["ServiceID"] = dtFirstSchedOrders.Rows[i]["ServiceID"];
                    drOthers["ServiceName"] = dtFirstSchedOrders.Rows[i]["ServiceName"];
                    drOthers["ProcedureID"] = dtFirstSchedOrders.Rows[i]["ProcedureID"];
                    drOthers["ProcedureName"] = dtFirstSchedOrders.Rows[i]["ProcedureName"];
                    drOthers["DeptID"] = dtFirstSchedOrders.Rows[i]["HospDeptId"];
                    drOthers["DeptName"] = dtFirstSchedOrders.Rows[i]["DepartmentName"];
                    drOthers["SpecialiseId"] = dtFirstSchedOrders.Rows[i]["SpecialiseID"];
                    drOthers["SpecialiseName"] = dtFirstSchedOrders.Rows[i]["Specialisation"];
                    drOthers["ScheduleId"] = dtFirstSchedOrders.Rows[i]["ScheduleID"];
                    drOthers["ProcId"] = 0;
                    drOthers["OrderId"] = 0;
                    string dtF = Convert.ToDateTime(dtFirstSchedOrders.Rows[i]["StartDate"]).Date.ToString().Split(' ')[0] + " " + Convert.ToDateTime(dtFirstSchedOrders.Rows[i]["FromUnit"]).TimeOfDay.ToString();
                    drOthers["ScheduleDate"] = Convert.ToDateTime(dtF);
                    DataTable dtOtherSpecimen = new DataTable();
                    if (dtFirstSchedOrders.Rows[i]["ServiceTypeID"].ToString() == "6")
                    {
                        dtOtherSpecimen = FetchTestSpecimen(Convert.ToInt32(drOthers["ProcedureID"]), 3, 0, 0, 0);
                        foreach (DataRow drsample in dtOtherSpecimen.Rows)
                        {
                            drOthers["SampleID"] = drsample["SPID"] == DBNull.Value ? "0" : drsample["SPID"].ToString();
                            drOthers["SampleName"] = Convert.ToString(drsample["Specimen"].ToString()) == "null" ? "0" : Convert.ToString(drsample["Specimen"].ToString());
                        }
                    }
                    else
                    {
                        drOthers["SampleID"] = ddlConsultatioType;
                        drOthers["SampleName"] = ddlConsultatioTypeText;
                    }

                    drOthers["Qty"] = 1;
                    DataTable dtItemPrices = new DataTable();
                    if (IsAppliedProfCharge == false)
                    {
                        dtService = DtOPServices.Copy();
                        DataRow[] drsvs = dtService.Select(" id = " + drOthers["ServiceID"].ToString());
                        if (drsvs.Length == 1)
                        {
                            switch ((string)drsvs[0]["ServiceTypeName"])
                            {
                                case "Investigation":
                                    dtItemPrices = GetPriceList(Convert.ToInt32(drOthers["ServiceID"]), Convert.ToInt32(drOthers["ProcedureID"].ToString()), intTariffID, intBillType, 13, 0, intUserID, strBedType, intWorkStationid, 0).Tables[0];
                                    break;
                                case "Employee Service":
                                    dtItemPrices = GetPriceList(Convert.ToInt32(drOthers["ServiceID"]), Convert.ToInt32(drOthers["ProcedureID"].ToString()), intTariffID, intBillType, intPriority, drOthers["SpecialiseId"] == DBNull.Value ? 0 : Convert.ToInt32(drOthers["SpecialiseId"]), intUserID, strBedType, intWorkStationid, 0).Tables[0];
                                    break;

                                case "Procedure":
                                    dtItemPrices = GetPriceList(Convert.ToInt32(drOthers["ServiceID"]), Convert.ToInt32(drOthers["ProcedureID"].ToString()), intTariffID, intBillType, 0, drOthers["SpecialiseId"] == DBNull.Value ? 0 : Convert.ToInt32(drOthers["SpecialiseId"]), intUserID, strBedType, intWorkStationid, 0).Tables[0];
                                    break;
                                case "Package":
                                    dtItemPrices = GetPriceList(Convert.ToInt32(drOthers["ServiceID"]), Convert.ToInt32(drOthers["ProcedureID"].ToString()), intTariffID, intBillType, 0, 0, intUserID, strBedType, intWorkStationid, 0).Tables[0];
                                    break;
                                default:
                                    dtItemPrices = GetPriceList(Convert.ToInt32(drOthers["ServiceID"]), Convert.ToInt32(drOthers["ProcedureID"].ToString()), intTariffID, intBillType, intPriority, 0, intUserID, strBedType, intWorkStationid, 0).Tables[0];
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (intPriority == 0)
                            dtItemPrices = GetPriceListWithProfCharge(Convert.ToInt32(drOthers["ServiceID"]), Convert.ToInt32(drOthers["ProcedureID"].ToString()), intTariffID, intBillType, 0, drOthers["SpecialiseId"] == DBNull.Value ? 0 : Convert.ToInt32(drOthers["SpecialiseId"]), Convert.ToInt32(intUserID), strBedType, Convert.ToInt32(intWorkStationid), 0).Tables[0];
                        else
                        {
                            dtService = DtOPServices.Copy();
                            DataRow[] drsvs = dtService.Select(" id = " + drOthers["ServiceID"].ToString());
                            if (drsvs.Length == 1)
                            {
                                switch ((string)drsvs[0]["ServiceTypeName"])
                                {
                                    case "Investigation":
                                        dtItemPrices = GetPriceListWithProfCharge(Convert.ToInt32(drOthers["ServiceID"]), Convert.ToInt32(drOthers["ProcedureID"].ToString()), intTariffID, intBillType, 13, drOthers["SpecialiseId"] == DBNull.Value ? 0 : Convert.ToInt32(drOthers["SpecialiseId"]), Convert.ToInt32(intUserID), strBedType, Convert.ToInt32(intWorkStationid), 0).Tables[0];
                                        break;

                                    case "Procedure":
                                        dtItemPrices = GetPriceListWithProfCharge(Convert.ToInt32(drOthers["ServiceID"]), Convert.ToInt32(drOthers["ProcedureID"].ToString()), intTariffID, intBillType, 0, drOthers["SpecialiseId"] == DBNull.Value ? 0 : Convert.ToInt32(drOthers["SpecialiseId"]), Convert.ToInt32(intUserID), strBedType, Convert.ToInt32(intWorkStationid), 0).Tables[0];
                                        break;
                                    default:
                                        dtItemPrices = GetPriceListWithProfCharge(Convert.ToInt32(drOthers["ServiceID"]), Convert.ToInt32(drOthers["ProcedureID"].ToString()), intTariffID, intBillType, intPriority, drOthers["SpecialiseId"] == DBNull.Value ? 0 : Convert.ToInt32(drOthers["SpecialiseId"]), Convert.ToInt32(intUserID), strBedType, Convert.ToInt32(intWorkStationid), 0).Tables[0];
                                        break;
                                }
                            }
                        }
                    }
                    DataRow drOthers1 = null;

                    if (IsAppliedProfCharge == true)
                    {
                        drOthers1 = DtScheduleders.NewRow();
                        drOthers1.ItemArray = drOthers.ItemArray;
                    }

                    if (dtItemPrices != null && dtItemPrices.Rows.Count > 0)
                    {
                        int loopCount = 0;
                        foreach (DataRow drServiceId in dtItemPrices.Rows)
                        {
                            DataRow[] drPrice = dtItemPrices.Select("ServiceID = " + drServiceId["ServiceID"].ToString());
                            string strPrice = "";
                            strPrice = GetItemPriceString(drPrice);
                            string[] PriceReturn = strPrice.Split(Convert.ToChar("/"));
                            if (loopCount == 1 && (drServiceId["ServiceID"].ToString() == strProfChargeServiceId || drServiceId["ServiceID"].ToString() == strProfChargeConsServiceId || drServiceId["ServiceID"].ToString() == strProfChargeInvServiceId)) // hak // means professional charge item record  // added investigation prof. charge serviceid
                            {
                                drOthers = DtScheduleders.NewRow();
                                drOthers.ItemArray = drOthers1.ItemArray;
                                drOthers["ServiceName"] = drServiceId["ServiceName"];
                                drOthers["ServiceID"] = drServiceId["ServiceID"];
                            }
                            loopCount = 1;
                            if (PriceReturn.Length > 1)
                            {
                                drOthers["BasePrice"] = PriceReturn[0].ToString();
                                drOthers["BillablePrice"] = PriceReturn[2].ToString();
                                drOthers["EligiblePrice"] = PriceReturn[1].ToString();
                            }
                            else
                            {
                                drOthers["BasePrice"] = 0;
                                drOthers["BillablePrice"] = 0;
                                drOthers["EligiblePrice"] = 0;
                            }
                            BasePrice = Convert.ToDecimal(drOthers["BasePrice"] == DBNull.Value ? 0 : drOthers["BasePrice"]);
                            EligiblePrice = Convert.ToDecimal(drOthers["EligiblePrice"] == DBNull.Value ? 0 : drOthers["EligiblePrice"]);
                            BillablePrice = Convert.ToDecimal(drOthers["BillablePrice"] == DBNull.Value ? 0 : drOthers["BillablePrice"]);
                            if ((decimal)BasePrice == -1)
                            {
                                drOthers["BasePrice"] = DBNull.Value;
                            }
                            else
                            {
                                if ((decimal)BasePrice == 0)
                                {
                                    drOthers["BasePrice"] = Convert.ToDecimal("0").ToString(strCurrencyFormat);
                                }
                                else
                                {
                                    drOthers["BasePrice"] = String.Format("{0:F}", Convert.ToDouble(BasePrice));
                                }
                            }
                            if ((decimal)EligiblePrice == -1)
                            {
                                drOthers["EligiblePrice"] = DBNull.Value;
                            }
                            else
                            {
                                drOthers["EligiblePrice"] = String.Format("{0:F}", Convert.ToDouble(EligiblePrice));
                            }
                            if ((decimal)BillablePrice == -1)
                            {
                                drOthers["BillablePrice"] = DBNull.Value;
                            }
                            else
                            {
                                drOthers["BillablePrice"] = String.Format("{0:F}", Convert.ToDouble(BillablePrice));
                            }
                            drOthers["ServiceTypeID"] = dtFirstSchedOrders.Rows[i]["ServiceTypeID"].ToString();
                            if (dtFirstSchedOrders != null && dtFirstSchedOrders.Rows.Count > 0)
                            {
                                if ((dtFirstSchedOrders.Select("ScheduleID=" + Convert.ToInt32(drOthers["ScheduleId"].ToString())))[0]["OrignalID"] != DBNull.Value)
                                    drOthers["DoctorId"] = (dtFirstSchedOrders.Select("ScheduleID=" + Convert.ToInt32(drOthers["ScheduleId"].ToString())))[0]["OrignalID"].ToString();
                            }
                            else
                            {
                                if (hdnotheroders.ToString() != "")
                                    drOthers["DoctorId"] = hdnotheroders;
                            }
                            drOthers["Checked"] = true;
                            if (dtFirstSchedOrders.Columns.Contains("DoctorName") && dtFirstSchedOrders.Columns.Contains("EMPID") && dtFirstSchedOrders.Columns.Contains("EMPNO")
                                && dtFirstSchedOrders.Columns.Contains("DoctorCode"))
                            {
                                DataRow dr = dtFirstSchedOrders.Select("ScheduleID=" + Convert.ToInt32(drOthers["ScheduleId"].ToString()))[0];
                                drOthers["DoctorName"] = dr["DoctorName"];
                                drOthers["DoctorCode"] = dr["DoctorCode"];
                                drOthers["EmpId"] = dr["EMPID"];
                                drOthers["EmpNo"] = dr["EMPNO"];
                            }
                            DtScheduleders.Rows.Add(drOthers);
                            intarray.Add(hdnotheroders);
                            intarray.Add(hdnspecialiseID);
                        }
                    }
                }

                DtScheduleders = SortTable(DtScheduleders, "ProcedureID", "ASC");
                DtScheduleders.AcceptChanges();

                DataTable dtItemsFroPrice = new DataTable();
                DataTable dtItemsPrices = new DataTable();
                if (dtItemsFroPrice != null && dtItemsFroPrice.Rows.Count > 0)
                    dtItemsPrices = GetAllItemsPrice(dtItemsFroPrice);

                DtOtherOders = SortItemByItemSequence(DtOtherOders.Copy());
                DtOtherOders.AcceptChanges();
                DsOtherOrders.Tables.Add(DtOtherOders.Copy());
                DsOtherOrders.Tables.Add(DtScheduleders.Copy());
                DsOtherOrders.Tables.Add(DtReferralOrders.Copy());
                OtherOrders = DsOtherOrders;
                OtherOrders.AcceptChanges();

                #endregion
                DataSet dsOrders = new DataSet();
                dsOrders = DsOtherOrders.Copy();
                DataTable DTTemLoadOrders = null;
                if (dsOrders != null && dsOrders.Tables.Count > 0)
                {
                    DTTemLoadOrders = LoadOtherOrders(dsOrders, strCompanyID, strGradeID, strTariffID, PatientBillList);
                }
                if (DTTemLoadOrders != null)
                {
                    #region for imgpayment
                    if (DTTemLoadOrders.Rows.Count > 0)
                        OrderVisitType = DTTemLoadOrders.Rows[0]["Sample"].ToString();
                    var obj = PaymentCheck(PatientBillList, PatientUHID, hdnIsfamilyLOA);
                    if (obj.Status == "Success")
                    {
                        return null;
                    }
                    if (BillSummary != null)
                    {
                        if (BillSummary.Rows.Count > 0)
                        {
                            int Trans = DeleteTempBill(SessionID);
                            PatientBiillInfoListN objPatientListN = new PatientBiillInfoListN();
                            objPatientListN.RegCode = PatientBillList.RegCode;
                            objPatientListN.HospitalID = PatientBillList.HospitalId.ToString();
                            objPatientListN.ScheduleID = PatientBillList.ScheduleID.ToString();
                            foreach (DataRow dr in BillSummary.Rows)
                            {
                                try
                                {
                                    if (dr["Description"].ToString().Trim() == "Bill Amount")
                                        objPatientListN.BillAmount = dr["Amount"].ToString();
                                    if (dr["Description"].ToString().Trim() == "Payer Amount")
                                        objPatientListN.PayerAmount = dr["Amount"].ToString();
                                    if (dr["Description"].ToString().Trim() == "Discount Amount")
                                        objPatientListN.DiscountAmount = dr["Amount"].ToString();
                                    if (dr["Description"].ToString().Trim() == "VAT")
                                        objPatientListN.VAT = dr["Amount"].ToString();
                                    if (dr["Description"].ToString().Trim() == "Deposit Amount")
                                        objPatientListN.DepositAmount = dr["Amount"].ToString();
                                    if (dr["Description"].ToString().Trim() == "Refund Amount")
                                        objPatientListN.RefundAmount = dr["Amount"].ToString();
                                    if (dr["Description"].ToString().Trim() == "Receipt Amount")
                                        objPatientListN.ReceiptAmount = dr["Amount"].ToString();
                                    if (dr["Description"].ToString().Trim() == "Balance Amount")
                                        objPatientListN.BalanceAmount = dr["Amount"].ToString();
                                    if (dr["Description"].ToString().Trim() == "Collectable")
                                        objPatientListN.Collectable = dr["Amount"].ToString();
                                    objPatientListN.OrderType = OrderVisitType.ToString();

                                }
                                catch (Exception ex)
                                {
                                    objPatientList.Code = (int)ProcessStatus.Fail;
                                    objPatientList.Status = ProcessStatus.Fail.ToString();
                                    objPatientList.Message = ex.Message;
                                }
                            }
                            objPatientList.BillSummary = new List<PatientBiillInfoListN>();
                            objPatientList.BillSummary.Add(objPatientListN);
                            if (objPatientList.BillSummary.Count > 0)
                            {
                                objPatientList.Code = (int)ProcessStatus.Success;
                                objPatientList.Status = ProcessStatus.Success.ToString();
                                objPatientList.Message = "";

                            }
                            else
                            {
                                objPatientList.Code = (int)ProcessStatus.Success;
                                objPatientList.Status = ProcessStatus.Success.ToString();
                                //objPatientList.Message = Resources.English.ResourceManager.GetString("NoRecordsFound");
                                //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("NoRecordsFound");
                                objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["NoRecordsFound"].ToString(); 
                                objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["NoRecordsFound"].ToString(); 
                            }
                        }
                        else
                        {
                            objPatientList.Code = (int)ProcessStatus.Success;
                            objPatientList.Status = ProcessStatus.Success.ToString();
                            //objPatientList.Message = Resources.English.ResourceManager.GetString("NoRecordsFound");
                            //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("NoRecordsFound");
                            objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["NoRecordsFound"].ToString();
                            objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["NoRecordsFound"].ToString();
                        }
                    }
                    else
                    {
                        objPatientList.Code = (int)ProcessStatus.Success;
                        objPatientList.Status = ProcessStatus.Success.ToString();
                        //objPatientList.Message = Resources.English.ResourceManager.GetString("NoRecordsFound");
                        //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("NoRecordsFound");
                        objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["NoRecordsFound"].ToString();
                        objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["NoRecordsFound"].ToString();
                    }


                    #endregion
                }


                //950
            }
            catch (Exception ex)
            { 
            
            }
            return objPatientList;
        }


        public int GetRootPatientIdPerformance(string UHID, int iUserId, int iWStationId)
        {
            int RootPatientIDPerformance;
            objFOClient = new FrontOfficeServiceContractClient();
            try
            {
                RootPatientIDPerformance = objFOClient.FetchRootPatientIdPerformance(UHID, iUserId, iWStationId);
            }

            finally
            {
                objFOClient.Close();
            }
            return RootPatientIDPerformance;
        }

        private bool CheckPinBlock(int PatId, PatientBillList PatientBillList)
        {
            try
            {
                string str = "PatientID = " + PatId + " and Status = 0";
                DataSet dtPinBlock = FetchPinBlockMAPI(PatId, Convert.ToInt32(strDefaultHospitalId), Convert.ToInt32(strDefWorkstationId), PatientBillList);
                if (dtPinBlock.Tables[0].Rows.Count > 0)
                {
                    StringBuilder strInternal;
                    DataRow[] dr = dtPinBlock.Tables[0].Select("", "EffectiveDate Desc");
                    if (dr.Length > 0)

                    {
                        if ((dr[0]["Blocktype"].ToString()) == "0")
                        {
                            strInternal = new StringBuilder();
                            strInternal.Append("UHID is Blocked.</br>");
                            strInternal.Append("Reason :" + dr[0]["BlockReason"].ToString() + "</br>");
                            strInternal.Append("Block Message :" + dr[0]["Discription"].ToString() + "");
                            blnCreditpinBlock = false;
                            if (dr[0]["blocktype"].ToString() == "2")
                                blnCreditpinBlock = true;
                            ReturnMessage = strInternal.ToString();
                            return false;
                        }
                        else if ((dr[0]["Blocktype"].ToString()) == "1")
                        {
                            strInternal = new StringBuilder();
                            strInternal.Append("UHID is Credit Blocked.</br>");
                            strInternal.Append("Reason :" + dr[0]["BlockReason"].ToString() + "</br>");
                            strInternal.Append("Block Message :" + dr[0]["Discription"].ToString() + "");
                            blnCreditpinBlock = true;
                            ReturnMessage = strInternal.ToString();
                            return false;
                        }
                        else if ((dr[0]["Blocktype"].ToString()) == "2")
                        {
                            strInternal = new StringBuilder();
                            strInternal.Append("UHID is Blocked.</br>");
                            strInternal.Append("Reason :" + dr[0]["BlockReason"].ToString() + "</br>");
                            strInternal.Append("Block Message :" + dr[0]["Discription"].ToString() + "");
                            blnCreditpinBlock = true;
                            ReturnMessage = strInternal.ToString();
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in CheckPinBlock", "");
                return false;
            }
            finally
            {

            }
        }

        public DataSet FetchPatientDetails(string strPatientID, string strRegCode, bool deleted, int UserId, int intWorkStationid, int intError, byte isReg)
        {
            objFOClient = new FrontOfficeServiceContractClient();
            try
            {
                return objFOClient.FetchPatientDetails(strPatientID, strRegCode, deleted, UserId, intWorkStationid, intError, isReg);
            }

            finally
            {
                objFOClient.Close();
            }
        }


        public DataSet FetchPinBlockMAPI(int PatientID, int HospitalID, int intWorkstationId, PatientBillList PatientBillList)
        {
            DataHelper objDataHelper = new DataHelper(DEFAULTWORKSTATION, (int)Database.Master);
            DataSet dsSpecConfig = new DataSet();
            try
            {
                List<IDbDataParameter> objIDbDataParameters = new List<IDbDataParameter>();
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@PatientID", PatientID.ToString(), DbType.Int32, ParameterDirection.Input));
                dsSpecConfig = objDataHelper.RunSPReturnDS("Pr_FetchPinBlock_MAPI", objIDbDataParameters.ToArray());
                return dsSpecConfig;
            }
            finally
            {

            }
        }

        public DataSet FetchOutstandingAmount(int intPatientID, int intPatientType)
        {
            objFOClient = new FrontOfficeServiceContractClient();
            try
            {

                return objFOClient.FetchOutstandingAmount(intPatientID, intPatientType);
            }

            finally
            {
                objFOClient.Close();
            }
        }

        public DataSet CheckPatientIPorEMR(string UHID, int HospitalID, int intWorkstationId, PatientBillList PatientBillList)
        {
            DataHelper objDataHelper = new DataHelper(DEFAULTWORKSTATION, (int)Database.Master);
            DataSet dsSpecConfig = new DataSet("ChkPatient");
            try
            {
                List<IDbDataParameter> objIDbDataParameters = new List<IDbDataParameter>();
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@UHID", UHID.ToString(), DbType.String, ParameterDirection.Input));
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@Hospitalid", HospitalID, DbType.Int32, ParameterDirection.Input));
                dsSpecConfig = objDataHelper.RunSPReturnDS("Pr_CheckPatientIPorEMR_MAPI", objIDbDataParameters.ToArray());
                return dsSpecConfig;
            }
            finally
            {

            }
        }

        private void FetchHisConfiguation(int intUserID, int intWorkStationID, PatientBillList PatientBillList, string HISConfigValues)
        {
            try
            {
                DataTable dtSpecCofig = GetSpecializationConfig(HISConfigValues, 0, intUserID, intWorkStationID, 0, PatientBillList).Tables[0];
                HISCONFIG = dtSpecCofig.Copy();
                HISCONFIG.AcceptChanges();

            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in GetSpecConfig", "");

            }
        }

        private bool ValidateExpiryDate(DataTable dtValidate, string strColumnName)
        {
            try
            {
                DataRow[] drValidate = dtValidate.Select();
                if (drValidate.Length == 0)
                { return true; }
                else
                {
                    DateTime dtExpiryDate = Convert.ToDateTime(drValidate[0][strColumnName]);
                    if ((DateTime.Compare(DateTime.Today, dtExpiryDate)) == 1)
                    { return false; }
                    else
                    { return true; }
                }
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in ValidateExpiryDate", "");
                return false;
            }
        }



        public DataSet GetSpecializationConfig(string HISConfigValues, int intResult, int intUserID, int intWorkstationId, int intError, PatientBillList PatientBillList)
        {
            DataHelper objDataHelper = new DataHelper(DEFAULTWORKSTATION, (int)Database.Master);
            DataSet dsSpecConfig = new DataSet("Specialization Configuratin");
            try
            {

                List<IDbDataParameter> objIDbDataParameters = new List<IDbDataParameter>();
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@Type", DBNull.Value, DbType.String, ParameterDirection.Input));
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@Hospitalid", PatientBillList.HospitalId, DbType.Int32, ParameterDirection.Input));

                dsSpecConfig = objDataHelper.RunSPReturnDS("Pr_HISConfiguration_MAPI", objIDbDataParameters.ToArray());
                return dsSpecConfig;
            }
            finally
            {

            }
        }

        public DataSet FetchScheduledConsultations1(long intPatientID, int intNoOfDays, int intHospitalsId)
        {
            DataHelper objDataHelper = new DataHelper(DEFAULTWORKSTATION, (int)Database.Master);
            DataSet ds = null;
            try
            {
                List<IDbDataParameter> objIDbDataParameters = new List<IDbDataParameter>();
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@PatientID", intPatientID, DbType.Int64, ParameterDirection.Input));
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@HospitalId", intHospitalsId, DbType.Int32, ParameterDirection.Input));
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@NoOfDays", intNoOfDays, DbType.Int32, ParameterDirection.Input));

                ds = objDataHelper.RunSPReturnDS("Pr_FetchResourceCalanderPerf_mapi", objIDbDataParameters.ToArray());

            }

            finally
            {
                objDataHelper = null;
            }
            return ds;
        }

        public DataSet FetchOtherDocDetails(int intPatientID, int intPatientType, int intUserId, int intWorkstationid, int intError)
        {
            objFOClient = new FrontOfficeServiceContractClient();
            try
            {
                return objFOClient.FetchOtherDocDetails(intPatientID, intPatientType, intUserId, intWorkstationid, intError);
            }
            finally
            {
                objFOClient.Close();
            }


        }

        public DataTable FetchADVProcedureDetails(string procedureName, string Tableid, string strFilteCond, int intUserID, int intWorkStationid, int intLanguageID)
        {
            objFOClient = new FrontOfficeServiceContractClient();
            try
            {
                return objFOClient.FetchADVProcedureDetailsLangID(procedureName, Tableid, strFilteCond, intUserID, intWorkStationid, 0, intLanguageID).Tables[0];
            }
            finally
            {
                objFOClient.Close();
            }
        }

        public int FetchDefaultTariff(string strORDFOLLOWUPDAYS)
        {
            objFOClient = new FrontOfficeServiceContractClient();
            try
            {

                return objFOClient.FetchDefaultTariff(strORDFOLLOWUPDAYS);
            }

            finally
            {
                objFOClient.Close();
            }
        }

        public DataSet FetchServices(int intTableId, string strFilter, int IntUserId, int intWorkStationId, int intError)
        {
            objFOClient = new FrontOfficeServiceContractClient();
            try
            {

                return objFOClient.FetchServices(intTableId, strFilter, IntUserId, intWorkStationId, 0);
            }

            finally
            {
                objFOClient.Close();
            }
        }

        public DataTable FetchFacilityServiceAndSpecialization(int intUserId, int intWorkStationId, int intFeatureID, int intHospitalID, int intFacilityID, int tablesId)
        {
            try
            {
                AdminClient objAdminClient = new AdminClient();
                return objAdminClient.FetchFacilityServiceAndSpecialization(intUserId, intWorkStationId, intFeatureID, intHospitalID, intFacilityID, tablesId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable CreateOtherOrderTable()
        {
            DataTable DtOtherOrders = new DataTable("BillDetails");
            try
            {
                DtOtherOrders.Columns.Add("ServiceName", typeof(String));
                DtOtherOrders.Columns.Add("ServiceId", typeof(int));
                DtOtherOrders.Columns.Add("ProcedureName", typeof(String));
                DtOtherOrders.Columns.Add("ProcedureId", typeof(int));
                DtOtherOrders.Columns.Add("SampleName", typeof(String));
                DtOtherOrders.Columns.Add("SampleId", typeof(int));
                DtOtherOrders.Columns.Add("DeptId", typeof(int));//
                DtOtherOrders.Columns.Add("DeptName", typeof(String));//
                DtOtherOrders.Columns.Add("SpecialiseId", typeof(int));
                DtOtherOrders.Columns.Add("SpecialiseName", typeof(String));//
                DtOtherOrders.Columns.Add("Qty", typeof(int));
                DtOtherOrders.Columns.Add("BasePrice", typeof(decimal));
                DtOtherOrders.Columns.Add("EligiblePrice", typeof(decimal));
                DtOtherOrders.Columns.Add("BillablePrice", typeof(decimal));
                DtOtherOrders.Columns.Add("ScheduleId", typeof(int));
                DtOtherOrders.Columns.Add("ProcId", typeof(int));
                DtOtherOrders.Columns.Add("OrderId", typeof(int));
                DtOtherOrders.Columns.Add("Isgroup", typeof(bool));

                DtOtherOrders.Columns.Add("DoctorId", typeof(int));
                DtOtherOrders.Columns.Add("DoctorName", typeof(string));
                DtOtherOrders.Columns.Add("intSpecid", typeof(int));
                DtOtherOrders.Columns.Add("DrSchedule", typeof(int));
                DtOtherOrders.Columns.Add("DrOrders", typeof(int));
                DtOtherOrders.Columns.Add("ServiceTypeID", typeof(int));
                DtOtherOrders.Columns.Add("Checked", typeof(bool));
                DtOtherOrders.Columns.Add("ItemSequence", typeof(int));
                DtOtherOrders.Columns.Add("Status", typeof(int));
                DtOtherOrders.Columns.Add("IPID", typeof(int));
                DtOtherOrders.Columns.Add("LOAApprovalID", typeof(string));
                DtOtherOrders.Columns.Add("Priority", typeof(int));
                DtOtherOrders.Columns.Add("ScheduleDate", typeof(DateTime));
                DtOtherOrders.Columns.Add("ApprovalAMT", typeof(string));
                DtOtherOrders.Columns.Add("SpecimenId", typeof(string));

                DtOtherOrders.Columns.Add("DoctorCode", typeof(string));
                DtOtherOrders.Columns.Add("EmpId", typeof(string));
                DtOtherOrders.Columns.Add("EmpNo", typeof(string));

                return DtOtherOrders;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in CreateOtherOrderTable", "");
                return null;
            }
        }

        public DataTable FetchTestSpecimen(int intTestID, int intTableid, int intUserid, int intWorkstionid, int intError)
        {
            objFOClient = new FrontOfficeServiceContractClient();
            try
            {
                return objFOClient.FetchTestSpecimen(intTestID, intTableid, intUserid, intWorkstionid, intError).Tables["Test Specimen"];
            }
            finally
            {
                objFOClient.Close();
            }
        }

        public DataSet GetPriceList(int intServiceid, int intItemID, int intTariffid, int intBillType, int intPriority, int intSpecialiseid, int intUserID, string strBedTypeid, int intWorkstationId, int intError)
        {
            try
            {
                objFOClient = new FrontOfficeServiceContractClient();
                return objFOClient.FetchPriceDetails(intServiceid, intItemID, intTariffid, intBillType, intPriority, intSpecialiseid, strBedTypeid, intUserID, intWorkstationId, intError);
            }
            finally
            {
                objFOClient.Close();
            }
        }

        public DataSet GetPriceListWithProfCharge(int intServiceid, int intItemID, int intTariffid, int intBillType, int intPriority, int intSpecialiseid, int intUserID, string strBedTypeid, int intWorkstationId, int intError)
        {
            try
            {
                objFOClient = new FrontOfficeServiceContractClient();
                return objFOClient.FetchPriceDetailsWithProfCharge(intServiceid, intItemID, intTariffid, intBillType, intPriority, intSpecialiseid, strBedTypeid, intUserID, intWorkstationId, intError);
            }

            finally
            {
                objFOClient.Close();
            }
        }

        private string GetItemPriceString(DataRow[] drPrice)
        {
            try
            {
                string strBasePrice = Convert.ToString(drPrice[0]["BasePrice"].ToString() == "-1" ? "0" : drPrice[0]["BasePrice"]);
                string strEligiblePrice = Convert.ToString(drPrice[0]["EligiblePrice"].ToString() == "-1" ? "-1" : drPrice[0]["EligiblePrice"]);
                string strBillablePrice = Convert.ToString(drPrice[0]["BillablePrice"].ToString() == "-1" ? "-1" : drPrice[0]["BillablePrice"]);
                string strPrice = strBasePrice + "/" + strEligiblePrice + "/" + strBillablePrice;
                return strPrice;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in GetItemPriceString", "");
                return string.Empty;
            }
        }

        public static DataTable SortTable(DataTable dtToBeSorted, string strSortField, string SortOrder)
        {
            #region Sorting the Given DataTable Based on the given <Sort Field> and <Sort Order>
            DataTable dttemp = new DataTable();
            try
            {
                dttemp = dtToBeSorted.Clone();
                foreach (DataRow dr in dtToBeSorted.Select("", strSortField.Trim() + " " + SortOrder.Trim()))
                { dttemp.ImportRow(dr); }
                dttemp.AcceptChanges();
                return dttemp;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in SortTable", "");
                return null;
            }
            #endregion
        }

        private DataTable GetAllItemsPrice(DataTable dtItems)
        {
            DataSet dsItemPrices = null;
            try
            {
                dsItemPrices = FetchItemPriceForMultipleItems(dtItems, intTariffID, 0, Convert.ToInt32(strBedType), Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId));
                return dsItemPrices.Tables[0];
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.LogToXMLFile = true;
                return null;
            }
        }

        public DataSet FetchItemPriceForMultipleItems(DataTable dtItems, int intTariffId, int intSpecialisationId, int intBedTypeID, int intUserId, int intWorkstationid)
        {
            try
            {
                objFOClient = new FrontOfficeServiceContractClient();
                return objFOClient.FetchItemPriceForMultipleItems(dtItems, intTariffId, intSpecialisationId, intBedTypeID, intUserId, intWorkstationid);

            }
            finally
            {
                objFOClient.Close();
            }
        }

        private DataTable SortItemByItemSequence(DataTable DtOtherOders)
        {
            try
            {
                DataRow[] drrow = DtOtherOders.Select("", "ItemSequence asc");
                DataTable dt = DtOtherOders.Clone();
                foreach (DataRow drow in drrow)
                {
                    dt.ImportRow(drow);
                }
                DtOtherOders = dt;

                return DtOtherOders;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in SortItemByItemSequence", "");
                return null;
            }
        }

        private DataTable LoadOtherOrders(DataSet dsOtherOrders, string strCompanyID, string strGradeID, string strTarriffID, PatientBillList PatientBillList)
        {
            try
            {
                dtScheduldOrders = dsOtherOrders.Tables["ScheduleOrder"];
                dtDocOrders = dsOtherOrders.Tables["DoctOrders"];
                dtReferralOrders = dsOtherOrders.Tables["ReferralOrder"];
                if (dtDocOrders != null && dtDocOrders.Rows.Count > 0)
                {
                    if (dtDocOrders.Columns.Contains("IPID"))
                    {
                        hdnIPID = string.IsNullOrEmpty(dtDocOrders.Rows[0]["IPID"].ToString()) ? "0" : dtDocOrders.Rows[0]["IPID"].ToString();
                        if (hdnIPID.ToString() != "0")
                        {
                            DataSet dsLetterIPID = FetchPatientAdmissionLetters(Convert.ToInt32(hdnIPID), Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId));
                            if (dsLetterIPID != null && dsLetterIPID.Tables[0].Rows.Count > 0)
                                hdnLetterIDforIPID = dsLetterIPID.Tables[0].Rows[dsLetterIPID.Tables[0].Rows.Count - 1]["letterid"].ToString();
                        }
                    }
                }
                else
                {
                    hdnIPID = "0";
                }

                foreach (DataRow drDocOrder in dtDocOrders.Rows)
                {
                    drDocOrder["ProcedureName"] = drDocOrder["ProcedureName"].ToString().Trim().Replace("&amp;", "&");
                    drDocOrder["ProcedureName"] = drDocOrder["ProcedureName"].ToString().Trim().Replace("&nbsp;", "");
                    if (drDocOrder["IsGroup"].ToString().ToUpper() == "TRUE")
                    {
                        PackageTestOrdersFormDoctorAppointment(drDocOrder);
                    }
                }
                dtDocOrders.AcceptChanges();
                foreach (DataRow drSchedOrder in dtScheduldOrders.Rows)
                {

                    drSchedOrder["ProcedureName"] = drSchedOrder["ProcedureName"].ToString().Trim().Replace("&amp;", "&");
                    drSchedOrder["ProcedureName"] = drSchedOrder["ProcedureName"].ToString().Trim().Replace("&nbsp;", "");

                    if (drSchedOrder["ServiceId"].ToString() == "4")
                    {
                        PackageTestOrdersFormDoctorAppointment(drSchedOrder);
                    }
                }
                dtScheduldOrders.AcceptChanges();
                DataRow dtTemp;

                #region ScheduleOrders

                if (dtScheduldOrders != null && dtScheduldOrders.Rows.Count > 0)
                {
                    hdnIPID = "0";
                    if (DTTemp != null && DTTemp.Rows.Count > 0)
                    {
                        if (!DTTemp.Columns.Contains("ApprovalAMT"))
                            DTTemp.Columns.Add("ApprovalAMT", typeof(string));

                        if (DTTemp.Columns.Contains("Checked"))
                        {
                            for (int intCnt = 0; intCnt < dtScheduldOrders.Rows.Count; intCnt++)
                            {
                                int intCondition = 0;
                                if (dtScheduldOrders.Rows[intCnt]["ServiceId"].ToString() == strProfChargeServiceId)
                                {
                                    intCondition = 1;
                                }
                                if (DTTemp.Select("ScheduleId=" + dtScheduldOrders.Rows[intCnt]["ScheduleId"] + " and SampleId = " + dtScheduldOrders.Rows[intCnt]["SampleId"] + " and ProcedureID=" + dtScheduldOrders.Rows[intCnt]["ProcedureID"]).Length <= intCondition)//0) hak
                                {
                                    dtTemp = DTTemp.NewRow();
                                    dtTemp["ServiceId"] = dtScheduldOrders.Rows[intCnt]["ServiceId"];
                                    dtTemp["ServiceName"] = dtScheduldOrders.Rows[intCnt]["ServiceName"];
                                    dtTemp["ProcedureId"] = dtScheduldOrders.Rows[intCnt]["ProcedureId"];

                                    dtScheduldOrders.Rows[intCnt]["ProcedureName"].ToString().Replace("&amp;", "");
                                    dtTemp["Procedure"] = dtScheduldOrders.Rows[intCnt]["ProcedureName"];
                                    dtTemp["SampleId"] = dtScheduldOrders.Rows[intCnt]["SampleId"];
                                    dtTemp["Sample"] = dtScheduldOrders.Rows[intCnt]["SampleName"];
                                    dtTemp["DeptId"] = dtScheduldOrders.Rows[intCnt]["DeptId"];
                                    dtTemp["DeptName"] = dtScheduldOrders.Rows[intCnt]["DeptName"];
                                    dtTemp["SpecialiseId"] = dtScheduldOrders.Rows[intCnt]["SpecialiseId"];
                                    dtTemp["SpecialiseName"] = dtScheduldOrders.Rows[intCnt]["SpecialiseName"];
                                    dtTemp["BedTypeId"] = -1;
                                    dtTemp["BedTypeName"] = "OPD";
                                    dtTemp["ProfileId"] = (dtScheduldOrders.Rows[intCnt]["ServiceId"].ToString() == "4") ? dtScheduldOrders.Rows[intCnt]["ProcedureId"] : 0;  // TFS ID::89016 0;//Need to change
                                    dtTemp["TariffId"] = Convert.ToInt32(strTariffID);
                                    dtTemp["ScheduleId"] = dtScheduldOrders.Rows[intCnt]["ScheduleId"];
                                    dtTemp["ProcId"] = dtScheduldOrders.Rows[intCnt]["ProcId"];
                                    dtTemp["BasePrice"] = Convert.ToDecimal(dtScheduldOrders.Rows[intCnt]["BasePrice"]).ToString(hdnsCurrencyFormat);
                                    dtTemp["EligiblePrice"] = dtScheduldOrders.Rows[intCnt]["EligiblePrice"];
                                    dtTemp["BillablePrice"] = dtScheduldOrders.Rows[intCnt]["BillablePrice"];
                                    dtTemp["OrderId"] = dtScheduldOrders.Rows[intCnt]["OrderId"];
                                    dtTemp["Isgroup"] = dtScheduldOrders.Rows[intCnt]["Isgroup"];
                                    dtTemp["Checked"] = dtScheduldOrders.Rows[intCnt]["Checked"];
                                    dtTemp["Price"] = Convert.ToDecimal(dtTemp["BasePrice"]).ToString(hdnsCurrencyFormat);
                                    dtTemp["Quantity"] = 1;
                                    dtTemp["MQTY"] = 1;
                                    dtTemp["SQTY"] = 0;
                                    dtTemp["Amount"] = Convert.ToDecimal(dtTemp["BasePrice"]).ToString(hdnsCurrencyFormat);
                                    dtTemp["PatientType"] = 1;//OutPatient HardCoded.
                                    DTTemp.Rows.Add(dtTemp);
                                }
                            }

                        }
                        else
                        {
                            DTTemp.Columns.Add("Checked", typeof(bool));
                            DTTemp.AcceptChanges();
                            for (int i = 0; i < dtScheduldOrders.Rows.Count; i++)
                            {
                                dtTemp = DTTemp.NewRow();
                                dtTemp["ServiceId"] = dtScheduldOrders.Rows[i]["ServiceId"];
                                dtTemp["ServiceName"] = dtScheduldOrders.Rows[i]["ServiceName"];
                                dtTemp["ProcedureId"] = dtScheduldOrders.Rows[i]["ProcedureId"];
                                dtTemp["Procedure"] = dtScheduldOrders.Rows[i]["ProcedureName"];
                                dtTemp["SampleId"] = dtScheduldOrders.Rows[i]["SampleId"];
                                dtTemp["Sample"] = dtScheduldOrders.Rows[i]["SampleName"];
                                dtTemp["DeptId"] = dtScheduldOrders.Rows[i]["DeptId"];
                                dtTemp["DeptName"] = dtScheduldOrders.Rows[i]["DeptName"];
                                dtTemp["SpecialiseId"] = dtScheduldOrders.Rows[i]["SpecialiseId"];
                                dtTemp["SpecialiseName"] = dtScheduldOrders.Rows[i]["SpecialiseName"];
                                dtTemp["BedTypeId"] = -1;
                                dtTemp["BedTypeName"] = "OPD";
                                dtTemp["ProfileId"] = (dtScheduldOrders.Rows[i]["ServiceId"].ToString() == "4") ? dtScheduldOrders.Rows[i]["ProcedureId"] : 0; // TFS ID::89016 0;//Need to change
                                dtTemp["TariffId"] = Convert.ToInt32(strTariffID);
                                dtTemp["ScheduleId"] = dtScheduldOrders.Rows[i]["ScheduleId"];
                                dtTemp["ProcId"] = dtScheduldOrders.Rows[i]["ProcId"];
                                dtTemp["BasePrice"] = Convert.ToDecimal(dtScheduldOrders.Rows[i]["BasePrice"]).ToString(hdnsCurrencyFormat);
                                dtTemp["EligiblePrice"] = dtScheduldOrders.Rows[i]["EligiblePrice"];
                                dtTemp["BillablePrice"] = dtScheduldOrders.Rows[i]["BillablePrice"];
                                dtTemp["OrderId"] = dtScheduldOrders.Rows[i]["OrderId"];
                                dtTemp["Isgroup"] = dtScheduldOrders.Rows[i]["Isgroup"];
                                dtTemp["Checked"] = dtScheduldOrders.Rows[i]["Checked"];
                                dtTemp["Price"] = Convert.ToDecimal(dtTemp["BasePrice"]).ToString(hdnsCurrencyFormat);
                                dtTemp["Quantity"] = 1;
                                dtTemp["MQTY"] = 1;
                                dtTemp["SQTY"] = 0;
                                dtTemp["Amount"] = Convert.ToDecimal(dtTemp["BasePrice"]).ToString(hdnsCurrencyFormat);
                                DTTemp.Rows.Add(dtTemp);
                            }
                        }
                        DTTemp.AcceptChanges();
                    }
                    else
                    {
                        DTTemp = CreateDTTemp();
                        if (DTTemp.Columns.Contains("Checked"))
                        {

                        }
                        else
                        {
                            DTTemp.Columns.Add("Checked", typeof(bool));
                        }
                        DTTemp.AcceptChanges();
                        if (!DTTemp.Columns.Contains("ApprovalAMT"))
                            DTTemp.Columns.Add("ApprovalAMT", typeof(string));
                        DTTemp.AcceptChanges();
                        for (int i = 0; i < dtScheduldOrders.Rows.Count; i++)
                        {
                            dtTemp = DTTemp.NewRow();
                            dtTemp["ServiceId"] = dtScheduldOrders.Rows[i]["ServiceId"];
                            dtTemp["ServiceName"] = dtScheduldOrders.Rows[i]["ServiceName"];
                            dtTemp["ProcedureId"] = dtScheduldOrders.Rows[i]["ProcedureId"];
                            dtTemp["Procedure"] = dtScheduldOrders.Rows[i]["ProcedureName"];
                            dtTemp["SampleId"] = dtScheduldOrders.Rows[i]["SampleId"];
                            dtTemp["Sample"] = dtScheduldOrders.Rows[i]["SampleName"];
                            dtTemp["DeptId"] = dtScheduldOrders.Rows[i]["DeptId"];
                            dtTemp["DeptName"] = dtScheduldOrders.Rows[i]["DeptName"];
                            dtTemp["SpecialiseId"] = dtScheduldOrders.Rows[i]["SpecialiseId"];
                            dtTemp["SpecialiseName"] = dtScheduldOrders.Rows[i]["SpecialiseName"];
                            dtTemp["BedTypeId"] = -1;
                            dtTemp["BedTypeName"] = "OPD";
                            dtTemp["ProfileId"] = (dtScheduldOrders.Rows[i]["ServiceId"].ToString() == "4") ? dtScheduldOrders.Rows[i]["ProcedureId"] : 0;
                            dtTemp["TariffId"] = Convert.ToInt32(strTariffID);
                            dtTemp["ScheduleId"] = dtScheduldOrders.Rows[i]["ScheduleId"];
                            dtTemp["ProcId"] = dtScheduldOrders.Rows[i]["ProcId"];
                            dtTemp["BasePrice"] = Convert.ToDecimal(dtScheduldOrders.Rows[i]["BasePrice"]).ToString(hdnsCurrencyFormat);
                            dtTemp["EligiblePrice"] = dtScheduldOrders.Rows[i]["EligiblePrice"];
                            dtTemp["BillablePrice"] = dtScheduldOrders.Rows[i]["BillablePrice"];
                            dtTemp["OrderId"] = dtScheduldOrders.Rows[i]["OrderId"];
                            dtTemp["Isgroup"] = dtScheduldOrders.Rows[i]["Isgroup"];
                            dtTemp["Checked"] = dtScheduldOrders.Rows[i]["Checked"];
                            dtTemp["Price"] = Convert.ToDecimal(dtTemp["BasePrice"]).ToString(hdnsCurrencyFormat);
                            dtTemp["Quantity"] = 1;
                            dtTemp["MQTY"] = 1;
                            dtTemp["SQTY"] = 0;
                            dtTemp["Amount"] = dtTemp["BasePrice"];
                            dtTemp["ApprovalAMT"] = dtScheduldOrders.Rows[i]["ApprovalAMT"];

                            DTTemp.Rows.Add(dtTemp);

                        }
                        DTTemp.AcceptChanges();

                    }
                    if (dtScheduldOrders != null && dtScheduldOrders.Rows.Count > 0)
                    {
                        if (dtScheduldOrders.Columns.Contains("DoctorName"))
                        {
                            if (Convert.ToInt32(dtScheduldOrders.Rows[0]["ServiceId"]) == 2)
                            {
                                hdnDocID = dtScheduldOrders.Rows[0]["DoctorId"].ToString();
                                hdnDocHospDeptId = dtScheduldOrders.Rows[0]["DeptID"].ToString();
                                OpdocId = hdnDocID;
                                hdnProcedureID = dtScheduldOrders.Rows[0]["ProcedureID"].ToString();
                                hdnDocSpecialiseId = dtScheduldOrders.Rows[0]["SpecialiseID"].ToString();
                                if (!string.IsNullOrEmpty(hdnDocID) && hdnDocID != null && (!string.IsNullOrEmpty(hdnDocSpecialiseId)) && (!string.IsNullOrEmpty(hdnPatientID)))
                                {
                                    GetGradeDocSpec(Convert.ToInt32(hdnPatientID), Convert.ToInt32(hdnDocID), Convert.ToInt32(hdnDocSpecialiseId), true, PatientBillList);
                                    if (hdnrblbilltypeCredit == true)
                                    {
                                        string strLOAFilter = "status= 0 and Blocked = 0 and GradeID = " + Convert.ToInt32(strGradeID) + " and payerid=" + Convert.ToInt32(strCompanyID) + " and patienttype=1 and " +
                                            "patientId= " + Convert.ToInt32(hdnPatientID) + " and  todate >=\'" + DateTime.Now.ToString("dd-MMM-yyyy") + "\' and fromdate <=getdate()" + " and SpecialisationId=" + hdnDocSpecialiseId;
                                        GetLOA(strLOAFilter);
                                    }
                                }
                            }
                            else if (Convert.ToInt32(dtScheduldOrders.Rows[0]["ServiceId"]) == 5)
                            {
                                hdnDocID = dtScheduldOrders.Rows[0]["EmpID"].ToString();
                                hdnGradeSplzID = hdnDocSpecialiseId = dtScheduldOrders.Rows[0]["SpecialiseID"].ToString();
                                hdnDocHospDeptId = dtScheduldOrders.Rows[0]["DeptID"].ToString();
                                if (!string.IsNullOrEmpty(hdnDocID) && hdnDocID != null && (!string.IsNullOrEmpty(hdnDocSpecialiseId)) && (!string.IsNullOrEmpty(hdnPatientID)))
                                {
                                    GetGradeDocSpec(Convert.ToInt32(hdnPatientID), Convert.ToInt32(hdnDocID), Convert.ToInt32(hdnDocSpecialiseId), true, PatientBillList);
                                    if (hdnrblbilltypeCredit == true)
                                    {
                                        string strLOAFilter = "status =0 and Blocked = 0 and GradeId = " + Convert.ToInt32(strGradeID) + " and payerid=" + Convert.ToInt32(strCompanyID) + " and patienttype=1 and " +
                                            "patientId= " + Convert.ToInt32(hdnPatientID) + " and  todate >=\'" + DateTime.Now.ToString("dd-MMM-yyyy") + "\' and fromdate <=getdate()" + " and SpecialisationId=" + hdnDocSpecialiseId;
                                        GetLOA(strLOAFilter);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion ScheduleOrders

                #region DoctorOrders
                else if (dtDocOrders != null && dtDocOrders.Rows.Count > 0)
                {
                    if (DTTemp != null && DTTemp.Rows.Count > 0)
                    {
                        if (DTTemp.Columns.Contains("Checked"))
                        {
                            if (dtDocOrders.Columns.Contains("PrescriptionId"))
                                dtDocOrders.Columns["PrescriptionId"].ColumnName = "ProcId";
                            for (int intCnt = 0; intCnt < dtDocOrders.Rows.Count; intCnt++)
                            {
                                if (DTTemp.Select("ProcId=" + dtDocOrders.Rows[intCnt]["ProcId"] + " and SampleId = " + dtDocOrders.Rows[intCnt]["SampleId"] + " and ProcedureID=" + dtDocOrders.Rows[intCnt]["ProcedureID"]).Length <= 0)
                                {
                                    dtTemp = DTTemp.NewRow();
                                    dtTemp["ServiceId"] = dtDocOrders.Rows[intCnt]["ServiceId"];
                                    dtTemp["ServiceName"] = dtDocOrders.Rows[intCnt]["ServiceName"];
                                    dtTemp["ProcedureId"] = dtDocOrders.Rows[intCnt]["ProcedureId"];
                                    dtTemp["Procedure"] = dtDocOrders.Rows[intCnt]["ProcedureName"];
                                    dtTemp["SampleId"] = dtDocOrders.Rows[intCnt]["SampleId"];
                                    dtTemp["Sample"] = dtDocOrders.Rows[intCnt]["SampleName"];
                                    dtTemp["DeptId"] = dtDocOrders.Rows[intCnt]["DeptId"];
                                    dtTemp["DeptName"] = dtDocOrders.Rows[intCnt]["DeptName"];
                                    dtTemp["SpecialiseId"] = dtDocOrders.Rows[intCnt]["SpecialiseId"];
                                    dtTemp["SpecialiseName"] = dtDocOrders.Rows[intCnt]["SpecialiseName"];
                                    dtTemp["BedTypeId"] = -1;
                                    dtTemp["BedTypeName"] = "OPD";
                                    dtTemp["ProfileId"] = 0;//Need to change
                                    dtTemp["TariffId"] = Convert.ToInt32(strTariffID);
                                    dtTemp["ScheduleId"] = 0;//dtScheduldOrders.Rows[intCnt]["ScheduleId"];
                                    dtTemp["ProcId"] = dtDocOrders.Rows[intCnt]["ProcId"];
                                    dtTemp["BasePrice"] = dtDocOrders.Rows[intCnt]["BasePrice"];
                                    dtTemp["EligiblePrice"] = dtDocOrders.Rows[intCnt]["EligiblePrice"];
                                    dtTemp["BillablePrice"] = dtDocOrders.Rows[intCnt]["BillablePrice"];
                                    dtTemp["OrderId"] = dtDocOrders.Rows[intCnt]["OrderId"];
                                    dtTemp["Isgroup"] = dtDocOrders.Rows[intCnt]["Isgroup"];
                                    dtTemp["Checked"] = dtDocOrders.Rows[intCnt]["Checked"];
                                    dtTemp["Price"] = dtTemp["BasePrice"];

                                    dtTemp["Quantity"] = dtDocOrders.Rows[intCnt]["Qty"]; //1;
                                    dtTemp["MQTY"] = 1;
                                    dtTemp["SQTY"] = 0;
                                    if (dtDocOrders.Columns.Contains("Priority") && !string.IsNullOrEmpty(dtDocOrders.Rows[intCnt]["Priority"].ToString()))
                                        dtTemp["Priority"] = dtDocOrders.Rows[intCnt]["Priority"];
                                    if (dtDocOrders.Columns.Contains("Status"))
                                    {
                                        if (dtDocOrders.Rows[intCnt]["Status"] != DBNull.Value && !string.IsNullOrEmpty(dtDocOrders.Rows[intCnt]["Status"].ToString()))
                                            dtTemp["Status"] = dtDocOrders.Rows[intCnt]["Status"].ToString();
                                    }
                                    /**********************************************************************/
                                    dtTemp["Amount"] = dtTemp["BasePrice"];
                                    dtTemp["PatientType"] = 1;//Convert.ToInt32(hdnPatientType);//OutPatient HardCoded.
                                    DTTemp.Rows.Add(dtTemp);
                                }
                            }

                        }
                        else
                        {
                            DTTemp.Columns.Add("Checked", typeof(bool));
                            DTTemp.AcceptChanges();
                            if (dtDocOrders.Columns.Contains("PrescriptionId"))
                                dtDocOrders.Columns["PrescriptionId"].ColumnName = "ProcId";
                            dtDocOrders.AcceptChanges();
                            for (int intCnt = 0; intCnt < dtDocOrders.Rows.Count; intCnt++)
                            {
                                dtTemp = DTTemp.NewRow();
                                dtTemp["ServiceId"] = dtDocOrders.Rows[intCnt]["ServiceId"];
                                dtTemp["ServiceName"] = dtDocOrders.Rows[intCnt]["ServiceName"];
                                dtTemp["ProcedureId"] = dtDocOrders.Rows[intCnt]["ProcedureId"];
                                dtTemp["Procedure"] = dtDocOrders.Rows[intCnt]["ProcedureName"];
                                dtTemp["SampleId"] = dtDocOrders.Rows[intCnt]["SampleId"];
                                dtTemp["Sample"] = dtDocOrders.Rows[intCnt]["SampleName"];
                                dtTemp["DeptId"] = dtDocOrders.Rows[intCnt]["DeptId"];
                                dtTemp["DeptName"] = dtDocOrders.Rows[intCnt]["DeptName"];
                                dtTemp["SpecialiseId"] = dtDocOrders.Rows[intCnt]["SpecialiseId"];
                                dtTemp["SpecialiseName"] = dtDocOrders.Rows[intCnt]["SpecialiseName"];
                                dtTemp["BedTypeId"] = -1;
                                dtTemp["BedTypeName"] = "OPD";
                                dtTemp["ProfileId"] = 0;//Need to change
                                dtTemp["TariffId"] = Convert.ToInt32(strTariffID);
                                dtTemp["ScheduleId"] = 0;//dtScheduldOrders.Rows[intCnt]["ScheduleId"];
                                dtTemp["ProcId"] = dtDocOrders.Rows[intCnt]["ProcId"];
                                dtTemp["BasePrice"] = dtDocOrders.Rows[intCnt]["BasePrice"];
                                dtTemp["EligiblePrice"] = dtDocOrders.Rows[intCnt]["EligiblePrice"];
                                dtTemp["BillablePrice"] = dtDocOrders.Rows[intCnt]["BillablePrice"];
                                dtTemp["OrderId"] = dtDocOrders.Rows[intCnt]["OrderId"];
                                dtTemp["Isgroup"] = dtDocOrders.Rows[intCnt]["Isgroup"];
                                dtTemp["Checked"] = dtDocOrders.Rows[intCnt]["Checked"];
                                dtTemp["Price"] = dtTemp["BasePrice"];
                                dtTemp["Quantity"] = dtDocOrders.Rows[intCnt]["Qty"];//1;
                                dtTemp["MQTY"] = 1;
                                dtTemp["SQTY"] = 0;
                                dtTemp["Amount"] = dtTemp["BasePrice"];
                                dtTemp["PatientType"] = 1;//Convert.ToInt32(hdnPatientType.Value);//OutPatient HardCoded.
                                if (dtDocOrders.Columns.Contains("Priority") && !string.IsNullOrEmpty(dtDocOrders.Rows[intCnt]["Priority"].ToString()))
                                    dtTemp["Priority"] = dtDocOrders.Rows[intCnt]["Priority"];
                                else
                                    dtTemp["Priority"] = 13;
                                if (dtDocOrders.Columns.Contains("Status"))
                                {
                                    if (dtDocOrders.Rows[intCnt]["Status"] != DBNull.Value && !string.IsNullOrEmpty(dtDocOrders.Rows[intCnt]["Status"].ToString()))
                                        dtTemp["Status"] = dtDocOrders.Rows[intCnt]["Status"].ToString();
                                }
                                DTTemp.Rows.Add(dtTemp);
                            }
                        }
                        DTTemp.AcceptChanges();
                    }
                    else
                    {
                        DTTemp = CreateDTTemp();
                        if (!DTTemp.Columns.Contains("Checked"))
                            DTTemp.Columns.Add("Checked", typeof(bool));
                        if (!DTTemp.Columns.Contains("ApprovalID"))
                            DTTemp.Columns.Add("ApprovalID", typeof(int));
                        if (dtDocOrders.Columns.Contains("ClaimStatusID"))
                            DTTemp.Columns.Add("ClaimStatusID", typeof(int));
                        if (dtDocOrders.Columns.Contains("StatusName"))
                            DTTemp.Columns.Add("StatusName", typeof(string));
                        if (dtDocOrders.Columns.Contains("StatusColour"))
                            DTTemp.Columns.Add("StatusColour", typeof(int));
                        if (!DTTemp.Columns.Contains("ApprovalAMT"))
                            DTTemp.Columns.Add("ApprovalAMT", typeof(string));
                        DTTemp.AcceptChanges();
                        if (dtDocOrders.Columns.Contains("PrescriptionId"))
                            dtDocOrders.Columns["PrescriptionId"].ColumnName = "ProcId";
                        dtDocOrders.AcceptChanges();
                        for (int intCnt = 0; intCnt < dtDocOrders.Rows.Count; intCnt++)
                        {
                            dtTemp = DTTemp.NewRow();
                            dtTemp["ServiceId"] = dtDocOrders.Rows[intCnt]["ServiceId"];
                            dtTemp["ServiceName"] = dtDocOrders.Rows[intCnt]["ServiceName"];
                            dtTemp["ProcedureId"] = dtDocOrders.Rows[intCnt]["ProcedureId"];
                            dtTemp["Procedure"] = dtDocOrders.Rows[intCnt]["ProcedureName"];
                            dtTemp["SampleId"] = dtDocOrders.Rows[intCnt]["SampleId"];
                            dtTemp["Sample"] = dtDocOrders.Rows[intCnt]["SampleName"];
                            if (!string.IsNullOrEmpty(dtDocOrders.Rows[intCnt]["DeptId"].ToString()))
                                dtTemp["DeptId"] = dtDocOrders.Rows[intCnt]["DeptId"];
                            else
                                dtTemp["DeptId"] = "0";
                            dtTemp["DeptName"] = dtDocOrders.Rows[intCnt]["DeptName"];
                            if (!string.IsNullOrEmpty(dtDocOrders.Rows[intCnt]["SpecialiseId"].ToString()))
                                dtTemp["SpecialiseId"] = dtDocOrders.Rows[intCnt]["SpecialiseId"];
                            else
                                dtTemp["SpecialiseId"] = "0";
                            dtTemp["SpecialiseName"] = dtDocOrders.Rows[intCnt]["SpecialiseName"];
                            dtTemp["BedTypeId"] = -1;
                            dtTemp["BedTypeName"] = "OPD";
                            dtTemp["ProfileId"] = (dtDocOrders.Rows[intCnt]["ServiceId"].ToString() == "4" || (dtDocOrders.Rows[intCnt]["ServiceId"].ToString() == "3" && dtDocOrders.Rows[intCnt]["IsGroup"].ToString().ToUpper() == "TRUE")) ? dtDocOrders.Rows[intCnt]["ProcedureId"] : 0;
                            dtTemp["TariffId"] = Convert.ToInt32(strTariffID);
                            dtTemp["ScheduleId"] = 0;
                            dtTemp["ProcId"] = dtDocOrders.Rows[intCnt]["ProcId"];
                            dtTemp["BasePrice"] = dtDocOrders.Rows[intCnt]["BasePrice"] == DBNull.Value ? Convert.ToDecimal("0").ToString(hdnsCurrencyFormat) : Convert.ToDecimal(dtDocOrders.Rows[intCnt]["BasePrice"]).ToString(hdnsCurrencyFormat);
                            dtTemp["EligiblePrice"] = dtDocOrders.Rows[intCnt]["EligiblePrice"];
                            dtTemp["BillablePrice"] = dtDocOrders.Rows[intCnt]["BillablePrice"];
                            dtTemp["OrderId"] = dtDocOrders.Rows[intCnt]["OrderId"];
                            dtTemp["Isgroup"] = dtDocOrders.Rows[intCnt]["Isgroup"];
                            dtTemp["Checked"] = dtDocOrders.Rows[intCnt]["Checked"];
                            dtTemp["Price"] = Convert.ToDecimal(dtTemp["BasePrice"]).ToString(hdnsCurrencyFormat);
                            dtTemp["Quantity"] = dtDocOrders.Rows[intCnt]["Qty"]; //1;
                            dtTemp["MQTY"] = 1;
                            dtTemp["SQTY"] = 0;
                            dtTemp["Amount"] = Convert.ToDecimal(dtTemp["BasePrice"]).ToString(hdnsCurrencyFormat);
                            if (dtDocOrders.Columns.Contains("Priority") && !string.IsNullOrEmpty(dtDocOrders.Rows[intCnt]["Priority"].ToString()))
                                dtTemp["Priority"] = dtDocOrders.Rows[intCnt]["Priority"];
                            else
                                dtTemp["Priority"] = 13;

                            dtTemp["PatientType"] = 1;
                            if (dtDocOrders.Columns.Contains("Status"))
                            {
                                if (dtDocOrders.Rows[intCnt]["Status"] != DBNull.Value && !string.IsNullOrEmpty(dtDocOrders.Rows[intCnt]["Status"].ToString()))
                                    dtTemp["Status"] = dtDocOrders.Rows[intCnt]["Status"].ToString();
                            }
                            if (dtDocOrders.Columns.Contains("LOAApprovalID"))
                                dtTemp["ApprovalID"] = dtDocOrders.Rows[intCnt]["LOAApprovalID"];
                            if (!string.IsNullOrEmpty(Convert.ToString(dtDocOrders.Rows[intCnt]["ClaimStatusID"])))
                            {
                                dtTemp["ClaimStatusID"] = dtDocOrders.Rows[intCnt]["ClaimStatusID"];
                                dtTemp["StatusName"] = dtDocOrders.Rows[intCnt]["StatusName"];
                                dtTemp["StatusColour"] = dtDocOrders.Rows[intCnt]["StatusColour"];
                                dtTemp["ApprovalAMT"] = dtDocOrders.Rows[intCnt]["ApprovalAMT"];
                            }
                            DTTemp.Rows.Add(dtTemp);
                        }
                        DTTemp.AcceptChanges();
                    }

                    if (dtDocOrders != null && dtDocOrders.Rows.Count > 0)
                    {
                        if (dtDocOrders.Columns.Contains("DoctorName"))
                        {
                            hdnDocID = dtDocOrders.Rows[0]["DoctorId"].ToString();
                            hdnDocSpecialiseId = dtDocOrders.Rows[0]["intSpecid"].ToString();
                            EMRCOPAY = "YES";
                            DataTable dtSpecCofig = HISCONFIG.Copy();
                            if (dtSpecCofig.Select("Parameter ='DEPT_EMERGENCY' AND  HospitalId = " + strDefaultHospitalId + " ").Length > 0)
                            {
                                DataRow[] drtemp = dtSpecCofig.Select("Parameter ='DEPT_EMERGENCY'  AND  HospitalId = " + strDefaultHospitalId + " ");
                                if (dtDocOrders.Rows[0]["BDHospDeptID"].ToString() == drtemp[0]["Value"].ToString())
                                {
                                    drtemp = dtSpecCofig.Select("Parameter ='EMR_COPAY'  AND  HospitalId = " + strDefaultHospitalId + " ");
                                    if (drtemp.Length > 0)
                                    {
                                        if (drtemp[0]["Value"].ToString().ToUpper() == "NO")
                                            EMRCOPAY = "NO";
                                        else
                                            EMRCOPAY = "YES";
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(hdnDocID) && hdnDocID != null && (!string.IsNullOrEmpty(hdnDocSpecialiseId)) && (!string.IsNullOrEmpty(hdnPatientID)))
                            {
                                GetGradeDocSpec(Convert.ToInt32(hdnPatientID), Convert.ToInt32(hdnDocID), Convert.ToInt32(hdnDocSpecialiseId), true, PatientBillList);
                                if (hdnrblbilltypeCredit == true)
                                {
                                    string strLOAFilter = "Status=0 and Blocked = 0  and GradeId = " + Convert.ToInt32(strGradeID) + " and payerid=" + Convert.ToInt32(strCompanyID) + " and patienttype=1 and " +
                                        "patientId= " + Convert.ToInt32(hdnPatientID) + " and  todate >=\'" + DateTime.Now.ToString("dd-MMM-yyyy") + "\' and fromdate <=getdate()" + " and SpecialisationId=" + hdnDocSpecialiseId;
                                    GetLOA(strLOAFilter);
                                }
                            }
                        }
                    }
                }
                if (dtDocOrders != null && dtDocOrders.Rows.Count > 0 && dtDocOrders.Columns.Contains("LOAApprovalID"))
                {
                    hdnLOAApprovalID = dtDocOrders.Rows[0]["LOAApprovalID"].ToString();
                }
                if (DTTemp != null)
                {
                    for (int i = 0; i < DTTemp.Rows.Count; i++)
                    {
                        BasePrice = Convert.ToDecimal(DTTemp.Rows[i]["BasePrice"] == DBNull.Value ? -1 : DTTemp.Rows[i]["BasePrice"]);
                        EligiblePrice = Convert.ToDecimal(DTTemp.Rows[i]["EligiblePrice"] == DBNull.Value ? -1 : DTTemp.Rows[i]["EligiblePrice"]);
                        BillablePrice = Convert.ToDecimal(DTTemp.Rows[i]["BillablePrice"] == DBNull.Value ? -1 : DTTemp.Rows[i]["BillablePrice"]);
                        if ((int)BasePrice == -1)
                        {
                            DTTemp.Rows[i]["BasePrice"] = DBNull.Value;
                            DTTemp.Rows[i]["Price"] = Convert.ToDecimal("0").ToString(hdnsCurrencyFormat);
                        }
                        else
                        {
                            if ((decimal)BasePrice == 0)
                            {
                                DTTemp.Rows[i]["BasePrice"] = Convert.ToDecimal("0").ToString(hdnsCurrencyFormat);
                                DTTemp.Rows[i]["Price"] = Convert.ToDecimal("0").ToString(hdnsCurrencyFormat);
                            }
                            else
                            {
                                DTTemp.Rows[i]["BasePrice"] = Convert.ToDecimal(BasePrice).ToString(hdnsCurrencyFormat);
                                DTTemp.Rows[i]["Price"] = Convert.ToDecimal(BasePrice).ToString(hdnsCurrencyFormat);
                            }
                        }
                        if ((int)EligiblePrice == -1)
                        {
                            DTTemp.Rows[i]["EligiblePrice"] = DBNull.Value;
                        }
                        else
                        {
                            DTTemp.Rows[i]["EligiblePrice"] = Convert.ToDecimal(EligiblePrice).ToString(hdnsCurrencyFormat);
                        }
                        if ((int)BillablePrice == -1)
                        {
                            DTTemp.Rows[i]["BillablePrice"] = DBNull.Value;
                        }
                        else
                        {
                            DTTemp.Rows[i]["BillablePrice"] = Convert.ToDecimal(BillablePrice).ToString(hdnsCurrencyFormat);
                        }
                        if ((int)EligiblePrice == -1 && (int)BillablePrice == -1)
                        {
                            DTTemp.Rows[i]["Price"] = Convert.ToDecimal(BasePrice).ToString(hdnsCurrencyFormat);
                        }
                        else if ((int)EligiblePrice <= (int)BillablePrice)
                        {
                            if ((int)EligiblePrice >= 0)
                                DTTemp.Rows[i]["Price"] = Convert.ToDecimal(EligiblePrice).ToString(hdnsCurrencyFormat);
                        }
                        if ((decimal)EligiblePrice == 0 && (decimal)BillablePrice == 0 && (decimal)BasePrice != 0 && hdnrblbilltypecash == true)
                        {
                            DTTemp.Rows[i]["EligiblePrice"] = Convert.ToDecimal(BasePrice).ToString(hdnsCurrencyFormat);
                            DTTemp.Rows[i]["BillablePrice"] = Convert.ToDecimal(BasePrice).ToString(hdnsCurrencyFormat);
                            DTTemp.Rows[i]["Price"] = Convert.ToDecimal(BasePrice).ToString(hdnsCurrencyFormat);
                        }
                        if (!string.IsNullOrEmpty(DTTemp.Rows[i]["EligiblePrice"].ToString()) & !string.IsNullOrEmpty(DTTemp.Rows[i]["ApprovalAMT"].ToString()) && DTTemp.Rows[i]["ClaimStatusID"].ToString() == "4")
                        {
                            if (Convert.ToDecimal(DTTemp.Rows[i]["EligiblePrice"]) > (Convert.ToDecimal(DTTemp.Rows[i]["ApprovalAMT"])))
                                DTTemp.Rows[i]["EligiblePrice"] = Convert.ToDecimal(DTTemp.Rows[i]["ApprovalAMT"]);
                        }
                    }
                }
                #endregion DoctorOrders

                string strDoctorFollowup = ConfigurationManager.AppSettings["DoctorFollowup"].ToString();
                if (strDoctorFollowup.ToUpper() == "YES")
                {
                    #region doctor appointment followups
                    if (dtScheduldOrders.Rows.Count > 0 && hdnDocID != string.Empty)
                    {
                        string[] StrOrdTyp = null;
                        Doctorid = hdnDocID;
                        DataRow[] drsconsalttion = DTTemp.Select("ServiceId=2");
                        strDummy = string.Empty;
                        if ((drsconsalttion != null && drsconsalttion.Length > 0))
                        {
                            specilaize = drsconsalttion[0]["SpecialiseId"].ToString();
                            if (!string.IsNullOrEmpty(specilaize))
                            {
                                hdnProcedureID = drsconsalttion[0]["ProcedureId"].ToString();
                                if (hdnrblbilltypeCredit == true)
                                {
                                    DataSet dsCompanyforLOA = null;
                                    if (hdnrblbilltypeCredit == true && !string.IsNullOrEmpty(strCompanyID))
                                    {
                                        dsCompanyforLOA = FetchHospitalCompanyDetails(Convert.ToInt32(strCompanyID), "C", "8", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, Convert.ToInt32(strDefaultHospitalId));
                                    }
                                    if (hdnrblbilltypeCredit == true && dsCompanyforLOA != null)
                                    {
                                        if (dsCompanyforLOA.Tables.Count == 2)
                                        {
                                            AssignLOAConfigDetails(dsCompanyforLOA.Tables[1].Copy(), 1, Convert.ToInt32(hdnPatientID), Convert.ToInt32(strCompanyID), Convert.ToInt32(strGradeID), Convert.ToInt32(specilaize), PatientBillList);
                                        }
                                    }
                                    if (Convert.ToInt32(hdnLOAfollowupLimit) > 0 || Convert.ToInt32(hdnLOAfollowupDays) > 0)
                                    {
                                        string strYiaco = ConfigurationManager.AppSettings["Yaico"].ToString();
                                        if (strYiaco.ToUpper() == "YES")
                                        {

                                        }
                                        else
                                        {
                                            StrOrdTyp = CheckLOAConsultationFollowupConfig(Convert.ToInt32(hdnPatientID), Convert.ToInt32(specilaize), Convert.ToInt32(drsconsalttion[0]["ProcedureId"].ToString()), Convert.ToInt32(drsconsalttion[0]["SampleId"]), Convert.ToString(drsconsalttion[0]["Sample"]), Convert.ToInt32(hdnLOAfollowupDays), Convert.ToInt32(hdnLOAfollowupLimit), PatientBillList).Split(Convert.ToChar("/"));
                                        }
                                        if (StrOrdTyp.Length == 1)
                                        { return null; }

                                        if (StrOrdTyp[1].ToString() == "")
                                        { return null; }
                                        else
                                        {
                                            hdnProcedureID = DTTemp.Rows[0]["ProcedureID"].ToString();
                                            DTTemp.Rows[0]["SampleId"] = StrOrdTyp[0].ToString();
                                            DTTemp.Rows[0]["Sample"] = StrOrdTyp[1].ToString();
                                        }
                                    }
                                }
                                if (hdnLOAfollowupLimit == "0")
                                {
                                    #region Yiaco Followup: 
                                    string strYiaco = ConfigurationManager.AppSettings["Yaico"].ToString();
                                    if (strYiaco.ToUpper() == "YES")
                                    {

                                    }
                                    else
                                    {
                                        strDummy = CheckConsultationFollowupConfig(Convert.ToInt32(hdnPatientID), Convert.ToInt32(specilaize.ToString()), Convert.ToInt32(drsconsalttion[0]["ProcedureId"].ToString()), Convert.ToInt32(drsconsalttion[0]["SampleId"].ToString()), drsconsalttion[0]["Sample"].ToString(), PatientBillList);
                                    }
                                    #endregion Yiaco Followup
                                }
                            }
                        }
                        if (strDummy != "")
                        {
                            StrOrdTyp = strDummy.Split(Convert.ToChar("/"));
                            hdnProcedureID = DTTemp.Rows[0]["ProcedureID"].ToString();
                            DTTemp.Rows[0]["SampleId"] = StrOrdTyp[0].ToString();
                            DTTemp.Rows[0]["Sample"] = StrOrdTyp[1].ToString();
                            DTTemp.AcceptChanges();
                        }

                    }
                    #endregion
                }
                if (!DTTemp.Columns.Contains("ProcId"))
                    DTTemp.Columns.Add("ProcId");

                gdvSearchResultData = DTTemp.Copy();
                gdvSearchResultData.AcceptChanges();

                if (DTTemp.Rows.Count > 0)
                {
                    DataTable dtLegend = new DataTable();
                    dtLegend.Columns.Add("StatusName", typeof(string));
                    dtLegend.Columns.Add("StatusColor", typeof(int));
                    dtLegend.AcceptChanges();
                    if (DTTemp.Columns.Contains("ApprovalID"))
                    {
                        foreach (DataRow dr in DTTemp.Rows)
                        {
                            if (dr["ApprovalID"] != null)
                            {
                                if (dr["ApprovalID"].ToString().Trim() == "")
                                {
                                    dr["StatusName"] = "Covered";
                                    dr["StatusColour"] = -16728065; //color code for covered items
                                }
                            }
                        }
                    }
                }



                DTTem = DTTemp.Copy();
                DTTem.AcceptChanges();

                #region for checking FOllowup for consultation
                if (OrderTypeVisit == "Followup")
                {
                    FetchFollowupData(PatientBillList);
                }

                #endregion 
                return DTTem;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.LogToXMLFile = false;
            }
            finally
            {

            }
            return DTTem;
        }

        public PatientBiillInfoList PaymentCheck(PatientBillList PatientBillList, string PatientUHID, string hdnIsfamilyLOA)
        {
            try
            {
                string TypeMapi, FilterMAPI, OrderMAPI = string.Empty;
                if (!string.IsNullOrEmpty(hdnPatientID))
                {

                    decimal decERExtraLoaLimit = 0;
                    DataTable dtEmpty = null; DataTable dtpatient = null; DataTable DtOPServices = null;
                    DataTable DtCashBillItems = null; DataTable DtBillsummary = null;
                    DataTable DtCreditBillItems = null; DataTable dtTempOut = null;
                    DataTable dtCompanyCreditContribution = null; DataTable dtOrders = null;
                    DataTable dtTestProfile = null; DataRow[] rowarray = null;
                    DataTable dtPinBlock = null;
                    try
                    {
                        bool blnValidateQty;
                        #region ServicesLoaded Validation
                        string strCreditMsg = string.Empty;
                        string strCashMsg = string.Empty;

                        if (hdnrblbilltypeCredit == true)
                        {
                            string strCreditBlock = "Patientid=" + Convert.ToInt32(hdnPatientID) + " and status =0";
                            dtPinBlock = GetPINBlockDetail(hdnPatientID, Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0).Tables[0].Copy();
                            if (dtPinBlock.Rows.Count > 0)
                            {
                                if ((dtPinBlock.Rows[0]["Blocktype"].ToString()) == "0")
                                {
                                    objPatientList.Code = (int)ProcessStatus.Success;
                                    objPatientList.Status = ProcessStatus.Success.ToString();
                                    //objPatientList.Message = Resources.English.ResourceManager.GetString("Creditbillnotpossibleforpatient");
                                    //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("Creditbillnotpossibleforpatient");
                                    objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["Creditbillnotpossibleforpatient"].ToString();
                                    objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["Creditbillnotpossibleforpatient"].ToString();
                                    return objPatientList;
                                }
                                if ((dtPinBlock.Rows[0]["Blocktype"].ToString()) == "1")
                                {
                                    if (DTTem != null)
                                    {
                                        DataTable dt = DTTem.Copy();
                                        dt.AcceptChanges();
                                        if (dt.Rows.Count > 0)
                                        {

                                            foreach (DataRow drview in dt.Rows)
                                            {
                                                if (Convert.ToString(drview["serviceid"]) == "2")
                                                {
                                                    foreach (DataRow dr in dtPinBlock.Rows)
                                                    {
                                                        if (Convert.ToString(drview["SpecialiseID"]) == Convert.ToString(dr["SpecialiseID"].ToString()))
                                                        {
                                                            StringBuilder strInternal = new StringBuilder();
                                                            strInternal.Append("Credit bill not possible for  " + dr["SpecialiseName"] + " Specialistion </br>");
                                                            strInternal.Append("Reason :" + dr["BlockReason"].ToString() + "</br>");
                                                            strInternal.Append("Block Message :" + dr["Discription"].ToString() + "");
                                                            objPatientList.Code = (int)ProcessStatus.Success;
                                                            objPatientList.Status = ProcessStatus.Success.ToString();
                                                            objPatientList.Message = strInternal.ToString();
                                                            objPatientList.Message2L = strInternal.ToString();
                                                            return objPatientList;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (DTTem != null)
                        {
                            DTTemp = DTTem.Copy();
                            DTTemp.AcceptChanges();
                            #region Excluded Items

                            if (System.Configuration.ConfigurationManager.AppSettings["IsGeneralExcluded"] != null && hdnrblbilltypeCredit == true)
                            {
                                if (System.Configuration.ConfigurationManager.AppSettings["IsGeneralExcluded"].ToString() == "YES")
                                {
                                    string strGeneralExclusions = string.Empty;
                                    string strTemp = string.Empty;
                                    string strServiceItemName = "";

                                    DataTable dtExc = (DataTable)GeneralExclusions(PatientBillList);
                                    DataTable dtExclGrade = null;
                                    if (Exclusions != null)
                                    {
                                        dtExclGrade = Exclusions.Copy();
                                        dtExclGrade.AcceptChanges();
                                    }
                                    if (dtExc.Rows.Count > 0)
                                    {
                                        for (int i = 0; i < dtExc.Rows.Count; i++)
                                        {
                                            strTemp += dtExc.Rows[i]["serviceitemid"].ToString();
                                            strServiceItemName += dtExc.Rows[i]["ServiceItemName"].ToString() + "<br />";

                                            if (i != dtExc.Rows.Count - 1)
                                            {
                                                strTemp += ",";
                                            }
                                        }
                                        if (strTemp != "")
                                        {
                                            strTemp = " serviceitemid in (" + strTemp + ")";
                                        }
                                    }
                                    if (dtExclGrade != null)
                                    {
                                        if (dtExclGrade.Rows.Count > 0)
                                        {
                                            if (DTTemp != null)
                                            {
                                                for (int iGroup = 0; iGroup < DTTemp.Rows.Count; iGroup++)
                                                {
                                                    String searchProcedure = DTTemp.Rows[iGroup]["ProcedureId"].ToString();
                                                    bool contains = dtExclGrade.AsEnumerable()
                                                                   .Any(row => searchProcedure == row.Field<Int32>("itemid").ToString());
                                                    if (contains == true)
                                                    {
                                                        objPatientList.Code = (int)ProcessStatus.Success;
                                                        objPatientList.Status = ProcessStatus.Success.ToString();
                                                        objPatientList.Message = "The Following Items are generally excluded : <br /><br />" + DTTemp.Rows[iGroup]["Procedure"].ToString();
                                                        return objPatientList;
                                                    }
                                                }
                                            }
                                        }

                                    }
                                    if (strServiceItemName.Length > 0)
                                    {
                                        objPatientList.Code = (int)ProcessStatus.Success;
                                        objPatientList.Status = ProcessStatus.Success.ToString();
                                        objPatientList.Message = "The Following Items are generally excluded : <br /><br />" + strServiceItemName;
                                        return objPatientList;
                                    }
                                }
                            }
                            #endregion

                            if (DTTemp.Rows.Count == 0)
                            {
                                dtEmpty = CreateDTTemp();
                                FillEmptyGridServices(dtEmpty);
                                return null;
                            }
                            else
                            {
                                #region testvalidate

                                #endregion
                            }
                        }
                        else
                        {
                            return null;
                            DTTemp = CreateDTTemp();
                        }
                        #endregion ServicesLoaded Validation


                        //checking Whether doctor is selected or not.
                        #region ReferalDoctorValidation
                        bool IsCons = false;
                        foreach (DataRow drrow in gdvSearchResultData.Rows)
                        {
                            if (drrow[0].ToString() == "Consultation " || drrow[0].ToString() == "Consultation")
                            {
                                IsCons = true;
                            }
                        }
                        if (hdnDocID == string.Empty)
                        {
                            objPatientList.Code = (int)ProcessStatus.Success;
                            objPatientList.Status = ProcessStatus.Success.ToString();
                            objPatientList.Message = "Select Referred Doctor";
                            return objPatientList;
                        }
                        if (string.IsNullOrEmpty(hdnDocSpecialiseId))
                        {
                            objPatientList.Code = (int)ProcessStatus.Success;
                            objPatientList.Status = ProcessStatus.Success.ToString();
                            objPatientList.Message = "Select Referred Doctor";
                            return objPatientList;
                        }
                        #endregion ReferalDoctorValidation

                        #region Added 
                        DataSet dsConsultConfig = null;


                        int DocID = Convert.ToInt32(Doctorid);
                        int HospID = Convert.ToInt32(strDefaultHospitalId);
                        int intUId = Convert.ToInt32(strDefaultUserId);
                        int intWId = Convert.ToInt32(strDefWorkstationId);
                        dsConsultConfig = FetchConsultantConfiguration(DocID, Convert.ToInt32(HospID), "1", intUId, intWId, 0);

                        if (dsConsultConfig != null)
                        {
                            if (dsConsultConfig.Tables[0].Rows.Count > 0)
                            {
                                bool bWalkinLimit = false;
                                int MCCCount = 0, intActAppt = 0, intWalkinBilled = 0;
                                System.Collections.Generic.Dictionary<int, string> dictWeekdays = GetWeekDaysPair();
                                System.Collections.Generic.List<string> strWeekDays = GetWeekDays();
                                dsConsultConfig.Tables[0].Columns.Add("WeekDayName");
                                DateTime ScheduleDate = DateTime.Today;
                                string strFilter = " Blocked = 0 AND DoctorID=" + DocID + " AND Convert(Varchar(10),ScheduleDate,120)='" + ScheduleDate.ToString("yyyy-MM-dd") + "' AND HospitalID=" + HospID.ToString();
                                DataSet dsAppointments = FetchAppointment(strFilter, intUId, intWId, 0);
                                string strWalkinFilter = "ScheduleID is null and Convert(Varchar(10),OrderDate,110) = '" + ScheduleDate.ToString("MM-dd-yyyy") + "' and DoctorID = " + DocID.ToString();
                                DataSet dsWalkinBilled = FetchConsultations_Perf(1, strWalkinFilter, intUId, intWId, 0, 0, DocID, ScheduleDate, HospID);
                                if (dsAppointments.Tables.Count > 0)
                                {
                                    if (dsAppointments.Tables[0].Rows.Count > 0)
                                    {
                                        intActAppt = Convert.ToInt32(dsAppointments.Tables[0].Rows[0]["Count"]);
                                    }
                                }
                                if (dsWalkinBilled.Tables[0].Rows.Count > 0)
                                {
                                    intWalkinBilled = dsWalkinBilled.Tables[0].Rows.Count;
                                }
                                foreach (DataRow drv in dsConsultConfig.Tables[0].Rows)
                                {
                                    foreach (System.Collections.Generic.KeyValuePair<int, string> item in dictWeekdays)
                                    {
                                        if (item.Key.ToString() == drv["weekday"].ToString())
                                        {
                                            drv["WeekdayName"] = item.Value;
                                        }
                                    }
                                }
                                dsConsultConfig.Tables[0].AcceptChanges();
                                foreach (DataRow dr in dsConsultConfig.Tables[0].Rows)
                                {
                                    if (dr["WeekDayName"].ToString() == DateTime.Today.DayOfWeek.ToString())
                                    {
                                        bWalkinLimit = (Boolean)dr["WalkinLimitAlert"];
                                        MCCCount = Convert.ToInt32(dr["MaxConsultation"]);
                                        break;
                                    }
                                }
                                if (bWalkinLimit)
                                {
                                    objPatientList.Code = (int)ProcessStatus.Success;
                                    objPatientList.Status = ProcessStatus.Success.ToString();
                                    objPatientList.Message = "The Doctor has Walkin Limit specified for" + DateTime.Today.DayOfWeek.ToString();
                                    return objPatientList;
                                }
                                else
                                {
                                    int balWalkin = MCCCount - intActAppt - intWalkinBilled;
                                    if (balWalkin <= 0)
                                    {
                                        if (string.IsNullOrEmpty(WalkinBalance) || WalkinBalance.ToString() == "false")
                                        {
                                            //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','There is no balance walkin for the Doctor.<br>Max Consultation Count: " + MCCCount.ToString() + "<br>Walkin Balance: " + balWalkin.ToString() + "<br>Do you want to continue ?','YESNO','Warning');", true);
                                            WalkinBalance = "true";
                                            if (OrderTypeVisit == "Followup")
                                            {
                                                FetchFollowupData(PatientBillList);
                                            }
                                            //objPatientList.Code = ProcessStatus.Success;
                                            //objPatientList.Status = ProcessStatus.Success.ToString();
                                            //objPatientList.Message = "There is no balance walkin for the Doctor.<br>Max Consultation Count: " + DateTime.Today.DayOfWeek.ToString();
                                            //return objPatientList;
                                        }
                                    }
                                }
                            }
                        }

                        #endregion
                        if ((!string.IsNullOrEmpty(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["OPConsultationSameDayDepartment"]))))
                        {

                            if (System.Configuration.ConfigurationSettings.AppSettings["OPConsultationSameDayDepartment"].ToString().ToUpper() == "YES")
                            {
                                if (hdnrblbilltypeCredit == true)
                                {
                                    if (!string.IsNullOrEmpty(Convert.ToString(dt3)))
                                    {
                                        if (IsAllowSameDayConsultation())
                                        {
                                            if (Convert.ToBoolean(dt3.ToString()) == true)
                                            {
                                                objPatientList.Code = (int)ProcessStatus.Success;
                                                objPatientList.Status = ProcessStatus.Success.ToString();
                                                //objPatientList.Message = Resources.English.ResourceManager.GetString("CCDSDSD");
                                                //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("CCDSDSD");
                                                objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["CCDSDSD"].ToString();
                                                objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["CCDSDSD"].ToString();
                                                return objPatientList;

                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #region PayerGradeValidation
                        if (hdnrblbilltypeCredit == true)
                        {
                            if (!string.IsNullOrEmpty(strCompanyID.Trim()) && Convert.ToInt32(strCompanyID.Trim()) > 0 && (!string.IsNullOrEmpty(strGradeID)) && (Convert.ToInt32(strGradeID) > 0))
                            {

                            }
                            else
                            {
                                objPatientList.Code = (int)ProcessStatus.Success;
                                objPatientList.Status = ProcessStatus.Success.ToString();
                                objPatientList.Message = "Select Payer and Grade";
                                objPatientList.Message2L = "Select Payer and Grade";
                                return objPatientList;
                            }
                        }
                        #endregion PayerGradeValidation

                        #region CompanyPatientBlocked
                        if (hdnrblbilltypeCredit == true && (hdnblnCOmpanyBlocked == "true" || hdnblnCompanyExpired == "true" || hdnblnInsuranceExpired == "true"))
                        {
                            string BlockedMessage = string.Empty; string BlockedMessage2L = string.Empty;
                            if (hdnblnCOmpanyBlocked == "true")
                            {
                                //BlockedMessage = Resources.English.ResourceManager.GetString("CompanyPatientBlocked") + "< br />";
                                //BlockedMessage2L = Resources.Arabic.ResourceManager.GetString("CompanyPatientBlocked") + "< br />";
                                BlockedMessage = System.Configuration.ConfigurationManager.AppSettings["CompanyPatientBlocked"].ToString() + "< br />";
                                BlockedMessage2L = System.Configuration.ConfigurationManager.AppSettings["CompanyPatientBlocked"].ToString() + "< br />";
                                if (hdnblnCOmpanyBlockedReason != null)
                                {
                                    BlockedMessage = BlockedMessage + " Reason: " + hdnblnCOmpanyBlockedReason;
                                }
                            }
                            else if (hdnblnCompanyExpired == "true" || hdnblnInsuranceExpired == "true")
                            {
                                BlockedMessage = hdnblnCOmpanyBlockedReason;
                            }
                            if (TagId == 0)
                            {
                                objPatientList.Code = (int)ProcessStatus.Success;
                                objPatientList.Status = ProcessStatus.Success.ToString();
                                objPatientList.Message = BlockedMessage;
                                objPatientList.Message2L = BlockedMessage2L;
                            }
                            else
                            {
                                objPatientList.Code = (int)ProcessStatus.Success;
                                objPatientList.Status = ProcessStatus.Success.ToString();
                                objPatientList.Message = BlockedMessage;
                                objPatientList.Message2L = BlockedMessage2L;
                            }
                            return objPatientList;
                        }
                        #endregion CompanyPatientBlocked

                        #region InsuranceCardValidityCheck
                        if (dtpCardValid != null)
                        {
                            if (ValidateInsurancCard(dtpCardValid))
                            {
                                objPatientList.Code = (int)ProcessStatus.Success;
                                objPatientList.Status = ProcessStatus.Success.ToString();
                                objPatientList.Message = CompanyReturnMessage;
                                objPatientList.Message2L = CompanyReturnMessage2L;
                                return objPatientList;
                            }
                        }
                        #endregion InsuranceCardValidityCheck

                        #region ValidateCompanyContractDate

                        if (dtCompanyContract != null)
                        {
                            if (ValidateExpiryDate(dtCompanyContract, "ValidTo") == false && hdnrblbilltypecash == false)
                            {
                                objPatientList.Code = (int)ProcessStatus.Success;
                                objPatientList.Status = ProcessStatus.Success.ToString();
                                //objPatientList.Message = Resources.English.ResourceManager.GetString("CompanyContractDateExpired");
                                //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("CompanyContractDateExpired");
                                objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["CompanyContractDateExpired"].ToString();
                                objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["CompanyContractDateExpired"].ToString();
                                return objPatientList;
                            }
                        }
                        #endregion ValidateCompanyContractDate

                        #region GradeValidation
                        if (dtGradeValidation != null)
                        {
                            if (hdnrblbilltypeCredit == true && dtGradeValidation.Columns.Contains("GradeId"))
                            {
                                DataRow[] drGradeActive = dtGradeValidation.Select("GradeId=" + Convert.ToInt32(strGradeID));
                                if (drGradeActive.Length == 0)
                                {
                                    objPatientList.Code = (int)ProcessStatus.Success;
                                    objPatientList.Status = ProcessStatus.Success.ToString();
                                    //objPatientList.Message = Resources.English.ResourceManager.GetString("GradeBlocked");
                                    //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("GradeBlocked");
                                    objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["GradeBlocked"].ToString();
                                    objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["GradeBlocked"].ToString();
                                    return objPatientList;
                                }
                            }
                        }
                        #endregion GradeValidation

                        if (OtherOrders != null && dtLetter == null && !string.IsNullOrEmpty(hdnLOAApprovalID) && hdnrblbilltypeCredit == true)
                        {
                            string strLOAFilter = "status= 0 and Blocked = 0 and GradeId = " + Convert.ToInt32(strGradeID) + " and payerid=" + Convert.ToInt32(strCompanyID) + " and patienttype=1 and " +
                            "patientId= " + Convert.ToInt32(hdnPatientID) + " and  todate >=\'" + DateTime.Now.ToString("dd-MMM-yyyy") + "\' and fromdate <=getdate()" + " and SpecialisationId=" + hdnDocSpecialiseId;
                            if (!string.IsNullOrEmpty(hdnLetterIDforIPID) && hdnLetterIDforIPID != "0")
                                strLOAFilter = strLOAFilter + " and letterid=" + Convert.ToInt32(hdnLetterIDforIPID);
                            GetLOA(strLOAFilter);
                        }

                        #region Letter Validity
                        if (dtLetter != null)
                        {
                            dtLOAExpirydate = (DataTable)dtLetter;
                            if (hdnrblbilltypeCredit == true && ValidateExpiryDate(dtLOAExpirydate, "ToDate") == false)
                            {
                                objPatientList.Code = (int)ProcessStatus.Success;
                                objPatientList.Status = ProcessStatus.Success.ToString();
                                //objPatientList.Message = Resources.English.ResourceManager.GetString("LAVDE");
                                //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("LAVDE");
                                objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["LAVDE"].ToString();
                                objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["LAVDE"].ToString();
                                return objPatientList;
                            }
                        }
                        #endregion Letter Validity

                        #region ValidateConslationOnPHCPack
                        //isConsReqForPHC = ValidateConslationOnPHCPack(DTTemp);
                        #endregion ValidateConslationOnPHCPack                       

                        #region Package QtyChecking

                        DataRow[] drValidateOPPack = null;
                        drValidateOPPack = DTTemp.Select("ServiceName like '%OP Package%' And Quantity <= 0");
                        if (drValidateOPPack.Length > 0)
                        {
                            objPatientList.Code = (int)ProcessStatus.Success;
                            objPatientList.Status = ProcessStatus.Success.ToString();
                            objPatientList.Message = "Enter valid OP Package Quantity";
                            objPatientList.Message2L = "Enter valid OP Package Quantity";
                            return objPatientList;
                        }

                        drValidateOPPack = DTTemp.Select("ServiceName like '%OP Package%' And Quantity > 1");
                        if (drValidateOPPack.Length > 0)
                        {
                            objPatientList.Code = (int)ProcessStatus.Success;
                            objPatientList.Status = ProcessStatus.Success.ToString();
                            objPatientList.Message = "Enter valid OP Package Quantity";
                            objPatientList.Message2L = "Enter valid OP Package Quantity";
                            return objPatientList;
                        }
                        drValidateOPPack = DTTemp.Select("ServiceName like '%OP Package%' ");
                        if (drValidateOPPack.Length > 0)
                        {
                            if (DTTemp.Rows.Count > 1)
                            {

                            }

                        }
                        #endregion Package QtyChecking

                        #region ValidateQuaty
                        //blnValidateQty = ValidateQtySelection(DTTemp);
                        //if (blnValidateQty == false)
                        //    return null;
                        //Base objF = ValidateQtySelection(PatientBillList, DTTemp);
                        //if (objF.Status == "Success")
                        //{
                        //    return null;
                        //}
                        #endregion ValidateQuaty

                        StringBuilder strMsg = null;
                        StringBuilder strMsg2L = null;
                        #region DefaulLOAChecking
                        DataTable dtAR = new DataTable();
                        if (hdnrblbilltypeCredit == true)
                        {
                            if (Convert.ToInt32(hdnPatientID) > 0)
                            {
                                if (!string.IsNullOrEmpty(hdnIsfamilyLOA) && Convert.ToBoolean(hdnIsfamilyLOA) == true)
                                {

                                }
                                else
                                {
                                    if (CheckDefaultLOA(Convert.ToInt32(hdnPatientID.Trim()), Convert.ToInt32(strCompanyID), Convert.ToInt32(strGradeID), Convert.ToInt32(hdnDocSpecialiseId)) == false)
                                    {
                                        strMsg = new StringBuilder(); strMsg2L = new StringBuilder();
                                        //strMsg.Append(Resources.English.ResourceManager.GetString("LOANM") + "<\br>");
                                        //strMsg.Append(Resources.English.ResourceManager.GetString("CARD"));
                                        //strMsg2L.Append(Resources.Arabic.ResourceManager.GetString("LOANM") + "<\br>");
                                        //strMsg2L.Append(Resources.Arabic.ResourceManager.GetString("CARD"));
                                        strMsg.Append(System.Configuration.ConfigurationManager.AppSettings["LOANM"].ToString() + "<\br>");
                                        strMsg.Append(System.Configuration.ConfigurationManager.AppSettings["CARD"].ToString());
                                        strMsg2L.Append(System.Configuration.ConfigurationManager.AppSettings["LOANM"].ToString() + "<\br>");
                                        strMsg2L.Append(System.Configuration.ConfigurationManager.AppSettings["CARD"].ToString());
                                        objPatientList.Code = (int)ProcessStatus.Success;
                                        objPatientList.Status = ProcessStatus.Success.ToString();
                                        objPatientList.Message = strMsg.ToString();
                                        objPatientList.Message2L = strMsg2L.ToString();
                                        return objPatientList;
                                    }
                                    #region Coveragegroup validation

                                    DataTable dtItems = new DataTable();

                                    if (hdnDoctSearchName == "OPBillExternalDocSearch")
                                    {
                                        // dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CR' and PatientId=" + hdnPatientID + " and servicedocid=" + hdnDocID + " and datediff(day,Orderdate,getdate())<" + hdnLOAfollowupDays, "Order by ORDERDATE desc", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0];
                                        TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID";
                                        FilterMAPI = "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CR' and PatientId=" + hdnPatientID + " and servicedocid=" + hdnDocID + " and datediff(day,Orderdate,getdate())<" + hdnLOAfollowupDays;
                                        OrderMAPI = "Order by ORDERDATE desc";
                                        dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                    }

                                    else
                                    {
                                        //dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CR' and PatientId=" + hdnPatientID + " and servicedocid=" + hdnDocID + " and datediff(day,Orderdate,getdate())<" + hdnLOAfollowupDays, "Order by ORDERDATE desc", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0];
                                        TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID";
                                        FilterMAPI = "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CR' and PatientId=" + hdnPatientID + " and servicedocid=" + hdnDocID + " and datediff(day,Orderdate,getdate())<" + hdnLOAfollowupDays;
                                        OrderMAPI = "Order by ORDERDATE desc";
                                        dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                    }


                                    if (dtItems.Rows.Count == 0)
                                    {
                                        // dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + " and BillType='CR' and PatientId=" + hdnPatientID + " and datediff(day,Orderdate,getdate()) <=" + hdnLOAfollowupDays, "order by ORDERDATE desc", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0];
                                        TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID";
                                        FilterMAPI = "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + " and BillType='CR' and PatientId=" + hdnPatientID + " and datediff(day,Orderdate,getdate()) <=" + hdnLOAfollowupDays;
                                        OrderMAPI = "Order by ORDERDATE desc";
                                        dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                    }

                                    int inttempipid = 0;
                                    if (hdnIPID == "0" && dtItems.Rows.Count > 0)
                                    {
                                        LatestIPID = dtItems.Rows[0]["IPID"].ToString();
                                        inttempipid = Convert.ToInt32(dtItems.Rows[0]["IPID"]);
                                    }
                                    else
                                        inttempipid = Convert.ToInt32(hdnIPID);
                                    //DataTable dtapprovals = FetchMISProcedureDetails("Pr_FetchApprovalRequestAdv", "EntryID", "Customerid=" + strCompanyID + " and GradeID=" + strGradeID + " and specialiseid=" + hdnDocSpecialiseId + " and visitid=" + inttempipid, "order by EntryID", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0].Copy();
                                    DataTable dtapprovals = FetchApprovalRequestEntryIDMAPI(Convert.ToInt32(inttempipid), Convert.ToInt32(strCompanyID), Convert.ToInt32(strGradeID), Convert.ToInt32(hdnDocSpecialiseId), Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];
                                    string reqNumber = string.Empty;

                                    if (dtapprovals.Rows.Count > 0)
                                    {
                                        foreach (DataRow dr in dtapprovals.Select())
                                        {
                                            reqNumber = dr["entryid"].ToString();
                                            DataTable dttempAR = FetchApprovalRequest(Convert.ToInt32(dr["entryid"]), "2", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 1509, 5, "View App Request").Tables[0].Copy();
                                            if (dtAR.Rows.Count == 0)
                                                dtAR = dttempAR.Copy();
                                            else
                                                dtAR.Merge(dttempAR);
                                        }
                                    }
                                    if (Convert.ToBoolean(hdnIsDefaultLOA))
                                    {
                                        DataSet dsresultsconfig = GetCoverageConfiguration(Convert.ToInt32(strCompanyID), "0", false, 1, Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0);
                                        if (dsresultsconfig == null || dsresultsconfig.Tables[0].Rows.Count == 0)//
                                        {
                                            dsresultsconfig = GetCoverageConfiguration(Convert.ToInt32(strCompanyID), "0", false, 0, Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0);
                                        }
                                        //fetching Patient type=0 for insurance company in case if result is null for company
                                        if (dsresultsconfig == null || dsresultsconfig.Tables[0].Rows.Count == 0)
                                        {
                                            dsresultsconfig = GetCoverageConfiguration(Convert.ToInt32(strCompanyID), "0", true, 0, Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0);
                                        }
                                        //fetching Patient type=1 for insurance company in case if result is null for company
                                        if (dsresultsconfig == null || dsresultsconfig.Tables[0].Rows.Count == 0)
                                        {
                                            dsresultsconfig = GetCoverageConfiguration(Convert.ToInt32(strCompanyID), "0", true, 1, Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0);
                                        }
                                        int inttempGSPID = -1;
                                        if (!string.IsNullOrEmpty(hdnGradeSplzID.ToString()))
                                        {
                                            if (Convert.ToInt32(hdnGradeSplzID) >= 0)
                                                if (dsresultsconfig.Tables[2].Select("companyid=" + strCompanyID + " and gradeid = " + strGradeID + " and GradeSpecialiseID=" + hdnGradeSplzID).Length > 0)
                                                    inttempGSPID = Convert.ToInt16(hdnGradeSplzID);
                                        }
                                        strERExceedMSG = string.Empty;
                                        if (dsresultsconfig.Tables[2].Select("companyid=" + strCompanyID + " and gradeid = " + strGradeID + " and GradeSpecialiseID=" + inttempGSPID).Length > 0)
                                        {
                                            if (dsresultsconfig.Tables[3].Rows.Count > 0)
                                            {
                                                DataTable dtcvrgconfig = new DataTable();
                                                DataTable dtGroupsconfig = new DataTable();
                                                dtcvrgconfig = dsresultsconfig.Tables[3].Clone();
                                                foreach (DataRow drcvrg in dsresultsconfig.Tables[3].Select("companyid=" + strCompanyID + " and gradeid = " + strGradeID + " and GradeSpecialiseID=" + inttempGSPID))
                                                {
                                                    if (drcvrg["Packageitemcategoryid"] == DBNull.Value && drcvrg["serviceid"] != null)
                                                    {
                                                        dtcvrgconfig.ImportRow(drcvrg); //Excluded items configuration
                                                    }
                                                    else if (drcvrg["Packageitemcategoryid"] != DBNull.Value)
                                                    {
                                                        //Excluded coverage group items configuration
                                                        DataTable dtcategory = GetCategoryItems(Convert.ToInt32(drcvrg["Packageitemcategoryid"]), Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 22566, 5, "Fetching Pharmacy Items", 0).Tables[0];
                                                        if (dtGroupsconfig == null)
                                                            dtGroupsconfig = dtcategory.Copy();
                                                        else
                                                            dtGroupsconfig.Merge(dtcategory);
                                                    }
                                                }

                                                StringBuilder strexclmsgitems = new StringBuilder("");
                                                foreach (DataRow dritem in DTTemp.Select())
                                                {
                                                    Boolean isexcluded = false;
                                                    if (dtcvrgconfig != null && dtcvrgconfig.Rows.Count > 0)//validating items configuration
                                                    {
                                                        if (dtAR.Rows.Count > 0)
                                                        {
                                                            DataRow[] drARResult = null;
                                                            if (!string.IsNullOrEmpty(dritem["procid"].ToString()) && hdnPatientType != "3") //When patienttype is 3, ProcID is fetching as 0.  //Code changes made for TFS ID :: 165983
                                                                drARResult = dtAR.Select("(ClaimStatusID<>1 and ClaimStatusID<>3 and ClaimStatusID<>4) and ItemId=" + dritem["ProcedureId"] + " and prescriptionid=" + dritem["procid"]);

                                                            else
                                                                drARResult = dtAR.Select("(ClaimStatusID<>1 and ClaimStatusID<>3 and ClaimStatusID<>4) and ItemId=" + dritem["ProcedureId"]);
                                                            if (drARResult.Length > 0)
                                                            {
                                                                if (drARResult[0]["ClaimStatusID"].ToString() == "0")
                                                                    strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + " - <b>Pending</b>.<b> ApprovalRequest no is </b>  " + reqNumber + "<br/>");

                                                                else
                                                                    strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + " - <b>" + drARResult[0]["ClaimStatus"].ToString() + "</b> <br/>");
                                                                isexcluded = true;
                                                            }
                                                        }
                                                        else if (dtcvrgconfig.Select("ItemID=" + dritem["ProcedureId"]).Length > 0)
                                                        {
                                                            strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + " -  <br/>");
                                                            isexcluded = true;
                                                        }
                                                    }
                                                    if (isexcluded == false && dtGroupsconfig != null && dtGroupsconfig.Rows.Count > 0) //validating items in coverage
                                                    {
                                                        if (dtAR.Rows.Count > 0)
                                                        {

                                                            DataRow[] drARResult = null;
                                                            if (!string.IsNullOrEmpty(dritem["procid"].ToString()) && hdnPatientType != "3")  //When patienttype is 3, ProcID is fetching as 0.  //Code changes made for TFS ID :: 165983
                                                                drARResult = dtAR.Select("(ClaimStatusID<>1 and ClaimStatusID<>3 and ClaimStatusID<>4) and ItemId=" + dritem["ProcedureId"] + " and prescriptionid=" + dritem["procid"]);

                                                            else
                                                                drARResult = dtAR.Select("(ClaimStatusID<>1 and ClaimStatusID<>3 and ClaimStatusID<>4) and ItemId=" + dritem["ProcedureId"]);

                                                            if (drARResult.Length > 0)
                                                            {
                                                                drARResult = dtAR.Select("(ClaimStatusID<>1 and ClaimStatusID<>3 and ClaimStatusID<>4) and ItemId=" + dritem["ProcedureId"]);
                                                                if (drARResult[0]["ClaimStatusID"].ToString() == "0")
                                                                    strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + " - <b> Approval Pending" + "</b>.<b> ApprovalRequest no is </b> " + reqNumber + "<br/>");
                                                                else
                                                                    strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + " - <b>" + drARResult[0]["ClaimStatus"].ToString() + "</b> <br/>");


                                                            }
                                                            else
                                                            {
                                                                //DataRow[] drARResult = dtAR.Select("(ClaimStatusID=1 or ClaimStatusID=3 or ClaimStatusID=4) and ItemId=" + dritem["ProcedureId"]);
                                                                drARResult = null;
                                                                if (!string.IsNullOrEmpty(dritem["procid"].ToString()) && hdnPatientType != "3")
                                                                    drARResult = dtAR.Select("(ClaimStatusID=1 or ClaimStatusID=3 or ClaimStatusID=4) and ItemId=" + dritem["ProcedureId"] + " and prescriptionid=" + dritem["procid"]);

                                                                else
                                                                    drARResult = dtAR.Select("(ClaimStatusID=1 or ClaimStatusID=3 or ClaimStatusID=4) and ItemId=" + dritem["ProcedureId"]);

                                                                if (drARResult.Length > 0)
                                                                {
                                                                    if (dtAR.Columns.Contains("BilledQty"))
                                                                    {
                                                                        if (Convert.ToInt16(drARResult[0]["Quantity"]) - Convert.ToInt16(drARResult[0]["BilledQty"]) < Convert.ToInt16(dritem["Quantity"]))
                                                                        {
                                                                            strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + " - <b>" + drARResult[0]["ClaimStatus"].ToString() + "</b>. Approved QTY: <b> " + drARResult[0]["Quantity"].ToString() + "</b> <br/>");
                                                                        }
                                                                    }
                                                                    else if (OtherOrders != null)
                                                                    {
                                                                        DataTable dtOtherDocOrders = ((DataSet)OtherOrders).Tables["DoctOrders"].Copy();
                                                                        if ((Convert.ToInt16(drARResult[0]["Quantity"]) - (Convert.ToInt16(dtOtherDocOrders.Select("ProcedureID=" + dritem["ProcedureId"])[0]["PrescribedQty"]) - Convert.ToInt16(dtOtherDocOrders.Select("ProcedureID=" + dritem["ProcedureId"])[0]["Qty"]))) < Convert.ToInt16(dritem["Quantity"]))
                                                                        {
                                                                            strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + " - <b>" + drARResult[0]["ClaimStatus"].ToString() + "</b>. Approved QTY: <b> " + drARResult[0]["Quantity"].ToString() + "</b> <br/>");
                                                                        }
                                                                    }
                                                                    else if (Convert.ToInt16(drARResult[0]["Quantity"]) < Convert.ToInt16(dritem["Quantity"]))
                                                                    {
                                                                        strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + " - <b>" + drARResult[0]["ClaimStatus"].ToString() + "</b>. Approved QTY: <b> " + drARResult[0]["Quantity"].ToString() + "</b> <br/>");
                                                                    }

                                                                }
                                                                else
                                                                {


                                                                    if (dtGroupsconfig.Select("mlevel=5 and itemid=" + dritem["ProcedureId"]).Length > 0)
                                                                        strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + "<br/>");

                                                                    if (!string.IsNullOrEmpty(dritem["SpecialiseId"].ToString()))
                                                                        if (dtGroupsconfig.Select("mlevel=4 and SpecializationID=" + dritem["SpecialiseId"] + " and HospDeptId=" + dritem["DeptId"] + " and Serviceid=" + dritem["Serviceid"]).Length > 0)
                                                                            strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + "<br/>");

                                                                    if (!string.IsNullOrEmpty(dritem["DeptId"].ToString()))
                                                                        if (dtGroupsconfig.Select("mlevel=3 and HospDeptId=" + dritem["DeptId"] + " and Serviceid=" + dritem["Serviceid"]).Length > 0)
                                                                            strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + "<br/>");

                                                                    if (dtGroupsconfig.Select("mlevel=2 and Serviceid=" + dritem["Serviceid"]).Length > 0)
                                                                        strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + "<br/>");

                                                                }
                                                            }

                                                        }
                                                        else
                                                        {


                                                            if (dtGroupsconfig.Select("mlevel=5 and itemid=" + dritem["ProcedureId"]).Length > 0)
                                                                strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + "<br/>");

                                                            if (!string.IsNullOrEmpty(dritem["SpecialiseId"].ToString()))
                                                                if (dtGroupsconfig.Select("mlevel=4 and SpecializationID=" + dritem["SpecialiseId"] + " and HospDeptId=" + dritem["DeptId"] + " and Serviceid=" + dritem["Serviceid"]).Length > 0)
                                                                    strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + "<br/>");

                                                            if (!string.IsNullOrEmpty(dritem["DeptId"].ToString()))
                                                                if (dtGroupsconfig.Select("mlevel=3 and HospDeptId=" + dritem["DeptId"] + " and Serviceid=" + dritem["Serviceid"]).Length > 0)
                                                                    strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + "<br/>");

                                                            if (dtGroupsconfig.Select("mlevel=2 and Serviceid=" + dritem["Serviceid"]).Length > 0)
                                                                strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + "<br/>");



                                                        }
                                                    }
                                                }

                                                if (strexclmsgitems.Length > 0)
                                                {
                                                    if (hdnPatientType != "3")
                                                    {
                                                        if (dtAR.Rows.Count == 0)
                                                        {
                                                            objPatientList.Code = (int)ProcessStatus.Success;
                                                            objPatientList.Status = ProcessStatus.Success.ToString();
                                                            objPatientList.Message = "Following item(s) NEED APPROVAL to proceed further<br/>" + strexclmsgitems.ToString();
                                                        }
                                                        else
                                                        {
                                                            objPatientList.Code = (int)ProcessStatus.Success;
                                                            objPatientList.Status = ProcessStatus.Success.ToString();
                                                            objPatientList.Message = "Approval Required for selected Items <br/>" + strexclmsgitems.ToString();
                                                        }
                                                        return objPatientList;
                                                    }
                                                    else
                                                        strERExceedMSG = strexclmsgitems.ToString();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            StringBuilder strexclmsgitems = new StringBuilder("");
                                            foreach (DataRow dritem in DTTemp.Select())
                                            {
                                                Boolean isexcluded = false;
                                                if (dtAR.Rows.Count > 0)
                                                {
                                                    DataRow[] drARResult = null;
                                                    if (!string.IsNullOrEmpty(dritem["procid"].ToString()) && hdnPatientType != "3")  //When patienttype is 3, ProcID is fetching as 0.  //Code changes made for TFS ID :: 165983
                                                    {
                                                        if (DTTemp.Select("ProcedureId=" + dritem["ProcedureId"]).Length > 1 || dtAR.Select("ItemId=" + dritem["ProcedureId"]).Length > 1)
                                                            drARResult = dtAR.Select("(ClaimStatusID<>1 and ClaimStatusID<>3 and ClaimStatusID<>4) and ItemId=" + dritem["ProcedureId"] + " and prescriptionid=" + dritem["procid"]);

                                                        else
                                                            drARResult = dtAR.Select("(ClaimStatusID<>1 and ClaimStatusID<>3 and ClaimStatusID<>4) and ItemId=" + dritem["ProcedureId"]);
                                                    }
                                                    else
                                                        drARResult = dtAR.Select("(ClaimStatusID<>1 and ClaimStatusID<>3 and ClaimStatusID<>4) and ItemId=" + dritem["ProcedureId"]);
                                                    if (drARResult.Length > 0)
                                                    {
                                                        if (drARResult[0]["ClaimStatusID"].ToString() == "0")
                                                            strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + " - <b>Pending</b>  and approval  request no is  " + reqNumber + "<br/>");
                                                        else
                                                            strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + " - " + drARResult[0]["ClaimStatus"].ToString() + "<br/>");
                                                        isexcluded = true;
                                                    }
                                                }
                                            }
                                            if (strexclmsgitems.Length > 0)
                                            {
                                                if (hdnPatientType != "3")
                                                {
                                                    if (dtAR.Rows.Count == 0)
                                                    {
                                                        objPatientList.Code = (int)ProcessStatus.Success;
                                                        objPatientList.Status = ProcessStatus.Success.ToString();
                                                        objPatientList.Message = "Following item(s) NEED APPROVAL to proceed further<br/>" + strexclmsgitems.ToString();
                                                    }
                                                    else
                                                    {
                                                        objPatientList.Code = (int)ProcessStatus.Success;
                                                        objPatientList.Status = ProcessStatus.Success.ToString();
                                                        objPatientList.Message = "Approval Status for selected Items <br/>" + strexclmsgitems.ToString();
                                                    }
                                                    return objPatientList;
                                                }
                                                else
                                                    strERExceedMSG = strexclmsgitems.ToString();
                                            }

                                        }
                                    }
                                    #endregion
                                }
                            }
                        }

                        #endregion DefaulLOAChecking

                        if (Service != null)
                            DtOPServices = (DataTable)Service;

                        DataRow[] dtRow = null;
                        string strPrice = string.Empty;
                        if (hdnrblbilltypecash == true)
                            hdnTariffID = "-1";

                        for (int intCount = 0; intCount < gdvSearchResultData.Rows.Count; intCount++)
                        {
                            if (gdvSearchResultData.Rows[intCount]["ServiceId"].ToString() == Convert.ToString(DTTemp.Rows[intCount]["ServiceId"]) && (gdvSearchResultData.Rows[intCount]["ProcedureID"].ToString() == Convert.ToString(DTTemp.Rows[intCount]["ProcedureID"])) && (gdvSearchResultData.Rows[intCount]["SampleId"].ToString() == Convert.ToString(DTTemp.Rows[intCount]["SampleId"])) && (gdvSearchResultData.Rows[intCount]["DeptId"].ToString() == Convert.ToString(DTTemp.Rows[intCount]["DeptId"])) && (gdvSearchResultData.Rows[intCount]["SpecialiseId"].ToString() == Convert.ToString(DTTemp.Rows[intCount]["SpecialiseId"])))
                            {
                                dtRow = DtOPServices.Select("id = " + DTTemp.Rows[intCount]["ServiceId"]);
                                if (dtRow.Length > 0 && Convert.ToInt32(dtRow[0]["OPEdit"]) == 1)
                                {

                                }
                                else
                                {
                                    DTTemp.Rows[intCount]["Price"] = Convert.ToDecimal((gdvSearchResultData.Rows[intCount]["Price"]));
                                    DTTemp.Rows[intCount]["BasePrice"] = DTTemp.Rows[intCount]["Price"];
                                    DTTemp.Rows[intCount]["EligiblePrice"] = DTTemp.Rows[intCount]["EligiblePrice"];
                                    DTTemp.Rows[intCount]["BillablePrice"] = DTTemp.Rows[intCount]["Price"];
                                }

                                string strqty = (gdvSearchResultData.Rows[intCount]["Quantity"]).ToString();
                                if (string.IsNullOrEmpty(strqty) || strqty == "0")
                                {
                                    objPatientList.Code = (int)ProcessStatus.Success;
                                    objPatientList.Status = ProcessStatus.Success.ToString();
                                    objPatientList.Message = "Quantity can not be  Zero/Empty";
                                    return objPatientList;
                                }
                                DTTemp.Rows[intCount]["Quantity"] = strqty;
                                DTTemp.Rows[intCount]["MQTY"] = strqty;
                                DTTemp.Rows[intCount]["SQTY"] = 0;
                                DTTemp.Rows[intCount]["PPAY"] = 0;
                                DTTemp.Rows[intCount]["CPAY"] = 0;
                                DTTemp.Rows[intCount]["SPAY"] = 0;
                                DTTemp.Rows[intCount]["DPAY"] = 0;
                                DTTemp.Rows[intCount]["PAmount"] = 0;
                                DTTemp.Rows[intCount]["Amount"] = 0;
                                DTTemp.Rows[intCount]["UnitRate"] = 0;
                                DTTemp.Rows[intCount]["TariffId"] = Convert.ToInt32(hdnTariffID);
                            }
                        }

                        if (DTTemp.Columns.Contains("SlNo"))
                            DTTemp.Columns.Remove("SlNo");
                        else if (DTTemp.Columns.Contains("Checked"))
                            DTTemp.Columns.Remove("Checked");
                        DTTemp.AcceptChanges();

                        #region LOA Item Status Checking

                        if (dtLetter != null)
                        {
                            Int32 OrderedDoctor = 0; Int32 LOADoctor = 0;
                            string ItemStatus = "false";
                            if (OtherOrders != null)
                            {
                                DataSet DsOthrOrders = (DataSet)OtherOrders;
                                DataTable dtDoctorOrders = new DataTable();
                                if (DsOthrOrders.Tables[0].Rows.Count > 0)
                                {
                                    dtDoctorOrders = DsOthrOrders.Tables[0];
                                }
                                if (dtDoctorOrders.Rows.Count > 0)
                                {
                                    Int32.TryParse(dtDoctorOrders.Rows[0]["DoctorId"].ToString(), out OrderedDoctor);
                                }
                            }
                            DataSet dsLOA = new DataSet();
                            dtLOAItemStatus = (DataTable)dtLetter;
                            int intLetterId = 0;
                            if (dtLOAItemStatus.Rows.Count > 0)
                            {
                                intLetterId = Convert.ToInt32(dtLOAItemStatus.Rows[0]["Letterid"]);
                                Int32.TryParse(dtLOAItemStatus.Rows[0]["DoctorId"].ToString(), out LOADoctor);
                                if (OrderedDoctor != 0 && LOADoctor != 0 && OrderedDoctor == LOADoctor)
                                {
                                    ItemStatus = "true";
                                }
                            }
                            int HospitalId = Convert.ToInt32(strDefaultHospitalId);
                            int intEntryID = 0;
                            dsLOA = GetLOA(intLetterId, 0, "1,3,7,8", 0, 0, 0, 391, 5, "PATIENT LETTER OF AUTHORITY", HospitalId);
                            if (dsLOA.Tables[0].Rows.Count > 0)
                            {
                                Int32.TryParse(dsLOA.Tables[0].Rows[0]["LOAAprovalEntryID"].ToString(), out intEntryID);
                            }
                            int intWorkStationId = Convert.ToInt32(strDefWorkstationId);
                            DataSet dsTemp = new DataSet();
                            if (intEntryID == 0)
                                dtLOAItems = null;
                            else
                            {
                                dsTemp = FetchApprovalRequest(intEntryID, "2", Convert.ToInt32(strDefaultUserId), intWorkStationId, 1509, 5, "View App Request");
                                if (dsTemp.Tables[0].Rows.Count > 0)
                                {
                                    dtLOAItems = dsTemp.Tables[0];
                                }
                            }

                            if (DTTemp != null && dtLOAItems != null)
                            {
                                foreach (DataRow dr in DTTemp.Rows)
                                {
                                    foreach (DataRow dr1 in dtLOAItems.Rows)
                                    {
                                        if (Convert.ToString(dr["ServiceId"]) == Convert.ToString(dr1["ServiceId"]))
                                        {
                                            if (Convert.ToString(dr["ProcedureId"]) == Convert.ToString(dr1["ItemId"]))
                                            {
                                                dr["ClaimStatus"] = dr1["ClaimStatus"].ToString();
                                            }
                                        }
                                    }
                                }
                            }

                            DTTemp.AcceptChanges();
                            DataTable dtNotApproved = new DataTable();
                            dtNotApproved.Columns.Add("Item Name");
                            dtNotApproved.Columns.Add("Service");
                            int docid = 0;

                            if (DTTemp != null)
                            {
                                foreach (DataRow dr in DTTemp.Rows)
                                {
                                    if ((OtherOrders != null && ItemStatus != "false" && docid != 0 && OrderedDoctor != 0 && docid == OrderedDoctor) || OtherOrders == null)
                                    {
                                        if (Convert.ToString(dr["ClaimStatus"]) == "Pending")
                                        {
                                            dr["BasePrice"] = 0;
                                            dr["EligiblePrice"] = 0;
                                            dr["Price"] = 0;
                                            dr["BillablePrice"] = 0;
                                            DataRow drNotApproved = dtNotApproved.NewRow();
                                            drNotApproved["Item Name"] = Convert.ToString(dr["Procedure"]);
                                            drNotApproved["Service"] = Convert.ToString(dr["ServiceName"]);
                                            dtNotApproved.Rows.Add(drNotApproved);
                                        }
                                    }
                                    ApprovalPendingItems = dtNotApproved;
                                }
                            }
                            DTTemp.AcceptChanges();

                        }

                        #endregion LOA Item Status Checking

                        DTPatientData = DTPatientDetails();
                        DataRow drRow = DTPatientData.NewRow();
                        drRow["PatientId"] = Convert.ToInt32(hdnPatientID);
                        int HospitalID = Convert.ToInt32(strDefaultHospitalId);
                        drRow["HospID"] = HospitalID;

                        if (hdnrblbilltypecash == true)
                        { drRow["BillType"] = 1; BillType = "1"; }
                        else
                        { drRow["BillType"] = 2; BillType = "2"; }

                        if (!string.IsNullOrEmpty(strCompanyID))
                            drRow["CompanyID"] = Convert.ToInt32(strCompanyID);
                        else
                            drRow["CompanyID"] = 0;
                        if (!string.IsNullOrEmpty(hdnTariffID))
                            drRow["TariffID"] = Convert.ToInt32(hdnTariffID);
                        else
                            drRow["TariffID"] = -1;
                        if (!string.IsNullOrEmpty(strGradeID))
                        {
                            drRow["GradeID"] = Convert.ToInt32(strGradeID);
                        }
                        else
                        {
                            drRow["GradeID"] = 0;
                            drRow["GradeName"] = "";
                        }
                        int letterNo = 0;
                        if (!string.IsNullOrEmpty(ViewStateLetterid))
                        {
                            string strLetterNo = ViewStateLetterid.ToString();
                            letterNo = Convert.ToInt32(strLetterNo);
                        }
                        if (VSParentLetterid != null)
                        {
                            if (!string.IsNullOrEmpty(VSParentLetterid.Trim()))
                                ParentLetterId = Convert.ToInt32(VSParentLetterid.ToString());
                        }
                        drRow["LetterNo"] = letterNo;
                        drRow["cmbOPackageSelected"] = 0;
                        drRow["BedTypeID"] = -1;
                        drRow["BedTypeName"] = "OPD";
                        if (!string.IsNullOrEmpty(hdnPatientType.Trim()) && hdnPatientType.Trim() != "null")
                            drRow["PatientType"] = Convert.ToInt32(hdnPatientType.Trim());
                        else
                            drRow["PatientType"] = 1;
                        drRow["CollectableType"] = (hdnCollectableType);
                        if (Convert.ToInt32(hdnPatientID) == 0)
                            drRow["IsCardCollectable"] = 0;
                        else
                        {
                            if (!string.IsNullOrEmpty(hdnIsCardCollectable))
                                drRow["IsCardCollectable"] = Convert.ToInt32(hdnIsCardCollectable);
                            else
                                drRow["IsCardCollectable"] = 0;
                        }
                        drRow["MaxCollectable"] = MaxCollectable;

                        if (DTTemp.Select("priority = 15").Length > 0)
                            drRow["Priority"] = 15;
                        else
                            if (DTTemp.Select("priority = 13").Length > 0)
                            drRow["Priority"] = 13;
                        else
                            drRow["Priority"] = 1;

                        if (hdnrblbilltypecash == true)
                            BillType = "1";
                        if (hdnrblbilltypeCredit == true)
                            BillType = "2";
                        if (!string.IsNullOrEmpty(PatientUHID.Trim()))
                        {
                            drRow["UHID"] = PatientUHID.Trim();
                            IPIDUHID = PatientUHID.Trim();
                            PType = hdnPatientType;
                        }
                        else
                        {
                            drRow["UHID"] = "0";
                            IPIDUHID = "0";
                            PType = "1";
                        }
                        drRow["SessionId"] = "123123123"; // System.Web.HttpContext.Current.Session.SessionID;
                        SessionID = drRow["SessionId"].ToString();
                        drRow["IsDefaultLOA"] = hdnIsDefaultLOA == "false" ? false : true;
                        if (!string.IsNullOrEmpty(hdnDocSpecialiseId))
                            drRow["RefDocSpecId"] = hdnDocSpecialiseId;
                        DTPatientData.Rows.Add(drRow);
                        DTPatientData.AcceptChanges();
                        PatientDet = DTPatientData;
                        PatientDet.AcceptChanges();

                        if (TestProfile != null)
                        {
                            dtTestProfile = TestProfile.Copy();
                            dtTestProfile.AcceptChanges();
                        }
                        else
                        {
                            dtTestProfile = DTTestProfiles();
                            dtTestProfile.AcceptChanges();
                        }
                        dtTestProfile.AcceptChanges();
                        if (OPPackageOrders != null)
                            dtOPPackageOrders = OPPackageOrders.Copy();
                        else
                            dtOPPackageOrders = DTOPPackage();
                        dtOPPackageOrders.AcceptChanges();
                        dtOrders = DtOtherOrders();

                        #region OtherOrders
                        if (OtherOrders != null)
                        {
                            dsTempOtherOrders = OtherOrders.Copy();
                            if (dsTempOtherOrders != null && dsTempOtherOrders.Tables.Count > 0)
                            {
                                dtTempOthersTable = dsTempOtherOrders.Tables["DoctOrders"].Copy();
                                dtTempOthersTable.TableName = "OtherOrderBillDetail";
                                dtTempOthersTable.AcceptChanges();
                            }
                        }

                        if (ServiceOrders != null)
                        {
                            dsTempOtherOrders = ServiceOrders.Copy();
                            dsTempOtherOrders.AcceptChanges();
                            if (dsTempOtherOrders != null && dsTempOtherOrders.Tables.Count > 0)
                            {
                                dtTempOthersTable = dsTempOtherOrders.Tables["DoctOrders"].Copy();
                                dtTempOthersTable.TableName = "OtherOrderBillDetail";
                                dtTempOthersTable.AcceptChanges();

                            }
                        }
                        #endregion OtherOrders

                        DsBillDetails = new DataSet();
                        DsBillDetails.Tables.Add(DTPatientData);

                        if (!DTTemp.Columns.Contains("OrderDocID"))
                            DTTemp.Columns.Add("OrderDocID", typeof(int));
                        DataTable dtMisc = null;
                        if (MiscDoctorEntired != null)
                        {
                            dtMisc = MiscDoctorEntired.Copy();
                            dtMisc.AcceptChanges();
                        }
                        if (dtMisc != null)
                        {
                            foreach (DataRow Dr in dtMisc.Rows)
                            {
                                DataRow[] drtemp = DTTemp.Select("Procedureid= " + Dr["Procedureid"]);
                                if (drtemp.Length > 0)
                                    drtemp[0]["OrderDocID"] = Dr["Doctorid"];
                                DTTemp.AcceptChanges();
                            }
                        }

                        #region VAT Calulation 
                        DataSet dsVATMasterConfig = new DataSet();
                        dsVATMasterConfig = GetMasterConfigurationForVAT(1, Convert.ToInt16(strDefaultHospitalId), 1);

                        if (!DTTemp.Columns.Contains("IsSaudi"))
                            DTTemp.Columns.Add("IsSaudi", typeof(bool)).DefaultValue = false;
                        bool VATFlag = false;
                        for (int i = 0; i < DTTemp.Rows.Count; i++)
                        {
                            VATFlag = false;
                            int serviceItemId = Convert.ToInt32(DTTemp.Rows[i]["ProcedureId"]);
                            int serviceId = Convert.ToInt32(DTTemp.Rows[i]["ServiceId"]);
                            for (int j = 0; j < dsVATMasterConfig.Tables[4].Rows.Count; j++)
                            {
                                if (serviceItemId == Convert.ToInt32(dsVATMasterConfig.Tables[4].Rows[j]["ServiceItemID"]) && serviceId == Convert.ToInt32(dsVATMasterConfig.Tables[4].Rows[j]["ServiceID"]))
                                {
                                    if (dsVATMasterConfig.Tables[4].Rows[j]["EffectiveDate"] != DBNull.Value)
                                    {
                                        if (Convert.ToDateTime(dsVATMasterConfig.Tables[4].Rows[j]["EffectiveDate"]) <= DateTime.Now)
                                        {
                                            DTTemp.Rows[i]["VAT"] = dsVATMasterConfig.Tables[4].Rows[j]["VATPerc"];

                                            DTTemp.Rows[i]["CVAT"] = 0.0;
                                            DTTemp.Rows[i]["PVAT"] = (Convert.ToDecimal(DTTemp.Rows[i]["VAT"])
                                                                    * Convert.ToDecimal(DTTemp.Rows[i]["BillablePrice"])) / 100;
                                            DTTemp.Rows[i]["isSaudi"] = dsVATMasterConfig.Tables[4].Rows[j]["isSaudi"] == DBNull.Value ? false : dsVATMasterConfig.Tables[4].Rows[j]["isSaudi"];
                                            VATFlag = true;
                                            break;
                                        }
                                        else
                                        {
                                            VATFlag = false;
                                        }
                                    }
                                }
                                else
                                {
                                    VATFlag = false;
                                }
                            }
                            if (!VATFlag)
                            {
                                int specialiseId = Convert.ToInt32(DTTemp.Rows[i]["SpecialiseId"]);
                                for (int j = 0; j < dsVATMasterConfig.Tables[3].Rows.Count; j++)
                                {
                                    if (specialiseId == Convert.ToInt32(dsVATMasterConfig.Tables[3].Rows[j]["SpecialiseID"]) && serviceId == Convert.ToInt32(dsVATMasterConfig.Tables[3].Rows[j]["ServiceID"]))
                                    {
                                        if (dsVATMasterConfig.Tables[3].Rows[j]["EffectiveDate"] != DBNull.Value)
                                        {
                                            if (Convert.ToDateTime(dsVATMasterConfig.Tables[3].Rows[j]["EffectiveDate"]) <= DateTime.Now)
                                            {
                                                DTTemp.Rows[i]["VAT"] = dsVATMasterConfig.Tables[3].Rows[j]["VATPerc"];

                                                DTTemp.Rows[i]["CVAT"] = 0.0;
                                                DTTemp.Rows[i]["PVAT"] = (Convert.ToDecimal(DTTemp.Rows[i]["VAT"])
                                                                        * Convert.ToDecimal(DTTemp.Rows[i]["BillablePrice"])) / 100;
                                                DTTemp.Rows[i]["isSaudi"] = dsVATMasterConfig.Tables[3].Rows[j]["isSaudi"] == DBNull.Value ? false : dsVATMasterConfig.Tables[3].Rows[j]["isSaudi"];
                                                VATFlag = true;
                                                break;
                                            }
                                            else
                                            {
                                                VATFlag = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        VATFlag = false;
                                    }
                                }
                            }
                            if (!VATFlag)
                            {
                                int departmentId = Convert.ToInt32(DTTemp.Rows[i]["DeptId"]);
                                for (int j = 0; j < dsVATMasterConfig.Tables[2].Rows.Count; j++)
                                {
                                    if (departmentId == Convert.ToInt32(dsVATMasterConfig.Tables[2].Rows[j]["DepartmentID"]) && serviceId == Convert.ToInt32(dsVATMasterConfig.Tables[2].Rows[j]["ServiceID"]))
                                    {
                                        if (dsVATMasterConfig.Tables[2].Rows[j]["EffectiveDate"] != DBNull.Value)
                                        {
                                            if (Convert.ToDateTime(dsVATMasterConfig.Tables[2].Rows[j]["EffectiveDate"]) <= DateTime.Now)
                                            {
                                                DTTemp.Rows[i]["VAT"] = dsVATMasterConfig.Tables[2].Rows[j]["VATPerc"];

                                                DTTemp.Rows[i]["CVAT"] = 0.0;
                                                DTTemp.Rows[i]["PVAT"] = (Convert.ToDecimal(DTTemp.Rows[i]["VAT"])
                                                                    * Convert.ToDecimal(DTTemp.Rows[i]["BillablePrice"])) / 100;
                                                DTTemp.Rows[i]["isSaudi"] = dsVATMasterConfig.Tables[2].Rows[j]["isSaudi"] == DBNull.Value ? false : dsVATMasterConfig.Tables[2].Rows[j]["isSaudi"];
                                                VATFlag = true;
                                                break;
                                            }
                                            else
                                            {
                                                VATFlag = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        VATFlag = false;
                                    }
                                }
                            }
                            if (!VATFlag)
                            {
                                int ServiceId = Convert.ToInt32(DTTemp.Rows[i]["ServiceId"]);
                                for (int j = 0; j < dsVATMasterConfig.Tables[1].Rows.Count; j++)
                                {
                                    if (ServiceId == Convert.ToInt32(dsVATMasterConfig.Tables[1].Rows[j]["ServiceID"]))
                                    {
                                        if (dsVATMasterConfig.Tables[1].Rows[j]["EffectiveDate"] != DBNull.Value)
                                        {
                                            if (Convert.ToDateTime(dsVATMasterConfig.Tables[1].Rows[j]["EffectiveDate"]) <= DateTime.Now)
                                            {


                                                DTTemp.Rows[i]["VAT"] = dsVATMasterConfig.Tables[1].Rows[j]["VATPerc"];

                                                DTTemp.Rows[i]["CVAT"] = 0.0;
                                                DTTemp.Rows[i]["PVAT"] = (Convert.ToDecimal(DTTemp.Rows[i]["VAT"])
                                                                            * Convert.ToDecimal(DTTemp.Rows[i]["BillablePrice"])) / 100;
                                                DTTemp.Rows[i]["isSaudi"] = dsVATMasterConfig.Tables[1].Rows[j]["isSaudi"] == DBNull.Value ? false : dsVATMasterConfig.Tables[1].Rows[j]["isSaudi"];
                                                VATFlag = true;
                                                break;
                                            }
                                            else
                                            {
                                                VATFlag = false;
                                            }
                                        }
                                    }
                                }
                            }
                            if (!VATFlag)
                            {
                                for (int j = 0; j < dsVATMasterConfig.Tables[0].Rows.Count; j++)
                                {
                                    if (dsVATMasterConfig.Tables[0].Rows[j]["EffectiveDate"] != DBNull.Value)
                                    {
                                        if (Convert.ToDateTime(dsVATMasterConfig.Tables[0].Rows[j]["EffectiveDate"]) <= DateTime.Now)
                                        {
                                            DTTemp.Rows[i]["VAT"] = dsVATMasterConfig.Tables[0].Rows[j]["VATPerc"];

                                            DTTemp.Rows[i]["CVAT"] = 0.0;
                                            DTTemp.Rows[i]["PVAT"] = (Convert.ToDecimal(DTTemp.Rows[i]["VAT"])
                                                            * Convert.ToDecimal(DTTemp.Rows[i]["BillablePrice"])) / 100;
                                            DTTemp.Rows[i]["isSaudi"] = dsVATMasterConfig.Tables[0].Rows[j]["isSaudi"] == DBNull.Value ? false : dsVATMasterConfig.Tables[0].Rows[j]["isSaudi"];
                                            VATFlag = true;
                                            break;
                                        }
                                        else
                                        {
                                            VATFlag = false;
                                            DTTemp.Rows[i]["VAT"] = Convert.ToDecimal(0).ToString(hdnsCurrencyFormat);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        if (SceduleDoctorAppointment != null)
                        {
                            DataRow[] drScheduleConsult = DTTemp.Select("ServiceId =2");
                            if (drScheduleConsult.Length > 0 && Convert.ToString(drScheduleConsult[0]["ScheduleId"]) == string.Empty)
                            {
                                DataTable dtScheduleOrderForCons = SceduleDoctorAppointment.Copy();
                                dtScheduleOrderForCons.AcceptChanges();
                                if (Convert.ToDateTime(dtScheduleOrderForCons.Rows[0]["StartDate"]).ToString("dd-MMM-yyyy") == DateTime.Now.ToString("dd-MMM-yyyy"))
                                {
                                    string[] presentTime = DateTime.Now.ToString("HH:mm").Split(':');
                                    int presentMinutes = Convert.ToInt32(presentTime[0]) * 60 + Convert.ToInt32(presentTime[1]);
                                    int fromTime = Convert.ToInt32(dtScheduleOrderForCons.Rows[0]["StartTime"]);
                                    int endTime = Convert.ToInt32(dtScheduleOrderForCons.Rows[0]["EndTime"]);

                                    if (presentMinutes >= (fromTime - 60) && presentMinutes <= (endTime + 60))
                                    {
                                        if (Convert.ToString(drScheduleConsult[0]["SpecialiseId"]) == Convert.ToString(dtScheduleOrderForCons.Rows[0]["SpecialiseId"]) &&
                                            Convert.ToString(drScheduleConsult[0]["ProcedureId"]) == Convert.ToString(dtScheduleOrderForCons.Rows[0]["ProcedureID"]) &&
                                            Convert.ToString(drScheduleConsult[0]["DeptId"]) == Convert.ToString(dtScheduleOrderForCons.Rows[0]["DeptId"]) &&
                                            Convert.ToString(drScheduleConsult[0]["ServiceId"]) == Convert.ToString(dtScheduleOrderForCons.Rows[0]["ServiceId"]))
                                        {
                                            drScheduleConsult[0]["ScheduleId"] = dtScheduleOrderForCons.Rows[0]["ScheduleId"];
                                        }
                                    }
                                }
                            }
                        }
                        DsBillDetails.Tables.Add(DTTemp);
                        DsBillDetails.Tables.Add(dtTestProfile);
                        DsBillDetails.Tables.Add(dtOPPackageOrders);


                        if (dtTempOthersTable != null && dtTempOthersTable.Rows.Count > 0)
                        {
                            DsBillDetails.Tables.Add(dtTempOthersTable);
                        }
                        else
                            DsBillDetails.Tables.Add(dtOrders);

                        DataTable dtScheduleDT = null;
                        if (ScheduleDT != null)
                        {
                            dtScheduleDT = ScheduleDT.Copy();
                            dtScheduleDT.AcceptChanges();
                        }
                        if (dtScheduleDT != null)
                        {
                            ScheduleDT = dtScheduleDT;
                            DataRow[] dtr = ScheduleDT.Select();
                            foreach (DataRow dr in dtr)
                            {
                                if (dr["schedDate"].ToString() == "")
                                {
                                    ScheduleDT.Rows.Remove(dr);
                                }
                            }
                            ScheduleDT.AcceptChanges();
                            DsBillDetails.Tables.Add(ScheduleDT.Copy());

                        }

                        DataTable dtTableSchedule = new DataTable();
                        dtTableSchedule.TableName = "ScheduleDT";
                        dtTableSchedule.Columns.Add("SlNo", typeof(int));
                        dtTableSchedule.Columns.Add("schedDate", typeof(DateTime));
                        dtTableSchedule.Columns.Add("ProcedureID", typeof(int));
                        dtTableSchedule.Columns.Add("ServiceName", typeof(string));
                        dtTableSchedule.Columns.Add("ServiceId", typeof(int));
                        dtTableSchedule.Columns.Add("Procedure", typeof(string));
                        dtTableSchedule.Columns.Add("ProcedureId", typeof(int));
                        dtTableSchedule.Columns.Add("Sample", typeof(string));
                        dtTableSchedule.Columns.Add("SampleId", typeof(int));
                        dtTableSchedule.Columns.Add("DeptId", typeof(int));
                        dtTableSchedule.Columns.Add("BedtypeId", typeof(int));
                        dtTableSchedule.Columns.Add("BedTypeName", typeof(string));
                        dtTableSchedule.Columns.Add("SpecialiseId", typeof(int));

                        if (dsTempOtherOrders != null)
                        {
                            if (dsTempOtherOrders.Tables[1].Rows.Count > 0)
                            {
                                foreach (DataRow dr in dsTempOtherOrders.Tables[1].Select())
                                {
                                    if (dr["ServiceID"].ToString() == "5" || dr["ServiceID"].ToString() == "3")
                                    {
                                        DataRow dr1 = dtTableSchedule.NewRow();
                                        dr1["SlNo"] = dtTableSchedule.Rows.Count + 1;
                                        dr1["schedDate"] = dr["ScheduleDate"].ToString();
                                        dr1["ProcedureID"] = dr["ProcedureId"].ToString();
                                        dr1["ServiceName"] = dr["ServiceName"].ToString();
                                        dr1["ServiceId"] = dr["ServiceId"].ToString();
                                        dr1["Procedure"] = dr["ProcedureName"].ToString();
                                        dr1["ProcedureId"] = dr["ProcedureId"].ToString();
                                        dr1["Sample"] = dr["SampleName"].ToString();
                                        dr1["SampleId"] = dr["SampleId"].ToString();
                                        dr1["DeptId"] = dr["DeptId"].ToString();
                                        dr1["BedtypeId"] = 0;
                                        dr1["BedTypeName"] = "";
                                        dr1["SpecialiseId"] = dr["SpecialiseId"].ToString();
                                        dtTableSchedule.Rows.Add(dr1);
                                    }
                                }
                                DsBillDetails.Tables.Add(dtTableSchedule.Copy());
                            }
                        }

                        DsBillDetails.AcceptChanges();
                        dtSelectedBillDetails = DTTemp.Copy();
                        dtSelectedBillDetails.AcceptChanges();

                        #region COLLECTABLE PAGE CALLING

                        //if (hdnIsCardCollectable == "1" && hdnCollectableType != "0")
                        //{
                        //    int intCalledPage = 0;
                        //    if (CallingCardColl > 0)
                        //        intCalledPage = CallingCardColl;

                        //    if (intCalledPage == 0)
                        //    {                               
                        //        CallingCardColl = 1;
                        //        dsDeductables = GetCompanyDeductables(Convert.ToInt32(strCompanyID), "C", "9,10,11,12,13", Convert.ToInt32(strGradeID), Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId));
                        //        BedTypes = dsDeductables.Tables["BedTypes"];                              
                        //        docSpecId = hdnDocSpecialiseId;                                
                        //    }
                        //}
                        #endregion COLLECTABLE PAGE CALLING


                        #region DEDUCTIBILES CONFIGURATION TABLES PREPARATION
                        DataSet dsOutDeductable = null;
                        if (DsOutputDeductable != null)
                        {
                            dsOutDeductable = DsOutputDeductable.Copy();
                            dsOutDeductable.AcceptChanges();
                        }
                        if (dsOutDeductable != null)
                        {
                            DsOutputDeductable = dsOutDeductable;
                            dtDeductablesTable = DsOutputDeductable.Tables[0].Copy();

                            dtDeductablesTable1 = DsOutputDeductable.Tables[1].Copy();
                            dtDeductablesTable2 = DsOutputDeductable.Tables[2].Copy();
                            dtDeductablesTable3 = DsOutputDeductable.Tables[3].Copy();
                            dtDeductablesTable4 = DsOutputDeductable.Tables[4].Copy();
                            dtBedTypes = BedTypes.Copy();
                            dtDeductablesTable.TableName = "deducTable"; dtDeductablesTable1.TableName = "deducTable1"; dtDeductablesTable2.TableName = "deducTable2"; dtDeductablesTable3.TableName = "deducTable3"; dtDeductablesTable4.TableName = "deducTable4"; dtBedTypes.TableName = "BedTypes";
                            dtDeductablesTable.AcceptChanges(); dtDeductablesTable1.AcceptChanges(); dtDeductablesTable2.AcceptChanges(); dtDeductablesTable3.AcceptChanges(); dtDeductablesTable4.AcceptChanges(); dtBedTypes.AcceptChanges();

                            if (DsOutputDeductable != null & DsOutputDeductable.Tables.Count > 0)
                            {
                                DsBillDetails.Tables.Add(dtDeductablesTable); DsBillDetails.Tables.Add(dtDeductablesTable1); DsBillDetails.Tables.Add(dtDeductablesTable2); DsBillDetails.Tables.Add(dtDeductablesTable3); DsBillDetails.Tables.Add(dtDeductablesTable4); DsBillDetails.Tables.Add(dtBedTypes);
                            }
                        }
                        #endregion DEDUCTIBILES CONFIGURATION TABLES PREPARATION

                        #region Diagnosis Validation CR -92009                       
                        if (hdnrblbilltypeCredit == true && (DTTemp.Select("Serviceid=2").Length == 0 || hdnPatientType == "3"))
                        {
                            if (!string.IsNullOrEmpty(ViewStateLetterid.Trim()) && !string.IsNullOrEmpty(VSParentLetterid.Trim()) && (ViewStateLetterid.ToString() == VSParentLetterid.ToString()))
                            {
                                int IPID = 0;
                                if (OtherOrders != null)
                                    dsTempOtherOrders = OtherOrders.Copy();
                                else if (ServiceOrders != null)
                                    dsTempOtherOrders = ServiceOrders.Copy();
                                dsTempOtherOrders.AcceptChanges();
                                if (dsTempOtherOrders != null && dsTempOtherOrders.Tables.Count > 0 && dsTempOtherOrders.Tables[0].Rows.Count > 0)
                                {
                                    dtTempOthersTable = dsTempOtherOrders.Tables["DoctOrders"].Copy();
                                    IPID = Convert.ToInt32(dtTempOthersTable.Rows[0]["IPID"].ToString());
                                }
                                if (IPID == 0 & !string.IsNullOrEmpty(LatestIPID.Trim()))
                                    IPID = Convert.ToInt32(LatestIPID);

                                if (IPID == 0 && hdnPatientType == "3")
                                    IPID = Convert.ToInt32(hdnIPID.ToString());

                                //Fetching Patient file Common for Agreement and Company Level
                                DataSet dsPatientDiagnosis = NewFetchPatientFile(0, IPID, 0, "9,24,32", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId));
                                //Fetching Company Level Diagnosis                      
                                DataSet dsCompanyDiagnosis = GetCompanyDetails(Convert.ToInt32(strCompanyID), null, "C", "18,19,20", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, null, HospitalID);

                                if (dsCompanyDiagnosis != null && dsCompanyDiagnosis.Tables[1].Rows.Count > 0)
                                {
                                    #region CompanyLevel Diagnosis
                                    if (dsPatientDiagnosis != null && dsPatientDiagnosis.Tables[0].Rows.Count > 0)
                                    {
                                        StringBuilder strPopUP = new StringBuilder();
                                        string strTemp = string.Empty;
                                        DataTable dtPatientDaignosis = dsPatientDiagnosis.Tables[0].Copy();
                                        DataView view = new DataView(dtPatientDaignosis);
                                        DataTable distinctValues = view.ToTable(true, "DiseaseID", "DiseaseName");

                                        foreach (DataRow dr1 in distinctValues.Rows)
                                        {
                                            DataRow[] dr1_ = dsCompanyDiagnosis.Tables[1].Select("DiseaseID='" + dr1["DiseaseID"].ToString().Trim() + "' and GradeID='" + Convert.ToInt32(strGradeID) + "' and GradeSpecialiseID='" + Convert.ToInt32(hdnDocSpecialiseId) + "'");
                                            if (dr1_.Length == 0)
                                                dr1_ = dsCompanyDiagnosis.Tables[1].Select("DiseaseID='" + dr1["DiseaseID"].ToString().Trim() + "' and GradeID='" + Convert.ToInt32(strGradeID) + "' and GradeSpecialiseID=-1");
                                            if (dr1_.Length > 0)
                                            {
                                                if (dtAR.Rows.Count > 0)
                                                {
                                                    strPopUP = IsApprovalAvailable(DTTemp, dtAR);
                                                }
                                                else
                                                {
                                                    strPopUP = strPopUP.Append(dr1["DiseaseName"].ToString());
                                                    strPopUP = strPopUP.Append("<BR/>");
                                                }
                                            }
                                            if (strPopUP.Length > 0 && hdnPatientType != "3")
                                            {
                                                objPatientList.Code = (int)ProcessStatus.Success;
                                                objPatientList.Status = ProcessStatus.Success.ToString();
                                                objPatientList.Message = "Diagnosis Excluded...Approval required for:<br><br>" + strPopUP.ToString();
                                                return objPatientList;
                                            }
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        #region Agreement Level Diagnosis
                                        DataSet dsAgreement = FetchTariffDetails(Convert.ToInt32(hdnTariffID), Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, Convert.ToInt32(strGradeID));
                                        if (dsAgreement != null && dsAgreement.Tables[7].Rows.Count > 0)
                                        {
                                            DataTable dtDiagnosisAgreement = dsAgreement.Tables[7].Copy();
                                            if (dsPatientDiagnosis != null && dsPatientDiagnosis.Tables[0].Rows.Count > 0)
                                            {
                                                StringBuilder strPopUP = new StringBuilder();
                                                string strTemp = string.Empty;
                                                DataTable dtPatientDaignosis = dsPatientDiagnosis.Tables[0].Copy();
                                                DataView view = new DataView(dtPatientDaignosis);
                                                DataTable distinctValues = view.ToTable(true, "DiseaseID", "DiseaseName");

                                                foreach (DataRow dr1 in distinctValues.Rows)
                                                {
                                                    foreach (DataRow dr in dtDiagnosisAgreement.Rows)
                                                    {
                                                        if (dr["DiseaseID"].ToString() == dr1["DiseaseID"].ToString())
                                                        {
                                                            strTemp = dr1["DiseaseName"].ToString();
                                                            strPopUP.Append(strTemp.Clone());
                                                            strPopUP.Append("<BR/>");
                                                        }
                                                    }
                                                }
                                                if (!string.IsNullOrEmpty(strPopUP.ToString()) && hdnPatientType != "3")
                                                {
                                                    objPatientList.Code = (int)ProcessStatus.Success;
                                                    objPatientList.Status = ProcessStatus.Success.ToString();
                                                    objPatientList.Message = "Diagnosis Excluded...Approval required for:<br><br>" + strPopUP.ToString();
                                                    return objPatientList;
                                                }
                                            }

                                        }
                                        #endregion
                                    }

                                    //For Agreement and Company Level Clinical Conditions    

                                    if (dsCompanyDiagnosis != null && dsCompanyDiagnosis.Tables[2].Rows.Count > 0)
                                    {
                                        #region Company Level Clinical Conditions
                                        if (dsPatientDiagnosis != null && dsPatientDiagnosis.Tables[1].Rows.Count > 0)
                                        {
                                            StringBuilder strPopUP = new StringBuilder();
                                            string strTemp = string.Empty;
                                            DataTable dtPatientDaignosis = dsPatientDiagnosis.Tables[1].Copy();
                                            DataView view = new DataView(dtPatientDaignosis);
                                            DataTable distinctValues = view.ToTable(true, "ClinicalCondtionid", "ClinicalCondtion");

                                            foreach (DataRow dr1 in distinctValues.Rows)
                                            {
                                                DataRow[] dr1_ = dsCompanyDiagnosis.Tables[2].Select("CaseSheetName='" + dr1["ClinicalCondtion"].ToString().Trim() + "' and LimitType=-1 and GradeID='" + Convert.ToInt32(strGradeID) + "' and GradeSpecialiseID='" + Convert.ToInt32(hdnDocSpecialiseId) + "'");
                                                DataRow[] dr2_ = dsCompanyDiagnosis.Tables[2].Select("CaseSheetName='" + dr1["ClinicalCondtion"].ToString().Trim() + "'  and GradeID='" + Convert.ToInt32(strGradeID) + "' and GradeSpecialiseID='" + Convert.ToInt32(hdnDocSpecialiseId) + "'");

                                                if (dr1_.Length == 0)
                                                    dr1_ = dsCompanyDiagnosis.Tables[2].Select("CaseSheetName='" + dr1["ClinicalCondtion"].ToString().Trim() + "' and LimitType=-1 and GradeID='" + Convert.ToInt32(strGradeID) + "' and GradeSpecialiseID=-1");
                                                if (dr2_.Length == 0)
                                                    dr2_ = dsCompanyDiagnosis.Tables[2].Select("CaseSheetName='" + dr1["ClinicalCondtion"].ToString().Trim() + "'  and GradeID='" + Convert.ToInt32(strGradeID) + "' and GradeSpecialiseID=-1");
                                                if (dr1_.Length > 0 || dr2_.Length == 0)
                                                {
                                                    if (dtAR.Rows.Count > 0)
                                                    {
                                                        strPopUP = IsApprovalAvailable(DTTemp, dtAR);
                                                    }
                                                    else
                                                        strPopUP = strPopUP.Append(dr1["ClinicalCondtion"].ToString() + "<BR/>");
                                                }
                                            }

                                            if (strPopUP.Length > 0 && hdnPatientType != "3")
                                            {
                                                hdnConsultationMsg = "MSG";
                                                if (!string.IsNullOrEmpty(hdnDelConfirm))
                                                {
                                                    hdnDelConfirm = null;
                                                }
                                                objPatientList.Code = (int)ProcessStatus.Success;
                                                objPatientList.Status = ProcessStatus.Success.ToString();
                                                objPatientList.Message = "Selected Clinical Conditions are Excluded...Approval required for:<br>" + strPopUP.ToString();
                                                return objPatientList;

                                            }
                                        }
                                    }
                                    #endregion
                                    else
                                    {
                                        #region Agreement Level Clinical Conditions
                                        DataSet dsAgreement = FetchTariffDetails(Convert.ToInt32(hdnTariffID), Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, Convert.ToInt32(strGradeID));
                                        if (dsAgreement != null && dsAgreement.Tables[6].Rows.Count > 0)
                                        {
                                            DataTable dtDiagnosisAgreement = dsAgreement.Tables[6].Copy();


                                            if (dsPatientDiagnosis != null && dsPatientDiagnosis.Tables[1].Rows.Count > 0)
                                            {
                                                StringBuilder strPopUP = new StringBuilder();
                                                string strTemp = string.Empty;
                                                DataTable dtPatientDaignosis = dsPatientDiagnosis.Tables[1].Copy();
                                                DataView view = new DataView(dtPatientDaignosis);
                                                DataTable distinctValues = view.ToTable(true, "ClinicalCondtionid", "ClinicalCondtion");

                                                foreach (DataRow dr1 in distinctValues.Rows)
                                                {
                                                    foreach (DataRow dr in dsCompanyDiagnosis.Tables[2].Rows)
                                                    {
                                                        //foreach (DataRow dr1 in dtPatientDaignosis.Rows)
                                                        DataRow[] dr1_ = dsCompanyDiagnosis.Tables[2].Select("CaseSheetName='" + dr1["ClinicalCondtion"].ToString().Trim() + "'");
                                                        if (dr1_.Length == 0)
                                                        {
                                                            strTemp = dr1["ClinicalCondtion"].ToString();
                                                            strPopUP.Append(strTemp.Clone());
                                                            strPopUP.Append("<BR/>");
                                                        }
                                                    }
                                                }

                                                if (!string.IsNullOrEmpty(strPopUP.ToString()) && hdnPatientType != "3")
                                                {
                                                    objPatientList.Code = (int)ProcessStatus.Success;
                                                    objPatientList.Status = ProcessStatus.Success.ToString();
                                                    objPatientList.Message = "Selected Clinical Conditions are Excluded...Approval required for:<br>" + strPopUP.ToString();
                                                    return objPatientList;
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                        }
                        #endregion Diagnosis Validation CR -92009

                        if (hdnPatientType == "3" && hdnrblbilltypeCredit == true && dtAR.Rows.Count > 0)
                        {
                            dtAR.TableName = "LoaApprovalEntryDetails";
                            DsBillDetails.Tables.Add(dtAR);
                        }
                        DataTable dtItemSplitDetails = new DataTable();
                        BDPopUP = DsBillDetails.Tables["BillDetails"];
                        BDPopUP.AcceptChanges();
                        if (DsBillDetails.Tables["PatientDetails"].Rows.Count == 1 && Convert.ToInt32(DsBillDetails.Tables["PatientDetails"].Rows[0]["BillType"]) == 2 && Convert.ToInt32(DsBillDetails.Tables["PatientDetails"].Rows[0]["PatientType"]) == 1)
                        {
                            if (DsBillDetails.Tables["BillDetails"].Select("ServiceID=2").Length == 1 && DsBillDetails.Tables["BillDetails"].Select("ServiceID=5").Length > 0)
                            {
                                decERExtraLoaLimit = Convert.ToDecimal(DsBillDetails.Tables["BillDetails"].Compute("SUM(BillablePrice)", "ServiceID=5"));
                            }
                            else
                                decERExtraLoaLimit = 0;
                        }
                        DsOutPutN = SetBillDetails(DsBillDetails, Convert.ToInt32(strDefWorkstationId));

                        if (strCompanyID.Length > 1)
                        {
                            DsOutPut.Tables.Add(DsOutPutN.Tables["CreditBillContributions"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["DtCompCRContribution"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["DiscountData"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["BillBreakUp"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["Deductibles"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["BillDetails"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["ItemSplitDetails"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["CreditBillItems"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["CreditBillPackageItems"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["BillSummary"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["LTable"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["LTable1"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["LTable2"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["LTable3"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["LTable4"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["Table"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["Table1"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["Table2"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["Table3"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["Table4"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["BedTypes"].Copy());
                            DsOutPut.AcceptChanges();
                        }
                        else
                        {
                            DsOutPut.Tables.Add(DsOutPutN.Tables["CashBillContributions"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["DtCompCSContribution"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["DiscountData"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["BillBreakUp"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["BillDetails"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["ItemSplitDetails"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["CashBillItems"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["CashBillPackageItems"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["BillSummary"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["Table1"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["LTable"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["LTable1"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["LTable2"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["LTable3"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["LTable4"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["Table"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["Table2"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["Table3"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["Table4"].Copy());
                            DsOutPut.Tables.Add(DsOutPutN.Tables["BedTypes"].Copy());
                            DsOutPut.AcceptChanges();

                        }



                        DataSet DsSplitCollectables = new DataSet();
                        if (DsOutPut != null && DsOutPut.Tables.Count > 0 && DsOutPut.Tables.Contains("ItemSplitDetails"))
                            dtItemSplitDetails = DsOutPut.Tables["ItemSplitDetails"].Copy();
                        string Strvalue = string.Empty;
                        string Strclinical = string.Empty;
                        DsOutputDeductable = null;
                        CallingCardColl = 0;
                        foreach (DataRow dr1 in DsOutPut.Tables[0].Select("Typ=0"))
                        {
                            dr1["VAT"] = DsOutPut.Tables[0].Compute("SUM(VAT)", "Typ=5");
                            dr1["CVAT"] = DsOutPut.Tables[0].Compute("SUM(CVAT)", "Typ=5");
                            dr1["PVAT"] = DsOutPut.Tables[0].Compute("SUM(PVAT)", "Typ=5");
                        }

                        DsOutPut.Tables[0].AcceptChanges();

                        if (DsOutPut.Tables.Count > 7)
                            hdnOutputBillTpe = "0";//BOTH
                        else if (DsOutPut.Tables.Count == 6)
                        {
                            if (DsOutPut.Tables.Contains("CashBillContributions"))
                                hdnOutputBillTpe = "1";//CASH
                            else if (DsOutPut.Tables.Contains("CreditBillContributions"))
                                hdnOutputBillTpe = "2";//CREDIT
                        }

                        #region  FOR DISCOUNT CALCULATION FOR COLLECTABLES

                        decimal intCPAY = 0; decimal intPAY = 0; decimal intSPAY = 0;

                        if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                        {
                            if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                            {
                                intCPAY = Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Rows[0]["CPAY"]);
                                intPAY = Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Rows[0]["PPAY"]);
                                intSPAY = Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Rows[0]["SPAY"]);
                            }
                            else
                            {
                                intCPAY = Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Rows[0]["CPAY"]);
                                intPAY = Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Rows[0]["PPAY"]);
                                intSPAY = Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Rows[0]["SPAY"]);
                            }
                        }
                        else
                        {
                            intCPAY = Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Rows[0]["CPAY"]);
                            intPAY = Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Rows[0]["PPAY"]);
                            intSPAY = Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Rows[0]["SPAY"]);
                        }
                        decimal Amount = 0;
                        Decimal intBillAmount = 0;
                        intBillAmount = Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Rows[0]["Amount"].ToString());
                        string BilType = string.Empty;
                        if (intCPAY != 0)
                        {
                            #region Assigning Bill Amount

                            if (CollectableType == 1)
                            {
                                #region Limit exceeds billing
                                Amount = intCPAY + Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Rows[0]["DPAY"].ToString());
                                #endregion
                            }
                            else if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                            {
                                if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                {
                                    Amount = intCPAY + intPAY;
                                }
                                else { Amount = intCPAY; }
                            }
                            else { Amount = intCPAY; }

                            #endregion
                        }
                        else if (intSPAY != 0)
                            Amount = intSPAY;
                        else
                            Amount = intPAY;

                        if (hdnrblbilltypecash == true)
                            BilType = "CS";
                        else
                            BilType = "CR";

                        #region ASSINING FACADE RETURN TABLES

                        if (DsOutPut.Tables.Contains("DtCompCSContribution"))
                            DtCompCSContribution = DsOutPut.Tables["DtCompCSContribution"].Copy();

                        if (DsOutPut.Tables.Contains("DtCompCRContribution"))
                            DtCompCRContribution = DsOutPut.Tables["DtCompCRContribution"].Copy();

                        dtSummary = DsOutPut.Tables["BillSummary"];
                        dtCompanyCashContribution = null;
                        if (DsOutPut.Tables.Contains("CashBillContributions"))
                        {
                            dtCompanyCashContribution = DsOutPut.Tables["CashBillContributions"];
                            dtCompanyCashContribution = DsOutPut.Tables["CashBillContributions"].Copy();
                        }
                        if (DsOutPut.Tables.Contains("CreditBillContributions"))
                        {
                            dtCRContribution = DsOutPut.Tables["CreditBillContributions"];
                            dtCompanyCashContribution = DsOutPut.Tables["CreditBillContributions"].Copy();
                        }

                        dtCashDiscounts = DsOutPut.Tables["DiscountData"];
                        if (DsOutPut.Tables.Contains("CreditBillPackageItems"))
                            dtPackageItems = DsOutPut.Tables["CreditBillPackageItems"];
                        if (DsOutPut.Tables.Contains("CashBillPackageItems"))
                            dtPackageItems = DsOutPut.Tables["CashBillPackageItems"];
                        #endregion ASSINING FACADE RETURN TABLES                        

                        if (hdnrblbilltypeCredit == true)
                            DtDiscountDetails = GetAvailDiscount(Amount, BilType);
                        else if (CRDiscountOnCash != null & Convert.ToBoolean(CRDiscountOnCash) == true)
                        {
                            DtDiscountDetails = GetAvailDiscount(Amount, BilType);
                        }
                        if (DtDiscountDetails != null)
                            DtDiscountDetails = DtDiscountDetails.Copy(); DtDiscountDetails.AcceptChanges();
                        #endregion FOR DISCOUNT CALCULATION FOR COLLECTABLES
                        CalculateVATForCredit(DsOutPut.Tables[0]);
                        #region BASED ON COLLECTABLES

                        decimal decTotalVATAmount = 0;

                        #region REASSINGING TABLE FROM FACADE

                        if (DsOutPut.Tables.Contains("CashBillItems"))
                        {
                            DtCashBillItems = DsOutPut.Tables["CashBillItems"].Copy();
                            BillDetails = DtCashBillItems.Copy();
                            BillDetails.AcceptChanges();
                        }
                        if (DsOutPut.Tables.Contains("CreditBillItems"))
                        {
                            DtCreditBillItems = DsOutPut.Tables["CreditBillItems"].Copy();
                            BillDetails = DtCreditBillItems.Copy();
                            BillDetails.AcceptChanges();
                        }
                        #endregion REASSINGING TABLE FROM FACADE

                        #region PATIENTDATA TO UPDATE DEFAULTLOA CONTRIBUTIONS
                        DataTable dtPatientDetails = DsBillDetails.Tables["PatientDetails"].Copy();
                        string strSessionId = Convert.ToString(dtPatientDetails.Rows[0]["SessionId"]);
                        string strAdmissionUHID = Convert.ToString(dtPatientDetails.Rows[0]["UHID"]);
                        int PatientType = Convert.ToInt32(dtPatientDetails.Rows[0]["PatientType"]);
                        int intBillType = Convert.ToInt32(dtPatientDetails.Rows[0]["BillType"]);
                        int intPatienId = Convert.ToInt32(dtPatientDetails.Rows[0]["PatientId"]);
                        #endregion PATIENTDATA TO UPDATE DEFAULTLOA CONTRIBUTIONS


                        #region SPLITING COLLECTABLEAGAIN AFTERDISCOUNT
                        dsDeductables_ColSplit = new DataSet();
                        dsDeductables_ColSplit.Tables.Add(DsOutPut.Tables["Table"].Copy());
                        dsDeductables_ColSplit.Tables.Add(DsOutPut.Tables["Table1"].Copy());
                        dsDeductables_ColSplit.Tables.Add(DsOutPut.Tables["Table2"].Copy());
                        dsDeductables_ColSplit.Tables.Add(DsOutPut.Tables["Table3"].Copy());
                        dsDeductables_ColSplit.Tables.Add(DsOutPut.Tables["Table4"].Copy());
                        dsDeductables_ColSplit.Tables.Add(DsOutPut.Tables["BedTypes"].Copy());

                        #endregion SPLITING COLLECTABLEAGAIN AFTERDISCOUNT


                        if (CollectableType == 1 || CollectableType == 3)
                        {
                            //CashContributions
                            if (DtCompCSContribution != null && DtCompCSContribution.Rows.Count > 0)//if (DtCompanyCash.Rows.Count>0)
                            {
                                #region Fetching BillContribution DataTable

                                dtCompanyCashContribution = GetCashContributions(DtCompCSContribution.Copy(), Convert.ToInt32(BillType), DsOutPut.Tables["BillSummary"].Copy(), DtDiscountDetails, DtCashBillItems);
                                dtCompanyCashContribution.AcceptChanges();
                                dtCompanyCashContribution = dtCompanyCashContribution.Copy();
                                dtCompanyCashContribution.AcceptChanges();

                                #region Newly Added on 26-08-2011 for dual bill
                                if (BillType == "2")
                                {
                                    if (!dtCompanyCashContribution.Columns.Contains("BillTypeID"))
                                        dtCompanyCashContribution.Columns.Add("BillTypeID", typeof(int));
                                    if (!dtCompanyCashContribution.Columns.Contains("BillType"))
                                        dtCompanyCashContribution.Columns.Add("BillType", typeof(string));


                                    for (int i = 0; i < dtCompanyCashContribution.Rows.Count; i++)
                                    {
                                        dtCompanyCashContribution.Rows[i]["BillTypeID"] = 2;
                                        dtCompanyCashContribution.Rows[i]["BillType"] = "SP";

                                    }
                                }
                                #endregion Newly Added on 26-08-2011 for dual bill

                                if (!string.IsNullOrEmpty(hdnPatientType.Trim()) && hdnPatientType.Trim() != "null")
                                    intPatientType = Convert.ToInt32(hdnPatientType);
                                else
                                    intPatientType = 1;
                                int intResult = SaveTempDefaultLOAContribution(dtCompanyCashContribution, strSessionId, strAdmissionUHID, intPatientType, 1, intPatienId);
                                #endregion
                                foreach (DataRow dr1 in dtCompanyCashContribution.Select("Typ=0"))
                                {
                                    dr1["VAT"] = dtCompanyCashContribution.Compute("SUM(VAT)", "Typ=5");
                                    dr1["CVAT"] = dtCompanyCashContribution.Compute("SUM(CVAT)", "Typ=5");
                                    dr1["PVAT"] = dtCompanyCashContribution.Compute("SUM(PVAT)", "Typ=5");
                                }
                                CalculateVATForCredit(dtCompanyCashContribution);

                                hdnPVATValueforSP = hdnPVATValue;
                                decTotalVATAmount = decTotalVATAmount + Convert.ToDecimal(hdnVATAmount);
                                hdnVATAmount = decTotalVATAmount.ToString(hdnsCurrencyFormat);
                            }

                            if (DtCompCRContribution != null && DtCompCRContribution.Rows.Count > 0)//if (DtCompanyCredit.Rows.Count>0)
                            {
                                #region Fetching BillContribution DataTable                               
                                dtCompanyCreditContribution = GetCreditContributions(DtCompCRContribution.Copy(), Convert.ToInt32(BillType), DsOutPut.Tables["BillSummary"].Copy(), DtDiscountDetails, DtCreditBillItems);
                                dtCompanyCreditContribution.AcceptChanges();
                                #region Newly Added on 26-08-2011 for dual bill

                                dtCRContribution = dtCompanyCreditContribution.Copy();
                                dtCRContribution.AcceptChanges();
                                if (!dtCompanyCreditContribution.Columns.Contains("BillTypeID"))
                                    dtCompanyCreditContribution.Columns.Add("BillTypeID", typeof(int));
                                if (!dtCompanyCreditContribution.Columns.Contains("BillType"))
                                    dtCompanyCreditContribution.Columns.Add("BillType", typeof(string));


                                for (int i = 0; i < dtCompanyCreditContribution.Rows.Count; i++)
                                {
                                    dtCompanyCreditContribution.Rows[i]["BillTypeID"] = 2;
                                    dtCompanyCreditContribution.Rows[i]["BillType"] = "CR";

                                }

                                dtCompanyCreditContribution.AcceptChanges();
                                #endregion

                                if (!string.IsNullOrEmpty(hdnPatientType.Trim()) && hdnPatientType.Trim() != "null")
                                    intPatientType = Convert.ToInt32(hdnPatientType);
                                else
                                    intPatientType = 1;
                                int intResult = SaveTempDefaultLOAContribution(dtCompanyCreditContribution, strSessionId, strAdmissionUHID, intPatientType, 2, intPatienId);
                                #endregion
                                foreach (DataRow dr1 in dtCompanyCreditContribution.Select("Typ=0"))
                                {
                                    dr1["VAT"] = dtCompanyCreditContribution.Compute("SUM(VAT)", "Typ=5");
                                    dr1["CVAT"] = dtCompanyCreditContribution.Compute("SUM(CVAT)", "Typ=5");
                                    dr1["PVAT"] = dtCompanyCreditContribution.Compute("SUM(PVAT)", "Typ=5");
                                }
                                CalculateVATForCredit(dtCompanyCreditContribution);

                                decTotalVATAmount = decTotalVATAmount + Convert.ToDecimal(hdnVATAmount);
                                hdnVATAmount = decTotalVATAmount.ToString(hdnsCurrencyFormat);
                            }
                        }

                        decimal DiscAmt = 0;

                        if (CollectableType == 2)
                        {
                            decimal decIndvDiscAmount = 0;
                            decimal decItemDiscAmount = 0;
                            decimal decIndvItemAmount = 0;
                            DataTable dtDisountData = null;
                            if (OPBAvailDisc != null)
                            {
                                dtDisountData = OPBAvailDisc.Copy();
                                dtDisountData.AcceptChanges();
                            }

                            DataSet dsTempBillDetails = GetTableDetails("tempbilldetail", "sessionid", strSessionId);
                            DataTable dtTempBillDetails = dsTempBillDetails.Tables[0];

                            decimal decCPAY = Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Rows[0]["CPAY"]);
                            dtDeductible = DsOutPut.Tables["Deductibles"].Copy();
                            if (DtDiscountDetails.Rows.Count > 0 && Convert.ToDecimal(intBillAmount) > 0)
                            {
                                if (decCPAY != 0)
                                    DiscAmt = (Convert.ToDecimal(DtDiscountDetails.Compute("Sum([DIS])", "")) / Convert.ToDecimal(decCPAY));
                                else
                                    DiscAmt = 0;
                                //Item Level
                                DataRow[] drDed = dtDeductible.Select("level=5 ");

                                for (int intCnt = 0; intCnt < drDed.Length; intCnt++)
                                {
                                    if (drDed[intCnt]["mserviceid"].ToString() != "4")
                                    {
                                        decIndvItemAmount = Convert.ToDecimal(DsOutPut.Tables["BillBreakUp"].Compute("Sum(CPAY)", "mlevel=5 and procedureid = " + drDed[intCnt]["mserviceitemid"].ToString() + " and serviceid = " + drDed[intCnt]["mserviceid"].ToString()));
                                        if (decIndvItemAmount != Convert.ToDecimal(dtTempBillDetails.Compute("sum(cpay)", " SIID = " + drDed[intCnt]["mserviceitemid"].ToString() + " and SID = " + drDed[intCnt]["mserviceid"].ToString())))
                                        {
                                            decIndvItemAmount = Convert.ToDecimal(decIndvItemAmount) * Convert.ToDecimal(dtTempBillDetails.Compute("sum(qty)", " siid = " + drDed[intCnt]["mserviceitemid"].ToString() + " and SID = " + drDed[intCnt]["mserviceid"].ToString()));
                                        }
                                        if (DtDiscountDetails.Compute("Sum([DIS])", "SIID = " + drDed[intCnt]["mserviceitemid"].ToString() + " and SID = " + drDed[intCnt]["mserviceid"].ToString()) == DBNull.Value)
                                            decIndvDiscAmount = 0;
                                        else
                                            decIndvDiscAmount = (Convert.ToDecimal(DtDiscountDetails.Compute("Sum([DIS])", "SIID = " + drDed[intCnt]["mserviceitemid"].ToString() + " and SID = " + drDed[intCnt]["mserviceid"].ToString())));

                                        if (OPBAvailDisc != null)
                                        { DiscAmt = (Convert.ToDecimal(DtDiscountDetails.Compute("Sum([DIS])", "")) / Convert.ToDecimal(intBillAmount)); }
                                        else
                                        {
                                            if (decIndvItemAmount == 0)
                                                decItemDiscAmount = 0;
                                            else
                                                decItemDiscAmount = decIndvDiscAmount / Convert.ToDecimal(decIndvItemAmount);
                                        }
                                    }

                                    if (Convert.ToDecimal(drDed[intCnt]["mCPAY"]) > 0)
                                    { drDed[intCnt]["mCPAY"] = Convert.ToDecimal(Convert.ToDecimal(drDed[intCnt]["mCPAY"]) - (Convert.ToDecimal(Convert.ToDecimal(drDed[intCnt]["mCPAY"]) * decItemDiscAmount))); }

                                    if (Convert.ToDecimal(drDed[intCnt]["mPPAY"]) > 0)
                                    { drDed[intCnt]["mPPAY"] = Convert.ToDecimal(Convert.ToDecimal(drDed[intCnt]["mPPAY"]) - (Convert.ToDecimal(Convert.ToDecimal(drDed[intCnt]["mPPAY"]) * decItemDiscAmount))); }
                                }
                                dtDeductible.AcceptChanges();
                                // Speciality Level
                                DataRow[] drSpDed = dtDeductible.Select("level=4 and mCPAY>0 ");
                                //decimal SPDiscAmt=Convert.ToDecimal(Convert.ToInt32(DiscAmt)/Convert.ToInt32(drSpDed.Length));
                                for (int intCnt = 0; intCnt < drSpDed.Length; intCnt++)
                                {
                                    // this is to check the discount given on individual item
                                    if (drSpDed[intCnt]["mserviceid"].ToString() != "4")
                                    {
                                        decIndvItemAmount = Convert.ToDecimal(DsOutPut.Tables["BillBreakUp"].Compute("Sum(CPAY)", "mlevel=5 and SpecialiseID = " + drSpDed[intCnt]["mSpecialiseID"].ToString() + " and DeptId = " + drSpDed[intCnt]["mHospDeptID"].ToString() + " and serviceid = " + drSpDed[intCnt]["mServiceId"].ToString()));
                                        if (decIndvItemAmount != Convert.ToDecimal(dtTempBillDetails.Compute("sum(cpay)", " SPID = " + drSpDed[intCnt]["mSpecialiseID"].ToString() + " and SID = " + drSpDed[intCnt]["mServiceId"].ToString())))
                                        {
                                            decIndvItemAmount = Convert.ToDecimal(decIndvItemAmount) * Convert.ToDecimal(dtTempBillDetails.Compute("sum(qty)", " siid = " + drDed[intCnt]["mserviceitemid"].ToString() + " and SID = " + drDed[intCnt]["mserviceid"].ToString()));
                                        }
                                        if (DtDiscountDetails.Compute("Sum([DIS])", "SPID = " + drSpDed[intCnt]["mSpecialiseID"].ToString() + " and DID = " + drSpDed[intCnt]["mHospDeptID"].ToString() + " and SID = " + drSpDed[intCnt]["mServiceId"].ToString()) == DBNull.Value)
                                            decIndvDiscAmount = 0;
                                        else
                                            decIndvDiscAmount = (Convert.ToDecimal(DtDiscountDetails.Compute("Sum([DIS])", "SPID = " + drSpDed[intCnt]["mSpecialiseID"].ToString() + " and DID = " + drSpDed[intCnt]["mHospDeptID"].ToString() + " and SID = " + drSpDed[intCnt]["mServiceId"].ToString())));

                                        if (OPBAvailDisc != null)
                                        { DiscAmt = (Convert.ToDecimal(DtDiscountDetails.Compute("Sum([DIS])", "")) / Convert.ToDecimal(intBillAmount)); }
                                        else
                                        {
                                            if (decIndvItemAmount == 0)
                                                decItemDiscAmount = 0;
                                            else
                                                decItemDiscAmount = decIndvDiscAmount / Convert.ToDecimal(decIndvItemAmount);
                                        }
                                    }

                                    if (decItemDiscAmount == 0)
                                        decItemDiscAmount = DiscAmt;
                                    if (Convert.ToDecimal(drSpDed[intCnt]["mCPAY"]) > 0)
                                    { drSpDed[intCnt]["mCPAY"] = Convert.ToDecimal(Convert.ToDecimal(drSpDed[intCnt]["mCPAY"]) - (Convert.ToDecimal(Convert.ToDecimal(drSpDed[intCnt]["mCPAY"]) * decItemDiscAmount))); }

                                    if (Convert.ToDecimal(drSpDed[intCnt]["mPPAY"]) > 0)
                                    { drSpDed[intCnt]["mPPAY"] = Convert.ToDecimal(Convert.ToDecimal(drSpDed[intCnt]["mPPAY"]) - (Convert.ToDecimal(Convert.ToDecimal(drSpDed[intCnt]["mPPAY"]) * decItemDiscAmount))); }
                                }
                                dtDeductible.AcceptChanges();


                                // Department Level
                                DataRow[] drDpDed = dtDeductible.Select("level=3 and mCPAY>0 ");
                                for (int intCnt = 0; intCnt < drDpDed.Length; intCnt++)
                                {
                                    if (drDpDed[intCnt]["mserviceid"].ToString() != "4")
                                    {
                                        decIndvItemAmount = Convert.ToDecimal(DsOutPut.Tables["BillBreakUp"].Compute("Sum(CPAY)", "mlevel=5" + " and DeptID = " + drDpDed[intCnt]["mHospDeptID"].ToString() + " and serviceid = " + drDpDed[intCnt]["mServiceId"].ToString()));
                                        if (decIndvItemAmount != Convert.ToDecimal(dtTempBillDetails.Compute("sum(cpay)", " SID = " + drDed[intCnt]["mserviceid"].ToString())))
                                        {
                                            decIndvItemAmount = Convert.ToDecimal(decIndvItemAmount) * Convert.ToDecimal(dtTempBillDetails.Compute("sum(qty)", " SID = " + drDed[intCnt]["mserviceid"].ToString()));
                                        }
                                        if (DtDiscountDetails.Compute("Sum([DIS])", " DID = " + drDpDed[intCnt]["mHospDeptID"].ToString() + " and SID = " + drDpDed[intCnt]["mServiceId"].ToString()) == DBNull.Value)
                                            decIndvDiscAmount = 0;
                                        else
                                            decIndvDiscAmount = (Convert.ToDecimal(DtDiscountDetails.Compute("Sum([DIS])", " DID = " + drDpDed[intCnt]["mHospDeptID"].ToString() + " and SID = " + drDpDed[intCnt]["mServiceId"].ToString())));

                                        if (OPBAvailDisc != null)
                                        { DiscAmt = (Convert.ToDecimal(DtDiscountDetails.Compute("Sum([DIS])", "")) / Convert.ToDecimal(intBillAmount)); }
                                        else
                                        {
                                            if (decIndvItemAmount == 0)
                                                decItemDiscAmount = 0;
                                            else
                                                decItemDiscAmount = decIndvDiscAmount / Convert.ToDecimal(decIndvItemAmount);
                                        }
                                    }
                                    if (decItemDiscAmount == 0)
                                        decItemDiscAmount = DiscAmt;
                                    if (Convert.ToDecimal(drDpDed[intCnt]["mCPAY"]) > 0)
                                    { drDpDed[intCnt]["mCPAY"] = Convert.ToDecimal(Convert.ToDecimal(drDpDed[intCnt]["mCPAY"]) - (Convert.ToDecimal(Convert.ToDecimal(drDpDed[intCnt]["mCPAY"]) * decItemDiscAmount))); }

                                    if (Convert.ToDecimal(drDpDed[intCnt]["mPPAY"]) > 0)
                                    { drDpDed[intCnt]["mPPAY"] = Convert.ToDecimal(Convert.ToDecimal(drDpDed[intCnt]["mPPAY"]) - (Convert.ToDecimal(Convert.ToDecimal(drDpDed[intCnt]["mPPAY"]) * decItemDiscAmount))); }
                                }
                                dtDeductible.AcceptChanges();

                                // Service Level
                                DataRow[] drSDed = dtDeductible.Select("level=2 ");
                                for (int intCnt = 0; intCnt < drSDed.Length; intCnt++)
                                {
                                    if (drSDed[intCnt]["mserviceid"].ToString() != "4")
                                    {
                                        decIndvItemAmount = Convert.ToDecimal(DsOutPut.Tables["BillBreakUp"].Compute("Sum(CPAY)", "mlevel=5 and " + " serviceid = " + drSDed[intCnt]["mServiceId"].ToString()));
                                        if (decIndvItemAmount != Convert.ToDecimal(dtTempBillDetails.Compute("sum(cpay)", " SID = " + drDed[intCnt]["mserviceid"].ToString())))
                                        {
                                            decIndvItemAmount = Convert.ToDecimal(dtTempBillDetails.Compute("sum(cpay)", " SID = " + drDed[intCnt]["mserviceid"].ToString()));
                                        }
                                        if (DtDiscountDetails.Compute("Sum([DIS])", " SID = " + drSDed[intCnt]["mServiceId"].ToString()) == DBNull.Value)
                                            decIndvDiscAmount = 0;
                                        else
                                            decIndvDiscAmount = (Convert.ToDecimal(DtDiscountDetails.Compute("Sum([DIS])", " SID = " + drSDed[intCnt]["mServiceId"].ToString())));

                                        if (OPBAvailDisc != null)
                                        { DiscAmt = (Convert.ToDecimal(DtDiscountDetails.Compute("Sum([DIS])", "")) / Convert.ToDecimal(intBillAmount)); }
                                        else
                                        {
                                            if (decIndvItemAmount == 0)
                                                decItemDiscAmount = 0;
                                            else
                                                decItemDiscAmount = decIndvDiscAmount / Convert.ToDecimal(decIndvItemAmount);
                                        }
                                    }

                                    if (decItemDiscAmount == 0)
                                        decItemDiscAmount = DiscAmt;

                                    if (Convert.ToDecimal(drSDed[intCnt]["mCPAY"]) > 0)
                                    { drSDed[intCnt]["mCPAY"] = Convert.ToDecimal(Convert.ToDecimal(drSDed[intCnt]["mCPAY"]) - (Convert.ToDecimal(Convert.ToDecimal(drSDed[intCnt]["mCPAY"]) * decItemDiscAmount))); }

                                    if (Convert.ToDecimal(drSDed[intCnt]["mPPAY"]) > 0)
                                    { drSDed[intCnt]["mPPAY"] = Convert.ToDecimal(Convert.ToDecimal(drSDed[intCnt]["mPPAY"]) - (Convert.ToDecimal(Convert.ToDecimal(drSDed[intCnt]["mPPAY"]) * decItemDiscAmount))); }
                                }
                                dtDeductible.AcceptChanges();

                                // Grade Level
                                DataRow[] drGr = dtDeductible.Select("level=1 ");
                                for (int intCnt = 0; intCnt < drGr.Length; intCnt++)
                                {
                                    if (Convert.ToDecimal(drGr[intCnt]["mCPAY"]) > 0)
                                    {
                                        drGr[intCnt]["mCPAY"] = Convert.ToDecimal(Convert.ToDecimal(drGr[intCnt]["mCPAY"])) - (Convert.ToDecimal(DtDiscountDetails.Compute("Sum([DCOM])", "")));
                                    }
                                    if (Convert.ToDecimal(drGr[intCnt]["mPPAY"]) > 0)
                                    {

                                    }
                                }
                                dtDeductible.AcceptChanges();
                            }

                            DtBillsummary = DsOutPut.Tables["BillSummary"].Clone();
                            decimal DiscAmount = 0;
                            if (DtDiscountDetails.Rows.Count > 0)
                            { DiscAmount = Convert.ToDecimal(DtDiscountDetails.Compute("Sum([DCOM])", "")); }
                            else
                            { DiscAmount = 0; }
                            if (String.IsNullOrEmpty(Convert.ToString(dtDeductible.Rows[0]["DedLTID"])))
                            {
                                if (!String.IsNullOrEmpty(hdnMaxCollectable))
                                {
                                    if (Convert.ToInt16(hdnMaxCollectable) > 0)
                                    {
                                        objPatientList.Code = (int)ProcessStatus.Success;
                                        objPatientList.Status = ProcessStatus.Success.ToString();
                                        //objPatientList.Message = Resources.English.ResourceManager.GetString("CCNDCARD");
                                        //objPatientList.Message2L = Resources.Arabic.ResourceManager.GetString("CCNDCARD");
                                        objPatientList.Message = System.Configuration.ConfigurationManager.AppSettings["CCNDCARD"].ToString();
                                        objPatientList.Message2L = System.Configuration.ConfigurationManager.AppSettings["CCNDCARD"].ToString();
                                        return objPatientList;
                                    }
                                    else
                                    {
                                        for (int i = 0; i < dtDeductible.Rows.Count; i++)
                                        {
                                            dtDeductible.Rows[i]["DedLIMITAMT"] = "0";
                                            if (i == 0)
                                                dtDeductible.Rows[i]["DedLTID"] = "3";
                                            else
                                                dtDeductible.Rows[i]["DedLTID"] = "2";
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < dtDeductible.Rows.Count; i++)
                                    {
                                        dtDeductible.Rows[i]["DedLIMITAMT"] = "0";
                                        if (i == 0)
                                            dtDeductible.Rows[i]["DedLTID"] = "3";
                                        else
                                            dtDeductible.Rows[i]["DedLTID"] = "2";
                                    }

                                }
                            }

                            DsSplitCollectables = CalculateDeductibles(dtDeductible.Copy(), false, false, DiscAmount);
                            if (DsSplitCollectables != null && DsSplitCollectables.Tables.Count > 0 && DsSplitCollectables.Tables[1] != null
                                && DsSplitCollectables.Tables[1].Rows.Count > 0)
                            {
                                DtBillsummary = DsSplitCollectables.Tables[1].Copy();
                            }

                            dtTempOut = DtBillsummary.Copy();
                            if (Convert.ToDouble(dtTempOut.Rows[0]["DPAY"] == DBNull.Value ? 0 : dtTempOut.Rows[0]["DPAY"]) >= Convert.ToDouble(MaxCollectable))
                            {
                                if (MaxCollectable > 0)
                                {
                                    dtTempOut.Rows[0]["CPAY"] = Convert.ToDouble(dtTempOut.Rows[0]["CPAY"]) + (Convert.ToDouble(dtTempOut.Rows[0]["DPAY"]) - (Convert.ToDouble(MaxCollectable)));
                                    if (Convert.ToDouble(dtTempOut.Rows[0]["PPAY"]) == Convert.ToDouble(dtTempOut.Rows[0]["DPAY"]))
                                    {
                                        dtTempOut.Rows[0]["PPAY"] = Convert.ToDouble(MaxCollectable);//Convert.ToDouble(dtTempOut.Rows[0]["PPAY"]) - Convert.ToDouble(MaxCollectable);	
                                        dtTempOut.Rows[0]["DPAY"] = Convert.ToDouble(MaxCollectable);
                                    }
                                    else if (Convert.ToDouble(dtTempOut.Rows[0]["PPAY"]) > Convert.ToDouble(dtTempOut.Rows[0]["DPAY"]))
                                    {
                                        dtTempOut.Rows[0]["PPAY"] = Convert.ToDouble(dtTempOut.Rows[0]["PPAY"]) - Convert.ToDouble(MaxCollectable);
                                        dtTempOut.Rows[0]["DPAY"] = Convert.ToDouble(MaxCollectable);
                                    }

                                    dtTempOut.AcceptChanges();
                                }
                                else if (MaxCollectable == 0)
                                {
                                    dtTempOut.Rows[0]["CPAY"] = Convert.ToDouble(dtTempOut.Rows[0]["CPAY"]) + (Convert.ToDouble(dtTempOut.Rows[0]["DPAY"]) - (Convert.ToDouble(MaxCollectable)));
                                    dtTempOut.Rows[0]["PPAY"] = Convert.ToDouble(dtTempOut.Rows[0]["PPAY"]) - Convert.ToDouble(dtTempOut.Rows[0]["DPAY"]);
                                    dtTempOut.Rows[0]["DPAY"] = Convert.ToDouble(MaxCollectable);

                                }
                            }

                            if (!string.IsNullOrEmpty(DsOutPut.Tables["BillSummary"].Rows[0]["DepositAmount"].ToString()))
                            {
                                if (!dtTempOut.Columns.Contains("DepositAmount"))
                                    dtTempOut.Columns.Add("DepositAmount", typeof(decimal));
                                dtTempOut.Rows[0]["DepositAmount"] = Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Rows[0]["DepositAmount"]);
                            }

                            DsOutPut.Tables["BillSummary"].Rows.Clear();

                            foreach (DataRow dr in dtTempOut.Select())
                            {
                                DsOutPut.Tables["BillSummary"].ImportRow(dr);
                            }

                            DsOutPut.Tables["BillSummary"].AcceptChanges();

                            dtSummary = DsOutPut.Tables["BillSummary"].Copy();
                            dtSummary.AcceptChanges();

                            if (DtCompCSContribution != null && DtCompCSContribution.Rows.Count > 0)
                            {
                                #region Fetching BillContribution DataTable                               
                                dtCompanyCashContribution = GetCashContributions(DtCompCSContribution.Copy(), Convert.ToInt32(BillType), DsOutPut.Tables["BillSummary"].Copy(), DtDiscountDetails, DtCashBillItems);
                                dtCompanyCashContribution.AcceptChanges();
                                dtCompanyCashContribution = dtCompanyCashContribution.Copy();
                                dtCompanyCashContribution.AcceptChanges();
                                #region Newly Added on 26-08-2011 for dual bill
                                if (BillType == "2")
                                {
                                    if (!dtCompanyCashContribution.Columns.Contains("BillTypeID"))
                                        dtCompanyCashContribution.Columns.Add("BillTypeID", typeof(int));
                                    if (!dtCompanyCashContribution.Columns.Contains("BillType"))
                                        dtCompanyCashContribution.Columns.Add("BillType", typeof(string));
                                    for (int i = 0; i < dtCompanyCashContribution.Rows.Count; i++)
                                    {
                                        dtCompanyCashContribution.Rows[i]["BillTypeID"] = 2;
                                        dtCompanyCashContribution.Rows[i]["BillType"] = "SP";
                                    }
                                }
                                #endregion Newly Added on 26-08-2011 for dual bill

                                if (!string.IsNullOrEmpty(hdnPatientType.Trim()) && hdnPatientType.Trim() != "null")
                                    intPatientType = Convert.ToInt32(hdnPatientType);
                                else
                                    intPatientType = 1;
                                int intResult = SaveTempDefaultLOAContribution(dtCompanyCashContribution, strSessionId, strAdmissionUHID, intPatientType, 1, intPatienId);
                                #endregion
                                foreach (DataRow dr1 in dtCompanyCashContribution.Select("Typ=0"))
                                {
                                    dr1["VAT"] = dtCompanyCashContribution.Compute("SUM(VAT)", "Typ=5");
                                    dr1["CVAT"] = dtCompanyCashContribution.Compute("SUM(CVAT)", "Typ=5");
                                    dr1["PVAT"] = dtCompanyCashContribution.Compute("SUM(PVAT)", "Typ=5");
                                }
                                CalculateVATForCredit(dtCompanyCashContribution);

                                hdnPVATValueforSP = hdnPVATValue;
                                decTotalVATAmount = decTotalVATAmount + Convert.ToDecimal(hdnVATAmount);
                                hdnVATAmount = decTotalVATAmount.ToString(hdnsCurrencyFormat);
                            }

                            if (DtCompCRContribution != null && DtCompCRContribution.Rows.Count > 0)
                            {
                                #region Fetching BillContribution DataTable                              
                                dtCompanyCreditContribution = GetCreditContributions(DtCompCRContribution.Copy(), Convert.ToInt32(BillType), DsOutPut.Tables["BillSummary"].Copy(), DtDiscountDetails, DtCreditBillItems);
                                dtCompanyCreditContribution.AcceptChanges();


                                dtCRContribution = dtCompanyCreditContribution.Copy();
                                dtCRContribution.AcceptChanges();
                                if (!dtCompanyCreditContribution.Columns.Contains("BillTypeID"))
                                    dtCompanyCreditContribution.Columns.Add("BillTypeID", typeof(int));
                                if (!dtCompanyCreditContribution.Columns.Contains("BillType"))
                                    dtCompanyCreditContribution.Columns.Add("BillType", typeof(string));

                                for (int i = 0; i < dtCompanyCreditContribution.Rows.Count; i++)
                                {
                                    dtCompanyCreditContribution.Rows[i]["BillTypeID"] = 2;
                                    dtCompanyCreditContribution.Rows[i]["BillType"] = "CR";
                                }
                                dtCompanyCreditContribution.AcceptChanges();

                                if (!string.IsNullOrEmpty(hdnPatientType.Trim()) && hdnPatientType.Trim() != "null")
                                    intPatientType = Convert.ToInt32(hdnPatientType);
                                else
                                    intPatientType = 1;

                                int intResult = SaveTempDefaultLOAContribution(dtCompanyCreditContribution, strSessionId, strAdmissionUHID, intPatientType, intBillType, intPatienId);
                                #endregion
                                foreach (DataRow dr1 in dtCompanyCreditContribution.Select("Typ=0"))
                                {
                                    dr1["VAT"] = DsOutPut.Tables[0].Compute("SUM(VAT)", "Typ=5");
                                    dr1["CVAT"] = DsOutPut.Tables[0].Compute("SUM(CVAT)", "Typ=5");
                                    dr1["PVAT"] = DsOutPut.Tables[0].Compute("SUM(PVAT)", "Typ=5");
                                }
                                CalculateVATForCredit(dtCompanyCreditContribution);
                                decTotalVATAmount = decTotalVATAmount + Convert.ToDecimal(hdnVATAmount);
                                hdnVATAmount = decTotalVATAmount.ToString(hdnsCurrencyFormat);
                            }

                        }


                        #endregion BASED ON COLLECTABLES

                        hdnDepositAmount = Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Rows[0]["DepositAmount"].ToString());

                        #region VALIDATION FOR CONTINUE BILL

                        patientamt = Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Compute("Sum([SPAY])+Sum([PPay])", ""));

                        StringBuilder strClinical = new StringBuilder();
                        if (!String.IsNullOrEmpty(Strclinical))
                        {
                            //strClinical.Append("<br/>Do you want to remove the Uncovered Item(s) ? ");
                            // ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','Following  Clinical Conditions are Not Covered! Need Approval :" + Strclinical.ToString() + "','YESNO','Information');", true);
                            hdnOutputBillTpe = "YESNO";
                        }

                        StringBuilder strDiagnosis = new StringBuilder();
                        if (!String.IsNullOrEmpty(Strvalue))
                        {
                            //strDiagnosis.Append("<br/>Do you want to remove the Uncovered Item(s) ? ");
                            // ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','Following  ICDCOde are Not Covered! Need Approval :" + Strvalue.ToString() + "','YESNO','Information');", true);
                            hdnOutputBillTpe = "YESNO";
                        }

                        string strvalue = string.Empty;
                        if (patientamt > 0 && hdnrblbilltypeCredit == true)
                        {
                            StringBuilder strPatShareMsg = new StringBuilder();
                            StringBuilder strLimitExceedPatShareMsg = new StringBuilder();

                            if (DtCompCRContribution.Columns.Contains("ServiceItemName"))
                                DtCompCRContribution.Columns["ServiceItemName"].ColumnName = "procedure";
                            DtCompCRContribution.AcceptChanges();
                            DataRow[] drPatBill = null;
                            if (DtCompCRContribution != null && DtCompCRContribution.Rows.Count > 0)
                            {
                                drPatBill = DtCompCRContribution.Select("mLevel in (2) and PPAY>0");
                                for (int i = 0; i < drPatBill.Length; i++)
                                {
                                    strLimitExceedPatShareMsg.Append("    " + (i + 1) + ".  " + drPatBill[i]["ServiceName"].ToString() + "  -- " + drPatBill[i]["procedure"].ToString() + "</br>");
                                }
                                if (strLimitExceedPatShareMsg.Length > 0)
                                {
                                    System.IO.StringWriter writer = new System.IO.StringWriter();
                                    DtCompCRContribution.WriteXml(writer, true);
                                }
                            }
                            DataRow[] drUncoveredItem = null;
                            if (DtCompCSContribution != null && DtCompCSContribution.Rows.Count != 0)
                            {
                                drPatBill = DtCompCSContribution.Select("mLevel in (5) and SPAY>0");
                                drUncoveredItem = DtCompCSContribution.Select("mLevel in (5) and SPAY>0");
                                for (int i = 0; i < drPatBill.Length; i++)
                                {
                                    strPatShareMsg.Append("    " + (i + 1) + ".  " + drPatBill[i]["ServiceName"].ToString() + " -- " + drPatBill[i]["procedure"].ToString() + "<br/>");
                                }
                                if (strPatShareMsg.Length > 0)
                                {
                                    System.IO.StringWriter writer = new System.IO.StringWriter();
                                    DtCompCSContribution.WriteXml(writer, true);
                                }
                            }
                            if (ApprovalPendingItems != null)
                            {
                                DataTable dtPending = ApprovalPendingItems.Copy();
                                dtPending.AcceptChanges();
                                foreach (DataRow i in dtPending.Rows)
                                {
                                    for (int j = 0; j < dtPending.Rows.Count; j++)
                                    {
                                        strPatShareMsg.Append("<br/>" + "    " + "Following Services Are Pending For Approval" + "<br/>" + (j + 1) + ".  " + Convert.ToString(i["Service"]) + " -- " + Convert.ToString(i["Item Name"]) + "<br/>");
                                    }
                                }
                            }
                            string str = ConfigurationManager.AppSettings.Get("IsLimitExceeds");
                            if ((strPatShareMsg.Length > 0 | strLimitExceedPatShareMsg.Length > 0) && hdnrblbilltypeCredit == true)
                            {
                                if (TagId == 0)
                                {
                                    if (drUncoveredItem != null && drUncoveredItem.Length > 0)
                                    {
                                    }
                                    else
                                    {
                                        if (intCPAY > 0 && intPAY > 0 || intCPAY > 0 && intPAY > 0)
                                        {
                                            if (intBillAmount > 0 && intCPAY <= 0)
                                            {
                                                blnLimitUtilization = true;
                                                objPatientList.Code = (int)ProcessStatus.Success;
                                                objPatientList.Status = ProcessStatus.Success.ToString();
                                                objPatientList.Message = "Bill cannot be saved as Limit amount got utilized completely.Please do Cash Bill";
                                                return objPatientList;
                                            }
                                            else
                                            {
                                                if (hdnPatientType != "3")
                                                {
                                                    objPatientList.Code = (int)ProcessStatus.Success;
                                                    objPatientList.Status = ProcessStatus.Success.ToString();
                                                    objPatientList.Message = "Limit exceeded";
                                                    return objPatientList;
                                                }
                                                else
                                                {
                                                    strERExceedMSG = "Limit Exceeded  <br/><br/>" + strERExceedMSG;
                                                    objPatientList.Code = (int)ProcessStatus.Success;
                                                    objPatientList.Status = ProcessStatus.Success.ToString();
                                                    objPatientList.Message = strERExceedMSG;
                                                    return objPatientList;
                                                }
                                            }

                                        }
                                        else
                                        {
                                            objPatientList.Code = (int)ProcessStatus.Success;
                                            objPatientList.Status = ProcessStatus.Success.ToString();
                                            objPatientList.Message = "Following Services are Not Covered! Need Approval" + strPatShareMsg.ToString();
                                            return objPatientList;
                                        }
                                        DisplayBillSummary(DtDiscountDetails, hdnDepositAmount);
                                    }
                                }
                                else
                                {
                                    if (drUncoveredItem != null && drUncoveredItem.Length > 0)
                                    {
                                    }
                                    else
                                    {
                                        if (intCPAY > 0 && intPAY > 0 || intCPAY > 0 && intPAY > 0)
                                        {
                                            if (intBillAmount > 0 && intCPAY <= 0)
                                            {
                                                blnLimitUtilization = true;
                                                objPatientList.Code = (int)ProcessStatus.Success;
                                                objPatientList.Status = ProcessStatus.Success.ToString();
                                                objPatientList.Message = "Bill cannot be saved as Limit amount got utilized completely.Please do Cash Bill";
                                                return objPatientList;
                                            }
                                            else
                                            {
                                                if (hdnPatientType != "3")
                                                {
                                                    blnLimitUtilization = true;
                                                    objPatientList.Code = (int)ProcessStatus.Success;
                                                    objPatientList.Status = ProcessStatus.Success.ToString();
                                                    objPatientList.Message = "Limit exceeded";
                                                    return objPatientList;
                                                }
                                                else
                                                {
                                                    strERExceedMSG = "Limit Exceeded  <br/><br/>" + strERExceedMSG;
                                                    objPatientList.Code = (int)ProcessStatus.Success;
                                                    objPatientList.Status = ProcessStatus.Success.ToString();
                                                    objPatientList.Message = strERExceedMSG;
                                                    return objPatientList;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            objPatientList.Code = (int)ProcessStatus.Success;
                                            objPatientList.Status = ProcessStatus.Success.ToString();
                                            objPatientList.Message = "Following Services are Not Covered! Need Approval" + strPatShareMsg.ToString();
                                            return objPatientList;
                                        }

                                        DisplayBillSummary(DtDiscountDetails, hdnDepositAmount);

                                    }
                                }
                                if (drUncoveredItem != null && drUncoveredItem.Length > 0)
                                {
                                    DtCompCSContribution = DtCompCSContribution.Copy();
                                    DtCompCSContribution.AcceptChanges();

                                    DisplayBillSummary(DtDiscountDetails, hdnDepositAmount);
                                    strPatShareMsg.Append("<br/>");
                                    if (str.ToUpper() == "NO")
                                    {
                                        strPatShareMsg.Append("<br/>Please remove the  UnCovered item and continue billing");
                                        // ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','Following Services are Not Covered! Need Approval" + strPatShareMsg.ToString() + "','OK','Information');", true);
                                        hdnOutputBillTpe = "YESNO";
                                    }
                                    else
                                    {
                                        if (hdnPatientType == "3")
                                        {
                                            strPatShareMsg.Append("<br/>Uncovered items will be moved to Supplementary bill!");
                                            //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','Following Services are Not Covered! Need Approval" + strPatShareMsg.ToString() + "','OK','Information');", true);
                                        }
                                        else
                                        {
                                            strPatShareMsg.Append("<br/>Do you want to remove the Uncovered Item(s) ? ");
                                            //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','Following Services are Not Covered! Need Approval" + strPatShareMsg.ToString() + "','YESNO','Information');", true);
                                            objPatientList.Code = (int)ProcessStatus.Success;
                                            objPatientList.Status = ProcessStatus.Success.ToString();
                                            objPatientList.Message = "Following Services are Not Covered! Need Approval" + strPatShareMsg.ToString();
                                            return objPatientList;
                                        }
                                        hdnOutputBillTpe = "YESNO";
                                    }
                                }

                            }
                        }

                        intLetterNo = letterNo;
                        if (intLetterNo > 0)
                        {
                            DataTable dtLOA = FetchLetterDetails(intLetterNo, 0, "2", Convert.ToInt32(strDefaultUserId.ToString()), Convert.ToInt32(strDefWorkstationId.ToString()), 0).Tables[0];
                            if (dtLOA.Rows.Count > 0)
                            {
                                if (TagId == 0)
                                {
                                    Limitbalance = Convert.ToInt32(dtLOA.Rows[0]["Limit"]) - Convert.ToInt32(dtLOA.Rows[0]["Utilization"] == DBNull.Value ? 0 : Convert.ToInt32(dtLOA.Rows[0]["Utilization"]));
                                    string str = "Limit: ";
                                    if (Convert.ToInt32(hdnMaxCollectable) < 0) { hdnMaxCollectable = "0"; }
                                    if (MaxCollectable == -1)
                                        str = str + Convert.ToInt32(dtLOA.Rows[0]["Limit"]).ToString(hdnsCurrencyFormat) + "    Balance: " + Convert.ToDecimal(Limitbalance).ToString(hdnsCurrencyFormat) + "    MaxColl.: " + Convert.ToDecimal(hdnMaxCollectable).ToString(hdnsCurrencyFormat) + "    Collected : " + Convert.ToDecimal(Convert.ToInt32(hdnMaxCollectable) - 0).ToString(hdnsCurrencyFormat);
                                    else
                                        str = str + Convert.ToInt32(dtLOA.Rows[0]["Limit"]).ToString(hdnsCurrencyFormat) + "    Balance: " + Convert.ToDecimal(Limitbalance).ToString(hdnsCurrencyFormat) + "    MaxColl.: " + Convert.ToDecimal(hdnMaxCollectable).ToString(hdnsCurrencyFormat) + "    Collected : " + Convert.ToDecimal(Convert.ToInt32(hdnMaxCollectable) - MaxCollectable).ToString(hdnsCurrencyFormat);

                                    Limitbalance = Limitbalance + decERExtraLoaLimit;
                                }
                                else
                                {
                                    Limitbalance = Convert.ToInt32(dtLOA.Rows[0]["Limit"]) - Convert.ToInt32(dtLOA.Rows[0]["Utilization"] == DBNull.Value ? 0 : Convert.ToInt32(dtLOA.Rows[0]["Utilization"]));
                                    string str = "CLimit : ";
                                    if (Convert.ToInt32(hdnMaxCollectable) < 0) { hdnMaxCollectable = "0"; }
                                    if (MaxCollectable == -1)
                                        str = str + Convert.ToInt32(dtLOA.Rows[0]["Limit"]).ToString(hdnsCurrencyFormat) + " CBalance :" + Convert.ToDecimal(Limitbalance).ToString(hdnsCurrencyFormat) + "MaxColl :" + Convert.ToDecimal(hdnMaxCollectable).ToString(hdnsCurrencyFormat) + "Collected : " + Convert.ToDecimal(Convert.ToInt32(hdnMaxCollectable) - 0).ToString(hdnsCurrencyFormat);
                                    else
                                        str = str + Convert.ToInt32(dtLOA.Rows[0]["Limit"]).ToString(hdnsCurrencyFormat) + "  CBalance : " + Convert.ToDecimal(Limitbalance).ToString(hdnsCurrencyFormat) + "MaxColl :" + Convert.ToDecimal(hdnMaxCollectable).ToString(hdnsCurrencyFormat) + "Collected : " + Convert.ToDecimal(Convert.ToInt32(hdnMaxCollectable) - MaxCollectable).ToString(hdnsCurrencyFormat);

                                    Limitbalance = Limitbalance + decERExtraLoaLimit;
                                }
                            }

                            DsForUserMessages.Tables.Add(DsOutPut.Tables["LTable"].Copy());
                            DsForUserMessages.Tables.Add(DsOutPut.Tables["LTable1"].Copy());
                            DsForUserMessages.Tables.Add(DsOutPut.Tables["LTable2"].Copy());
                            DsForUserMessages.Tables.Add(DsOutPut.Tables["LTable3"].Copy());
                            DsForUserMessages.Tables.Add(DsOutPut.Tables["LTable4"].Copy());


                            double dblAMT = ServiceWiseUserMessages(DsForUserMessages, DTTemp);

                            if ((dblAMT == -1 || dblAMT > 0) & hdnrblbilltypeCredit == true)
                            {
                                decimal dblactualbillAmount = 0;
                                if (dblAMT == -1)
                                    dblactualbillAmount = Convert.ToDecimal(DsOutPut.Tables["BillSummary"].Rows[0]["Amount"]); //Convert.ToDecimal(lblBillAmount.Text);
                                else
                                    dblactualbillAmount = Convert.ToDecimal(dblAMT);

                                if (Convert.ToDecimal(Limitbalance) < dblactualbillAmount && (Convert.ToInt32(dtLOA.Rows[0]["LimitType"]) == 2 || Convert.ToInt32(dtLOA.Rows[0]["LimitType"]) == 4))
                                {
                                    if (TagId == 0)
                                    {
                                        if (dblactualbillAmount > 0 && intCPAY <= 0)
                                        {
                                            blnLimitUtilization = true;
                                            objPatientList.Code = (int)ProcessStatus.Success;
                                            objPatientList.Status = ProcessStatus.Success.ToString();
                                            objPatientList.Message = "Bill cannot be saved as Limit amount got utilized completely.Please do Cash Bill";
                                            return objPatientList;
                                        }
                                        else
                                        {
                                            objPatientList.Code = (int)ProcessStatus.Success;
                                            objPatientList.Status = ProcessStatus.Success.ToString();
                                            objPatientList.Message = "Limit Exceeded";
                                            return objPatientList;
                                        }

                                    }
                                    else
                                    {
                                        objPatientList.Code = (int)ProcessStatus.Success;
                                        objPatientList.Status = ProcessStatus.Success.ToString();
                                        objPatientList.Message = "Limit Exceeded";
                                        return objPatientList;
                                    }

                                    string strF = System.Configuration.ConfigurationSettings.AppSettings.Get("IsLimitExceeds");

                                    if (blnLimitUtilization)
                                    {
                                        DisplayBillSummary(DtDiscountDetails, hdnDepositAmount);
                                        return null;
                                    }
                                    if (strF.ToUpper() == "NO")
                                    {
                                        DisplayBillSummary(DtDiscountDetails, hdnDepositAmount);
                                        return null;
                                    }
                                }
                            }
                        }

                        #endregion VALIDATION FOR CONTINUE BILL

                        string amount = Convert.ToString(DsOutPut.Tables["BillSummary"].Rows[0]["Amount"].ToString());
                        DisplayBillSummary(DtDiscountDetails, hdnDepositAmount);

                        if (!dtItemSplitDetails.Columns.Contains("AmountSplit"))
                            dtItemSplitDetails.Columns.Add("AmountSplit", typeof(Decimal));
                        if (!dtItemSplitDetails.Columns.Contains("BalanceSplit"))
                            dtItemSplitDetails.Columns.Add("BalanceSplit", typeof(Decimal));
                        DataRow[] RowCheck = null;
                        if (BillType == "2" && dtItemSplitDetails != null && dtItemSplitDetails.Rows.Count > 0)
                        {
                            #region Splitting Collectables After Discount
                            if (CollectableType == 2 && DsSplitCollectables != null && DsSplitCollectables.Tables.Count > 0 && DsSplitCollectables.Tables["Deductibles"] != null
                           && DsSplitCollectables.Tables["Deductibles"].Rows.Count > 0)
                            {
                                object objLevel1 = null;
                                if (DsSplitCollectables.Tables["Deductibles"].Columns.Contains("CPAY") &&
                                       DsSplitCollectables.Tables["Deductibles"].Columns.Contains("DPAY"))
                                {
                                    DataRow[] drowCheck = DsSplitCollectables.Tables["Deductibles"].Select("CPAY >0 and DPAY >0", "mLevel ASC");

                                    foreach (DataRow row in drowCheck)
                                    {
                                        objLevel1 = row["mLevel"];
                                        if (!(objLevel1 is DBNull))
                                        {
                                            DataRow[] drArray = DsSplitCollectables.Tables["Deductibles"].Select("mLevel = " + Convert.ToInt32(objLevel1) + " And CPAY >0 And DPAY >0 ");
                                            for (int intcount = 0; intcount < drArray.Length; intcount++)
                                            {
                                                if (drArray.Length > 0)
                                                {

                                                    decimal dcDPAY = Convert.ToDecimal(drArray[intcount]["DPAY"] is DBNull ? 0 : drArray[intcount]["DPAY"]);
                                                    decimal dcCPAY = Convert.ToDecimal(drArray[intcount]["CPAY"] is DBNull ? 0 : drArray[intcount]["CPAY"]);
                                                    decimal dcDPAYPER = dcDPAY * 100 / (dcCPAY);
                                                    DataRow[] drItemsArray = null;
                                                    switch (Convert.ToInt32(objLevel1))
                                                    {
                                                        case 1:
                                                            drItemsArray = DsSplitCollectables.Tables["Deductibles"].Select();
                                                            break;
                                                        case 2:
                                                            drItemsArray = DsSplitCollectables.Tables["Deductibles"].Select("mlevel > " + Convert.ToInt32(objLevel1) + " And ServiceID= " + drArray[intcount]["ServiceID"]);
                                                            break;
                                                        case 3:
                                                            drItemsArray = DsSplitCollectables.Tables["Deductibles"].Select("mlevel > " + Convert.ToInt32(objLevel1) + " And ServiceID= " + drArray[intcount]["ServiceID"] + " And HospDeptID = " + drArray[intcount]["HospDeptID"]);
                                                            break;
                                                        case 4:
                                                            drItemsArray = DsSplitCollectables.Tables["Deductibles"].Select("mlevel > " + Convert.ToInt32(objLevel1) + " And ServiceID= " + drArray[intcount]["ServiceID"] + " And HospDeptID = " + drArray[intcount]["HospDeptID"] + " And SpecialiseId = " + drArray[intcount]["SpecialiseId"]);
                                                            break;
                                                        case 5:
                                                            drItemsArray = DsSplitCollectables.Tables["Deductibles"].Select("mlevel >= " + Convert.ToInt32(objLevel1) + " And ServiceID= " + drArray[intcount]["ServiceID"] + " And HospDeptID = " + drArray[intcount]["HospDeptID"] + " And SpecialiseId = " + drArray[intcount]["SpecialiseId"] + " And ProcedureId = " + drArray[intcount]["ProcedureId"]);
                                                            break;
                                                    }
                                                    Decimal DiscountAmt = 0;
                                                    foreach (DataRow dr in drItemsArray)
                                                    {
                                                        if (Convert.ToDecimal(dr["DPAY"]) != 0 || Convert.ToDecimal(dr["CPAY"]) != 0)
                                                        {
                                                            RowCheck = null;
                                                            DiscountAmt = 0;
                                                            if (DtDiscountDetails != null && DtDiscountDetails.Rows.Count > 0)
                                                            {
                                                                RowCheck = DtDiscountDetails.Select("TYP='5' AND SIID='" + dr["ProcedureId"].ToString() + "'");
                                                                if (RowCheck.Length > 0)
                                                                {
                                                                    if (RowCheck[0]["DIS"] != DBNull.Value && !string.IsNullOrEmpty(RowCheck[0]["DIS"].ToString()))
                                                                    {
                                                                        DiscountAmt = Convert.ToDecimal(RowCheck[0]["DIS"]);
                                                                    }
                                                                }
                                                            }
                                                            decimal totalpay = Convert.ToDecimal(dr["CPAY"]) + DiscountAmt;
                                                            if (dr["Mlevel"] != DBNull.Value && !string.IsNullOrEmpty(dr["Mlevel"].ToString()) && Convert.ToInt32(dr["Mlevel"]) == 5)
                                                            {
                                                                DataRow[] rowCheck = dtItemSplitDetails.Select("ProcedureId='" + dr["ProcedureId"].ToString() + "'");
                                                                if (rowCheck.Length > 0)
                                                                {
                                                                    rowCheck[0]["DpaySplit"] = totalpay * dcDPAYPER / 100;
                                                                    rowCheck[0]["CollectablePerc"] = dcDPAYPER;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    dtItemSplitDetails.AcceptChanges();
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                            #endregion

                        }
                    }
                    catch (Exception ex)
                    {
                        //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in imgbtnPayment_Click Event", "");
                    }
                    finally
                    {

                    }
                }
                else
                {

                }
                return objPatientList;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in imgbtnPayment_Click Event", "");
            }
            finally
            {

            }
            return objPatientList;
        }

        private int DeleteTempBill(string SessionID)
        {
            DataHelper objDataHelper = new DataHelper(DEFAULTWORKSTATION, (int)Database.Master);
            int intTransaction = -1;
            try
            {
                List<IDbDataParameter> objIDbDataParameters = new List<IDbDataParameter>();
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@sessionid", SessionID, DbType.String, ParameterDirection.Input));
                objDataHelper.RunSP("Pr_DeleteTempBillDetails_MAPI", objIDbDataParameters.ToArray());
                if (intTransaction >= 0)
                    intTransaction = 1;
                else
                    intTransaction = 0;
            }
            catch (Exception ex)
            { 
                
            }
            finally
            {
                objDataHelper = null;
            }
            return intTransaction;
        }

        private void PackageTestOrdersFormDoctorAppointment(DataRow drotherOrders)
        {
            try
            {

                int ProcedureId = 0;
                if (drotherOrders["ServiceId"].ToString() == "4" || drotherOrders["ServiceId"].ToString() == "3")
                {
                    ProcedureId = Convert.ToInt32(drotherOrders["ProcedureId"]);
                }
                if (drotherOrders["ServiceId"].ToString() == "3")
                {
                    dtTestSpecimen = GetTestProfileSpecimen(ProcedureId, 13, 3, 0, 0, 0);
                    int intPrevId = 0;
                    foreach (DataRow drsample in dtTestSpecimen.Rows)
                    {
                        if (intPrevId != Convert.ToInt32(drsample["TestID"]))
                        {
                            string strname1 = "";
                            DataRow drTestProfile = dtTestsProfile.NewRow();
                            drTestProfile["TestId"] = drsample["TestID"];
                            drTestProfile["Name"] = drsample["Name"];
                            drTestProfile["SpecimenId"] = drsample["SpecimenID"];
                            drTestProfile["ProfileId"] = ProcedureId;// Convert.ToInt32(drNewRow["ProcedureId"]);
                            drTestProfile["SpecialiseId"] = drsample["SpID"];
                            drTestProfile["Contribution"] = (Convert.IsDBNull(drsample["UnitRate"]) ? 0 : drsample["UnitRate"]);
                            if (drsample["SpecimenName"] is DBNull)
                            {
                                drTestProfile["SpecimenName"] = "";
                            }
                            else
                            {
                                drTestProfile["SpecimenName"] = drsample["SpecimenName"];
                            }
                            dtTestsProfile.Rows.Add(drTestProfile);
                            intPrevId = Convert.ToInt32(drsample["TestID"]);
                        }
                    }
                }
                else
                {
                    dtTestSpecimen = FetchPackageItems(ProcedureId, 2, 0, Convert.ToInt32(strDefWorkstationId), 0);
                    if (TestProfile != null)
                    {
                        dtTestsProfile = TestProfile.Copy();
                        if (dtTestsProfile != null && dtTestsProfile.Rows.Count > 0)
                        {
                        }
                    }
                    else
                        dtTestsProfile = DTTestProfiles();

                    foreach (DataRow drsample in dtTestSpecimen.Rows)
                    {

                        DataRow drTestProfile = dtTestsProfile.NewRow();
                        if (drsample["ServiceTypeName"].ToString() == "Employee Service")
                        {
                            drTestProfile["Name"] = "";
                            drTestProfile["TestId"] = 0;
                        }
                        else
                        {
                            drTestProfile["Name"] = drsample["Name"];
                            drTestProfile["TestId"] = drsample["ServiceItemId"];
                        }

                        drTestProfile["SpecimenId"] = drsample["SpecimenID"];
                        drTestProfile["ProfileId"] = drsample["PackageId"];

                        drTestProfile["ServiceId"] = drsample["ServiceId"];
                        drTestProfile["SpecialiseId"] = drsample["SpecialiseId"];
                        drTestProfile["ServiceTypeId"] = drsample["ServiceTypeId"];
                        drTestProfile["Contribution"] = drsample["Contribution"];

                        if (drsample["SpecimenName"] is DBNull && drsample["ServiceTypeName"].ToString() == "Employee Service")
                        {
                            drTestProfile["SpecimenName"] = drsample["Specialisation"];
                            drTestProfile["SpecimenId"] = drsample["SpecialiseId"];
                        }
                        else
                        { drTestProfile["SpecimenName"] = drsample["SpecimenName"]; }

                        strValidatePrfileAndPHCItems = ConfigurationSettings.AppSettings["OpBillingValidationofProfileAndPHC"].ToString();
                        if (strValidatePrfileAndPHCItems.ToUpper() == "YES" && !string.IsNullOrEmpty(strValidatePrfileAndPHCItems))
                        {
                            string strname = "";
                            for (int i = 0; i < dtTestsProfile.Rows.Count; i++)
                            {
                                DataRow drtest = dtTestsProfile.NewRow();
                                drtest["TestID"] = dtTestsProfile.Rows[i]["TestID"];
                                drtest["SpecialiseId"] = dtTestsProfile.Rows[i]["SpecialiseId"];
                                if (drtest["TestID"].ToString() == drTestProfile["TestId"].ToString() && drtest["SpecialiseId"].ToString() == drTestProfile["SpecialiseId"].ToString())
                                {
                                    if (dtTestsProfile.Rows[i]["Name"].ToString() != string.Empty)
                                        strname += dtTestsProfile.Rows[i]["Name"].ToString() + "<br />";
                                    if (strname.Length > 0)
                                    {
                                        //System.Web.UI.ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','The Following item(s) Already Exists In The package or Profile:" + "<br />" + strname + "','OK','Information');", true);
                                        TestProfile = null;
                                        return;
                                    }
                                }
                            }
                        }
                        dtTestsProfile.Rows.Add(drTestProfile);
                    }
                }
                TestProfile = dtTestsProfile.Copy(); TestProfile.AcceptChanges();
            }
            finally
            {

            }
        }


        public DataTable FetchPackageItems(int intPackageId, int intTableId, int intUserId, int intWorkStationId, int intError)
        {
            try
            {
                objFOClient = new FrontOfficeServiceContractClient();
                return objFOClient.FetchPackageItems(intPackageId, intTableId, intUserId, intWorkStationId, intError).Tables["Package Items"];
            }

            finally
            {
                objFOClient.Close();
            }
        }

        private DataTable DTTestProfiles()
        {
            DataTable dtTestsProfile = new DataTable("TestProfiles");
            try
            {
                dtTestsProfile.Columns.Add("TestID", typeof(int));
                dtTestsProfile.Columns.Add("Name", typeof(string));
                dtTestsProfile.Columns.Add("SpecimenID", typeof(int));
                dtTestsProfile.Columns.Add("DeptId", typeof(int));
                dtTestsProfile.Columns.Add("SpecimenName", typeof(string));
                dtTestsProfile.Columns.Add("ProfileId", typeof(int));
                dtTestsProfile.Columns.Add("ServiceId", typeof(int));
                dtTestsProfile.Columns.Add("SpecialiseId", typeof(int));
                dtTestsProfile.Columns.Add("ServiceName", typeof(string));
                dtTestsProfile.Columns.Add("ServiceTypeId", typeof(int));
                dtTestsProfile.Columns.Add("Contribution", typeof(decimal));
                dtTestsProfile.Columns.Add("mLevel", typeof(decimal));
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in DTTestProfiles", "");
                return null;
            }
            return dtTestsProfile;
        }

        public DataTable GetTestProfileSpecimen(int intTestID, int intPriority, int intTableid, int intUserid, int intWorkstionid, int intError)
        {
            try
            {
                objFOClient = new FrontOfficeServiceContractClient();
                return objFOClient.GetTestProfileSpecimen(intTestID, intPriority, intTableid, intUserid, intWorkstionid, intError).Tables["Test Profile Specimen"];
            }

            finally
            {
                objFOClient.Close();
            }
        }

        private DataTable CreateDTTemp()
        {
            DTTemp = new DataTable("BillDetails");
            try
            {
                DataColumn workColDA = DTTemp.Columns.Add("ServiceName", typeof(String));
                DTTemp.Columns.Add("ServiceId", typeof(int));
                DTTemp.Columns.Add("Procedure", typeof(String));
                DTTemp.Columns.Add("ProcedureId", typeof(int));
                DTTemp.Columns.Add("Sample", typeof(String));
                DTTemp.Columns.Add("SampleId", typeof(int));
                DTTemp.Columns.Add("DeptId", typeof(int));//
                DTTemp.Columns.Add("DeptName", typeof(String));//
                DTTemp.Columns.Add("BedTypeId", typeof(int));//
                DTTemp.Columns.Add("BedTypeName", typeof(String));//
                DTTemp.Columns.Add("SpecialiseId", typeof(int));
                DTTemp.Columns.Add("SpecialiseName", typeof(String));//
                DTTemp.Columns.Add("OrderNo", typeof(int));//
                DTTemp.Columns.Add("OrderDate", typeof(String));//
                DTTemp.Columns.Add("IsGroup", typeof(Boolean));
                DTTemp.Columns.Add("Quantity", typeof(int));
                DTTemp.Columns.Add("Amount", typeof(decimal));
                DTTemp.Columns.Add("PPAY", typeof(decimal));
                DTTemp.Columns.Add("CPAY", typeof(decimal));
                DTTemp.Columns.Add("SPAY", typeof(decimal));
                DTTemp.Columns.Add("Seq", typeof(int));
                DTTemp.Columns.Add("ProfileId", typeof(int));
                DTTemp.Columns.Add("TariffId", typeof(int));//
                DTTemp.Columns.Add("MQTY", typeof(int));
                DTTemp.Columns.Add("SQTY", typeof(int));
                //added by anandm
                DataColumn dc = new DataColumn();
                dc.ColumnName = "Status";
                dc.DataType = typeof(Int32);
                dc.DefaultValue = 0;
                DTTemp.Columns.Add(dc);//added by anandm
                ////////////////////////////////////////////////////////
                DTTemp.Columns.Add("ScheduleId", typeof(int));
                DTTemp.Columns.Add("ProcId", typeof(int));
                DTTemp.Columns.Add("BasePrice", typeof(decimal));
                DTTemp.Columns.Add("BillablePrice", typeof(decimal));
                DTTemp.Columns.Add("EligiblePrice", typeof(decimal));
                DTTemp.Columns.Add("PAmount", typeof(decimal));
                DTTemp.Columns.Add("UnitRate", typeof(decimal));
                DTTemp.Columns.Add("OrderId", typeof(int));
                DTTemp.Columns.Add("Price", typeof(decimal));
                //Code Added from V3.2 for Deductable
                DTTemp.Columns.Add("DPAY", typeof(decimal));
                DTTemp.Columns.Add("DAmount", typeof(decimal));
                DTTemp.Columns.Add("ServiceTypeID", typeof(int));
                DTTemp.Columns.Add("PatientType", typeof(int));
                DTTemp.Columns.Add("OrderItemID", typeof(int));
                DTTemp.Columns.Add("Priority", typeof(int));
                DTTemp.Columns.Add("ClaimStatus", typeof(string));//nidhi 

                DTTemp.Columns.Add("VAT", typeof(decimal));
                DTTemp.Columns.Add("VATAmount", typeof(decimal));
                DTTemp.Columns.Add("CVAT", typeof(decimal));
                DTTemp.Columns.Add("OPPackAssignID", typeof(int));
                DTTemp.Columns.Add("PVAT", typeof(decimal));
                DTTemp.Columns.Add("UOMID", typeof(int));  //Code changes made for TFS ID :: 165983
                DTTemp.Columns.Add("BatchID", typeof(int));
                //end 
                return DTTemp;
            }
            catch (Exception ex)
            {
               // HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in CreateDTTemp", "");
                return null;
            }
        }

        private void GetGradeDocSpec(int PatientId, int DocId, int SpecId, bool validate, PatientBillList PatientBillList)
        {
            try
            {

                DataTable dtForCompany = new DataTable();
                dtForCompany.Columns.Add("CompanyName", typeof(string));
                dtForCompany.Columns.Add("companyId", typeof(string));
                string strGradeName = string.Empty, strCompanyName = string.Empty;
                int intCompanyId = 0, intGradeId = 0;

                int intUserId = Convert.ToInt32(strDefaultUserId);
                int intWorkstationid = Convert.ToInt32(strDefWorkstationId);
                int intError = 0;
                if (PatientId == 0)
                {
                    return;
                }
                DataSet DsGrades = new DataSet();
                DataRow[] DrGrades;

                DsGrades = GetGradeDoctorSpecializationWS(PatientId, DocId, SpecId, 2, "0", 0, 0, 0);
                //Added second filter to avoid loading company for cash
                if (DsGrades.Tables[0].Rows.Count > 0 && hdnrblbilltypeCredit == true)
                {
                    string strFilterGrades = "";
                    strFilterGrades = "ParentLetterId is not null and LoaBlocked=0 and PolicyExpiryDate>'" + DateTime.Now.ToString("dd-MMM-yyyy") + "' and todate >= '" + DateTime.Now.ToString("dd-MMM-yyyy") + "'";
                    DrGrades = DsGrades.Tables[0].Select(strFilterGrades, "billdate asc");
                    if (DrGrades.Length == 0)
                    {
                        strFilterGrades = "ParentLetterId is not null and LoaBlocked=0 and PolicyExpiryDate>'" + DateTime.Now.ToString("dd-MMM-yyyy") + "' ";
                        DrGrades = DsGrades.Tables[0].Select(strFilterGrades, "billdate asc");
                        if (DrGrades.Length > 0)
                        {
                            DateTime billDate = Convert.ToDateTime(DrGrades[DrGrades.Length - 1]["BillDate"]);
                            TimeSpan TS = DateTime.Now - billDate;
                            if (TS.Days < Convert.ToInt32(DrGrades[DrGrades.Length - 1]["FollowUPdays"]))
                            {
                                strCompanyID = DrGrades[DrGrades.Length - 1]["payerid"].ToString();
                                strGradeID = DrGrades[DrGrades.Length - 1]["GradeId"].ToString();
                                strTariffID = DrGrades[DrGrades.Length - 1]["TariffID"].ToString();
                                if (!string.IsNullOrEmpty(strCompanyID))
                                    intCompanyId = Convert.ToInt32(strCompanyID);
                                if (!string.IsNullOrEmpty(strGradeID))
                                    intGradeId = Convert.ToInt32(strGradeID);
                                strCompanyName = DrGrades[DrGrades.Length - 1]["payerName"].ToString();
                                strGradeName = DrGrades[DrGrades.Length - 1]["GradeName"].ToString();
                                if (validate == true)
                                {
                                    hdnrblbilltypeCredit = true;
                                    hdnrblbilltypecash = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        DataTable DtCompanyvalid = FetchADVProcedureDetails("FetchCompaniesAdv", "0", "Blocked=0 and companytype ='C' and validto>getdate() and CompanyID=" + Convert.ToInt32(DrGrades[DrGrades.Length - 1]["payerid"]) + " ", intUserId, intWorkstationid, 0);
                        if (DtCompanyvalid.Rows.Count == 0)
                        {
                            return;
                        }
                        strCompanyID = DrGrades[DrGrades.Length - 1]["payerid"].ToString();
                        strGradeID = DrGrades[DrGrades.Length - 1]["GradeId"].ToString();
                        strTariffID = DrGrades[DrGrades.Length - 1]["TariffID"].ToString();
                        if (!string.IsNullOrEmpty(strCompanyID.Trim()))
                            intCompanyId = Convert.ToInt32(strCompanyID);
                        if (!string.IsNullOrEmpty(strGradeID.Trim()))
                            intGradeId = Convert.ToInt32(strGradeID);
                        if (validate == true)
                        {
                            hdnrblbilltypeCredit = true;
                            hdnrblbilltypecash = false;
                        }
                    }
                    if (!string.IsNullOrEmpty(strCompanyID.ToString()))
                    {
                        if (Convert.ToInt32(strCompanyID) > 0)
                        {
                            DataRow drFC = dtForCompany.NewRow();
                            drFC["CompanyName"] = "";
                            drFC["CompanyID"] = strCompanyID;
                            dtForCompany.Rows.Add(drFC);
                            dtForCompany.AcceptChanges();
                        }
                    }
                }
                else
                {
                    DataTable dtPatientDetails = null;
                    dtPatientDetails = ViewStatedtPatientDetails.Copy();
                    if (dtPatientDetails == null || dtPatientDetails.Rows.Count == 0)
                    {
                        return;
                    }
                    if (dtPatientDetails.Rows.Count > 1 && DsPatient != null)
                    {
                        if (DsPatient.Tables.Count > 0 && DsPatient.Tables[0].Rows[0]["Gradeid"].ToString() != "0")
                        {
                            DataRow[] DrpatientDemo = dtPatientDetails.Select();
                            strCompanyID = (Convert.IsDBNull(DrpatientDemo[0]["CompanyID"]) ? 0 : Convert.ToInt32((DrpatientDemo[0]["CompanyID"]))).ToString();
                            strGradeID = (Convert.IsDBNull(DrpatientDemo[0]["GradeID"]) ? 0 : Convert.ToInt32((DrpatientDemo[0]["GradeID"]))).ToString();
                            if (dtCompanyContract != null)
                            {
                                dtCompanyContract = dtCompanyContract.Copy();
                                if (dtCompanyContract.Rows.Count > 0)
                                    strTariffID = dtCompanyContract.Select()[0]["TariffId"].ToString();
                            }
                        }
                        else
                        {
                            strCompanyID = "0";
                            hdnrblbilltypecash = true;
                            hdnrblbilltypeCredit = false;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(strCompanyID.ToString()) && hdnrblbilltypeCredit == true)
                {
                    if (Convert.ToInt32(strCompanyID) > 0)
                    {
                        int HospitalID = Convert.ToInt32(strDefaultHospitalId);
                        dsPayerforLOA678 = FetchHospitalCompanyDetails(Convert.ToInt32(strCompanyID), "c", "6,7,8", intUserId, intWorkstationid, 0, HospitalID);
                        //dsPayerforLOA678.Tables.Add(dsPayerforLOA1678.Tables[1].Copy());
                        //dsPayerforLOA678.Tables.Add(dsPayerforLOA1678.Tables[2].Copy());
                        //dsPayerforLOA678.Tables.Add(dsPayerforLOA1678.Tables[3].Copy());
                        //dsPayerforLOA678.AcceptChanges();

                        PredefinedDiscount = dsPayerforLOA678.Tables[1].Copy();
                        dtGradeValidation = dsPayerforLOA678.Tables[1].Copy();
                        dtCompanyContract = dsPayerforLOA678.Tables[0].Copy();

                        if (dsPayerforLOA678.Tables[0].Rows.Count > 0)
                        {
                            strTariffID = dsPayerforLOA678.Tables[0].Rows[0]["TariffID"].ToString();
                            if (!string.IsNullOrEmpty(strTariffID))
                                strTariffID = strTariffID;
                            if (dsPayerforLOA678.Tables[3].Rows.Count > 0)
                            {
                                HasDefaultLOA = false;
                                HasDefaultLOA = Convert.ToBoolean(dsPayerforLOA678.Tables[3].Rows[0]["IsDefaultLOA"]);
                                if (HasDefaultLOA)
                                    hdnHasDefaultLOA = "true";
                                else
                                    hdnHasDefaultLOA = "false";
                            }
                        }
                    }
                }
                SplzFollowupConfig(SpecId, PatientBillList);
            }
            finally
            {

            }
        }

        public void GetLOA(string strFilter)
        {
            try
            {
                DataTable dtLetter = CreateLOAGrid();
                DataTable dtApprovalLetter = new DataTable();
                int intType = 0;
                DataSet dsLOADetails = GetLOA_Deatils(1, 100, intType, strFilter, Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0);
                if (dsLOADetails != null && dsLOADetails.Tables[1].Rows.Count > 0)
                {
                    DataTable dtSort = dsLOADetails.Tables[1];
                    dtLOADetails = SortbyLetterID(dtSort);
                    LoaGrid = dtLOADetails;
                    dtLetter.Columns.Remove("SlNo");
                    dtLOADetails.Columns["FromDate"].ColumnName = "FrmDate";
                    dtLetter.Rows.Add(dtLOADetails);
                    dtLetter.AcceptChanges();
                    dtLetter.Columns["FrmDate"].ColumnName = "fromdate";

                    if (!string.IsNullOrEmpty(hdnLOAApprovalID))
                    {
                        DataRow[] drApprovalLetters = dtLOADetails.Select("LOAApprovalEntryID=" + hdnLOAApprovalID, "letterid desc");
                        if (drApprovalLetters != null && drApprovalLetters.Length > 0)
                        {
                            LOALetterID = drApprovalLetters[0]["Letterid"].ToString();
                            foreach (DataRow dr in dtLOADetails.Rows)
                            {
                                if (Convert.ToInt32(LOALetterID) == Convert.ToInt32(dr["Letterid"].ToString()))
                                {
                                    LOARB_CheckedChanged(dr["ParentLetterId"].ToString(), dr["Letterid"].ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        DataTable dtDefaultLOA = null;
                        dtDefaultLOA = dtLOADetails.Clone();
                        foreach (DataRow dr in dtLOADetails.Rows)
                        {
                            if (dr["LetterId"].ToString() == dr["ParentLetterId"].ToString())
                            {
                                dtDefaultLOA.ImportRow(dr);
                                dtDefaultLOA.AcceptChanges();
                            }
                        }
                        if (dtDefaultLOA != null && dtDefaultLOA.Rows.Count > 0)
                        {
                            int DefaultletterId = Convert.ToInt32(dtDefaultLOA.Rows[0]["LetterId"].ToString());
                            foreach (DataRow dr in dtLOADetails.Rows)
                            {
                                if (Convert.ToInt32(DefaultletterId) == Convert.ToInt32(dr["Letterid"].ToString()))
                                {
                                    LOARB_CheckedChanged(dr["ParentLetterId"].ToString(), dr["Letterid"].ToString());
                                }
                            }
                        }
                    }
                }
                else
                {
                    FillEmptyLOAGrid();
                }
            }
            finally
            {

            }
        }

        private void AssignLOAConfigDetails(DataTable dtPayer, int PatientType, int PatientId, int CompanyId, int GradeId, int SplId, PatientBillList PatientBillList)
        {
            try
            {
                int intUserid = 0, intWorkstationid = 0;
                intUserid = Convert.ToInt32(strDefaultUserId); intWorkstationid = Convert.ToInt32(strDefWorkstationId);
                hdnLOAfollowupLimit = "0"; hdnLOAfollowupDays = "0";
                if (dtPayer.Rows.Count == 0) { return; }
                DataTable dtCompany = dtPayer.Copy();
                IntLetterIddatechanged = 0; hdnIntLetterIddatechanged = "0";

                if (Convert.ToBoolean(dtCompany.Rows[0]["ISConsultationSpecific"].ToString()) == true && Convert.ToBoolean(dtCompany.Rows[0]["IsDefaultLOA"].ToString()) == true)
                {
                    int LetterId = 0;
                    string strFilterr = "";
                    strFilterr = " Status=0 and Blocked=0 And LetterId = ParentLetterId and PatientType =" + PatientType + " and todate >=convert(varchar(12),getdate(),106) and  patientid=" + PatientId + " and SpecialisationID = " + SplId + " and PayerId = " + CompanyId + " and Gradeid = " + GradeId;
                    DataTable dtLOA = FetchADVProcedureDetails("FetchPatientLettersAdv", "0", strFilterr, intUserid, intWorkstationid, 0);
                    if (dtLOA.Rows.Count > 0)
                    {
                        LetterId = Convert.ToInt32(dtLOA.Rows[dtLOA.Rows.Count - 1]["LetterID"].ToString());
                        IntLetterIddatechanged = LetterId;
                        hdnIntLetterIddatechanged = Convert.ToString(IntLetterIddatechanged);
                    }
                    if (LetterId > 0)
                    {
                        DataTable dtConfig = FetchLetterDetails(LetterId, 0, "9", intUserid, intWorkstationid, 0).Tables[0].Copy();
                        if (dtConfig.Rows.Count > 0)
                        {
                            LOAfollowupLimit = Convert.IsDBNull(dtConfig.Rows[0]["NoOfFollwUps"]) ? 0 : Convert.ToInt32(dtConfig.Rows[0]["NoOfFollwUps"]);
                            hdnLOAfollowupLimit = Convert.ToString(LOAfollowupLimit);
                            // if (hdnLOAfollowupLimit.ToString() == null)
                            if (string.IsNullOrEmpty(hdnLOAfollowupLimit))
                            {

                                if (!string.IsNullOrEmpty(Followuplimit))
                                {
                                    hdnLOAfollowupLimit = Followuplimit.ToString();
                                }
                            }

                            System.TimeSpan ts = Convert.ToDateTime(dtConfig.Rows[0]["ToDate"].ToString()) - Convert.ToDateTime(dtConfig.Rows[0]["FromDate"].ToString());
                            LOAfollowupDays = Convert.ToInt32(ts.Days.ToString());
                            LOAfollowupDays++;
                            hdnLOAfollowupDays = Convert.ToString(LOAfollowupDays);
                            if (CONSFollowupDAYS.ToString() != "")
                                hdnLOAfollowupDays = CONSFollowupDAYS.ToString();
                        }
                        else
                        {
                            LOAfollowupLimit = 0; hdnLOAfollowupLimit = "0";
                            LOAfollowupDays = 0; hdnLOAfollowupDays = "0";
                        }
                    }

                    SplzFollowupConfig(SplId, PatientBillList);
                    if (!string.IsNullOrEmpty(SPLFOLLOWUPLMT))
                        hdnLOAfollowupLimit = SPLFOLLOWUPLMT.ToString();
                    if (!string.IsNullOrEmpty(SPLFOLLOWUPDAYS))
                        hdnLOAfollowupDays = SPLFOLLOWUPDAYS.ToString();

                }

            }
            finally
            {

            }
        }

        public DataSet FetchHospitalCompanyDetails(int CompanyID, string CompanyType, string Tables, int intUserID, int intWorkStationID, int intError, int Hospitalid)
        {
            objFOClient = new FrontOfficeServiceContractClient();
            try
            {
                return objFOClient.FetchHospitalCompanyDetails(CompanyID, CompanyType, Tables, intUserID, intWorkStationID, intError, Hospitalid);
            }
            finally
            {
                objFOClient.Close();
            }
        }

        private string CheckLOAConsultationFollowupConfig(int patientId, int SplId, int DocId, int OrderTypeId, string OrderType, int FollowupDays, int FollowupLimit, PatientBillList PatientBillList)
        {
            try
            {
                hdnDelConfirm = "0";
                bool CheckType;
                int intUserid = Convert.ToInt32(strDefaultUserId), intWorkstationid = Convert.ToInt32(strDefWorkstationId), intError = 0;
                int doctorid = Convert.ToInt32(hdnDocID);
                DataTable dtConfollowupD = FetchFollowupdays(doctorid, SplId, intUserid, intWorkstationid, intError, patientId).Tables[0];
                dtConfollowup = dtConfollowupD;
                CheckType = Convert.ToBoolean(Convert.IsDBNull(dtConfollowup.Rows[1]["AllDoctors"]) ? 1 : dtConfollowup.Rows[1]["AllDoctors"]);
                string TypeMapi = string.Empty; string FilterMAPI, FilterMAPI2 = string.Empty; string OrderMAPI = string.Empty;
                if (FollowupDays > 0 || FollowupLimit > 0)
                {
                    ConsBaseOrderType = 0;
                    BaseOrderType = Convert.ToInt32(FetchDefaultTariff("ORD_FOLLOWUP_DAYS"));
                    //ViewState["BaseOrderType"] = BaseOrderType;
                    ConsBaseOrderType = BaseOrderType;
                    string BaseOrderName = "";
                    DataTable dtOrdertype = FetchADVProcedureDetails("FetchOrderTypesAdv", "1", " ServiceTypeId=4 and Blocked=0 and OrderTypeID= " + BaseOrderType, intUserid, intWorkstationid, 0);
                    if (dtOrdertype.Rows.Count > 0)
                    {
                        DataRow[] dr = dtOrdertype.Select("");
                        BaseOrderName = Convert.ToString(dr[0]["OrderType"]);
                    }
                    DataTable dtItems = new DataTable();
                    if (FollowupDays > 0 && FollowupLimit > 0)
                    {
                        if (hdnrblbilltypeCredit == true)
                        {
                            DataTable dtSpecCofig = HISCONFIG.Copy();
                            DataRow[] drtempFollowup = dtSpecCofig.Select("Parameter ='EMRFOLLOWUP' AND  HospitalId = " + strDefaultHospitalId + " ");
                            DataRow[] drtemp = dtSpecCofig.Select("Parameter ='DEPT_EMERGENCY'  AND  HospitalId = " + strDefaultHospitalId + " ");
                            string strsplzconfig = string.Empty;
                            if (drtemp.Length > 0)
                                strsplzconfig = drtemp[0]["Value"].ToString();
                            if (drtempFollowup.Length > 0 & !string.IsNullOrEmpty(hdnDocHospDeptId) && strsplzconfig == hdnDocHospDeptId)
                            {
                                if (drtempFollowup[0]["Value"].ToString() == "0")
                                {
                                    if (drtemp.Length > 0)
                                    {
                                        //dtItems = CheckType ? FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,monitorID,OrderSlNO,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + SplId + " and BillType='CR' and PatientId=" + patientId + " and datediff(minute,Orderdate,getdate()) <=1440 and HospDeptID not in (" + strsplzconfig + ")", "", intUserid, intWorkstationid, 0, false).Tables[0] : FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,MonitorID,OrderSlNO,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + SplId + "  and BillType='CR' and PatientId=" + patientId + " and servicedocid=" + DocId + " and datediff(day,Orderdate,getdate())<" + FollowupDays, "", intUserid, intWorkstationid, 0, false).Tables[0];
                                        TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,monitorID,OrderSlNO,COMPANYID,GRADEID";
                                        FilterMAPI = "serviceid=2 and specialiseId=" + SplId + " and BillType='CR' and PatientId=" + patientId + " and datediff(minute,Orderdate,getdate()) <=1440 and HospDeptID not in (" + strsplzconfig + ")";
                                        FilterMAPI2 = "serviceid=2 and specialiseId=" + SplId + "  and BillType='CR' and PatientId=" + patientId + " and servicedocid=" + DocId + " and datediff(day,Orderdate,getdate())<" + FollowupDays;
                                        OrderMAPI = "";
                                        //dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];
                                        dtItems = CheckType ? FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0] : FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI2, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];
                                    }
                                }
                                else
                                {
                                    //dtItems = CheckType ? FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,monitorID,OrderSlNO,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + SplId + " and BillType='CR' and PatientId=" + patientId + " and datediff(minute,Orderdate,getdate())<=1440", "", intUserid, intWorkstationid, 0, false).Tables[0] : FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,MonitorID,OrderSlNO,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + SplId + "  and BillType='CR' and PatientId=" + patientId + " and servicedocid=" + DocId + " and datediff(day,Orderdate,getdate())<" + FollowupDays, "", intUserid, intWorkstationid, 0, false).Tables[0];
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,monitorID,OrderSlNO,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + SplId + " and BillType='CR' and PatientId=" + patientId + " and datediff(minute,Orderdate,getdate())<=1440";
                                    FilterMAPI2 = "serviceid=2 and specialiseId=" + SplId + "  and BillType='CR' and PatientId=" + patientId + " and servicedocid=" + DocId + " and datediff(day,Orderdate,getdate())<" + FollowupDays;
                                    OrderMAPI = "";
                                    dtItems = CheckType ? FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0] : FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI2, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];
                                }

                            }
                            else
                            {
                                //
                                //dtItems = CheckType ? FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,monitorID,OrderSlNO,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + SplId + " and BillType='CR' and PatientId=" + patientId + " and datediff(day,Orderdate,getdate())<" + FollowupDays, "", intUserid, intWorkstationid, 0, false).Tables[0] : FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,MonitorID,OrderSlNO,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + SplId + "  and BillType='CR' and PatientId=" + patientId + " and servicedocid=" + DocId + " and datediff(day,Orderdate,getdate())<" + FollowupDays, "", intUserid, intWorkstationid, 0, false).Tables[0];
                                TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,monitorID,OrderSlNO,COMPANYID,GRADEID";
                                FilterMAPI = "serviceid=2 and specialiseId=" + SplId + " and BillType='CR' and PatientId=" + patientId + " and datediff(day,Orderdate,getdate())<" + FollowupDays;
                                FilterMAPI2 = "serviceid=2 and specialiseId=" + SplId + "  and BillType='CR' and PatientId=" + patientId + " and servicedocid=" + DocId + " and datediff(day,Orderdate,getdate())<" + FollowupDays;
                                OrderMAPI = "";
                                dtItems = CheckType ? FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0] : FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI2, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                            }

                            if (dtItems.Rows.Count > 0)
                            {
                                if (string.IsNullOrEmpty(dtItems.Rows[0]["monitorID"].ToString()))
                                {
                                    string strbillcancel = "Please cancel the previous bill: <b> " + dtItems.Rows[0]["Orderslno"].ToString() + "</b> </br> as patient not visited Doctor and generate the new bill.";
                                    // ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strbillcancel + "','OK','Information');", true);
                                    return strbillcancel;
                                }
                            }

                        }
                        if (hdnIntLetterIddatechanged == "0" && SPLFOLLOWUPLMT.ToString() == "")
                        {
                            dtItems.Rows.Clear();
                        }
                    }
                    else
                    {
                        if (FollowupDays == 0 && FollowupLimit > 0)
                        {
                            //dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + SplId + " and PatientId=" + patientId, "", intUserid, intWorkstationid, 0, false).Tables[0];
                            TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,COMPANYID,GRADEID";
                            FilterMAPI = "serviceid=2 and specialiseId=" + SplId + " and PatientId=" + patientId;
                            OrderMAPI = "";
                            dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                        }
                        else if (FollowupDays > 0 && FollowupLimit == 0)
                        {
                            //dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + SplId + " and PatientId=" + patientId + " and datediff(day,Orderdate,getdate())<" + FollowupDays, "", intUserid, intWorkstationid, 0, false).Tables[0];
                            TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,COMPANYID,GRADEID";
                            FilterMAPI = "serviceid=2 and specialiseId=" + SplId + " and PatientId=" + patientId + " and datediff(day,Orderdate,getdate())<" + FollowupDays;
                            OrderMAPI = "";
                            dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                        }

                    }

                    if (dtItems.Rows.Count > 0)
                    {
                        if ((!string.IsNullOrEmpty(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["OPConsultationSameDayDepartment"]))))
                        {
                            if (System.Configuration.ConfigurationSettings.AppSettings["OPConsultationSameDayDepartment"].ToString().ToUpper() == "YES")
                            {
                                if (hdnrblbilltypeCredit == true)
                                {
                                    if (dtItems != null && dtItems.Rows.Count > 0)
                                    {
                                        DateTime dt1 = Convert.ToDateTime(dtItems.Rows[dtItems.Rows.Count - 1]["ORDERDATE"].ToString());
                                        bool dt3;
                                        if (dt1.Date == DateTime.Now.Date)
                                        {
                                            dt3 = true;
                                        }
                                        else dt3 = false;

                                        // dt3 = dt3;
                                    }
                                    else
                                        dt3 = false;
                                }
                                else
                                    dt3 = false;
                            }
                            else
                                dt3 = false;
                        }
                    }

                    if (FollowupLimit > 0)
                    {
                        DataTable dttemp = dtItems.Clone();
                        DataRow[] dr1 = dtItems.Select("OrderTypeId=" + BaseOrderType, "Orderdate desc");
                        if (dr1.Length >= FollowupLimit)
                        {
                            for (int ict = 0; ict < dr1.Length; ict++)
                            {
                                int BillId1 = Convert.ToInt32(dr1[ict]["BillId"]);
                                int BillId2 = Convert.ToInt32(dr1[(ict + (FollowupLimit - 1))]["BillId"]);

                                DataRow[] drtemp = dtItems.Select(" billId>=" + BillId2 + " and billId<" + BillId1 + " and Ordertypeid <> " + BaseOrderType, "");
                                if (drtemp.Length > 0)
                                {
                                    DataRow[] dr2 = dtItems.Select(" billId>" + BillId2, "");
                                    for (int ictr = 0; ictr < dr2.Length; ictr++)
                                    {
                                        dttemp.ImportRow(dr2[ictr]);

                                    }
                                    break;
                                }
                                else
                                {
                                    DataRow[] dr2 = dtItems.Select(" billId>" + BillId1, "");
                                    for (int ictr = 0; ictr < dr2.Length; ictr++)
                                    {
                                        dttemp.ImportRow(dr2[ictr]);

                                    }
                                    break;
                                }
                            }
                            dttemp.AcceptChanges();
                            dtItems.Rows.Clear();
                            dtItems = dttemp.Copy();


                        }
                    }

                    if (dtItems.Rows.Count > 0)
                    {
                        if (FollowupDays > 0 && FollowupLimit > 0)
                        {
                            if (FollowupDays >= FollowupLimit)
                            { return CheckforLimits(FollowupLimit, CheckType, dtItems.Copy(), DocId, OrderTypeId, OrderType, BaseOrderType, BaseOrderName); }
                            else if (FollowupDays < FollowupLimit)
                            { return CheckforDays(FollowupDays, CheckType, dtItems.Copy(), DocId, OrderTypeId, OrderType, BaseOrderType, BaseOrderName); }
                        }
                        else
                        {
                            if (FollowupDays == 0 && FollowupLimit > 0)
                            { return CheckforLimits(FollowupLimit, CheckType, dtItems.Copy(), DocId, OrderTypeId, OrderType, BaseOrderType, BaseOrderName); }
                            //else if (SPLFOLLOWUPLMT != null) 
                            else if (!string.IsNullOrEmpty(SPLFOLLOWUPLMT))
                            {
                                if (FollowupDays > 0 && FollowupLimit == 0 && Convert.ToInt16(SPLFOLLOWUPLMT.ToString()) > 0)
                                { return CheckforDays(FollowupDays, CheckType, dtItems.Copy(), DocId, OrderTypeId, OrderType, BaseOrderType, BaseOrderName); }
                            }
                        }

                    }
                    else
                    {
                        if (OrderTypeId == BaseOrderType)
                        {
                            if (DischargeFollowUp())
                            {
                                if (dtConfollowup.Rows.Count > 0)
                                {
                                    if (!string.IsNullOrEmpty(dtConfollowup.Rows[1]["DischargeFollowUpDays"].ToString()))
                                        FollowupDays = Convert.ToInt32(dtConfollowup.Rows[1]["DischargeFollowUpDays"].ToString());
                                    if (!string.IsNullOrEmpty(dtConfollowup.Rows[1]["DischargeFollowUpLimit"].ToString()))
                                        FollowupLimit = Convert.ToInt32(dtConfollowup.Rows[1]["DischargeFollowUpLimit"].ToString());
                                }
                                if (FollowupDays > 0 && FollowupLimit > 0)
                                {
                                    if (FollowupDays >= FollowupLimit)
                                    {
                                        return CheckforLimits(FollowupLimit, CheckType, dtItems.Copy(), DocId, OrderTypeId, OrderType, BaseOrderType, BaseOrderName);
                                    }
                                    else if (FollowupDays < FollowupLimit)
                                    {
                                        return CheckforDays(FollowupDays, CheckType, dtItems.Copy(), DocId, OrderTypeId, OrderType, BaseOrderType, BaseOrderName);
                                    }
                                }
                            }
                            else
                            {
                                StringBuilder strMsg;
                                if (CheckType)
                                { //MessageBox.Show(resources1.GetString("frmOPBillingNew.cs.MSG93"), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information); 
                                    strMsg = new StringBuilder();
                                    strMsg.Append("FollowUp cannot be possible for the selected speciality</br> ");
                                    strMsg.Append("As the patient doesnot have previous Visits to this Speciality");
                                    //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                }
                                else
                                { //MessageBox.Show(resources1.GetString("frmOPBillingNew.cs.MSG94"), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information); 
                                    strMsg = new StringBuilder();
                                    strMsg.Append("FollowUp cannot be possible for the selected doctor</br> ");
                                    strMsg.Append("As the patient doesnot have previous visits to this doctor");
                                    //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                }
                            }
                            return ""; //false;

                        }
                        else
                        {
                            if (DischargeFollowUp())
                            {
                                if (dtConfollowup.Rows.Count > 0)
                                {
                                    if (!string.IsNullOrEmpty(dtConfollowup.Rows[1]["DischargeFollowUpDays"].ToString()))
                                        FollowupDays = Convert.ToInt32(dtConfollowup.Rows[1]["DischargeFollowUpDays"].ToString());
                                    if (!string.IsNullOrEmpty(dtConfollowup.Rows[1]["DischargeFollowUpLimit"].ToString()))
                                        FollowupLimit = Convert.ToInt32(dtConfollowup.Rows[1]["DischargeFollowUpLimit"].ToString());
                                }
                                if (FollowupDays > 0 && FollowupLimit > 0)
                                {
                                    if (FollowupDays >= FollowupLimit)
                                    { return CheckforLimits(FollowupLimit, CheckType, dtItems.Copy(), DocId, OrderTypeId, OrderType, BaseOrderType, BaseOrderName); }
                                    else if (FollowupDays < FollowupLimit)
                                    { return CheckforDays(FollowupDays, CheckType, dtItems.Copy(), DocId, OrderTypeId, OrderType, BaseOrderType, BaseOrderName); }
                                }
                            }
                            else
                            {
                                ViewStateLetterid = null;
                                VSParentLetterid = null;
                                return OrderTypeId + "/" + OrderType;
                            }
                        }

                    }
                    return OrderTypeId + "/" + OrderType;
                }
                else
                { return OrderTypeId + "/" + OrderType; }// true;	}

            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in ValidateInsurancCard", "");
                return string.Empty;
            }
            finally
            {

            }
        }

        private string CheckConsultationFollowupConfig(int patientId, int SplId, int DocId, int OrderTypeId, string OrderType, PatientBillList PatientBillList)
        {

            try
            {
                MaxStr = string.Empty;
                bool HasConsData = false;
                int FollowupDays = 0;
                int FollowupLimit = 0;
                // bool CheckType;
                int intUserid = 0; int intworkStationId = 0;
                string TypeMapi, FilterMAPI, OrderMAPI = string.Empty;
                intUserid = Convert.ToInt32(strDefaultUserId); intworkStationId = Convert.ToInt32(strDefWorkstationId);
                DataTable dtItemsCon = new DataTable();

                if (DoctorsConsultations != null)
                {
                    //dtItemsCon = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + SplId + " and CompanyID='" + strCompanyID + "' and PatientId=" + patientId + " and Convert(date, Orderdate)=Convert(date, getdate())", "", intUserid, intworkStationId, 0, false).Tables[0];

                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,COMPANYID,GRADEID";
                    FilterMAPI = "serviceid=2 and specialiseId=" + SplId + " and CompanyID='" + strCompanyID + "' and PatientId=" + patientId + " and Convert(date, Orderdate)=Convert(date, getdate())";
                    OrderMAPI = "";
                    dtItemsCon = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                    if ((!string.IsNullOrEmpty(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["OPConsultationSameDayDepartment"]))))
                    {
                        if (System.Configuration.ConfigurationSettings.AppSettings["OPConsultationSameDayDepartment"].ToString().ToUpper() == "YES")
                        {
                            if (hdnrblbilltypeCredit == true)
                            {
                                if (dtItemsCon != null && dtItemsCon.Rows.Count > 0)
                                {
                                    DateTime dt1 = Convert.ToDateTime(dtItemsCon.Rows[0]["ORDERDATE"].ToString());
                                    bool dt3;
                                    if (dt1.Date == DateTime.Now.Date)
                                    {
                                        dt3 = true;
                                    }
                                    else dt3 = false;

                                    //ViewState["dt3"] = dt3;
                                }
                                else
                                    dt3 = false;
                            }
                            else
                                dt3 = false;
                        }
                        else
                            dt3 = false;
                    }
                    var results = from myRow in dtItemsCon.AsEnumerable()
                                  where myRow.Field<string>("ORDERTYPE").ToUpper().Contains("CONSULTATION")
                                  select myRow;
                    if (DoctorsConsultations.ToString().Trim() != "0" && DoctorsConsultations.ToString() != string.Empty)
                    {


                        if (Convert.ToInt32(DoctorsConsultations) <= System.Linq.Enumerable.Count(results))
                        {
                            string strMax = "Patient consultations limit exceeded for this company";
                            MaxStr = strMax;
                            //ScriptManager.RegisterStartupScript(this, typeof(Page), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMax + "','OK','Information');", true);
                            return strMax;
                        }
                    }
                }
                DataSet dsMaxConsult = FetchMaxconsultdays(Convert.ToInt32(Doctorid.ToString()), "1", intUserid, intworkStationId, 0);
                if (dsMaxConsult.Tables[0].Rows.Count > 0)
                {
                    if (dsMaxConsult.Tables[0].Rows[0][1].ToString() != null && dsMaxConsult.Tables[0].Rows[0][1].ToString() != "" && Convert.ToInt32(dsMaxConsult.Tables[0].Rows[0][1].ToString()) > 0)
                    {
                        int daycount = Convert.ToInt32(dsMaxConsult.Tables[0].Rows[0][0].ToString());
                        int MaxCount = Convert.ToInt32(dsMaxConsult.Tables[0].Rows[0][1].ToString());
                        if (daycount > MaxCount - 1)
                        {
                            Maxconsult = "consult";
                            string strMax = "Max Consultation limit for this doctor exceeded. <br/> Max Consultations: " + MaxCount + " <br/> Billed count: " + daycount + "";
                            MaxStr = strMax;
                            // ScriptManager.RegisterStartupScript(this, typeof(Page), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMax + "','OK','Information');", true);
                            //    System.Web.UI.ScriptManager.RegisterStartupScript(this, typeof(Page), this.Title, "ShowMsgBox('" + this.Title + "','" + strMax + "','OK','Information');", true);
                            //  Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "OP", strMax, true);


                        }
                    }
                }
                if (DoctorAvailable.ToString() == "NO")
                {
                    string strConsultMsg = "";
                    if (GetDoctorConfiguration())
                    {
                        return "";
                    }
                    MaxStr = strConsultMsg;
                }
                int tsDays = 0, fDays = 0, fLimit = 0;
                DataTable dtConfollowup = FetchFollowupdays(Convert.ToInt32(Doctorid.ToString()), Convert.ToInt32(specilaize.ToString()), intUserid, intworkStationId, 0, patientId).Tables[0];

                dtConfollowupM = dtConfollowup;
                dtConfollowupM.AcceptChanges();
                CheckType = Convert.ToBoolean(Convert.IsDBNull(dtConfollowup.Rows[1]["AllDoctors"]) ? 1 : dtConfollowup.Rows[1]["AllDoctors"]);
                if (dtConfollowup.Rows.Count > 0)
                {
                    if (dtConfollowup.Rows[0]["Ddate"].ToString() != null && dtConfollowup.Rows[0]["Ddate"].ToString() != "")
                    {
                        tsDays = (DateTime.Now - Convert.ToDateTime(dtConfollowup.Rows[0]["Ddate"]).Date).Days;
                    }
                    if (dtConfollowup.Rows[1]["DischargeFollowupDays"].ToString() != null && dtConfollowup.Rows[1]["DischargeFollowupDays"].ToString() != "")
                    {
                        fDays = Convert.ToInt32(dtConfollowup.Rows[1]["DischargeFollowupDays"].ToString());
                    }
                    if (dtConfollowup.Rows[1]["DischargeFollowuplimit"].ToString() != null && dtConfollowup.Rows[1]["DischargeFollowuplimit"].ToString() != "")
                    {
                        fLimit = Convert.ToInt32(dtConfollowup.Rows[1]["DischargeFollowuplimit"].ToString());
                    }
                    if (dtConfollowup.Rows[0]["Ddate"].ToString() != "" && dtConfollowup.Rows[0]["Ddate"].ToString() != null && tsDays <= fDays && fLimit > 0)
                    {
                        if (dtConfollowup.Rows[1]["DischargeFollowupDays"].ToString() != "" && dtConfollowup.Rows[1]["DischargeFollowupDays"].ToString() != null)
                        {
                            FollowupDaysM = Convert.ToInt32(dtConfollowup.Rows[1]["DischargeFollowupDays"].ToString());
                            //Followupdays = FollowupDays;
                            TimeSpan ts = DateTime.Now - Convert.ToDateTime(dtConfollowup.Rows[0]["Ddate"]).Date;
                            int days = ts.Days;
                            if (days < FollowupDays)
                            {
                                FollowupDays = Convert.ToInt32(dtConfollowup.Rows[1]["DischargeFollowupDays"].ToString());

                                if (dtConfollowup.Rows[1]["DischargeFollowuplimit"].ToString() != null && dtConfollowup.Rows[1]["DischargeFollowuplimit"].ToString() != "")
                                {
                                    FollowupLimitM = Convert.ToInt32(dtConfollowup.Rows[1]["DischargeFollowuplimit"].ToString());
                                    FollowupLimit = Convert.ToInt32(dtConfollowup.Rows[1]["DischargeFollowuplimit"].ToString());

                                }
                                if (hdnrblbilltypeCredit == true & ViewStateLetterid != null & VSParentLetterid != null)
                                {
                                    if (ViewStateLetterid.ToString() == VSParentLetterid.ToString())
                                    { ViewStateLetterid = null; VSParentLetterid = null; }
                                }

                                FollowupDaysM = FollowupDays;
                                FollowupLimitM = FollowupLimit;
                            }
                        }
                    }
                    else if (dtConfollowup.Rows[0]["ConFollowupDays"].ToString() != "" && dtConfollowup.Rows[0]["ConFollowupDays"].ToString() != null && dtConfollowup.Rows[0]["Ddate"].ToString() != "" && dtConfollowup.Rows[0]["Ddate"].ToString() != null)
                    {
                        FollowupDays = Convert.ToInt32(dtConfollowup.Rows[0]["ConFollowupDays"].ToString());

                        FollowupDaysM = FollowupDays;

                        if (dtConfollowup.Rows[0]["Followuplimit"].ToString() != null && dtConfollowup.Rows[0]["Followuplimit"].ToString() != "")
                            FollowupLimit = Convert.ToInt32(dtConfollowup.Rows[0]["Followuplimit"].ToString());
                        FollowupLimitM = FollowupLimit;
                        CheckType = Convert.ToBoolean(0);
                        if (FollowupDays == 0 && FollowupLimit == 0)
                        {
                            if (dtConfollowup.Rows.Count > 1)
                            {
                                if (dtConfollowup.Rows[1]["ConFollowupDays"].ToString() != "" && dtConfollowup.Rows[1]["ConFollowupDays"].ToString() != null)
                                {
                                    FollowupDays = Convert.ToInt32(dtConfollowup.Rows[1]["ConFollowupDays"].ToString());
                                    FollowupDaysM = FollowupDays;
                                    //Manipal Development CR#172 added by venkat-- end

                                    if (dtConfollowup.Rows[1]["Followuplimit"].ToString() != null && dtConfollowup.Rows[1]["Followuplimit"].ToString() != "")
                                        FollowupLimit = Convert.ToInt32(dtConfollowup.Rows[1]["Followuplimit"].ToString());

                                    //Manipal Development CR#172 added by venkat-- start
                                    FollowupLimitM = FollowupLimit;
                                    //Manipal Development CR#172 added by venkat-- end

                                    CheckType = Convert.ToBoolean(Convert.IsDBNull(dtConfollowup.Rows[1]["AllDoctors"]) ? 1 : dtConfollowup.Rows[1]["AllDoctors"]);
                                }
                            }
                        }
                    }
                    else if (dtConfollowup.Rows[1]["ConFollowupDays"].ToString() != "" && dtConfollowup.Rows[1]["ConFollowupDays"].ToString() != null)
                    {
                        FollowupDays = Convert.ToInt32(dtConfollowup.Rows[1]["ConFollowupDays"].ToString());

                        if (dtConfollowup.Rows[1]["Followuplimit"].ToString() != null && dtConfollowup.Rows[1]["Followuplimit"].ToString() != "")
                            FollowupLimit = Convert.ToInt32(dtConfollowup.Rows[1]["Followuplimit"].ToString());

                        if (hdnrblbilltypeCredit == true & ViewStateLetterid == null)
                        { FollowupLimit = 0; FollowupDays = 0; }

                        FollowupLimitM = FollowupLimit;
                        FollowupDaysM = FollowupDays;

                        CheckType = Convert.ToBoolean(Convert.IsDBNull(dtConfollowup.Rows[1]["AllDoctors"]) ? 1 : dtConfollowup.Rows[1]["AllDoctors"]);
                    }
                }
                SplzFollowupConfig(SplId, PatientBillList);
                if (SPLFOLLOWUPLMT != null)
                {
                    if (!string.IsNullOrEmpty(SPLFOLLOWUPLMT.ToString()))
                        FollowupLimit = Convert.ToInt16(SPLFOLLOWUPLMT);
                }

                if (SPLFOLLOWUPDAYS != null)
                {
                    if (!string.IsNullOrEmpty(SPLFOLLOWUPDAYS.ToString()))
                        FollowupDays = Convert.ToInt16(SPLFOLLOWUPDAYS);
                }
                if (FollowupDays >= 0 && patientId > 0)
                {

                    if (FollowupDays > 0 || FollowupLimit >= 0)
                    {
                        int BaseOrderType = 0;
                        ConsBaseOrderType = 0;
                        BaseOrderType = Convert.ToInt32(FetchDefaultTariff("ORD_FOLLOWUP_DAYS"));
                        BaseOrderTypeM = BaseOrderType;
                        ConsBaseOrderType = BaseOrderType;

                        string BaseOrderName = "";
                        DataTable dtOrdertype = FetchADVProcedureDetails("FetchOrderTypesAdv", "1", " ServiceTypeId=4 and Blocked=0 and OrderTypeID= " + BaseOrderType, intUserid, intworkStationId, 0);
                        if (dtOrdertype.Rows.Count > 0)
                        {
                            DataRow[] dr = dtOrdertype.Select("");
                            BaseOrderName = Convert.ToString(dr[0]["OrderType"]);
                        }
                        DataTable dtItems = new DataTable();

                        if (FollowupDays > 0 && FollowupLimit > 0)
                        {
                            #region samedoctor specilisation                           
                            if (hdnrblbilltypecash == true)
                            {
                                if (CheckType)
                                {
                                    //  dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,MONITORID,ORDERSLNO,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + SplId + " and BillType='CS' and PatientId=" + patientId + " and datediff(day,Orderdate,getdate())<" + FollowupDays, "", intUserId, intworkStationId, 0, false).Tables[0];
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,MONITORID,ORDERSLNO,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + SplId + " and BillType='CS' and PatientId=" + patientId + " and datediff(day,Orderdate,getdate())<" + FollowupDays;
                                    OrderMAPI = "";
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                }

                                else
                                {
                                    //  dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,MONITORID,ORDERSLNO,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + SplId + " and BillType='CS' and PatientId=" + patientId + " and servicedocid=" + DocId + " and datediff(day,Orderdate,getdate())<" + FollowupDays, "", intUserId, intworkStationId, 0, false).Tables[0];
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,MONITORID,ORDERSLNO,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + SplId + " and BillType='CS' and PatientId=" + patientId + " and servicedocid=" + DocId + " and datediff(day,Orderdate,getdate())<" + FollowupDays;
                                    OrderMAPI = "";
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                }

                                if (dtItems.Rows.Count > 0)
                                {
                                    if (string.IsNullOrEmpty(dtItems.Rows[0]["monitorID"].ToString()))
                                    {
                                        string strbillcancel = "Please cancel the previous bill: <b> " + dtItems.Rows[0]["Orderslno"].ToString() + "</b> </br> as patient not visited Doctor and generate the new bill.";
                                        //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strbillcancel + "','OK','Information');", true);
                                        //return strbillcancel;
                                    }
                                }

                            }
                            else
                            {
                                if (CheckType)
                                {
                                    //dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,BILLTYPE,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + SplId + " and BillType='CR' and PatientId=" + patientId + " and datediff(day,Orderdate,getdate())<" + FollowupDays, "", intUserId, intworkStationId, 0, false).Tables[0];
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,BILLTYPE,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + SplId + " and BillType='CR' and PatientId=" + patientId + " and datediff(day,Orderdate,getdate())<" + FollowupDays;
                                    OrderMAPI = "";
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                }
                                else
                                {
                                    // dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,BILLTYPE,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + SplId + " and BillType='CR' and PatientId=" + patientId + " and servicedocid=" + DocId + " and datediff(day,Orderdate,getdate())<" + FollowupDays, "", intUserId, intworkStationId, 0, false).Tables[0];
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,BILLTYPE,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + SplId + " and BillType='CR' and PatientId=" + patientId + " and servicedocid=" + DocId + " and datediff(day,Orderdate,getdate())<" + FollowupDays;
                                    OrderMAPI = "";
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                }
                            }
                            #endregion

                        }
                        else
                        {
                            if (FollowupDays == 0 && FollowupLimit > 0)
                            {
                                //dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + SplId + " and PatientId=" + patientId, "", intUserId, intworkStationId, 0, false).Tables[0];
                                TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,COMPANYID,GRADEID";
                                FilterMAPI = "serviceid=2 and specialiseId=" + SplId + " and PatientId=" + patientId;
                                OrderMAPI = "";
                                dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                            }
                            else if (SPLFOLLOWUPLMT != null)
                            {
                                if (FollowupDays > 0 && FollowupLimit == 0 && Convert.ToInt16(SPLFOLLOWUPLMT.ToString()) > 0)
                                {
                                    // dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + SplId + " and PatientId=" + patientId + " and datediff(day,Orderdate,getdate())<" + FollowupDays, "", intUserId, intworkStationId, 0, false).Tables[0];
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + SplId + " and PatientId=" + patientId + " and datediff(day,Orderdate,getdate())<" + FollowupDays;
                                    OrderMAPI = "";
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                }
                            }
                        }
                        #region IPFollowup
                        if (dtItems.Rows.Count > 0)
                        {
                            dtItems = dtItems.Copy();
                        }
                        #endregion
                        if (FollowupLimit > 0)
                        {
                            DataTable dttemp = dtItems.Clone();
                            DataRow[] dr1 = dtItems.Select("OrderTypeId=" + BaseOrderType, "Orderdate desc");
                            if (dr1.Length >= FollowupLimit)
                            {
                                HasConsData = true;
                                for (int ict = 0; ict < dr1.Length; ict++)
                                {
                                    int BillId1 = Convert.ToInt32(dr1[ict]["BillId"]);
                                    int BillId2 = Convert.ToInt32(dr1[(ict + (FollowupLimit - 1))]["BillId"]);

                                    DataRow[] drtemp = dtItems.Select(" billId>=" + BillId2 + " and billId<" + BillId1 + " and Ordertypeid <> " + BaseOrderType, "");
                                    if (drtemp.Length > 0)
                                    {
                                        DataRow[] dr2 = dtItems.Select(" billId>" + BillId2, "");
                                        for (int ictr = 0; ictr < dr2.Length; ictr++)
                                        {
                                            dttemp.ImportRow(dr2[ictr]);

                                        }
                                        break;
                                    }
                                    else
                                    {
                                        DataRow[] dr2 = dtItems.Select(" billId>" + BillId1, "");
                                        for (int ictr = 0; ictr < dr2.Length; ictr++)
                                        {
                                            dttemp.ImportRow(dr2[ictr]);

                                        }
                                        break;
                                    }
                                }
                                dttemp.AcceptChanges();
                                dtItems.Rows.Clear();
                                dtItems = dttemp.Copy();


                            }
                        }
                        #region IPConsultation Followups 
                        if (dtConfollowup.Rows.Count > 0)
                        {
                            DataTable dtIPtemp = new DataTable();
                            dtIPtemp = dtItems.Clone();
                            if (dtConfollowup.Rows[0]["Ddate"].ToString() != "" && dtConfollowup.Rows[0]["Ddate"].ToString() != null && tsDays < fDays && fLimit > 0)
                            {
                                //OrderTypeId = 46;
                                //OrderType = "Doctor Visit";
                                //BaseOrderType = 49;
                                //BaseOrderName = "Followup";
                                DateTime dtIPitem = Convert.ToDateTime(dtConfollowup.Rows[0]["Ddate"].ToString());
                                if (dtItems != null)
                                {
                                    DataTable dtitemtemp = new DataTable();
                                    dtitemtemp = dtItems.Copy();
                                    if (dtitemtemp.Rows.Count > 0)
                                    {
                                        DateTime dtitem = Convert.ToDateTime(dtitemtemp.Compute("max([ORDERDATE])", ""));
                                        if (dtitem < dtIPitem)
                                        {
                                            DataRow dr = dtIPtemp.NewRow();
                                            dr[0] = OrderTypeId;
                                            dr[1] = OrderType;
                                            dr[2] = dtIPitem;
                                            dr[4] = DocId;
                                            dr[5] = patientId;
                                            dtIPtemp.Rows.Add(dr);
                                            dtIPtemp.AcceptChanges();
                                            dtItems = dtIPtemp.Copy();

                                        }
                                    }
                                    dtItems = null;
                                }
                                else
                                {
                                    DataRow dr = dtIPtemp.NewRow();
                                    dr[0] = OrderTypeId;
                                    dr[1] = OrderType;
                                    dr[2] = dtIPitem;
                                    dr[4] = DocId;
                                    dr[5] = patientId;
                                    dtIPtemp.Rows.Add(dr);
                                    dtIPtemp.AcceptChanges();
                                    dtItems = dtIPtemp.Copy();
                                }
                            }
                        }
                        #endregion

                        if (dtItems.Rows.Count > 0)
                        {

                            if (FollowupDays > 0 && FollowupLimit > 0)
                            {

                                intFollowupID = OrderTypeId;

                                if (FollowupDays >= FollowupLimit)
                                { return CheckforLimits(FollowupLimit, CheckType, dtItems.Copy(), DocId, OrderTypeId, OrderType, BaseOrderType, BaseOrderName); }
                                else if (FollowupDays < FollowupLimit)
                                { return CheckforDays(FollowupDays, CheckType, dtItems.Copy(), DocId, OrderTypeId, OrderType, BaseOrderType, BaseOrderName); }
                            }
                            else
                            {
                                if (FollowupDays == 0 && FollowupLimit > 0)
                                { return CheckforLimits(FollowupLimit, CheckType, dtItems.Copy(), DocId, OrderTypeId, OrderType, BaseOrderType, BaseOrderName); }
                                else if (FollowupDays > 0 && FollowupLimit == 0)
                                { return CheckforDays(FollowupDays, CheckType, dtItems.Copy(), DocId, OrderTypeId, OrderType, BaseOrderType, BaseOrderName); }

                            }

                        }
                        else
                        {
                            if (OrderTypeId == BaseOrderType)
                            {

                                if (intLetterNo == ParentLetterId && intLetterNo > 0)

                                { return OrderTypeId + "/" + OrderType; }
                                else
                                {
                                    StringBuilder strMsg = null;
                                    if (TagId == 0)
                                    {
                                        if (CheckType)
                                        {
                                            strMsg = new StringBuilder();
                                            strMsg.Append("FollowUp cannot be possible for the selected speciality <br/>");
                                            strMsg.Append("As the patient doesnot have previous visits to this Speciality.");

                                            //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                            //MessageBox.Show("FollowUp cannot be possible for the selected speciality \nAs the patient doesnot have previous visits to this Speciality", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information); 
                                        }
                                        else
                                        {
                                            strMsg = new StringBuilder();
                                            strMsg.Append("FollowUp cannot be possible for the selected Doctor <br/>");
                                            strMsg.Append("As the patient doesnot have previous visits to this doctor.");

                                            //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                            //MessageBox.Show("FollowUp cannot be possible for the selected Doctor \nAs the patient doesnot have previous visits to this doctor ", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information); 
                                        }
                                        return ""; //false;
                                    }
                                    else
                                    {
                                        if (CheckType)
                                        {
                                            strMsg = new StringBuilder();
                                            strMsg.Append("FollowUp cannot be possible for the selected speciality. <br/>");
                                            strMsg.Append("As the Patient doesnot have previous visits to this Speciality.");

                                            //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                            //MessageBox.Show(resources1.GetString("frmOPBillingNew.cs.MSG93"), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information); 
                                        }
                                        else
                                        {
                                            strMsg = new StringBuilder();
                                            strMsg.Append("FollowUp cannot be possible for the selected Doctor <br/>");
                                            strMsg.Append("As the patient doesnot have previous visits to this Doctor.");

                                            //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                            //MessageBox.Show(resources1.GetString("frmOPBillingNew.cs.MSG94"), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information); 
                                        }
                                        return ""; //false;
                                    }

                                }

                            }
                            else
                            {
                                #region  BMH nextconsultation visit
                                if (HasConsData == true)
                                {
                                    int OrderTypeId1 = Convert.ToInt32(FetchDefaultTariff("ORD_SECOND_FOLLOWUP"));
                                    if (OrderTypeId1 != null && OrderTypeId1 > 0)
                                    {
                                        DataTable dtOrdertype1 = FetchADVProcedureDetails("FetchOrderTypesAdv", "1", " ServiceTypeId=4 and Blocked=0 and OrderTypeID= " + OrderTypeId1, intUserid, intworkStationId, 0);
                                        if (dtOrdertype1.Rows.Count > 0)
                                        {
                                            DataRow[] dr = dtOrdertype1.Select("");
                                            OrderType = Convert.ToString(dr[0]["OrderType"]);
                                            OrderTypeId = OrderTypeId1;
                                            OrderTypeID = OrderTypeId;//added newly for yes event.
                                        }
                                    }
                                }
                                #endregion
                                if (MaxStr != null)
                                {
                                    if (MaxStr.ToString().Length > 0)
                                    {
                                        string strMessage = MaxStr.ToString();
                                        if (strMessage.Contains("Max Consultation Count"))
                                        {
                                            //ScriptManager.RegisterStartupScript(this, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMessage + "','YESNO','Information');", true);
                                        }
                                        else
                                        {
                                            // ScriptManager.RegisterStartupScript(this, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMessage + "','OK','Information');", true);
                                        }
                                    }

                                }
                                return OrderTypeId + "/" + OrderType;
                            }

                        }
                        if (MaxStr != null)
                        {
                            if (MaxStr.ToString().Length > 0)
                            {
                                string strMessage = MaxStr.ToString();
                                if (strMessage.Contains("Max Consultation Count"))
                                {
                                    // ScriptManager.RegisterStartupScript(this, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMessage + "','YESNO','Information');", true);
                                }
                                else
                                {
                                    // ScriptManager.RegisterStartupScript(this, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMessage + "','OK','Information');", true);
                                }
                            }

                        }
                        return OrderTypeId + "/" + OrderType;
                    }
                    else
                    {
                        if (MaxStr != null)
                        {
                            if (MaxStr.ToString().Length > 0)
                            {
                                string strMessage = MaxStr.ToString();
                                if (strMessage.Contains("Max Consultation Count"))
                                {
                                    // ScriptManager.RegisterStartupScript(this, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMessage + "','YESNO','Information');", true);
                                }
                                else
                                {
                                    //ScriptManager.RegisterStartupScript(this, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMessage + "','OK','Information');", true);
                                }
                            }

                        }
                        return OrderTypeId + "/" + OrderType;
                    }// true;	}
                }
                else
                {
                    if (MaxStr != null)
                    {
                        if (MaxStr.ToString().Length > 0)
                        {
                            string strMessage = MaxStr.ToString();
                            if (strMessage.Contains("Max Consultation Count"))
                            {
                                //ScriptManager.RegisterStartupScript(this, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMessage + "','YESNO','Information');", true);
                            }
                            else
                            {
                                //ScriptManager.RegisterStartupScript(this, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMessage + "','OK','Information');", true);
                            }
                        }

                    }
                    return OrderTypeId + "/" + OrderType;
                }

            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in CheckConsultationFollowupConfig", "");
                return string.Empty;
            }
            finally
            {

            }
        }

        private void FetchFollowupData(PatientBillList PatientBillList)
        {
            try
            {
                if (hdnConsultationMsg != "MSG")
                {
                    DataTable DtCompCSContributionM = null;
                    DocAvail = null;
                    #region REMOVING UNCOVERD ITEMS FROM BILLING
                    if (!string.IsNullOrEmpty(hdnOutputBillTpe) && hdnOutputBillTpe == "YESNO")
                    {
                        if (DtCompCSContribution != null)
                        {
                            DtCompCSContributionM = DtCompCSContribution.Copy();
                            DtCompCSContributionM.AcceptChanges();
                        }
                        if (DtCompCSContributionM != null && DtCompCSContributionM.Rows.Count > 0)
                        {
                            DTTemp = (DataTable)DTTem.Copy();
                            DTTemp.AcceptChanges();
                            DataRow[] drUncoveredItem = DtCompCSContributionM.Select("mLevel in (5) and SPAY>0");
                            for (int ii = 0; ii < drUncoveredItem.Length; ii++)
                            {
                                DataRow[] rr = DTTemp.Select("procedureid=" + drUncoveredItem[ii]["procedureid"] + " and serviceid =" + drUncoveredItem[ii]["serviceid"]);
                                foreach (DataRow dd in rr)
                                    dd.Delete();
                                DTTemp.AcceptChanges();

                            }
                            hdnOutputBillTpe = "0";
                            if (!DTTemp.Columns.Contains("ProcId"))
                                DTTemp.Columns.Add("ProcId");
                            if (DTTemp.Rows.Count > 0)
                            {
                                DTTem = DTTemp.Copy();
                                DTTem.AcceptChanges();

                                gdvSearchResultData = DTTem.Copy();
                                gdvSearchResultData.AcceptChanges();

                                DataTable DtOPServices = null;
                                if (Service != null)
                                    DtOPServices = (DataTable)Service.Copy();
                                DtOPServices.AcceptChanges();
                            }
                            else
                            {
                                DataTable dtEmpty = CreateDTTemp();
                                FillEmptyGridServices(dtEmpty);
                                //imgbtnServices_Click(sender, e);  // need to implement
                            }

                        }
                    }
                    #endregion REMOVING UNCOVERD ITEMS FROM BILLING

                    #region CONSULTATION FOLLOWUP ORDERTYPE REASSIGNING

                    if (!string.IsNullOrEmpty(hfConfirmNext) && hfConfirmNext == "ORDERTYPE")
                    {
                        DataTable dtItemPrices = null;
                        int OrderTypeId = 0, BaseOrderType = 0; string OrderType = string.Empty; string BaseOrderName = string.Empty;
                        DTTemp = (DataTable)DTTem.Copy();
                        OrderTypeId = Convert.ToInt32(OrderTypeID);
                        BaseOrderType = Convert.ToInt32(BaseOrderTypeM);
                        OrderType = OrderTypeM.ToString();

                        DataTable dtOrdertype = FetchADVProcedureDetails("FetchOrderTypesAdv", "1", " ServiceTypeId=4 and Blocked=0 and OrderTypeID= " + BaseOrderType, Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0);
                        if (dtOrdertype.Rows.Count > 0)
                        {
                            DataRow[] dr = dtOrdertype.Select("");
                            BaseOrderName = Convert.ToString(dr[0]["OrderType"]);
                        }
                        DataRow[] drRowCon = DTTemp.Select("Serviceid=2 and Procedureid=" + Convert.ToInt32(hdnProcedureID) + " and Sampleid=" + OrderTypeId);
                        DataRow[] drPrice = null;
                        if (drRowCon.Length > 0)
                        {
                            string strBillBedType = "-1";
                            drRowCon[0]["Sample"] = BaseOrderName;
                            drRowCon[0]["SampleID"] = BaseOrderType;
                            dtItemPrices = GetPriceList(0, Convert.ToInt32(drRowCon[0]["ProcedureID"].ToString()), Convert.ToInt32(hdnTariffID), -1, Convert.ToInt32(drRowCon[0]["SampleId"]), Convert.ToInt32(drRowCon[0]["SpecialiseId"].ToString()), Convert.ToInt32(strDefaultUserId), strBillBedType, Convert.ToInt32(strDefWorkstationId), 0).Tables[0];
                            //dtItemPrices = GetPriceList(Convert.ToInt32(ddlservices), Convert.ToInt32(drRowCon[0]["ProcedureID"].ToString()), Convert.ToInt32(hdnTariffID), -1, Convert.ToInt32(drRowCon[0]["SampleId"]), Convert.ToInt32(drRowCon[0]["SpecialiseId"].ToString()), Convert.ToInt32(strDefaultUserId), strBillBedType, Convert.ToInt32(strDefWorkstationId), 0).Tables[0];
                            drPrice = dtItemPrices.Select();
                        }
                        if (dtItemPrices.Rows.Count > 0)
                        {
                            BasePrice = Convert.ToDecimal(drPrice[0]["BasePrice"] == DBNull.Value ? 0 : drPrice[0]["BasePrice"]);
                            EligiblePrice = Convert.ToDecimal(drPrice[0]["EligiblePrice"] == DBNull.Value ? 0 : drPrice[0]["EligiblePrice"]);
                            BillablePrice = Convert.ToDecimal(drPrice[0]["BillablePrice"] == DBNull.Value ? 0 : drPrice[0]["BillablePrice"]);
                            if ((int)BasePrice == -1)
                            {
                                drRowCon[0]["BasePrice"] = DBNull.Value;
                                drRowCon[0]["Price"] = Convert.ToDecimal("0").ToString(hdnsCurrencyFormat);
                            }
                            else
                            {
                                if ((int)BasePrice == 0)
                                {
                                    drRowCon[0]["BasePrice"] = Convert.ToDecimal("0").ToString(hdnsCurrencyFormat);
                                    drRowCon[0]["Price"] = Convert.ToDecimal("0").ToString(hdnsCurrencyFormat);
                                }
                                else
                                {
                                    drRowCon[0]["BasePrice"] = String.Format("{0:F}", Convert.ToDouble(BasePrice));
                                    drRowCon[0]["Price"] = String.Format("{0:F}", Convert.ToDouble(BasePrice));
                                }
                            }
                            if ((int)EligiblePrice == -1)
                            {
                                drRowCon[0]["EligiblePrice"] = DBNull.Value;
                            }
                            else
                            {
                                drRowCon[0]["EligiblePrice"] = String.Format("{0:F}", Convert.ToDouble(EligiblePrice));
                            }

                            if ((int)BillablePrice == -1)
                            {
                                drRowCon[0]["BillablePrice"] = DBNull.Value;
                            }
                            else
                            {
                                drRowCon[0]["BillablePrice"] = String.Format("{0:F}", Convert.ToDouble(BillablePrice));
                            }
                            if ((int)EligiblePrice == -1 && (int)BillablePrice == -1)
                            {
                                drRowCon[0]["Price"] = Convert.ToDecimal(BasePrice).ToString(hdnsCurrencyFormat);
                            }
                            else if ((int)EligiblePrice <= (int)BillablePrice)
                            {
                                if ((int)EligiblePrice >= 0)
                                {
                                    drRowCon[0]["Price"] = Convert.ToDecimal(EligiblePrice).ToString(hdnsCurrencyFormat);
                                }
                            }
                        }

                        DTTemp.AcceptChanges();
                        if (!DTTemp.Columns.Contains("ProcId"))
                            DTTemp.Columns.Add("ProcId");


                        gdvSearchResultData = DTTemp.Copy();
                        gdvSearchResultData.AcceptChanges();

                        DataTable DtOPServices = null;
                        if (Service != null)
                            DtOPServices = (DataTable)Service.Copy();
                        DtOPServices.AcceptChanges();
                        DataRow[] dtRow = null;


                        DTTem = DTTemp.Copy();
                        DTTem.AcceptChanges();
                        hfConfirmNext = null;
                    }

                    #endregion CONSULTATION FOLLOWUP ORDERTYPE REASSIGNING

                    if (!string.IsNullOrEmpty(DoctorAvailable.Trim()))
                    {
                        if (DoctorAvailable.ToString() == "NO")
                        {
                            DoctorAvailable = "YES";
                            //imgItemprocedure_Click(PatientBillList);

                        }
                    }
                }
                else if (hdnConsultationMsg == "MSG")
                {
                    DataTable dtLOARequest = new DataTable();
                    string strContract = string.Empty;
                    int intUcafID = 0;
                    dtLOARequest.Columns.Add("EntryID", typeof(Int32));
                    dtLOARequest.Columns.Add("PayerId", typeof(Int32));
                    dtLOARequest.Columns.Add("GradeID", typeof(Int32));
                    dtLOARequest.Columns.Add("PrescriptionID", typeof(Int32));
                    dtLOARequest.Columns.Add("UHID", typeof(Int32));
                    dtLOARequest.Columns.Add("DoctorId", typeof(Int32));
                    dtLOARequest.Columns.Add("SpecialisationId", typeof(Int32));
                    dtLOARequest.Columns.Add("ReferralBasis", typeof(string));
                    dtLOARequest.Columns.Add("CoverageType", typeof(Int32));
                    dtLOARequest.Columns.Add("bedtypeID", typeof(Int32));
                    dtLOARequest.Columns.Add("Status", typeof(Int32));
                    dtLOARequest.Columns.Add("Remarks", typeof(string));

                    DataRow dtrow = dtLOARequest.NewRow();

                    dtrow["PayerId"] = Convert.ToInt32(strCompanyID);
                    dtrow["GradeID"] = Convert.ToInt32(strGradeID);
                    dtrow["UHID"] = Convert.ToInt32(hdnPatientID);
                    dtrow["DoctorId"] = Convert.ToInt32(hdnDocID);
                    dtrow["SpecialisationId"] = Convert.ToInt32(hdnDocSpecialiseId);
                    dtrow["CoverageType"] = Convert.ToInt32(hdnPatientType);
                    dtrow["bedtypeID"] = -1;
                    dtrow["Status"] = 4;
                    dtrow["Remarks"] = "OP Billing LOA Request";
                    dtLOARequest.Rows.Add(dtrow);
                    DataSet dsLOARequest = new DataSet();
                    dsLOARequest.Tables.Add(dtLOARequest.Copy());

                    // Get Items from Grid
                    DTTemp = (DataTable)DTTem.Copy();
                    DTTemp.AcceptChanges();
                    if (!DTTemp.Columns.Contains("ReasonID"))
                        DTTemp.Columns.Add("ReasonID", typeof(int));
                    DataTable LOAItemDetails = new DataTable();
                    LOAItemDetails = DTTemp.Copy();
                    if (LOAItemDetails != null)
                    {
                        if (LOAItemDetails.Rows[LOAItemDetails.Rows.Count - 1]["ServiceId"] == DBNull.Value ||
                            LOAItemDetails.Rows[LOAItemDetails.Rows.Count - 1]["ProcedureId"] == DBNull.Value)
                            LOAItemDetails.Rows.RemoveAt(LOAItemDetails.Rows.Count - 1);
                        DataTable DtAppOrderdetail = new DataTable();
                        DtAppOrderdetail.Columns.Add("SID", typeof(int));
                        DtAppOrderdetail.Columns.Add("IID", typeof(int));
                        DtAppOrderdetail.Columns.Add("PRI", typeof(double));
                        DtAppOrderdetail.Columns.Add("QTY", typeof(int));
                        DtAppOrderdetail.Columns.Add("CSID", typeof(int));
                        DtAppOrderdetail.Columns.Add("CSRID", typeof(int));//StatusID
                        DtAppOrderdetail.Columns.Add("DEPID", typeof(int));//ReasonID
                        DtAppOrderdetail.Columns.Add("SPID", typeof(int));

                        foreach (DataRow Drobj in LOAItemDetails.Rows)
                        {
                            DataRow dtrAgr = DtAppOrderdetail.NewRow();
                            dtrAgr["SID"] = Drobj["ServiceId"];
                            dtrAgr["IID"] = Drobj["ProcedureId"];
                            dtrAgr["PRI"] = Drobj["Price"];
                            dtrAgr["QTY"] = Drobj["Quantity"];
                            dtrAgr["CSID"] = 6;
                            dtrAgr["CSRID"] = Drobj["ReasonID"];
                            dtrAgr["DEPID"] = Drobj["DeptID"];
                            dtrAgr["SPID"] = Drobj["SpecialiseID"];
                            DtAppOrderdetail.Rows.Add(dtrAgr);
                        }
                        DtAppOrderdetail.AcceptChanges();
                        strContract = Utilities.ConvertDTToXML("REQ", "ITEM", DtAppOrderdetail);
                    }
                    hdnConsultationMsg = null;
                }
                if (hdnIsFollowUp == "YESNO")
                {
                    hdnIsFollowUp = "OK";
                }
                hdnblnYes = "true";
                hdnblnNo = "false";
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.LogToXMLFile = false;
            }
            finally
            {

            }
        }

        private DataTable GeneralExclusions(PatientBillList PatientBillList)
        {
            try
            {
                if (!string.IsNullOrEmpty(hdnTariffID))
                {
                    if (hdnTariffID != "-1")
                    {
                        if (System.Configuration.ConfigurationManager.AppSettings["IsGeneralExcludedGroup"] != null && System.Configuration.ConfigurationManager.AppSettings["IsGeneralExcludedGroup"].ToString().ToUpper() == "YES")
                        {
                            DataTable dtExclusions = GetGeneralExclusionCategory(Convert.ToInt32(hdnTariffID));
                            Exclusions = dtExclusions;
                        }
                    }
                }
                string str = "";
                string strGeneralExclusions = string.Empty;
                DataTable dsExcludedItems = new DataTable();
                DataRow[] dr = DTTemp.Select("procedureid > 0");
                if (dr.Length > 0)
                {
                    for (int i = 0; i < dr.Length; i++)
                    {
                        str += dr[i]["procedureid"].ToString();
                        if (i != dr.Length - 1)
                        {
                            str += ",";
                        }
                    }
                    //if (str != "")
                    //{
                    //    str = " and " + " serviceitemid in (" + str + ") and CompanyID = " + strCompanyID + " and GradeID = " + strGradeID + "";
                    //}
                    //dsExcludedItems = FetchADVProcedureDetailsM("Pr_FetchGeneralExculusionAdv", string.Empty, "blocked=0 " + str, Convert.ToInt32(strDefaultUserId.ToString()), Convert.ToInt32(strDefWorkstationId.ToString()), 0);
                    dsExcludedItems = FetchGeneralExculusionMAPI(Convert.ToInt32(strCompanyID), Convert.ToInt32(strGradeID), str, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];
                }
                return dsExcludedItems;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in GeneralExclusions", "");
                return null;
            }
            finally
            {

            }
        }

        public DataSet GetPINBlockDetail(string PatientID, int intUserId, int intWorkStationId, int intError)
        {
            ContractMgmtServiceContractClient objContractMgtClient = new ContractMgmtServiceContractClient();
            try
            {
                return objContractMgtClient.GetPINBlockDetail(PatientID, intUserId, intWorkStationId, intError, 775, -2, "Fetch PIN Block");
            }
            finally
            {
                objContractMgtClient.Close();
            }
        }


        public class PatientBillList
        {
            public int HospitalId;
            public string RegCode;
            public string ScheduleID;
        }

        private void FillEmptyGridServices(DataTable dt)
        {
            if (dt.Columns.Contains("Sl.No"))
            {
                dt = DTTem.Copy();
                dt.AcceptChanges();
            }
            else
            {
                dt.Columns.Add("Sl.No", typeof(int));

            }

            dt.AcceptChanges();
            DataRow dr = dt.NewRow();
            dr["ServiceId"] = 0; dr["Procedure"] = ""; dr["Sample"] = "";
            dr["Quantity"] = 1;
            dr["Price"] = Convert.ToDouble(0).ToString(hdnsCurrencyFormat);

            dt.Rows.Add(dr);
            if (!dt.Columns.Contains("ProcId"))
                dt.Columns.Add("ProcId");
            dt.AcceptChanges();

            DTTem = null;

        }

        public DataSet FetchConsultantConfiguration(int DocID, int HospitalID, string TBL, int UserID, int WorkstationID, int intError)
        {
            DataSet dsConsultantConfig = new DataSet();
            FrontOfficeServiceContractClient objFOProxy = new FrontOfficeServiceContractClient();
            try
            {
                dsConsultantConfig = objFOProxy.FetchConsultationConfig(DocID, HospitalID, TBL, UserID, WorkstationID, intError);
                return dsConsultantConfig;
            }
            finally
            {
                objFOProxy.Close();

            }
        }

        public Dictionary<int, string> GetWeekDaysPair()
        {
            Dictionary<int, string> kvpWeekDays = new Dictionary<int, string>();
            int iCount = 1;
            if (CultureInfo.CurrentCulture.Name.StartsWith("en-"))
            {
                string[] days = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

                foreach (string item in days)
                {
                    kvpWeekDays.Add(iCount, item);
                    iCount++;
                }
            }
            else
            {
                foreach (string item in CultureInfo.CurrentCulture.DateTimeFormat.DayNames)
                {
                    kvpWeekDays.Add(iCount, item);
                    iCount++;
                }

            }

            return kvpWeekDays;
        }

        public List<string> GetWeekDays()
        {
            List<string> strWeekDays;

            if (CultureInfo.CurrentCulture.Name.StartsWith("en-"))
            {
                string[] days = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                strWeekDays = new List<string>(days);
            }
            else
            {
                strWeekDays = new List<string>(CultureInfo.CurrentCulture.DateTimeFormat.DayNames);
            }

            return strWeekDays;
        }

        public DataSet FetchAppointment(string Filter, int intUserID, int intWorkStationID, int intError)
        {
            DataSet dsAppointment = new DataSet();
            CommonSearchServiceContractClient objProxy = new CommonSearchServiceContractClient();
            try
            {
                dsAppointment = objProxy.GetSearchResultsPageWise("AppointmentSearch", 1, 100, Filter, intUserID, intWorkStationID, -4, -1, null, string.Empty);
                return dsAppointment;
            }
            finally
            {
                objProxy.Close();

            }
        }
        public DataSet FetchConsultations_Perf(int intTableId, string strFilter, int intUserId, int intWorkStationId, int intError, int LangID, int DoctorID, DateTime OrderDate, int hospitalID)
        {
            DataSet dsConsPerf = new DataSet();
            FrontOfficeServiceContractClient objFOProxy = new FrontOfficeServiceContractClient();
            try
            {
                dsConsPerf = objFOProxy.FetchConsultations_Perf(intTableId, strFilter, intUserId, intWorkStationId, intError, LangID, DoctorID, OrderDate, hospitalID);
                return dsConsPerf;
            }
            finally
            {
                objFOProxy.Close();

            }
        }

        private bool CheckDefaultLOA(int PatientID, int CompanyID, int GradeID, int SpecialisationID)
        {

            try
            {
                hdnIsDefaultLOA = "false";
                string strCollectableFilter = string.Empty;
                string strCollectableConfigFilter = string.Empty;
                AvailDiscount = null;

                DataSet dsPayerDetails = new DataSet();
                int intUserId = 0, intWorkstationid = 0, intError = 0;
                if (string.IsNullOrEmpty(ViewStateLetterid) && hdnrblbilltypeCredit == true)
                {
                    int OrderType = Convert.ToInt32(FetchDefaultTariff("ORD_FOLLOWUP_DAYS"));
                    BaseOrderType = OrderType;
                    if (string.IsNullOrEmpty(hdnordertypeID) && OtherOrders != null && !string.IsNullOrEmpty(OrdertypeOtherorder.Trim()))
                    {
                        hdnordertypeID = OrdertypeOtherorder.ToString();
                    }
                    if (!string.IsNullOrEmpty(hdnordertypeID) && Convert.ToInt32(hdnordertypeID) == OrderType)
                    {
                        string strLOAfltr = "STATUS=0 and Blocked = 0 and  GradeId = " + Convert.ToInt32(strGradeID) + " and payerid=" + Convert.ToInt32(strCompanyID) + " and patienttype=1 and " +
                                        "patientId= " + Convert.ToInt32(hdnPatientID) + " and  todate >=\'" + DateTime.Now.ToString("dd-MMM-yyyy") + "\' and fromdate <=getdate()" + " and SpecialisationId=" + hdnDocSpecialiseId;
                        GetLOA(strLOAfltr);
                        //if (ViewState["Letterid"] == null)
                        //    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','Please load the patient again and try billing.','OK','Information');", true);
                    }
                }
                //if (ViewState["OrderTypeID"] != null && ViewState["BaseOrderType"] != null && ViewState["OrderTypeID"] != ViewState["BaseOrderType"])
                //{
                //    if (string.IsNullOrEmpty(ViewState["VSParentLetterid"].ToString()))
                //    {
                //        ViewState["Letterid"] = null;
                //        ViewState["VSParentLetterid"] = null;
                //    }
                //    else if(ViewState["VSParentLetterid"].ToString()=="0")
                //    { }
                //    else { 
                //        ViewState["Letterid"] = null;
                //        ViewState["VSParentLetterid"] = null;
                //    }
                //}

                if (!string.IsNullOrEmpty(ViewStateLetterid))
                    intLetterNo = Convert.ToInt32(ViewStateLetterid.ToString());

                if (!string.IsNullOrEmpty(VSParentLetterid))
                    ParentLetterId = Convert.ToInt32(VSParentLetterid.ToString());
                //////////////////////////////////////////////////////////////////
                intUserId = Convert.ToInt32(strDefaultUserId.ToString()); intWorkstationid = Convert.ToInt32(strDefWorkstationId.ToString()); intError = 0;

                int HospitalID = Convert.ToInt32(strDefaultHospitalId);

                dsPayerDetails = FetchHospitalCompanyDetails(CompanyID, "C", "7,8,15", Convert.ToInt32(intUserId), Convert.ToInt32(intWorkstationid), 0, HospitalID).Copy();

                dtTable7 = dsPayerDetails.Tables[0].Copy();
                dtTable7.AcceptChanges();

                DataTable dtCollectable = dsPayerDetails.Tables[0].Copy();

                if (hdnMaternityConfig != "0" && hdnSPCMaternityConfig != "0" && hdnIsPregnent == "1" && !string.IsNullOrEmpty(hdnEDD) && dtCollectable.Select("DiscountSpecialiseId = " + Convert.ToInt32(hdnSPCMaternityConfig)).Length > 0)
                    strCollectableFilter = "Patienttype=1 and GradeID =" + GradeID + " and DiscountSpecialiseId = " + Convert.ToInt32(hdnSPCMaternityConfig);
                else if (dtCollectable.Select("Patienttype=1 and GradeID =" + GradeID + " and DiscountSpecialiseId = " + Convert.ToInt32(hdnDocSpecialiseId)).Length > 0)
                    strCollectableFilter = "Patienttype=1 and GradeID =" + GradeID + " and DiscountSpecialiseId = " + Convert.ToInt32(hdnDocSpecialiseId);
                else
                    strCollectableFilter = "Patienttype=1 and GradeID =" + GradeID + " and DiscountSpecialiseId = " + "-1";

                DataRow[] dr = dtCollectable.Select(strCollectableFilter);
                if (dr.Length > 0)
                {
                    MaxCollectable = Convert.ToInt32(dr[0]["MaxCollectable"] == DBNull.Value ? 0 : Convert.ToInt32(dr[0]["MaxCollectable"]));
                    if (!string.IsNullOrEmpty(Convert.ToString(PATMAXCOLL)))
                        MaxCollectable = Convert.ToInt32(Convert.ToDecimal(PATMAXCOLL));
                    hdnMaxCollectable = MaxCollectable.ToString();
                    intBillMaxCollectable = MaxCollectable;
                }
                if (MaxCollectable == 0)
                { MaxCollectable = -1; hdnMaxCollectable = MaxCollectable.ToString(); intBillMaxCollectable = MaxCollectable; }

                // Getting Collectable Configuration
                DataTable dtCollectableConfig = dsPayerDetails.Tables[3].Copy();

                if (hdnMaternityConfig != "0" && hdnSPCMaternityConfig != "0" && hdnIsPregnent == "1" && !string.IsNullOrEmpty(hdnEDD) && dtCollectableConfig.Select("GradeSpecialiseID = " + Convert.ToInt32(hdnSPCMaternityConfig)).Length > 0)
                    strCollectableConfigFilter = "Patienttype = 1 and Gradeid =" + GradeID + " and CompanyId=" + CompanyID + " and GradeSpecialiseID = " + Convert.ToInt32(hdnSPCMaternityConfig);
                else if (dtCollectableConfig.Select("Patienttype = 1 and Gradeid =" + GradeID + " and GradeSpecialiseID=" + Convert.ToInt32(hdnDocSpecialiseId)).Length > 0)
                    strCollectableConfigFilter = "Patienttype = 1 and Gradeid =" + GradeID + " and CompanyId=" + CompanyID + " and GradeSpecialiseID = " + Convert.ToInt32(hdnDocSpecialiseId);
                else
                    strCollectableConfigFilter = "Patienttype = 1 and Gradeid =" + GradeID + " and CompanyId=" + CompanyID + " and GradeSpecialiseID = " + "-1";


                DataRow[] drConfig = dtCollectableConfig.Select(strCollectableConfigFilter);
                CollectableType = 0;
                IsCardCollectable = false;

                if (drConfig.Length > 0)
                {
                    // CollectableType=0 "No collectable" <><><><>CollectableType=1 "Before Discount" <><><><> CollectableType=2 "After Discount"
                    CollectableType = Convert.ToInt32(drConfig[0]["CollectableId"] == DBNull.Value ? 0 : Convert.ToInt32(drConfig[0]["CollectableId"]));
                    if (CollectableType == -1) CollectableType = 0;
                    IsCardCollectable = Convert.ToBoolean(drConfig[0]["CardCollectable"]);
                    if (IsCardCollectable)
                        hdnIsCardCollectable = "1";
                    else
                        hdnIsCardCollectable = "0";
                    if (!string.IsNullOrEmpty(EMRCOPAY))
                    {
                        if (EMRCOPAY.ToString() == "NO")
                            CollectableType = 0;
                    }
                    hdnCollectableType = Convert.ToString(CollectableType);
                }
                //

                if (hdnPatientType != "4" && hdnPatientType != "3")
                {
                    //if (gdvLOA.Rows.Count == 0)
                    //{
                    //    ParentLetterId = 0;
                    //    intLetterNo = 0;
                    //}
                }
                if (intLetterNo == 0)
                {

                    DataTable dtCompany = dsPayerDetails.Tables[2].Copy();
                    if (dtCompany.Rows.Count == 0) { return true; }

                    //Checking Consultation Specific
                    if (Convert.ToBoolean(dtCompany.Rows[0]["ISConsultationSpecific"].ToString()) == true)
                    {
                        int intJ = 0;
                        DataTable dtGetLOAs = new DataTable();
                        if (BaseOrderType != 0)
                        {
                            ConsBaseOrderType = Convert.ToInt32(BaseOrderType.ToString());
                        }
                        if (gdvSearchResultData.Rows.Count > 0)
                        {
                            intJ = 0;
                            for (int intI = 0; intI <= gdvSearchResultData.Rows.Count - 1; intI++)
                            {
                                //if (Convert.ToInt32(dgServices[intI, 2]) == 2 && Convert.ToInt32(dgServices[intI, 6]) != ConsBaseOrderType)
                                if (Convert.ToInt32(gdvSearchResultData.Rows[intI]["ServiceId"].ToString()) == 2 && Convert.ToInt32(gdvSearchResultData.Rows[intI]["SampleId"].ToString()) != ConsBaseOrderType)
                                { intJ++; }
                            }
                        }

                        if (intJ == 0)
                        {
                            //Added for Bug ID-95060
                            string str;
                            int doctorid = Convert.ToInt32(hdnDocID);
                            DataTable dtSpl = FetchFollowupdays(doctorid, SpecialisationID, intUserId, intWorkstationid, intError, PatientID).Tables[0];
                            dtConfollowup = dtSpl.Copy();
                            dtConfollowup.AcceptChanges();
                            if (dtSpl.Rows.Count >= 2)
                                CheckType = Convert.ToBoolean(Convert.IsDBNull(dtSpl.Rows[1]["AllDoctors"]) ? 1 : dtSpl.Rows[1]["AllDoctors"]);
                            if (CheckType)
                            {
                                str = "status=0 and Blocked = 0 and SpecialisationID = " + SpecialisationID + " and GradeId = " + GradeID + " and payerid=" + CompanyID + " and patienttype=1 And LetterId = ParentLetterId and patientId= " + PatientID + " and todate >= convert(varchar(12),getdate(),106) order by fromdate";
                            }
                            else
                            {
                                str = "status=0 and Blocked = 0 and SpecialisationID = " + SpecialisationID + " and DoctorID=" + doctorid + " and GradeId = " + GradeID + " and payerid=" + CompanyID + " and patienttype=1 And LetterId = ParentLetterId and patientId= " + PatientID + " and todate >= convert(varchar(12),getdate(),106) order by fromdate";
                            }

                            if (!string.IsNullOrEmpty(hdnLetterIDforIPID) && hdnLetterIDforIPID != "0")
                                str = "letterid=" + Convert.ToInt32(hdnLetterIDforIPID) + " and " + str;
                            dtGetLOAs = FetchPackageLOA(0, str, intUserId, intWorkstationid, 0, HospitalID).Tables[0].Copy();
                        }

                        if (dtGetLOAs.Rows.Count > 0)
                        {
                            if (intLetterNo == 0)
                            {
                                intLetterNo = Convert.ToInt32(dtGetLOAs.Rows[dtGetLOAs.Rows.Count - 1]["LetterID"].ToString());
                                ParentLetterId = Convert.IsDBNull(dtGetLOAs.Rows[dtGetLOAs.Rows.Count - 1]["ParentLetterId"]) ? 0 : Convert.ToInt32(dtGetLOAs.Rows[dtGetLOAs.Rows.Count - 1]["ParentLetterId"].ToString());
                            }



                        }
                        else
                        {

                            if (Convert.ToBoolean(dtCompany.Rows[0]["IsDefaultLOA"].ToString()) == true)
                            {
                                string strFilter = "PatientType=1 and status= 0 and Blocked = 0 and GradeId = " + GradeID + " and payerid=" + CompanyID + " and patientId=0"; // and todate >='" + DateTime.Now.ToString("dd-MMM-yyyy")+ "'" ;
                                DataTable dtGetDefaultLOA = FetchPackageLOA(0, strFilter, intUserId, intWorkstationid, 0, HospitalID).Tables[0].Copy();

                                //Getting Default LOA
                                if (dtGetDefaultLOA.Rows.Count > 0)
                                {

                                    if (hdnDocSpecialiseId != "-1")
                                    {
                                        String Filter = "Patienttype=1 and GradeID =" + GradeID + " and DiscountSpecialiseId = " + Convert.ToInt32(hdnDocSpecialiseId);
                                        if (dsPayerDetails.Tables[0].Select(Filter).Length > 0)
                                        {
                                            if (dtGetDefaultLOA.Select("SpecialisationID = " + Convert.ToInt32(hdnDocSpecialiseId)).Length > 0)
                                            {
                                                intLetterNo = Convert.ToInt32((dtGetDefaultLOA.Select("SpecialisationID = " + Convert.ToInt32(hdnDocSpecialiseId)))[0]["LetterID"].ToString());// getting letterid based on the same grade-specialisation if available
                                            }
                                            else
                                            {
                                                StringBuilder strMsg = new StringBuilder();
                                                strMsg.Append("LOA not mapped <br/>");
                                                strMsg.Append("Contact AR Department");
                                                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);

                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            if ((dtGetDefaultLOA.Select("SpecialisationID = " + "-1")).Length > 0)
                                            {
                                                intLetterNo = Convert.ToInt32((dtGetDefaultLOA.Select("SpecialisationID = " + "-1"))[0]["LetterID"].ToString());// getting letterid based on the general grade-specialisation available
                                            }
                                            else
                                            {
                                                StringBuilder strMsg = new StringBuilder();
                                                strMsg.Append("LOA not mapped <br/>");
                                                strMsg.Append("Contact AR Department");
                                                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);

                                                return false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if ((dtGetDefaultLOA.Select("SpecialisationID = " + "-1")).Length > 0)
                                        {
                                            intLetterNo = Convert.ToInt32((dtGetDefaultLOA.Select("SpecialisationID = " + "-1"))[0]["LetterID"].ToString());// getting letterid based on the general grade-specialisation available
                                        }

                                        else
                                        {
                                            StringBuilder strMsg = new StringBuilder();
                                            strMsg.Append("LOA not mapped <br/>");
                                            strMsg.Append("Contact AR Department");
                                            //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                            return false;
                                        }
                                    }
                                    DataSet dsLOA = FetchLetterDetails(intLetterNo, 0, "1,7", intUserId, intWorkstationid, 0, HospitalID);
                                    dtLetterSeventh = dsLOA.Tables[1].Copy();
                                    dtLetterGenInfo = dsLOA.Tables[0].Copy();
                                    ViewStatedsLOA = dsLOA.Copy();
                                    ViewStatedsLOA.AcceptChanges();
                                    blnSaveDefaultLOA = true;
                                    hdnblnSaveDefaultLOA = "true";
                                }
                                else
                                {
                                    StringBuilder strMsg = new StringBuilder();
                                    strMsg.Append("LOA not mapped <br/>");
                                    strMsg.Append("Contact AR Department");
                                    //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);

                                    return false;
                                }
                            }
                        }
                    }
                    else if (Convert.ToBoolean(dtCompany.Rows[0]["IsDefaultLOA"].ToString()) == true)
                    {

                        int intJ = 0;
                        DataTable dtGetLOAs = new DataTable();

                        if (gdvSearchResultData.Rows.Count > 0)
                        {
                            intJ = 0;
                            for (int intI = 0; intI <= gdvSearchResultData.Rows.Count - 1; intI++)
                            {
                                if (Convert.ToInt32(gdvSearchResultData.Rows[intI]["ServiceId"].ToString()) == 2)
                                { intJ++; }
                            }
                        }

                        if (intJ == 0)
                        {
                            string str = "status=0 and Blocked = 0 and SpecialisationID = " + SpecialisationID + " and GradeId = " + GradeID + " and payerid=" + CompanyID + " and patienttype=1 And LetterId = ParentLetterId and patientId= " + PatientID + " and todate >= getdate()";

                        }


                        if (dtGetLOAs.Rows.Count > 0)
                        {
                            if (intLetterNo == 0)
                            {
                                intLetterNo = Convert.ToInt32(dtGetLOAs.Rows[dtGetLOAs.Rows.Count - 1]["LetterID"].ToString());
                                ParentLetterId = Convert.IsDBNull(dtGetLOAs.Rows[dtGetLOAs.Rows.Count - 1]["ParentLetterId"]) ? 0 : Convert.ToInt32(dtGetLOAs.Rows[dtGetLOAs.Rows.Count - 1]["ParentLetterId"].ToString());
                            }

                        }
                        else
                        {
                            string strFilter = "PatientType=1 and status=0 and Blocked = 0 and GradeId = " + GradeID + " and payerid=" + CompanyID + " and patientid=0"; //and todate >='" + DateTime.Now.ToString("dd-MMM-yyyy")+ "'" ;
                            DataTable dtGetDefaultLOA = FetchPackageLOA(0, strFilter, intUserId, intWorkstationid, 0).Tables[0].Copy();

                            if (dtGetDefaultLOA.Rows.Count > 0)
                            {
                                if (hdnDocSpecialiseId != "-1")
                                {
                                    String Filter = "Patienttype=1 and GradeID =" + GradeID + " and DiscountSpecialiseId = " + Convert.ToInt32(hdnDocSpecialiseId);
                                    if (dsPayerDetails.Tables[0].Select(Filter).Length > 0)
                                    {
                                        if (dtGetDefaultLOA.Select("SpecialisationID = " + Convert.ToInt32(hdnDocSpecialiseId)).Length > 0)
                                        {
                                            intLetterNo = Convert.ToInt32((dtGetDefaultLOA.Select("SpecialisationID = " + Convert.ToInt32(hdnDocSpecialiseId)))[0]["LetterID"].ToString());// getting letterid based on the same grade-specialisation if available
                                        }
                                        else
                                        {
                                            StringBuilder strMsg = new StringBuilder();
                                            strMsg.Append("LOA not mapped <br/>");
                                            strMsg.Append("Contact AR Department");
                                            //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);

                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        if ((dtGetDefaultLOA.Select("SpecialisationID = " + "-1")).Length > 0)
                                        {
                                            intLetterNo = Convert.ToInt32((dtGetDefaultLOA.Select("SpecialisationID = " + "-1"))[0]["LetterID"].ToString());// getting letterid based on the general grade-specialisation available
                                        }
                                        else
                                        {
                                            StringBuilder strMsg = new StringBuilder();
                                            strMsg.Append("LOA not mapped <br/>");
                                            strMsg.Append("Contact AR Department");
                                            //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);

                                            return false;
                                        }
                                    }
                                }
                                else
                                {
                                    if ((dtGetDefaultLOA.Select("SpecialisationID = " + "-1")).Length > 0)
                                    {
                                        intLetterNo = Convert.ToInt32((dtGetDefaultLOA.Select("SpecialisationID = " + "-1"))[0]["LetterID"].ToString());// getting letterid based on the general grade-specialisation available
                                    }

                                    else
                                    {
                                        StringBuilder strMsg = new StringBuilder();
                                        strMsg.Append("LOA not mapped <br/>");
                                        strMsg.Append("Contact AR Department");
                                        // ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);

                                        return false;
                                    }
                                }
                                // Added by shashi - Astra issue 47130 - 15/06/2012 - End

                                DataSet dsLOA = FetchLetterDetails(intLetterNo, 0, "1,7", intUserId, intWorkstationid, 0);
                                dtLetterSeventh = dsLOA.Tables[1].Copy();
                                dtLetterGenInfo = dsLOA.Tables[0].Copy();
                                ViewStatedsLOA = dsLOA.Copy();
                                ViewStatedsLOA.AcceptChanges();
                                blnSaveDefaultLOA = true;
                                hdnblnSaveDefaultLOA = "true";
                            }
                            else
                            {
                                StringBuilder strMsg = new StringBuilder();
                                strMsg.Append("LOA not mapped <br/>");
                                strMsg.Append("Contact AR Department");
                                // ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                return false;
                            }
                        }
                    }
                }

                GetMaxCollectable(ParentLetterId);
                if (intLetterNo == ParentLetterId) hdnIsDefaultLOA = "true";
                ViewStateLetterid = intLetterNo.ToString();
                VSParentLetterid = ParentLetterId.ToString();
                return true;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in CheckDefaultLOA", "");
                return false;
            }
            finally
            {

            }
        }

        private bool IsAllowSameDayConsultation()
        {
            try
            {
                if (dtConfollowup != null)
                {
                    DataTable dtSp = dtConfollowup.Copy();
                    dtSp.AcceptChanges();
                    if (Convert.ToBoolean(dtSp.Rows[1]["SameDayFollowUp"]))
                        return false;
                    else
                        return true;
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in IsAllowSameDayConsultation", "");
                return false;
            }
        }

        private bool ValidateInsurancCard(DateTime dtpCardValid)
        {
            bool blnStatus = false;
            try
            {
                if ((hdnrblbilltypeCredit == true && Convert.ToInt32(strCompanyID) > 0) || (hdnrblbilltypeCredit == true && Convert.ToInt32(strGradeID) > 0))
                {
                    if (Convert.ToInt32(strCompanyID) > 0 && dtpCardValid < DateTime.Today.Date)
                    {
                        if (TagId == 0)
                        {
                            //CompanyReturnMessage = Resources.English.ResourceManager.GetString("ICECBNP");
                            //CompanyReturnMessage2L = Resources.Arabic.ResourceManager.GetString("ICECBNP");
                            CompanyReturnMessage = System.Configuration.ConfigurationManager.AppSettings["ICECBNP"].ToString();
                            CompanyReturnMessage2L = System.Configuration.ConfigurationManager.AppSettings["ICECBNP"].ToString();
                        }
                        else
                        {
                            //CompanyReturnMessage = Resources.English.ResourceManager.GetString("ICECBNP");
                            //CompanyReturnMessage2L = Resources.Arabic.ResourceManager.GetString("ICECBNP");
                            CompanyReturnMessage = System.Configuration.ConfigurationManager.AppSettings["ICECBNP"].ToString();
                            CompanyReturnMessage2L = System.Configuration.ConfigurationManager.AppSettings["ICECBNP"].ToString();
                        }

                        blnStatus = true;
                    }
                }
                return blnStatus;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in ValidateInsurancCard", "");
                return false;
            }
        }

        public DataSet FetchMISProcedureDetailsMAPI(int PatientID, string TypeC, string Filter, string order, int HospitalID, int WstationId, PatientBillList PatientBillList)
        {
            try
            {
                return FetchConsFollowUpDetailsMAPI(PatientID, TypeC, Filter, order, HospitalID, WstationId, PatientBillList);
            }
            finally
            {
                objFOClient.Close();
            }
        }

        public DataSet FetchConsFollowUpDetailsMAPI(int PatientID, string Type, string Filter, string order, int HospitalID, int intWorkstationId, PatientBillList PatientBillList)
        {
            DataHelper objDataHelper = new DataHelper(DEFAULTWORKSTATION, (int)Database.Master);
            DataSet dsSpecConfig = new DataSet();
            try
            {
                List<IDbDataParameter> objIDbDataParameters = new List<IDbDataParameter>();
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@PatientID", PatientID.ToString(), DbType.Int32, ParameterDirection.Input));
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@Type", Type.ToString(), DbType.String, ParameterDirection.Input));
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@Filter", Filter.ToString(), DbType.String, ParameterDirection.Input));
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@order", order.ToString(), DbType.String, ParameterDirection.Input));
                dsSpecConfig = objDataHelper.RunSPReturnDS("Pr_FetchConsFollowUpDetails_MAPI", objIDbDataParameters.ToArray());
                return dsSpecConfig;
            }
            finally
            {

            }
        }

        private DataTable GetCashContributions(DataTable dtBillDetails, int BType, DataTable DtBillSummary, DataTable dtDiscountDetails, DataTable dtCompanyCashBLItemDetails)
        {
            DataTable dtBillContribution = new DataTable();
            try
            {
                dtBillContribution = dtBillDetails.Copy();

                //Renaming
                dtBillContribution.Columns["mLevel"].ColumnName = "TYP";
                dtBillContribution.Columns["ServiceId"].ColumnName = "SID";
                dtBillContribution.Columns["DeptID"].ColumnName = "DID";
                dtBillContribution.Columns["SpecialiseId"].ColumnName = "SPID";
                dtBillContribution.Columns["ProcedureID"].ColumnName = "SIID";
                dtBillContribution.Columns["Amount"].ColumnName = "TOT";
                dtBillContribution.Columns["ProcId"].ColumnName = "PRID";
                //dtBillContribution.Columns["Amount"].ColumnName="TOT";

                //Removing
                dtBillContribution.Columns.Remove("ServiceName");
                dtBillContribution.Columns.Remove("Procedure");
                dtBillContribution.Columns.Remove("Sample");
                dtBillContribution.Columns.Remove("SampleId");
                dtBillContribution.Columns.Remove("DeptName");
                dtBillContribution.Columns.Remove("BedTypeId");
                dtBillContribution.Columns.Remove("BedTypeName");
                dtBillContribution.Columns.Remove("SpecialiseName");
                dtBillContribution.Columns.Remove("OrderNo");
                dtBillContribution.Columns.Remove("OrderDate");
                dtBillContribution.Columns.Remove("IsGroup");
                //dtBillContribution.Columns.Remove("Qty");
                //dtBillContribution.Columns.Remove("Seq");
                dtBillContribution.Columns.Remove("ProfileId");
                dtBillContribution.Columns.Remove("TariffId");
                dtBillContribution.Columns.Remove("mBedTypeID");
                dtBillContribution.Columns.Remove("mServiceID");
                dtBillContribution.Columns.Remove("mHospDeptID");
                dtBillContribution.Columns.Remove("mSpecialiseID");
                dtBillContribution.Columns.Remove("mItemID");
                dtBillContribution.Columns.Remove("mTypeID");
                dtBillContribution.Columns.Remove("mTypeName");
                dtBillContribution.Columns.Remove("mChildID");
                dtBillContribution.Columns.Remove("mParentID");
                dtBillContribution.Columns.Remove("mLTID");
                dtBillContribution.Columns.Remove("mLimitAmt");

                //Adding
                dtBillContribution.Columns.Add("DCOM", typeof(decimal));
                //dtBillContribution.Columns["DCOM"].Expression ="0";
                dtBillContribution.Columns.Add("DPAT", typeof(decimal));
                //dtBillContribution.Columns["DPAT"].Expression ="0";
                dtBillContribution.Columns.Add("DIS", typeof(decimal));
                //dtBillContribution.Columns["DIS"].Expression ="0";
                dtBillContribution.Columns.Add("DPER", typeof(decimal));
                //dtBillContribution.Columns["DPER"].Expression ="0";


                if (dtBillContribution.Rows.Count > 0)
                {
                    DataRow drTemp = dtBillContribution.NewRow();
                    drTemp["TYP"] = 0;
                    drTemp["CPAY"] = 0;// DsCreditBillDetails.Tables[1].Compute("Sum([CPAY])","");				
                    drTemp["DPAY"] = 0;

                    if (BType == 1)
                    {
                        dtBillContribution.Columns.Remove("PPAY");
                        dtBillContribution.Columns["SPAY"].ColumnName = "PPAY";
                        if (dtDiscountDetails != null && dtDiscountDetails.Rows.Count > 0)
                        {
                            //decimal Discount=Convert.ToDecimal(dtDiscountDetails.Compute("Sum([DIS])","")); 
                            drTemp["DIS"] = Convert.ToDecimal(dtDiscountDetails.Compute("Sum([DIS])", ""));
                            drTemp["DCOM"] = 0;
                            drTemp["DPAT"] = Convert.ToDecimal(DtBillSummary.Compute("Sum([DIS])", ""));
                            drTemp["DPER"] = 0;
                            drTemp["PPAY"] = Convert.ToDecimal(DtBillSummary.Compute("Sum([SPAY])", ""));
                        }
                        else
                        {
                            drTemp["DIS"] = 0;
                            drTemp["DCOM"] = 0;
                            drTemp["DPAT"] = 0;
                            drTemp["DPER"] = 0;
                            drTemp["PPAY"] = DtBillSummary.Compute("Sum([SPAY])", "");
                        }

                        drTemp["TOT"] = DtBillSummary.Compute("Sum([SPAY])", "");
                    }
                    else if (BType == 2)
                    {
                        if (dtCompanyCashBLItemDetails.Rows.Count > 0)
                        {
                            dtBillContribution.Columns.Remove("PPAY");
                            dtBillContribution.Columns["SPAY"].ColumnName = "PPAY";
                            if (dtDiscountDetails != null && dtDiscountDetails.Rows.Count > 0)
                            {
                                decimal Discount = Convert.ToDecimal(dtDiscountDetails.Compute("Sum([DIS])", ""));
                                drTemp["DIS"] = (decimal)Discount;
                                drTemp["DCOM"] = 0;
                                drTemp["DPAT"] = (decimal)Discount;
                                drTemp["DPER"] = 0;
                                drTemp["PPAY"] = Convert.ToDecimal(DtBillSummary.Compute("Sum([SPAY])", ""));
                            }
                            else
                            {
                                drTemp["DIS"] = 0;
                                drTemp["DCOM"] = 0;
                                drTemp["DPAT"] = 0;
                                drTemp["DPER"] = 0;
                                drTemp["PPAY"] = DtBillSummary.Compute("Sum([SPAY])", "");
                            }
                            //drTemp["PPAY"] = DsCreditBillDetails.Tables[1].Compute("Sum([SPAY])","");
                            drTemp["TOT"] = DtBillSummary.Compute("Sum([SPAY])", "");
                        }
                    }
                    dtBillContribution.Rows.Add(drTemp);
                    dtBillContribution.AcceptChanges();
                }
                return dtBillContribution;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in GetCashContributions", "");
                return null;
            }
        }

        public DataSet GetCompanyDetails(int intCompanyid, string strCompanyName, string strCompanyType, string strTableid, int intUserId, int intWorkstationid, int intError, string strComapnyName2L, int HospitalID)
        {
            ContractMgmtServiceContractClient objContractMgtClient = new ContractMgmtServiceContractClient();
            try
            {

                return objContractMgtClient.GetHospitalCompanyDetails(intCompanyid, strCompanyName, strCompanyType, strTableid, intUserId, intWorkstationid, intError, strComapnyName2L, HospitalID);
            }

            finally
            {
                objContractMgtClient.Close();
            }
        }

        public DataSet NewFetchPatientFile(int EpisodeID, int VisitID, int MonitorID, string TBL, int intUserId, int intWorkStationId)
        {
            FrontOfficeServiceContractClient objFOServiceClient = new FrontOfficeServiceContractClient();
            DataSet dsPatientFile = new DataSet();
            try
            {
                dsPatientFile = objFOServiceClient.NewFetchPatientFile(EpisodeID, VisitID, MonitorID, TBL, intUserId, intWorkStationId);
            }
            finally
            {
                objFOServiceClient.Close();
            }
            return dsPatientFile;
        }

        public int SaveTempDefaultLOAContribution(DataTable dtLOAContribution, string strSessionId, string strAdmissionUHID, int intPatientType, int intBillType, int intPatienId)
        {
            try
            {
                objFOClient = new FrontOfficeServiceContractClient();
                return objFOClient.SaveTempLOAContribution(dtLOAContribution, strSessionId, strAdmissionUHID, intPatientType, intBillType, intPatienId);

            }

            finally
            {
                objFOClient.Close();
            }
        }

        private DataTable GetAvailDiscount(decimal Amount, string BillType)
        {

            try
            {
                string strDiscFilter = string.Empty;

                int DiscountId = 0; int DiscLevel = -2; string DiscName = string.Empty;

                DataTable dtPredefined = PredefinedDiscount.Copy(); dtPredefined.AcceptChanges();
                if (string.IsNullOrEmpty(strGradeID))
                    strGradeID = "0";


                if (dtPredefined == null || dtPredefined.Rows.Count == 0)
                    return dtContDiscountDetails;

                if (hdnMaternityConfig != "0" && hdnSPCMaternityConfig != "0" && hdnIsPregnent == "1" && !string.IsNullOrEmpty(hdnEDD) && dtPredefined.Select("DiscountSpecialiseId = " + Convert.ToInt32(hdnSPCMaternityConfig)).Length > 0)
                    strDiscFilter = "PatientType=1 and GradeID=" + Convert.ToInt32(strGradeID) + " and DiscountSpecialiseId = " + Convert.ToInt32(hdnSPCMaternityConfig);
                else if (dtPredefined.Select("DiscountSpecialiseId = " + Convert.ToInt32(hdnDocSpecialiseId)).Length > 0)
                    strDiscFilter = "PatientType=1 and GradeID=" + Convert.ToInt32(strGradeID) + " and DiscountSpecialiseId = " + Convert.ToInt32(hdnDocSpecialiseId);
                else
                    strDiscFilter = "PatientType=1 and GradeID=" + Convert.ToInt32(strGradeID) + " and DiscountSpecialiseId = " + "-1";


                DataRow[] drRow = dtPredefined.Select(strDiscFilter);
                if (drRow.Length > 0)
                {
                    DiscountId = Convert.ToInt32(drRow[0]["DiscountId"]);

                    DiscLevel = Convert.ToInt32((drRow[0]["DiscountLevel"] == DBNull.Value ? -1 : drRow[0]["DiscountLevel"]));

                    DiscName = (drRow[0]["DiscountName"]).ToString();
                }
                if (Convert.ToInt16(DiscLevel) > -1)
                {

                    DataSet dsCommon = GetDiscountsConfig(DiscountId, "null", 1157, 5, "FetchDiscountsMaster", Convert.ToInt32(0), Convert.ToInt32(strDefWorkstationId), 0);
                    DataSet dsreturn = GetDiscountConfigNew(dsCommon, DiscountId, Convert.ToDouble(Amount));
                    DataTable dtBillDetails = dtCashDiscounts.Copy();
                    dtBillDetails.AcceptChanges();
                    if (CollectableType == 3)
                    {
                        DataTable dtTotCreditContr = dtCRContribution.Copy();
                        dtTotCreditContr.AcceptChanges();
                        for (int intmlevelcnt = 2; intmlevelcnt <= 5; intmlevelcnt++)
                        {

                            DataRow[] drDiscDet = dtBillDetails.Select("mlevel =" + intmlevelcnt);

                            if (drDiscDet.Length > 0 && intmlevelcnt == 2)
                            {
                                foreach (DataRow drDiscDet1 in drDiscDet)
                                {
                                    object dblPPAY = dtTotCreditContr.Compute("  Sum(PPAY) ", " typ = 5 and SID=" + drDiscDet1["serviceID"].ToString());
                                    if (dblPPAY != DBNull.Value)
                                    {
                                        if (Convert.ToDecimal(dblPPAY) > 0)
                                        {
                                            DataRow[] drbDet = dtBillDetails.Select("mlevel =" + intmlevelcnt + " and serviceID=" + drDiscDet1["serviceid"]);
                                            if (drbDet.Length > 0)
                                            {
                                                foreach (DataRow drrCRdon in drbDet)
                                                {
                                                    drrCRdon["Cpay"] = Convert.ToDecimal(drrCRdon["Cpay"]) - Convert.ToDecimal(dblPPAY);
                                                }
                                            }
                                        }
                                        dtBillDetails.AcceptChanges();
                                    }
                                }
                            }
                            // department level
                            if (drDiscDet.Length > 0 && intmlevelcnt == 3)
                            {
                                foreach (DataRow drDiscDet1 in drDiscDet)
                                {
                                    // DataRow[] drDPAY = dtTotCreditContr.Select("mlevel=5 and serviceID=" + drDiscDet1["serviceID"].ToString());
                                    object dblPPAY = dtTotCreditContr.Compute("  Sum(PPAY) ", " typ = 5 and SID=" + drDiscDet1["serviceID"].ToString() + " and " + "DID=" + drDiscDet1["Hospdeptid"].ToString());

                                    if (dblPPAY != DBNull.Value)
                                    {
                                        if (Convert.ToDecimal(dblPPAY) > 0)
                                        {
                                            DataRow[] drbDet = dtBillDetails.Select("mlevel =" + intmlevelcnt + " and serviceID=" + drDiscDet1["serviceid"] + " and " + "Hospdeptid=" + drDiscDet1["Hospdeptid"].ToString());
                                            if (drbDet.Length > 0)
                                            {
                                                foreach (DataRow drrCRdon in drbDet)
                                                {
                                                    drrCRdon["Cpay"] = Convert.ToDecimal(drrCRdon["Cpay"]) - Convert.ToDecimal(dblPPAY);
                                                }
                                            }
                                        }
                                        dtBillDetails.AcceptChanges();
                                    }
                                }
                            }

                            // specialisation level

                            if (drDiscDet.Length > 0 && intmlevelcnt == 4)
                            {
                                foreach (DataRow drDiscDet1 in drDiscDet)
                                {
                                    // DataRow[] drDPAY = dtTotCreditContr.Select("mlevel=5 and serviceID=" + drDiscDet1["serviceID"].ToString());
                                    object dblPPAY = dtTotCreditContr.Compute("  Sum(PPAY) ", " typ = 5 and SID=" + drDiscDet1["serviceID"].ToString() + " and " + "DID=" + drDiscDet1["Hospdeptid"].ToString() + " and SPID=" + drDiscDet1["specialiseid"].ToString());

                                    if (dblPPAY != DBNull.Value)
                                    {
                                        if (Convert.ToDecimal(dblPPAY) > 0)
                                        {
                                            DataRow[] drbDet = dtBillDetails.Select("mlevel =" + intmlevelcnt + " and serviceID=" + drDiscDet1["serviceid"] + " and " + "Hospdeptid=" + drDiscDet1["Hospdeptid"].ToString() + " and specialiseid= " + drDiscDet1["specialiseid"].ToString());
                                            if (drbDet.Length > 0)
                                            {
                                                foreach (DataRow drrCRdon in drbDet)
                                                {
                                                    drrCRdon["Cpay"] = Convert.ToDecimal(drrCRdon["Cpay"]) - Convert.ToDecimal(dblPPAY);
                                                }
                                            }
                                        }
                                        dtBillDetails.AcceptChanges();
                                    }
                                }
                            }

                            // item level
                            if (drDiscDet.Length > 0 && intmlevelcnt == 5)
                            {
                                foreach (DataRow drDiscDet1 in drDiscDet)
                                {
                                    // DataRow[] drDPAY = dtTotCreditContr.Select("mlevel=5 and serviceID=" + drDiscDet1["serviceID"].ToString());
                                    object dblPPAY = dtTotCreditContr.Compute("  Sum(PPAY) ", " typ = 5 and SIID=" + drDiscDet1["serviceitemid"].ToString());

                                    if (dblPPAY != DBNull.Value)
                                    {
                                        if (Convert.ToDecimal(dblPPAY) > 0)
                                        {
                                            DataRow[] drbDet = dtBillDetails.Select("mlevel =" + intmlevelcnt + " and serviceitemid=" + drDiscDet1["serviceitemid"].ToString());
                                            if (drbDet.Length > 0)
                                            {
                                                foreach (DataRow drrCRdon in drbDet)
                                                {
                                                    drrCRdon["Cpay"] = Convert.ToDecimal(drrCRdon["Cpay"]) - Convert.ToDecimal(dblPPAY);
                                                }
                                            }
                                        }
                                        dtBillDetails.AcceptChanges();
                                    }
                                }
                            }
                        }
                        #region Commented

                        #endregion
                    }
                    DataTable dtServices = Service.Copy();
                    dtServices.AcceptChanges();
                    DataTable dtDiscountDetails = ProcessDiscountConfigurationLatest(DiscLevel, dtBillDetails, dtServices, 2, dsreturn, BillType, DiscName, "1", Convert.ToDecimal(Amount));
                    hdnDiscountID = DiscountId.ToString();
                    bool bIsDiscDetailsAvailable = (dtDiscountDetails != null && dtDiscountDetails.Rows.Count > 0) ? true : false;
                    if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                    {
                        if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES" & bIsDiscDetailsAvailable)
                        {
                            decimal disPPay = Convert.ToDecimal(dtDiscountDetails.Compute("Sum([DPAT])", " "));
                            decimal disCPay = Convert.ToDecimal(dtDiscountDetails.Compute("Sum([DCOM])", " "));
                            decimal actualPDIS = 0;
                            decimal actualCDIS = 0;
                            foreach (DataRow dr in dtDiscountDetails.Select())
                            {
                                if (dr["DCOM"] != null)
                                {
                                    dr["DCOM"] = Math.Round(Convert.ToDecimal(dr["DCOM"]), 5);
                                    actualCDIS += Convert.ToDecimal(dr["DCOM"]);
                                }
                                if (dr["DPAT"] != null)
                                {
                                    dr["DPAT"] = Math.Round(Convert.ToDecimal(dr["DPAT"]), 5);
                                    actualPDIS += Convert.ToDecimal(dr["DPAT"]);
                                }
                            }

                            DataRow[] drdisc = dtDiscountDetails.Select();
                            int length = dtDiscountDetails.Rows.Count;
                            if (Math.Abs(actualCDIS - disCPay) < 1)
                            {
                                drdisc[length - 1]["DCOM"] = Convert.ToDecimal(drdisc[length - 1]["DCOM"]) - (actualCDIS - disCPay);
                                drdisc[length - 1]["DPAT"] = Convert.ToDecimal(drdisc[length - 1]["DPAT"]) - (actualPDIS - disPPay);
                            }
                        }
                    }

                    if (CollectableType == 3)
                    {
                        foreach (DataRow drFinalRow in dtDiscountDetails.Rows)
                        {
                            drFinalRow["Total"] = DTTemp.Select("ProcedureId=" + drFinalRow["ServiceItemID"].ToString())[0]["Price"].ToString();


                        }
                        dtDiscountDetails.AcceptChanges();

                    }
                    // DataTable dtDiscountDetails = AvailDiscountConfig.ProcessDiscountConfigurationLatest(intDiscountlevel, dtCashDiscounts, dtServices, 2, dsreturn, strBillType, strDiscountName, "1", Convert.ToDecimal(dblCompanyPay));//IPBILLING METHOD
                    if (dtDiscountDetails != null && dtDiscountDetails.Rows.Count > 0)
                    {
                        dtContDiscountDetails.Columns.Add("TYP", typeof(int));
                        dtContDiscountDetails.Columns.Add("SID", typeof(int));
                        dtContDiscountDetails.Columns.Add("DID", typeof(int));
                        dtContDiscountDetails.Columns.Add("SPID", typeof(int));
                        dtContDiscountDetails.Columns.Add("SIID", typeof(int));
                        dtContDiscountDetails.Columns.Add("TOT", typeof(decimal));
                        dtContDiscountDetails.Columns.Add("CPAY", typeof(decimal));
                        dtContDiscountDetails.Columns.Add("PPAY", typeof(decimal));
                        dtContDiscountDetails.Columns.Add("DCOM", typeof(decimal));
                        dtContDiscountDetails.Columns.Add("DPAT", typeof(decimal));
                        dtContDiscountDetails.Columns.Add("DIS", typeof(decimal));
                        dtContDiscountDetails.Columns.Add("DPER", typeof(decimal));
                        dtContDiscountDetails.Columns.Add("QTY", typeof(int));
                        dtContDiscountDetails.Columns.Add("BQTY", typeof(int));
                        dtContDiscountDetails.Columns.Add("SEQ", typeof(int));
                        dtContDiscountDetails.Columns.Add("AuthorisedBy", typeof(string));
                        dtContDiscountDetails.Columns.Add("AuthorisedId", typeof(int));
                        dtContDiscountDetails.Columns.Add("Remarks", typeof(string));

                        DataRow[] drContDiscountDetails = dtDiscountDetails.Select();
                        foreach (DataRow drCon in drContDiscountDetails)
                        {
                            dtContDiscountDetails.Rows.Add(new object[] { Convert.ToInt32(drCon["mLevel"] == DBNull.Value ? 0 : drCon["mLevel"]), Convert.ToInt32(drCon["ServiceId"] == DBNull.Value ? 0 : drCon["ServiceId"]), Convert.ToInt32(drCon["HospDeptID"] == DBNull.Value ? 0 : drCon["HospDeptID"]), Convert.ToInt32(drCon["SpecialiseID"] == DBNull.Value ? 0 : drCon["SpecialiseID"]), Convert.ToInt32(drCon["ServiceItemID"] == DBNull.Value ? 0 : drCon["ServiceItemID"]), Convert.ToDecimal(drCon["Total"] == DBNull.Value ? 0 : drCon["Total"]), Convert.ToDecimal(drCon["CPAY"] == DBNull.Value ? 0 : drCon["CPAY"]), Convert.ToDecimal(drCon["PPAY"] == DBNull.Value ? 0 : drCon["PPAY"]), Convert.ToDecimal(drCon["DCOM"] == DBNull.Value ? 0 : drCon["DCOM"]), Convert.ToDecimal(drCon["DPAT"] == DBNull.Value ? 0 : drCon["DPAT"]), Convert.ToDecimal(drCon["Discount"] == DBNull.Value ? 0 : drCon["Discount"]), Convert.ToDecimal(drCon["DPER"] == DBNull.Value ? 0 : drCon["DPER"]), Convert.ToInt32(drCon["QTY"] == DBNull.Value ? 0 : drCon["QTY"]), Convert.ToInt32(drCon["BQTY"] == DBNull.Value ? 0 : drCon["BQTY"]), Convert.ToInt32(drCon["SEQ"] == DBNull.Value ? 0 : drCon["SEQ"]), "", 0, "" });
                        }

                        DataRow[] drTypeZero = dtContDiscountDetails.Select("TYP=0");
                        if (drTypeZero.Length == 0)
                        {
                            dtContDiscountDetails.Rows.Add(new object[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "", 0, "" });
                        }
                        //BindDiscountData(dtContDiscountDetails);

                        AvailDiscount = dtContDiscountDetails.Copy();
                        AvailDiscount.AcceptChanges();
                        return dtContDiscountDetails;
                    }

                }
                return dtContDiscountDetails;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in GetAvailDiscount", "");
                return null;
            }
            finally
            {

            }
        }


        private void CalculateVATForCredit(DataTable dtBillContributions)
        {
            decimal CPAYFinal = 0, CPAY = 0;
            decimal tempcvat = 0, VATPercOnCPAY = 0;
            decimal temppvat = 0, PPAY = 0, VATPercOnPPAY = 0;
            decimal PPAYFinal = 0;
            DataTable dtdisc = new DataTable();
            DataTable dtBDPopUP = BDPopUP.Copy();
            dtBDPopUP.AcceptChanges();
            if (!dtBDPopUP.Columns.Contains("DISCOM"))
                dtBDPopUP.Columns.Add("DISCOM");
            if (!dtBDPopUP.Columns.Contains("DISPAT"))
                dtBDPopUP.Columns.Add("DISPAT");
            if (!dtBDPopUP.Columns.Contains("TDISCOUNT"))
                dtBDPopUP.Columns.Add("TDISCOUNT", typeof(decimal));
            if (!dtBDPopUP.Columns.Contains("TVAT"))
                dtBDPopUP.Columns.Add("TVAT", typeof(decimal));

            if (DtDiscountDetails != null)
                dtdisc = DtDiscountDetails.Copy();
            dtdisc.AcceptChanges();

            bool blnIsSaud = false;

            if (HISCONFIG != null)
            {
                DataTable dtconfig = HISCONFIG.Copy();
                dtconfig.AcceptChanges();
                DataRow[] drnat = dtconfig.Select("Parameter ='Host_Nationality'");
                if (drnat[0]["Value"].ToString() == hdnNationalityId)// need to implement and assign value
                    blnIsSaud = true;
            }
            foreach (DataRow dr in dtBillContributions.Select())
            {
                DataRow[] drtemp;
                Decimal discom = 0, dispat = 0;
                if (DtDiscountDetails == null || dtdisc.Rows.Count == 0)
                {
                    CPAY = Convert.ToDecimal(dr["CPAY"]);
                    PPAY = Convert.ToDecimal(dr["PPAY"]);
                    if (hdnrblbilltypecash == true & !string.IsNullOrEmpty(dr["Quantity"].ToString()))
                        PPAY = Convert.ToDecimal(dr["PPAY"]) * Convert.ToDecimal(dr["Quantity"]); ;
                }
                else
                {
                    if (dr["TYP"].ToString() == "5")
                    {
                        drtemp = dtdisc.Select("TYP=5 and SID=" + dr["SID"] + " and SIID=" + dr["SIID"]);
                        if (drtemp.Length > 0)
                        {
                            CPAY = Convert.ToDecimal(dr["CPAY"]) - Convert.ToDecimal(Convert.ToDecimal((drtemp.CopyToDataTable().Compute("SUM(DCOM)", ""))).ToString(hdnsCurrencyFormat));
                            PPAY = Convert.ToDecimal(dr["PPAY"]) - Convert.ToDecimal(Convert.ToDecimal((drtemp.CopyToDataTable().Compute("SUM(DPAT)", ""))).ToString(hdnsCurrencyFormat));
                            discom = Convert.ToDecimal(drtemp[0]["DCOM"]);
                            dispat = Convert.ToDecimal(drtemp[0]["DPAT"]);
                        }
                        else
                        {
                            CPAY = Convert.ToDecimal(dr["CPAY"]);
                            PPAY = Convert.ToDecimal(dr["PPAY"]);
                            if (hdnrblbilltypecash == true)
                                PPAY = Convert.ToDecimal(dr["PPAY"]) * Convert.ToDecimal(dr["Quantity"]); ;
                        }
                    }
                    else
                    {
                        CPAY = Convert.ToDecimal(dr["CPAY"]);
                        PPAY = Convert.ToDecimal(dr["PPAY"]);
                        if (hdnrblbilltypecash == true)
                            PPAY = Convert.ToDecimal(dr["PPAY"]) * Convert.ToDecimal(dr["Quantity"]);
                    }
                }

                if (string.IsNullOrEmpty(dr["VAT"].ToString()))//|| blnIsSaud)
                    dr["VAT"] = 0;
                VATPercOnCPAY = Convert.ToDecimal((Convert.ToDecimal(dr["VAT"]) * CPAY / 100).ToString(hdnsCurrencyFormat));
                dr["CVAT"] = VATPercOnCPAY;
                if (string.IsNullOrEmpty(dr["isSaudi"].ToString()))
                    dr["isSaudi"] = false;

                VATPercOnPPAY = Convert.ToDecimal((Convert.ToDecimal(dr["VAT"]) * PPAY / 100).ToString(hdnsCurrencyFormat));
                dr["PVAT"] = VATPercOnPPAY;

                if (blnIsSaud & !Convert.ToBoolean(dr["isSaudi"]))
                {
                    VATPercOnPPAY = 0;
                    dr["PVAT"] = 0;
                }

                if (dr["TYP"].ToString() == "5")
                {
                    foreach (DataRow drin in dtBDPopUP.Select("ServiceID=" + dr["SID"] + " and ProcedureID = " + dr["SIID"]))
                    {
                        drin["CPAY"] = CPAY.ToString(hdnsCurrencyFormat);
                        drin["PPAY"] = PPAY.ToString(hdnsCurrencyFormat);
                        drin["CVAT"] = VATPercOnCPAY.ToString(hdnsCurrencyFormat);
                        drin["PVAT"] = VATPercOnPPAY.ToString(hdnsCurrencyFormat);
                        drin["DISCOM"] = discom.ToString(hdnsCurrencyFormat);
                        drin["DISPAT"] = dispat.ToString(hdnsCurrencyFormat);
                        drin["TDISCOUNT"] = (discom + dispat).ToString(hdnsCurrencyFormat);
                        drin["TVAT"] = (VATPercOnCPAY + VATPercOnPPAY).ToString(hdnsCurrencyFormat);
                        //drin["Amount"] = Convert.ToDecimal(drin["quantity"]) * Convert.ToDecimal(drin["Price"]);   //Code changes made for TFS ID :: 165983
                        drin["Amount"] = drin["ServiceID"].ToString() == "50" ? Convert.ToDecimal(drin["Price"]) : Convert.ToDecimal(drin["quantity"]) * Convert.ToDecimal(drin["Price"]);
                    }

                    CPAY += VATPercOnCPAY;
                    CPAYFinal += CPAY;
                    tempcvat += VATPercOnCPAY;
                    PPAY += VATPercOnPPAY;
                    PPAYFinal += PPAY;
                    temppvat += VATPercOnPPAY;
                }

            }
            dtBDPopUP.AcceptChanges();
            hdnCVATValue = tempcvat.ToString();
            hdnPVATValue = temppvat.ToString();
            decimal TotalVATAmount = tempcvat + temppvat;
            hdnCPAYAfterVAT = CPAYFinal.ToString(hdnsCurrencyFormat);
            hdnPPAYAfterVAT = PPAYFinal.ToString(hdnsCurrencyFormat);
            hdnVATAmount = TotalVATAmount.ToString(hdnsCurrencyFormat);
            TotalVATAmount.ToString(hdnsCurrencyFormat);
            taxdiscount = dtBillContributions.Copy();
            BDPopUP = dtBDPopUP.Copy(); BDPopUP.AcceptChanges();
        }

        public DataSet GetTableDetails(string strTableName, string strColName, string strParam)
        {
            DataSet dsTempBillDetails = new DataSet();
            FrontOfficeServiceContractClient objfrontoffice = new FrontOfficeServiceContractClient();
            try
            {
                dsTempBillDetails = objfrontoffice.GetTableDetails(strTableName, strColName, strParam);
            }
            finally
            {
                objfrontoffice.Close();
            }
            return dsTempBillDetails;
        }

        private StringBuilder IsApprovalAvailable(DataTable dtTemp, DataTable dtAR)
        {
            try
            {
                StringBuilder strexclmsgitems = new StringBuilder("");
                foreach (DataRow dritem in dtTemp.Select())
                {
                    if (dtAR.Select("(ClaimStatusID=1 or ClaimStatusID=3 or ClaimStatusID=4) and ItemId=" + dritem["ProcedureId"]).Length == 0)
                    {
                        strexclmsgitems = strexclmsgitems.Append(dritem["Procedure"].ToString() + "<br/>");
                    }
                }
                return strexclmsgitems;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in IsApprovalAvailable", "");
                return null;
            }
        }


        private DataTable GetCreditContributions(DataTable dtBillDetails, int BType, DataTable DtBillSummary, DataTable dtDiscountDetails, DataTable DtCompanyCreditBLItemDetails)
        {
            DataTable dtBillContribution = new DataTable();
            try
            {
                dtBillContribution = dtBillDetails.Copy();

                //Renaming
                dtBillContribution.Columns["mLevel"].ColumnName = "TYP";
                dtBillContribution.Columns["ServiceId"].ColumnName = "SID";
                dtBillContribution.Columns["DeptID"].ColumnName = "DID";
                dtBillContribution.Columns["SpecialiseId"].ColumnName = "SPID";
                dtBillContribution.Columns["ProcedureID"].ColumnName = "SIID";
                dtBillContribution.Columns["Amount"].ColumnName = "TOT";
                dtBillContribution.Columns["ProcId"].ColumnName = "PRID";

                //Removing
                dtBillContribution.Columns.Remove("ServiceName");
                dtBillContribution.Columns.Remove("Procedure");
                dtBillContribution.Columns.Remove("Sample");
                dtBillContribution.Columns.Remove("SampleId");
                dtBillContribution.Columns.Remove("DeptName");
                dtBillContribution.Columns.Remove("BedTypeId");
                dtBillContribution.Columns.Remove("BedTypeName");
                dtBillContribution.Columns.Remove("SpecialiseName");
                dtBillContribution.Columns.Remove("OrderNo");
                dtBillContribution.Columns.Remove("OrderDate");
                dtBillContribution.Columns.Remove("IsGroup");
                //dtBillContribution.Columns.Remove("Qty");
                //dtBillContribution.Columns.Remove("Seq");
                dtBillContribution.Columns.Remove("ProfileId");
                dtBillContribution.Columns.Remove("TariffId");
                dtBillContribution.Columns.Remove("mBedTypeID");
                dtBillContribution.Columns.Remove("mServiceID");
                dtBillContribution.Columns.Remove("mHospDeptID");
                dtBillContribution.Columns.Remove("mSpecialiseID");
                dtBillContribution.Columns.Remove("mItemID");
                dtBillContribution.Columns.Remove("mTypeID");
                dtBillContribution.Columns.Remove("mTypeName");
                dtBillContribution.Columns.Remove("mChildID");
                dtBillContribution.Columns.Remove("mParentID");
                dtBillContribution.Columns.Remove("mLTID");
                dtBillContribution.Columns.Remove("mLimitAmt");

                dtBillContribution.Columns.Add("DCOM", typeof(decimal));
                dtBillContribution.Columns.Add("DPAT", typeof(decimal));
                dtBillContribution.Columns.Add("DIS", typeof(decimal));
                dtBillContribution.Columns.Add("DPER", typeof(decimal));

                if (BType == 1)
                {

                }
                if (dtBillContribution.Rows.Count > 0)
                {
                    decimal DcDiscountSum = 0;
                    if (BType == 2)
                    {
                        dtBillContribution = SplitCollectables(dtBillContribution, DtBillSummary, dtDiscountDetails);
                    }

                    decimal decPPay = 0;
                    decimal decPPayTemp = 0;
                    decimal decDPayTemp = 0;
                    decimal decPPayUsedForTotal = 0;
                    foreach (DataRow drowitem in dtBillContribution.Select("TYP=5"))
                    {
                        decPPayTemp = 0;
                        decDPayTemp = 0;
                        if (drowitem["PPAY"] != DBNull.Value && !string.IsNullOrEmpty(drowitem["PPAY"].ToString()))
                            decPPayTemp = Convert.ToDecimal(drowitem["PPAY"]);
                        if (drowitem["DPAY"] != DBNull.Value && !string.IsNullOrEmpty(drowitem["DPAY"].ToString()))
                            decDPayTemp = Convert.ToDecimal(drowitem["DPAY"]);
                        if (decPPayTemp > decDPayTemp)
                        {
                            decPPayTemp = Convert.ToDecimal(decPPayTemp) - Convert.ToDecimal(decDPayTemp);
                            decPPay += (decPPayTemp * Convert.ToDecimal(drowitem["Quantity"])) + Convert.ToDecimal(decDPayTemp);
                        }
                        else
                        {
                            decPPay += decPPayTemp;
                        }
                    }
                    decPPayUsedForTotal = Convert.ToDecimal(dtBillContribution.Compute("Sum([PPAY])", "TYP=5"));
                    DataRow drTemp = dtBillContribution.NewRow();
                    drTemp["TYP"] = 0;
                    if (dtDiscountDetails.Rows.Count > 0)
                    {
                        DataRow[] drr = dtDiscountDetails.Select("TYP=5");
                        if (drr.Length > 0)
                        {
                            DcDiscountSum = Convert.ToDecimal(dtDiscountDetails.Compute("Sum([DIS])", "TYP=5"));
                            drTemp["CPAY"] = DtBillSummary.Compute("Sum([CPAY])", "");
                            drTemp["DPAY"] = DtBillSummary.Compute("Sum([DPAY])", "");
                        }
                        else
                        {
                            drTemp["CPAY"] = DtBillSummary.Compute("Sum([CPAY])", "");
                            drTemp["DPAY"] = DtBillSummary.Compute("Sum([DPAY])", "");
                        }
                    }
                    else
                    {
                        drTemp["CPAY"] = DtBillSummary.Compute("Sum([CPAY])", "");
                        drTemp["DPAY"] = DtBillSummary.Compute("Sum([DPAY])", "");
                    }
                    string strYiaco = ConfigurationSettings.AppSettings["Yaico"].ToString();
                    if (strYiaco.ToUpper() == "YES")
                    {
                        drTemp["CPAY"] = RoundCorrect(Convert.ToDouble(drTemp["CPAY"]), 2);
                        drTemp["DPAY"] = RoundCorrect(Convert.ToDouble(drTemp["DPAY"]), 2);
                    }

                    if (BType == 2)
                    {
                        if (DtCompanyCreditBLItemDetails.Rows.Count > 0)
                        {
                            if (dtDiscountDetails.Rows.Count > 0)
                            {
                                drTemp["DIS"] = (decimal)DcDiscountSum;
                                drTemp["DCOM"] = (decimal)DcDiscountSum;
                                drTemp["DPAT"] = 0;
                                drTemp["DPER"] = 0;
                                drTemp["PPAY"] = decPPay;
                            }
                            else
                            {
                                drTemp["DIS"] = 0;
                                drTemp["DCOM"] = 0;
                                drTemp["DPAT"] = 0;
                                drTemp["DPER"] = 0;
                                drTemp["PPAY"] = decPPay;
                            }
                            decimal dectot = 0;
                            dectot = Convert.ToDecimal(dtBillContribution.Compute("Sum([TOT]) - Sum([PPAY])", "TYP=5"));
                            if (strYiaco.ToUpper() == "YES")
                            {
                                drTemp["CPAY"] = dectot;
                            }
                            drTemp["TOT"] = dectot + decPPayUsedForTotal;

                        }
                    }

                    dtBillContribution.Rows.Add(drTemp);
                }
                return dtBillContribution;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in GetCreditContributions", "");
                return null;
            }
        }

        public DataSet FetchTariffDetails(int TariffID, int intUserID, int intWorkStationID, int intError, int GradeID)
        {
            FrontOfficeServiceContractClient objFOServiceClient = new FrontOfficeServiceContractClient();
            DataSet dsRateAgreement = new DataSet();
            try
            {

                dsRateAgreement = objFOServiceClient.FetchTariffDetails(TariffID, intUserID, intWorkStationID, intError, GradeID);
            }

            finally
            {
                objFOServiceClient.Close();
            }
            return dsRateAgreement;
        }


        private void DisplayBillSummary(DataTable dtAvailDiscountDetails, decimal AvailDesposit)
        {
            decimal intBillAmount = 0;
            decimal dcPayerAmount = 0;
            decimal dcDiscountAmount = 0;
            DataTable dtSummaryF = dtSummary.Copy();
            dtSummaryF.AcceptChanges();
            DtDiscountDetails = dtAvailDiscountDetails.Copy();
            string strYiaco = ConfigurationManager.AppSettings["Yaico"].ToString();
            intBillAmount = Convert.ToDecimal(dtSummary.Rows[0]["Amount"]);
            DataTable DtBillSummary = CreateDtSummary();
            dcPayerAmount = Convert.ToDecimal(dtSummary.Rows[0]["CPAY"]);
            if (DtDiscountDetails != null && DtDiscountDetails.Rows.Count > 0)
                dcDiscountAmount = Convert.ToDecimal(DtDiscountDetails.Compute("Sum([DIS])", "TYP=5"));

            if (!string.IsNullOrEmpty(hdnVATAmount))
            {
                decimal totalBillAmount = Convert.ToDecimal(dtSummary.Rows[0]["Amount"]) + Convert.ToDecimal(hdnVATAmount);
                DtBillSummary.Rows[0]["Amount"] = totalBillAmount.ToString(hdnsCurrencyFormat);
            }
            else
            {
                DtBillSummary.Rows[0]["Amount"] = Convert.ToDecimal(dtSummary.Rows[0]["Amount"]).ToString(hdnsCurrencyFormat);
            }
            if (dcDiscountAmount > 0)
            {
                if (hdnrblbilltypecash)
                { DtBillSummary.Rows[1]["Amount"] = Convert.ToDecimal(0).ToString(hdnsCurrencyFormat); }
                else if (hdnrblbilltypeCredit == true)
                {
                    if (!string.IsNullOrEmpty(hdnCPAYAfterVAT))
                        dtSummary.Rows[0]["CPAY"] = hdnCPAYAfterVAT;
                    if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                    {
                        if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                            DtBillSummary.Rows[1]["Amount"] = (Convert.ToDecimal(dtSummary.Rows[0]["CPAY"])).ToString(hdnsCurrencyFormat);
                        else
                            DtBillSummary.Rows[1]["Amount"] = (Convert.ToDecimal(dtSummary.Rows[0]["CPAY"])).ToString(hdnsCurrencyFormat);
                    }
                    else
                        DtBillSummary.Rows[1]["Amount"] = (Convert.ToDecimal(dtSummary.Rows[0]["CPAY"])).ToString(hdnsCurrencyFormat);
                }
                DtBillSummary.Rows[2]["Amount"] = Convert.ToDecimal(DtDiscountDetails.Compute("Sum([DIS])", "TYP=5")).ToString(hdnsCurrencyFormat);
            }
            else
            {
                if (hdnrblbilltypecash)
                { DtBillSummary.Rows[1]["Amount"] = Convert.ToDecimal(0).ToString(hdnsCurrencyFormat); }
                else if (hdnrblbilltypeCredit == true)
                {
                    if (!string.IsNullOrEmpty(hdnCPAYAfterVAT))
                        dtSummary.Rows[0]["CPAY"] = hdnCPAYAfterVAT;
                    DtBillSummary.Rows[1]["Amount"] = Convert.ToDecimal(dtSummary.Rows[0]["CPAY"]).ToString(hdnsCurrencyFormat);
                }
                DtBillSummary.Rows[2]["Amount"] = Convert.ToDecimal(0).ToString(hdnsCurrencyFormat);//DiscountAmount
            }

            if ((Convert.ToDecimal(AvailDesposit) >= Convert.ToDecimal(Convert.ToDecimal(DtBillSummary.Rows[0]["Amount"]) - (Convert.ToDecimal(DtBillSummary.Rows[1]["Amount"]) + Convert.ToDecimal(DtBillSummary.Rows[2]["Amount"])))))
            { DtBillSummary.Rows[4]["Amount"] = Convert.ToDecimal(Convert.ToDecimal(DtBillSummary.Rows[0]["Amount"]) - (Convert.ToDecimal(DtBillSummary.Rows[1]["Amount"]) + Convert.ToDecimal(DtBillSummary.Rows[2]["Amount"]))).ToString(hdnsCurrencyFormat); }
            else if ((Convert.ToDecimal(AvailDesposit) < Convert.ToDecimal(Convert.ToDecimal(DtBillSummary.Rows[0]["Amount"]) - (Convert.ToDecimal(DtBillSummary.Rows[1]["Amount"]) + Convert.ToDecimal(DtBillSummary.Rows[2]["Amount"])))))
            { DtBillSummary.Rows[4]["Amount"] = Convert.ToDecimal(AvailDesposit).ToString(hdnsCurrencyFormat); }
            else if (Convert.ToDecimal(AvailDesposit) == 0)
            { DtBillSummary.Rows[4]["Amount"] = Convert.ToDecimal(0).ToString(hdnsCurrencyFormat); }
            DtBillSummary.Rows[5]["Amount"] = Convert.ToDecimal(0).ToString(hdnsCurrencyFormat);
            DtBillSummary.Rows[6]["Amount"] = Convert.ToDecimal(0).ToString(hdnsCurrencyFormat);
            DtBillSummary.Rows[3]["Amount"] = hdnVATAmount;
            DtBillSummary.Rows[7]["Amount"] = Convert.ToDecimal(Convert.ToDecimal(DtBillSummary.Rows[0]["Amount"]) - (Convert.ToDecimal(DtBillSummary.Rows[1]["Amount"]) +
                                                                                                Convert.ToDecimal(DtBillSummary.Rows[2]["Amount"]) +
                                                                                        Convert.ToDecimal(DtBillSummary.Rows[4]["Amount"]) +
                                                                                                Convert.ToDecimal(DtBillSummary.Rows[5]["Amount"]) +
                                                                                                Convert.ToDecimal(DtBillSummary.Rows[6]["Amount"]))).ToString(hdnsCurrencyFormat);


            DtBillSummary.Rows[8]["Amount"] = Convert.ToDecimal(dtSummary.Rows[0]["DPAY"]).ToString(hdnsCurrencyFormat);
            BlanceAomunt = Convert.ToDecimal(Convert.ToDecimal(DtBillSummary.Rows[7]["Amount"]).ToString(hdnsCurrencyFormat));
            BAmount = BlanceAomunt;
            txtamount = BlanceAomunt.ToString(hdnsCurrencyFormat);

            #region Rounding Amounts

            decimal decamount = Convert.ToDecimal(DtBillSummary.Rows[0]["Amount"]);
            decimal totalamount = Convert.ToDecimal(DtBillSummary.Rows[1]["Amount"]) + Convert.ToDecimal(DtBillSummary.Rows[2]["Amount"]) + Convert.ToDecimal(DtBillSummary.Rows[0]["Amount"]);
            if (decamount > totalamount)
            {

            }
            #endregion

            DtBillSummary.AcceptChanges();
            if (ConfigurationManager.AppSettings["TAXtype"] != null)
            {
                if (Convert.ToInt32(ConfigurationManager.AppSettings["TAXtype"]) != 1)
                {
                    DtBillSummary.Rows.RemoveAt(3);
                }
            }
            BillSummary = DtBillSummary;
            BillSummary.AcceptChanges();
            Decimal decAmount = Convert.ToDecimal(txtamount);
            if (dtAvailDiscountDetails != null && dtAvailDiscountDetails.Rows.Count > 0)
            {
                DataTable dtItemSplitDetails = null;
                DataRow[] rowCheck = null;
                DataRow[] rowCheckDiscount = null;
                Decimal DiscountUnCoveredItems = 0;
                Decimal SplitedUncoverdDiscount = 0;
                Decimal TotalCPayAmount = 0;

                if (ItemSplitDetails != null)
                {
                    dtItemSplitDetails = ItemSplitDetails.Copy();
                    dtItemSplitDetails.AcceptChanges();
                    dtItemSplitDetails.Rows.RemoveAt(dtItemSplitDetails.Rows.Count - 1);
                    if (dtItemSplitDetails != null && dtItemSplitDetails.Rows.Count > 0)
                    {
                        for (int rowindex1 = 0; rowindex1 < dtItemSplitDetails.Rows.Count; rowindex1++)
                        {
                            if (hdnrblbilltypecash)
                            {
                                dtItemSplitDetails.Rows[rowindex1]["SpaySplit"] = Convert.ToDecimal(dtItemSplitDetails.Rows[rowindex1]["SpaySplit"]) + Convert.ToDecimal(dtItemSplitDetails.Rows[rowindex1]["DiscSplit"]);
                            }
                            else
                            {
                                dtItemSplitDetails.Rows[rowindex1]["CpaySplit"] = Convert.ToDecimal(dtItemSplitDetails.Rows[rowindex1]["CpaySplit"]) + Convert.ToDecimal(dtItemSplitDetails.Rows[rowindex1]["DiscSplit"]);
                                if (Convert.ToDecimal(dtItemSplitDetails.Rows[rowindex1]["CpaySplit"]) <= 0)
                                {
                                    rowCheckDiscount = dtAvailDiscountDetails.Select("SIID='" + dtItemSplitDetails.Rows[rowindex1]["ProcedureId"].ToString() + "'");
                                    if (rowCheckDiscount != null && rowCheckDiscount.Length > 0)
                                    {
                                        DiscountUnCoveredItems += Convert.ToDecimal(rowCheckDiscount[0]["DIS"]);
                                    }
                                }
                                else
                                {

                                }
                                TotalCPayAmount += Convert.ToDecimal(dtItemSplitDetails.Rows[rowindex1]["CpaySplit"]);
                            }
                            dtItemSplitDetails.Rows[rowindex1]["DiscSplit"] = 0;
                            dtItemSplitDetails.Rows[rowindex1]["BalanceSplit"] = Convert.ToDecimal(dtItemSplitDetails.Rows[rowindex1]["PpaySplit"]) + Convert.ToDecimal(dtItemSplitDetails.Rows[rowindex1]["SpaySplit"]) + Convert.ToDecimal(dtItemSplitDetails.Rows[rowindex1]["DpaySplit"]);
                            dtItemSplitDetails.Rows[rowindex1]["AmountSplit"] = Convert.ToDecimal(dtItemSplitDetails.Rows[rowindex1]["PpaySplit"]) + Convert.ToDecimal(dtItemSplitDetails.Rows[rowindex1]["SpaySplit"]) + Convert.ToDecimal(dtItemSplitDetails.Rows[rowindex1]["DpaySplit"]) + Convert.ToDecimal(dtItemSplitDetails.Rows[rowindex1]["CpaySplit"]) + Convert.ToDecimal(dtItemSplitDetails.Rows[rowindex1]["DiscSplit"]);
                        }
                    }

                    for (int rowindex = 0; rowindex < dtAvailDiscountDetails.Rows.Count; rowindex++)
                    {
                        if (dtAvailDiscountDetails.Rows[rowindex]["SIID"] != DBNull.Value && !string.IsNullOrEmpty(dtAvailDiscountDetails.Rows[rowindex]["SIID"].ToString()))
                        {

                            string strFilter = string.Empty;
                            if (Convert.ToInt32(dtAvailDiscountDetails.Rows[rowindex]["SIID"]) > 0)
                            {
                                strFilter = "ServiceId='" + Convert.ToString(dtAvailDiscountDetails.Rows[rowindex]["SID"]) + "' ";
                                strFilter += " AND DeptId='" + Convert.ToString(dtAvailDiscountDetails.Rows[rowindex]["DID"]) + "'";
                                strFilter += " AND SpecialiseId='" + Convert.ToString(dtAvailDiscountDetails.Rows[rowindex]["SPID"]) + "'";
                                strFilter += " AND ProcedureId='" + Convert.ToString(dtAvailDiscountDetails.Rows[rowindex]["SIID"]) + "'";
                            }
                            else if (Convert.ToInt32(dtAvailDiscountDetails.Rows[rowindex]["SPID"]) > 0)
                            {
                                strFilter = "ServiceId='" + Convert.ToString(dtAvailDiscountDetails.Rows[rowindex]["SID"]) + "' ";
                                strFilter += " AND DeptId='" + Convert.ToString(dtAvailDiscountDetails.Rows[rowindex]["DID"]) + "'";
                                strFilter += " AND SpecialiseId='" + Convert.ToString(dtAvailDiscountDetails.Rows[rowindex]["SPID"]) + "'";
                            }
                            else if (Convert.ToInt32(dtAvailDiscountDetails.Rows[rowindex]["DID"]) > 0)
                            {
                                strFilter = "ServiceId='" + Convert.ToString(dtAvailDiscountDetails.Rows[rowindex]["SID"]) + "' ";
                                strFilter += " AND DeptId='" + Convert.ToString(dtAvailDiscountDetails.Rows[rowindex]["DID"]) + "'";

                            }
                            else if (Convert.ToInt32(dtAvailDiscountDetails.Rows[rowindex]["SID"]) > 0)
                            {
                                strFilter = "ServiceId='" + Convert.ToString(dtAvailDiscountDetails.Rows[rowindex]["SID"]) + "' ";
                            }
                            if (strFilter != string.Empty)
                            {

                                rowCheck = dtItemSplitDetails.Select(strFilter);
                                if (rowCheck != null && rowCheck.Length > 0)
                                {
                                    Decimal TotalAmount = 0;
                                    for (int i = 0; i < rowCheck.Length; i++)
                                    {
                                        if (hdnrblbilltypecash)
                                        {
                                            TotalAmount += (Decimal)rowCheck[i]["SpaySplit"];
                                        }
                                        else
                                        {
                                            TotalAmount += (Decimal)rowCheck[i]["CpaySplit"];
                                        }
                                    }
                                    Decimal TotalDiscount = Convert.ToDecimal(dtAvailDiscountDetails.Rows[rowindex]["DIS"]);
                                    Decimal Perc = 0;
                                    if (TotalAmount > 0)
                                        Perc = (TotalDiscount / TotalAmount) * 100;
                                    Decimal SingleItemDisc = 0;
                                    Decimal UncoveredItemDiscPerc = 0;
                                    for (int i = 0; i < rowCheck.Length; i++)
                                    {
                                        SingleItemDisc = 0;
                                        UncoveredItemDiscPerc = 0;
                                        SplitedUncoverdDiscount = 0;
                                        if (hdnrblbilltypeCredit == true)
                                        {
                                            if (Convert.ToDecimal(rowCheck[i]["CpaySplit"]) > 0)
                                            {
                                                SingleItemDisc = (Convert.ToDecimal(rowCheck[i]["CpaySplit"]) / 100) * Perc;
                                                if (TotalCPayAmount > 0)
                                                    UncoveredItemDiscPerc = (Convert.ToDecimal(rowCheck[i]["CpaySplit"]) / TotalCPayAmount) * 100;
                                                SplitedUncoverdDiscount = (DiscountUnCoveredItems / 100) * UncoveredItemDiscPerc;

                                                rowCheck[i]["DiscSplit"] = SingleItemDisc + SplitedUncoverdDiscount;
                                                rowCheck[i]["CpaySplit"] = Convert.ToDecimal(rowCheck[i]["CpaySplit"]) - (SingleItemDisc + SplitedUncoverdDiscount);
                                            }
                                            else
                                            {
                                                rowCheck[i]["DiscSplit"] = 0;
                                                rowCheck[i]["CpaySplit"] = Convert.ToDecimal(rowCheck[i]["CpaySplit"]);
                                            }
                                        }
                                        else
                                        {
                                            SingleItemDisc = (Convert.ToDecimal(rowCheck[i]["SpaySplit"]) / 100) * Perc;
                                            rowCheck[i]["DiscSplit"] = SingleItemDisc;
                                            rowCheck[i]["SpaySplit"] = Convert.ToDecimal(rowCheck[i]["SpaySplit"]) - SingleItemDisc;
                                        }
                                        rowCheck[i]["BalanceSplit"] = Convert.ToDecimal(rowCheck[i]["PpaySplit"]) + Convert.ToDecimal(rowCheck[i]["SpaySplit"]) + Convert.ToDecimal(rowCheck[i]["DpaySplit"]);
                                        rowCheck[i]["AmountSplit"] = Convert.ToDecimal(rowCheck[i]["PpaySplit"]) + Convert.ToDecimal(rowCheck[i]["SpaySplit"]) + Convert.ToDecimal(rowCheck[i]["DpaySplit"]) + Convert.ToDecimal(rowCheck[i]["CpaySplit"]) + Convert.ToDecimal(rowCheck[i]["DiscSplit"]);
                                    }
                                }
                            }

                        }
                    }
                    dtItemSplitDetails.AcceptChanges();
                    DataRow rowTemp = dtItemSplitDetails.NewRow();
                    rowTemp["PpaySplit"] = dtItemSplitDetails.Compute("SUM(PpaySplit)", "");
                    rowTemp["CpaySplit"] = dtItemSplitDetails.Compute("SUM(CpaySplit)", "");
                    rowTemp["DpaySplit"] = dtItemSplitDetails.Compute("SUM(DpaySplit)", "");
                    rowTemp["SpaySplit"] = dtItemSplitDetails.Compute("SUM(SpaySplit)", "");
                    rowTemp["DiscSplit"] = dtItemSplitDetails.Compute("SUM(DiscSplit)", "");
                    rowTemp["BalanceSplit"] = dtItemSplitDetails.Compute("SUM(BalanceSplit)", "");
                    rowTemp["AmountSplit"] = dtItemSplitDetails.Compute("SUM(AmountSplit)", "");
                    dtItemSplitDetails.Rows.Add(rowTemp);
                    ItemSplitDetails = dtItemSplitDetails.Copy();
                    ItemSplitDetails.AcceptChanges();
                }
            }
        }

        public DataSet SetBillDetails(DataSet dsBillDetails, int InputWorkStationId)
        {
            BillingFacadeServiceContractClient objFacadeClient = new BillingFacadeServiceContractClient();
            try
            {

                return objFacadeClient.CalucalteOPBill(dsBillDetails, InputWorkStationId);
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                objFacadeClient.Close();
            }
        }

        public DataSet FetchLetterDetails(int intLetterNo, int intType, string strTableID, int intUserId, int intWorkstationid, int intError, int HospitalID)
        {
            try
            {
                objFOClient = new FrontOfficeServiceContractClient();
                return objFOClient.FetchHospitalLetterDetails(intLetterNo, intType, strTableID, intUserId, intWorkstationid, intError, HospitalID);
            }

            finally
            {
                objFOClient.Close();
            }
        }
        private double ServiceWiseUserMessages(DataSet DsLimits, DataTable dtBills)
        {
            try
            {
                double TotalServiceAmt = 0;
                DataSet temp1 = new DataSet();
                temp1.Tables.Add(dtBills.Copy());
                if (DsLimits.Tables[0].Rows.Count > 0 && DsLimits.Tables[0].Rows[0]["LimitType"].ToString() == "4" && DsLimits.Tables[0].Columns.Contains("Letterid"))
                {
                    string strfilter = "";
                    DataRow[] drOuter = DsLimits.Tables[1].Select(strfilter);
                    if (drOuter.Length > 0)
                    {
                        foreach (DataRow drr in drOuter)
                        {
                            DataRow[] drservices = dtBills.Select("Serviceid = " + drr["Serviceid"].ToString());
                            if (drservices.Length > 0)
                            {
                                foreach (DataRow drrr in drservices)
                                {
                                    if (drrr["ServiceID"].ToString() == "50")  //Code changes made for TFS ID :: 165983
                                    {
                                        if (drrr["billableprice"].ToString() == "")
                                            TotalServiceAmt += Convert.ToDouble(drrr["baseprice"]);
                                        else
                                            TotalServiceAmt += Convert.ToDouble(drrr["billableprice"]);
                                    }
                                    else
                                    {
                                        if (drrr["billableprice"].ToString() == "")
                                            TotalServiceAmt += Convert.ToDouble(drrr["baseprice"]) * Convert.ToDouble(drrr["Quantity"]);
                                        else
                                            TotalServiceAmt += Convert.ToDouble(drrr["billableprice"]) * Convert.ToDouble(drrr["Quantity"]);
                                    }
                                }
                            }
                        }
                    }

                }
                else
                {
                    TotalServiceAmt = -1;  // this means the configuration is from Agreement

                }
                return TotalServiceAmt;

            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in ServiceWiseUserMessages", "");
                return -1;
            }

        }

        public DataSet GetGradeDoctorSpecializationWS(int PatientId, int DocId, int SpecId, int intServiceid, string strTable, int userid, int intWorkstationid, int intError)
        {
            try
            {
                objFOClient = new FrontOfficeServiceContractClient();
                return objFOClient.GetGradeDoctorSpecializationWS(PatientId, DocId, SpecId, intServiceid, strTable, userid, intWorkstationid, intError);
            }
            finally
            {
                objFOClient.Close();
            }
        }

        protected void SplzFollowupConfig(int intSplzID, PatientBillList PatientBillList)
        {
            try
            {
                DataTable dtSpecCofig = HISCONFIG.Copy(); string TypeMapi = string.Empty; string FilterMAPI = string.Empty; string OrderMAPI = string.Empty;
                if (dtSpecCofig.Select("Parameter ='SPC_Dental'").Length > 0 || dtSpecCofig.Select("Parameter ='SPC_Dermatology'").Length > 0)
                {
                    DataRow[] drtemp = dtSpecCofig.Select("Parameter ='SPC_Dental'");
                    string strsplzconfig = string.Empty;
                    if (drtemp.Length > 0)
                    {
                        strsplzconfig = drtemp[0]["Value"].ToString();
                        drtemp = dtSpecCofig.Select("Parameter ='SPC_Dermatology'");
                        if (drtemp.Length > 0)
                            strsplzconfig = strsplzconfig + "," + drtemp[0]["Value"].ToString();
                    }
                    else
                    {
                        drtemp = dtSpecCofig.Select("Parameter ='SPC_Dermatology'");
                        strsplzconfig = drtemp[0]["Value"].ToString();
                    }
                    string strNewVisitOrderTypeId = string.Empty;
                    if (ConfigurationManager.AppSettings["NewVisitOrderTypeId"] != null)
                    {
                        strNewVisitOrderTypeId = ConfigurationManager.AppSettings["NewVisitOrderTypeId"];
                    }
                    string[] splitSeperatorValues = { "," };
                    string[] strsplarray = strsplzconfig.Split(splitSeperatorValues, StringSplitOptions.None);

                    foreach (string strsplz in strsplarray)
                    {
                        if (strsplz == intSplzID.ToString())
                        {
                            DataTable dtItems;
                            if (hdnrblbilltypeCredit == true)
                            {
                                if (!string.IsNullOrEmpty(hdnProcedureID))
                                {
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CR' and OrderTypeID=" + strNewVisitOrderTypeId + " and PatientId=" + hdnPatientID + " and servicedocid=" + hdnProcedureID + " and datediff(day,Orderdate,getdate())<= 60";
                                    OrderMAPI = "Order by ORDERDATE desc,ORDERTYPEID desc";
                                    //dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CR' and OrderTypeID=" + strNewVisitOrderTypeId + " and PatientId=" + hdnPatientID + " and servicedocid=" + hdnProcedureID + " and datediff(day,Orderdate,getdate())<= 60", "Order by ORDERDATE desc,ORDERTYPEID desc", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0];
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];
                                }
                                else
                                {
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CR' and OrderTypeID=" + strNewVisitOrderTypeId + " and PatientId=" + hdnPatientID + " and datediff(day,Orderdate,getdate())<= 60";
                                    OrderMAPI = "Order by ORDERDATE desc,ORDERTYPEID desc";
                                    //dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CR' and OrderTypeID=" + strNewVisitOrderTypeId + " and PatientId=" + hdnPatientID + " and datediff(day,Orderdate,getdate())<= 60", "Order by ORDERDATE desc,ORDERTYPEID desc", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0];
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];
                                }

                                if (dtItems.Rows.Count == 0)
                                {
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + " and BillType='CR' and OrderTypeID=" + strNewVisitOrderTypeId + " and PatientId=" + hdnPatientID + " and datediff(day,Orderdate,getdate()) <= 60";
                                    OrderMAPI = "Order by ORDERDATE desc,ORDERTYPEID desc";
                                    // dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + " and BillType='CR' and OrderTypeID=" + strNewVisitOrderTypeId + " and PatientId=" + hdnPatientID + " and datediff(day,Orderdate,getdate()) <= 60", "order by IPID desc, ORDERDATE desc,ORDERTYPEID desc", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0];
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];
                                }

                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(hdnProcedureID))
                                {
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CS' and OrderTypeID=" + strNewVisitOrderTypeId + " and PatientId=" + hdnPatientID + " and servicedocid=" + hdnProcedureID + " and datediff(day,Orderdate,getdate())<=60";
                                    OrderMAPI = "Order by ORDERDATE desc,ORDERTYPEID desc";
                                    //dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CS' and OrderTypeID=" + strNewVisitOrderTypeId + " and PatientId=" + hdnPatientID + " and servicedocid=" + hdnProcedureID + " and datediff(day,Orderdate,getdate())<=60", "Order by ORDERDATE desc,ORDERTYPEID desc", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0];
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];
                                }
                                else
                                {
                                    // dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CS' and OrderTypeID=" + strNewVisitOrderTypeId + " and PatientId=" + hdnPatientID + " and datediff(day,Orderdate,getdate())<=60", "Order by ORDERDATE desc,ORDERTYPEID desc", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0];
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CS' and OrderTypeID=" + strNewVisitOrderTypeId + " and PatientId=" + hdnPatientID + " and datediff(day,Orderdate,getdate())<=60";
                                    OrderMAPI = "Order by ORDERDATE desc,ORDERTYPEID desc";
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                }
                                if (dtItems.Rows.Count == 0)
                                {
                                    // dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + " and BillType='CS' and OrderTypeID=" + strNewVisitOrderTypeId + " and PatientId=" + hdnPatientID + " and datediff(day,Orderdate,getdate()) <=60", "order by ORDERDATE desc,ORDERTYPEID desc", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0];
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + " and BillType='CS' and OrderTypeID=" + strNewVisitOrderTypeId + " and PatientId=" + hdnPatientID + " and datediff(day,Orderdate,getdate()) <=60";
                                    OrderMAPI = "Order by ORDERDATE desc,ORDERTYPEID desc";
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                }

                            }
                            int inttempipid = 0;
                            if (hdnIPID == "0" && dtItems.Rows.Count > 0)
                            {
                                LatestIPID = dtItems.Rows[0]["IPID"].ToString();
                                inttempipid = Convert.ToInt32(dtItems.Rows[0]["IPID"]);
                            }
                            else
                                inttempipid = Convert.ToInt32(hdnIPID);

                            DataTable dtPatientfollowup = NewFetchPatientFile(0, inttempipid, 0, "39", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId)).Tables[0];

                            if (dtPatientfollowup.Rows.Count > 0)
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(dtPatientfollowup.Rows[0]["FollowupDAYS"])))
                                {
                                    DateTime dtToday = DateTime.Today;
                                    DateTime dtfollowupValidity = Convert.ToDateTime(dtPatientfollowup.Rows[0]["CreateDate"].ToString());
                                    if ((dtToday - dtfollowupValidity).Days <= Convert.ToDecimal(dtPatientfollowup.Rows[0]["FollowupDAYS"]))
                                    {
                                        if (!string.IsNullOrEmpty(Convert.ToString(dtPatientfollowup.Rows[0]["FollowupCount"])))
                                            SPLFOLLOWUPLMT = dtPatientfollowup.Rows[0]["FollowupCount"].ToString();
                                        if (!string.IsNullOrEmpty(Convert.ToString(dtPatientfollowup.Rows[0]["FollowupDAYS"])))
                                            SPLFOLLOWUPDAYS = dtPatientfollowup.Rows[0]["FollowupDAYS"].ToString();
                                    }
                                }
                            }
                            break;
                        }
                        else
                        {
                            SPLFOLLOWUPLMT = null;
                            SPLFOLLOWUPDAYS = null;
                        }
                    }
                }
                if (dtSpecCofig.Select("Parameter ='DEPT_EMERGENCY' AND  HospitalId = " + strDefaultHospitalId + " ").Length > 0)
                {
                    DataRow[] drtemp = dtSpecCofig.Select("Parameter ='DEPT_EMERGENCY'  AND  HospitalId = " + strDefaultHospitalId + " ");
                    string strsplzconfig = string.Empty;
                    if (drtemp.Length > 0)
                    {
                        strsplzconfig = drtemp[0]["Value"].ToString();
                    }
                    string[] splitSeperatorValues = { "," };
                    string[] strsplarray = strsplzconfig.Split(splitSeperatorValues, StringSplitOptions.None);

                    foreach (string strsplz in strsplarray)
                    {
                        if (strsplz == hdnDocHospDeptId)
                        {
                            DataTable dtItems;
                            if (hdnrblbilltypeCredit == true)
                            {
                                if (!string.IsNullOrEmpty(hdnProcedureID))
                                {
                                    // dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID", "serviceid=2  and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CR' and OrderTypeID=49 and PatientId=" + hdnPatientID + " and servicedocid=" + hdnProcedureID + " and datediff(minute,Orderdate,getdate()) <=1440", "Order by ORDERDATE,ORDERTYPEID desc", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0];
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2  and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CR' and OrderTypeID=49 and PatientId=" + hdnPatientID + " and servicedocid=" + hdnProcedureID + " and datediff(minute,Orderdate,getdate()) <=1440";
                                    OrderMAPI = "Order by ORDERDATE desc,ORDERTYPEID desc";
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                }
                                else
                                {
                                    //dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID", "serviceid=2  and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CR' and OrderTypeID=49 and PatientId=" + hdnPatientID + " and datediff(minute,Orderdate,getdate()) <=1440", "Order by ORDERDATE,ORDERTYPEID desc", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0];
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2  and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CR' and OrderTypeID=49 and PatientId=" + hdnPatientID + " and datediff(minute,Orderdate,getdate()) <=1440";
                                    OrderMAPI = "Order by ORDERDATE desc,ORDERTYPEID desc";
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                }

                                if (dtItems.Rows.Count == 0)
                                {
                                    //dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + " and BillType='CR' and OrderTypeID=49 and PatientId=" + hdnPatientID + " and datediff(minute,Orderdate,getdate()) <=1440", "order by ORDERDATE,ORDERTYPEID desc", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0];
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + " and BillType='CR' and OrderTypeID=49 and PatientId=" + hdnPatientID + " and datediff(minute,Orderdate,getdate()) <=1440";
                                    OrderMAPI = "Order by ORDERDATE desc,ORDERTYPEID desc";
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                }

                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(hdnProcedureID))
                                {
                                    // dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CS' and OrderTypeID=49 and PatientId=" + hdnPatientID + " and servicedocid=" + hdnProcedureID + " and datediff(minute,Orderdate,getdate()) <=1440", "Order by ORDERDATE,ORDERTYPEID desc", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0];
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CS' and OrderTypeID=49 and PatientId=" + hdnPatientID + " and servicedocid=" + hdnProcedureID + " and datediff(minute,Orderdate,getdate()) <=1440";
                                    OrderMAPI = "Order by ORDERDATE desc,ORDERTYPEID desc";
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                }
                                else
                                {
                                    // dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CS' and OrderTypeID=49 and PatientId=" + hdnPatientID + " and datediff(minute,Orderdate,getdate()) <=1440", "Order by ORDERDATE,ORDERTYPEID desc", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0];
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + "  and BillType='CS' and OrderTypeID=49 and PatientId=" + hdnPatientID + " and datediff(minute,Orderdate,getdate()) <=1440";
                                    OrderMAPI = "Order by ORDERDATE desc,ORDERTYPEID desc";
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                }

                                if (dtItems.Rows.Count == 0)
                                {
                                    //dtItems = FetchMISProcedureDetails("Pr_FetchConsFollowUpDetailsAdv", "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID", "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + " and BillType='CS' and OrderTypeID=49 and PatientId=" + hdnPatientID + " and datediff(minute,Orderdate,getdate()) <=1440", "order by ORDERDATE,ORDERTYPEID desc", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), 0, false).Tables[0];
                                    TypeMapi = "ORDERTYPEID,ORDERTYPE,ORDERDATE,BILLID,SERVICEDOCID,PATIENTID,IPID,COMPANYID,GRADEID";
                                    FilterMAPI = "serviceid=2 and specialiseId=" + hdnDocSpecialiseId + " and BillType='CS' and OrderTypeID=49 and PatientId=" + hdnPatientID + " and datediff(minute,Orderdate,getdate()) <=1440";
                                    OrderMAPI = "Order by ORDERDATE desc,ORDERTYPEID desc";
                                    dtItems = FetchMISProcedureDetailsMAPI(Convert.ToInt32(hdnPatientID), TypeMapi, FilterMAPI, OrderMAPI, PatientBillList.HospitalId, Convert.ToInt32(strDefWorkstationId), PatientBillList).Tables[0];

                                }

                            }
                            if (dtItems.Rows.Count > 0)
                            {
                                DataRow[] drtempFollowup = dtSpecCofig.Select("Parameter ='EMRFOLLOWUP' AND  HospitalId = " + strDefaultHospitalId + " ");
                                if (drtempFollowup.Length > 0)
                                {
                                    SPLFOLLOWUPLMT = drtempFollowup[0]["Value"].ToString();
                                    SPLFOLLOWUPDAYS = "2";
                                    DataRow[] drFollowUpConsulStatus = dtSpecCofig.Select("Parameter = 'FollowUpConsultation' AND  HospitalId = " + strDefaultHospitalId + "");
                                    if (drFollowUpConsulStatus.Length > 0)
                                    {
                                        string strFollowUpConsulStatus = dtSpecCofig.Select("Parameter = 'FollowUpConsultation'  AND  HospitalId = " + strDefaultHospitalId + " ")[0]["Value"].ToString();
                                        FollowUpConsulStatus = strFollowUpConsulStatus;
                                    }
                                    if (SPLFOLLOWUPLMT.ToString() == "0")
                                    {
                                        ViewStateLetterid = null;
                                        VSParentLetterid = null;
                                        drtemp = dtSpecCofig.Select("Parameter ='EMR_COPAY'  AND  HospitalId = " + strDefaultHospitalId + " ");
                                        if (drtemp.Length > 0)
                                        {
                                            if (drtemp[0]["Value"].ToString().ToUpper() == "NO")
                                                EMRCOPAY = "NO";
                                            else
                                                EMRCOPAY = "YES";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                DataRow[] drtempFollowup = dtSpecCofig.Select("Parameter ='EMRFOLLOWUP' AND  HospitalId = " + strDefaultHospitalId + " ");
                                if (drtempFollowup.Length > 0)
                                {
                                    SPLFOLLOWUPLMT = drtempFollowup[0]["Value"].ToString();
                                    if (SPLFOLLOWUPLMT.ToString() == "0")
                                    {
                                        ViewStateLetterid = null;
                                        VSParentLetterid = null;
                                        drtemp = dtSpecCofig.Select("Parameter ='EMR_COPAY'  AND  HospitalId = " + strDefaultHospitalId + " ");
                                        if (drtemp.Length > 0)
                                        {
                                            if (drtemp[0]["Value"].ToString().ToUpper() == "NO")
                                                EMRCOPAY = "NO";
                                            else
                                                EMRCOPAY = "YES";
                                        }
                                    }
                                    else
                                    {
                                        SPLFOLLOWUPLMT = null;
                                        SPLFOLLOWUPDAYS = null;
                                    }
                                }
                                else
                                {
                                    SPLFOLLOWUPLMT = null;
                                    SPLFOLLOWUPDAYS = null;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            finally
            {

            }
        }

        private void FillEmptyLOAGrid()
        {
            DataTable dtEmpty = CreateLOAGrid();
            hdnblnSaveDefaultLOA = "true";
            DataRow dr = dtEmpty.NewRow();
            dr["LetterNo"] = ""; dr["FrmDate"] = ""; dr["ToDate"] = "";
            dr["Payer"] = ""; dr["Payerid"] = ""; dr["Letterid"] = "";
            dtEmpty.Rows.Add(dr);
            dtEmpty.AcceptChanges();

        }

        private DataTable CreateLOAGrid()
        {
            DataTable dtLOAGrid = new DataTable();
            try
            {
                dtLOAGrid.Columns.Add("SlNo", typeof(int)).AutoIncrement = true;
                dtLOAGrid.Columns["SlNo"].AutoIncrementSeed = 1;
                dtLOAGrid.Columns.Add("LetterNo", typeof(string));
                dtLOAGrid.Columns.Add("FrmDate", typeof(string));
                dtLOAGrid.Columns.Add("ToDate", typeof(string));
                dtLOAGrid.Columns.Add("Payer", typeof(string));
                dtLOAGrid.Columns.Add("Payerid", typeof(string));
                dtLOAGrid.Columns.Add("Letterid", typeof(string));
                dtLOAGrid.Columns.Add("ParentLetterid", typeof(string));
                dtLOAGrid.Columns.Add("DoctorName", typeof(string));
                return dtLOAGrid;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in CreateLOAGrid", "");
                return null;
            }
        }
        private DataTable SortbyLetterID(DataTable DtLtetters)
        {
            DataTable dt = new DataTable();
            try
            {
                DataRow[] drrow = DtLtetters.Select("", "LetterId desc");
                dt = DtLtetters.Clone();
                foreach (DataRow drow in drrow)
                {
                    dt.ImportRow(drow);
                }
                DtLtetters = null;
                DtLtetters = dt;
                dt = null;
                return DtLtetters;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in SortbyLetterID", "");
                return null;
            }
        }


        public DataSet GetLOA_Deatils(int intMin, int intMax, int intType, string strFilter, int intUserID, int intWorkstationID, int intError)
        {

            FrontOfficeServiceContractClient objFOServiceClient = new FrontOfficeServiceContractClient();
            DataSet dsGetCons = new DataSet();
            try
            {
                dsGetCons = objFOServiceClient.GetLOA_Deatils(intMin, intMax, intType, strFilter, intUserID, intWorkstationID, intError);
            }
            finally
            {
                objFOServiceClient.Close();
            }
            return dsGetCons;
        }

        public DataSet CalculateDeductibles(DataTable dtDeductibles, bool blfalse, bool strIsPackage, decimal discAmount)
        {
            BillingCalculatonServiceContractClient objBillCalcClient = new BillingCalculatonServiceContractClient();
            try
            {

                return objBillCalcClient.CalculateDeductibles(dtDeductibles, blfalse, strIsPackage, discAmount);
            }

            finally
            {
                objBillCalcClient.Close();
            }
        }

        protected void LOARB_CheckedChanged(string ParentLetterId, string Letterid)
        {
            try
            {
                if (!string.IsNullOrEmpty(ParentLetterId.ToString()))
                    VSParentLetterid = ParentLetterId.ToString();
                else
                    VSParentLetterid = "0";
                ViewStateLetterid = Letterid.ToString();
                LOALetterID = Letterid.ToString();
                hdnblnSaveDefaultLOA = "false";
                DataTable dt = dtLOADetails.Copy();
                DataRow[] dr = dt.Select("LetterId=" + ViewStateLetterid);
                dtLetter = dr.CopyToDataTable();
                dtLetter.AcceptChanges();
                TimeSpan ts = Convert.ToDateTime(dr[0]["ToDate"]).Date - Convert.ToDateTime(dr[0]["frmDate"]).Date;
                hdnLOAfollowupDays = ts.Days.ToString();
            }
            catch (Exception ex)
            {
               // HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in LOARB_CheckedChanged Event", "");
            }
        }

        public DataSet FetchFollowupdays(int DocID, int SpecilaseId, int UserId, int WstationId, int intError, int PatientID)
        {

            FrontOfficeServiceContractClient objfrontoffice = new FrontOfficeServiceContractClient();
            try
            {
                return objfrontoffice.FetchFollowupdays(DocID, SpecilaseId, UserId, WstationId, intError, PatientID);
            }

            finally
            {
                objfrontoffice.Close();
            }
        }

        public DataSet FetchLetterDetails(int intLetterNo, int intType, string strTableID, int intUserId, int intWorkstationid, int intError)
        {
            try
            {
                objFOClient = new FrontOfficeServiceContractClient();
                return objFOClient.FetchLetterDetails(intLetterNo, intType, strTableID, intUserId, intWorkstationid, intError);
            }

            finally
            {
                objFOClient.Close();
            }
        }

        private string CheckforLimits(int FollowupLimit, bool CheckType, DataTable dtItems, int DocId, int OrderTypeId, string OrderType, int BaseOrderType, string BaseOrderName)
        {
            try
            {
                hdnDelConfirm = "0";
                if (FollowupLimit > 0)
                {
                    if (OrderTypeId != BaseOrderType)
                    {
                        OrderTypeID = OrderTypeId; BaseOrderTypeM = BaseOrderType;
                        DataTable dttempLt = dtItems.Clone();
                        string strFilter = "";
                        string str = "";

                        if (CheckType)
                        { strFilter = " OrderTypeId = " + BaseOrderType; }
                        else
                        { strFilter = " OrderTypeId = " + BaseOrderType + " and ServiceDocId = " + DocId; }

                        foreach (DataRow dr in dtItems.Select(strFilter, " OrderDate Desc"))
                        { dttempLt.ImportRow(dr); }
                        dttempLt.AcceptChanges();

                        //Added below condition to validate, if consultation bill is having different company/grade-Vasu
                        bool isSameCompany = false;
                        if (hdnrblbilltypeCredit == true)
                        {
                            var dr = dtItems.Select("COMPANYID is not null and GRADEID is not null and COMPANYID=" + strCompanyID + " AND GRADEID=" + strGradeID);
                            if (dr.Length > 0)
                                isSameCompany = true;
                            if (!isSameCompany && (dtItems.Rows.Count == 0 || dtItems.Select("COMPANYID is null and GRADEID is null").Length > 0))
                            {
                                isSameCompany = true;
                            }
                        }


                        if (dttempLt.Rows.Count < FollowupLimit && (isSameCompany || hdnrblbilltypecash == true))
                        {
                            if (TagId == 0)
                                str = "Patient can avail " + BaseOrderName.ToUpper() + " visit, Do you want to avail? ";
                            else
                                str = "Patient can avail " + BaseOrderName.ToUpper() + " visit, Do you want to avail? ";

                        }
                        else
                        { str = ""; }

                        if (dttempLt.Rows.Count < FollowupLimit && (isSameCompany || hdnrblbilltypecash == true))
                        {
                            //DialogResult dlg;
                            if (TagId == 0)
                            {
                                if (CheckType)
                                {
                                    StringBuilder strMsg = new StringBuilder();
                                    //if (MaxStr != null)
                                    if (!string.IsNullOrEmpty(MaxStr))
                                    {
                                        string strmax = MaxStr.ToString();
                                        strMsg.Append(strmax + " <br/>");
                                        Maxconsult = null;
                                        MaxStr = null;
                                    }
                                    strMsg.Append("Patients Previous visits to this Speciality is within the follow up limit defined. ");
                                    strMsg.Append("" + str + "");

                                    // if (FollowUpConsulStatus != null)
                                    if (!string.IsNullOrEmpty(FollowUpConsulStatus))
                                    {
                                        if ((FollowUpConsulStatus.ToString()).ToUpper() != "YES")
                                        {
                                            if (System.Configuration.ConfigurationSettings.AppSettings["FollowUpMsgYESNO"].ToString().ToLower() == "yes")
                                            {
                                                OrderTypeVisit = "Followup";
                                                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','YESNO','Information');", true);
                                                hdnIsFollowUp = "YESNO";
                                            }
                                            else
                                            {
                                                OrderTypeVisit = "Followup";
                                                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                                hdnIsFollowUp = "YES";
                                            }
                                        }
                                    }
                                    OrderTypeID = OrderTypeId;
                                    BaseOrderTypeM = BaseOrderType;
                                    OrderTypeM = OrderType;

                                    hfConfirmNext = "ORDERTYPE";
                                    return OrderTypeId + "/" + OrderType;//New
                                    //dlg = System.Windows.Forms.MessageBox.Show("Patients previous visits to this Speciality is within the followup limit defined " + str, this.Title, MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Information);
                                }
                                else
                                {
                                    StringBuilder strMsg = new StringBuilder();
                                    // if (MaxStr != null)
                                    if (!string.IsNullOrEmpty(MaxStr))
                                    {
                                        string strmax = MaxStr.ToString();
                                        strMsg.Append(strmax + " <br/>");
                                        Maxconsult = null;
                                        MaxStr = null;
                                    }
                                    strMsg.Append("Patients previous visits to this doctor is within the follow up limit defined. ");
                                    strMsg.Append("" + str + "");

                                    //if (FollowUpConsulStatus != null)
                                    if (!string.IsNullOrEmpty(FollowUpConsulStatus))
                                    {
                                        if ((FollowUpConsulStatus.ToString()).ToUpper() != "YES")
                                        {
                                            if (System.Configuration.ConfigurationSettings.AppSettings["FollowUpMsgYESNO"].ToString().ToLower() == "yes")
                                            {
                                                // ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','YESNO','Information');", true);
                                                hdnIsFollowUp = "YESNO";
                                            }
                                            else
                                            {
                                                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                                hdnIsFollowUp = "YES";
                                            }
                                        }
                                    }

                                    OrderTypeID = OrderTypeId; BaseOrderTypeM = BaseOrderType;
                                    OrderTypeM = OrderType;
                                    hfConfirmNext = "ORDERTYPE";
                                    return OrderTypeId + "/" + OrderType;//New

                                    //dlg = MessageBox.Show("Patients previous visits to this doctor is within the followup limit defined " + str, this.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                }
                            }
                            else
                            {
                                if (CheckType)
                                {
                                    StringBuilder strMsg = new StringBuilder();
                                    // if (MaxStr != null)
                                    if (!string.IsNullOrEmpty(MaxStr))
                                    {
                                        string strmax = MaxStr.ToString();
                                        strMsg.Append(strmax + " <br/>");
                                        Maxconsult = null;
                                        MaxStr = null;
                                    }
                                    strMsg.Append("Patients previous visits to this Speciality is within the Follow up limit defined. ");
                                    strMsg.Append("" + str + "");

                                    //Manipal Development CR#172 added by Venkat -- start here

                                    //if (FollowUpConsulStatus != null)
                                    if (!string.IsNullOrEmpty(FollowUpConsulStatus))
                                    {
                                        if ((FollowUpConsulStatus.ToString()).ToUpper() != "YES")
                                        {
                                            if (System.Configuration.ConfigurationSettings.AppSettings["FollowUpMsgYESNO"].ToString().ToLower() == "yes")
                                            {
                                                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','YESNO','Information');", true);
                                                hdnIsFollowUp = "YESNO";
                                            }
                                            else
                                            {
                                                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                                hdnIsFollowUp = "YES";
                                            }
                                        }
                                    }
                                    OrderTypeID = OrderTypeId; BaseOrderTypeM = BaseOrderType;
                                    OrderTypeM = OrderType;
                                    hfConfirmNext = "ORDERTYPE";
                                    return OrderTypeId + "/" + OrderType;

                                }
                                else
                                {
                                    StringBuilder strMsg = new StringBuilder();
                                    if (!string.IsNullOrEmpty(MaxStr))
                                    {
                                        string strmax = MaxStr.ToString();
                                        strMsg.Append(strmax + " <br/>");
                                        Maxconsult = null;
                                        MaxStr = null;
                                    }
                                    strMsg.Append("Patients previous visits to this Doctor is within the follow up limit defined. ");
                                    strMsg.Append("" + str + "");


                                    if (!string.IsNullOrEmpty(FollowUpConsulStatus))
                                    {
                                        if ((FollowUpConsulStatus.ToString()).ToUpper() != "YES")
                                        {
                                            if (System.Configuration.ConfigurationSettings.AppSettings["FollowUpMsgYESNO"].ToString().ToLower() == "yes")
                                            {
                                                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','YESNO','Information');", true);
                                                hdnIsFollowUp = "YESNO";
                                            }
                                            else
                                            {
                                                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                                hdnIsFollowUp = "YES";
                                            }
                                        }
                                    }
                                    OrderTypeID = OrderTypeId; BaseOrderTypeM = BaseOrderType;
                                    OrderTypeM = OrderType;
                                    hfConfirmNext = "ORDERTYPE";
                                    return OrderTypeId + "/" + OrderType;

                                }
                            }

                        }
                        else
                        { return OrderTypeId + "/" + OrderType; }

                    }
                    else if (OrderTypeId == BaseOrderType)
                    {
                        BaseOrderTypeM = BaseOrderType;
                        DataTable dttempLt = dtItems.Clone();
                        string strFilter = "";

                        if (CheckType)
                        { strFilter = "OrderTypeId = " + BaseOrderTypeM; }
                        else
                        { strFilter = "OrderTypeId = " + BaseOrderTypeM + " and ServiceDocId = " + DocId; }

                        if (dtItems.Columns.Contains("BillID"))
                        {
                            strFilter += " and billid is not null and BillID > 0";
                        }

                        if (hdnrblbilltypeCredit == true && dtItems.Rows.Count > 0)
                        {
                            var dr = dtItems.Select("COMPANYID is not null and GRADEID is not null and COMPANYID=" + strCompanyID + " AND GRADEID=" + strGradeID);
                            if (dr.Length == 0)
                                dr = dtItems.Select("COMPANYID is null and GRADEID is null");
                            if (dr.Length == 0)
                            {
                                strMsg = new StringBuilder();
                                strMsg.Append("FollowUp cannot be possible, As the Consultation bill is under different Company or Grade.");

                                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                return "";
                            }
                        }


                        foreach (DataRow dr in dtItems.Select(strFilter, " OrderDate Desc"))
                        { dttempLt.ImportRow(dr); }
                        dttempLt.AcceptChanges();

                        if (dttempLt.Rows.Count > 0)
                        {

                            if (dttempLt.Rows.Count >= FollowupLimit)
                            {
                                StringBuilder strMsg = null;

                                if (TagId == 0)
                                {
                                    if (CheckType)
                                    {
                                        strMsg = new StringBuilder();
                                        strMsg.Append("FollowUp cannot be possible for the selected speciality <br/>");
                                        strMsg.Append("As the patient doesnot have previous visits to this Speciality exceeds the followup limit defined ");
                                        // ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                        //MessageBox.Show("FollowUp cannot be possible for the selected speciality \nAs the patients previous visits to this Speciality exceeds the followup limit defined ", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information); 
                                    }
                                    else
                                    {
                                        strMsg = new StringBuilder();
                                        strMsg.Append("FollowUp cannot be possible for the selected doctor <br/>");
                                        strMsg.Append("As the patient doesnot have previous visits to this doctor exceeds the followup limit defined ");
                                        // ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                        //MessageBox.Show("FollowUp cannot be possible for the selected Doctor \nAs the patients previous visits to this doctor exceeds the followup limit defined ", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information); 
                                    }
                                    return ""; //false;
                                }
                                else
                                {
                                    if (CheckType)
                                    {
                                        strMsg = new StringBuilder();
                                        strMsg.Append("FollowUp cannot be possible for the selected speciality <br/>");
                                        strMsg.Append("As the patient doesnot have previous visits to this Speciality exceeds the Followup limit defined ");

                                        //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                        //MessageBox.Show(resources1.GetString("frmOPBillingNew.cs.MSG15"), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information); 
                                    }
                                    else
                                    {
                                        strMsg = new StringBuilder();
                                        strMsg.Append("FollowUp cannot be possible for the selected doctor <br/>");
                                        strMsg.Append("As the patient doesnot have previous visits to this doctor exceeds the followup limit defined ");

                                        //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                        //MessageBox.Show(resources1.GetString("frmOPBillingNew.cs.MSG16"), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    return ""; //false;
                                }
                            }
                            else
                            { return OrderTypeId + "/" + OrderType; }// true;	}
                        }
                        else
                        { return OrderTypeId + "/" + OrderType; }// true;	}

                    }
                    else
                    { return OrderTypeId + "/" + OrderType; }
                }
                else
                { return OrderTypeId + "/" + OrderType; }

            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in CheckforLimits", "");
                return string.Empty;
            }

        }

        private string CheckforDays(int FollowupDays, bool CheckType, DataTable dtItems, int DocId, int OrderTypeId, string OrderType, int BaseOrderType, string BaseOrderName)
        {
            try
            {
                hdnDelConfirm = "0";
                if (FollowupDays > 0)
                {

                    if (OrderTypeId != BaseOrderType)
                    {
                        DataTable dttemp = dtItems.Clone();
                        string strFilter = "";
                        string str = "";

                        DataRow[] drChk = dtItems.Select("OrderTypeId = " + BaseOrderType, " OrderDate Desc");
                        if (drChk.Length > 0)
                        {
                            if (CheckType)
                            { strFilter = " OrderTypeId = " + BaseOrderType; }
                            else
                            { strFilter = " OrderTypeId = " + BaseOrderType + " and ServiceDocId = " + DocId; }

                        }
                        else
                        {
                            if (CheckType)
                            { strFilter = ""; }
                            else
                            { strFilter = " ServiceDocId = " + DocId; }
                        }
                        if (dtItems.Columns.Contains("BillID") && strFilter != "")
                        {
                            strFilter += " and billid is not null and BillID > 0";
                        }
                        else
                            strFilter += "billid is not null and BillID > 0";
                        foreach (DataRow dr in dtItems.Select(strFilter, " OrderDate Desc"))
                        { dttemp.ImportRow(dr); }
                        dttemp.AcceptChanges();


                        bool isSameCompany = false;
                        if (hdnrblbilltypeCredit == true)
                        {
                            var dr = dtItems.Select("COMPANYID is not null and GRADEID is not null and COMPANYID=" + strCompanyID + " AND GRADEID=" + strGradeID);
                            if (dr.Length > 0)
                                isSameCompany = true;
                            if (!isSameCompany && (dtItems.Rows.Count == 0 || dtItems.Select("COMPANYID is null and GRADEID is null").Length > 0))
                            {
                                isSameCompany = true;
                            }
                        }

                        if (dttemp.Rows.Count > 0 && (isSameCompany || hdnrblbilltypecash == true))
                        {
                            DataRow[] drItem = dttemp.Select();
                            TimeSpan ts = DateTime.Now - Convert.ToDateTime(drItem[0]["OrderDate"]).Date;

                            if (ts.Days < FollowupDays)
                            {
                                if (TagId == 0)
                                    str = "<br>Patient can avail " + BaseOrderName.ToUpper() + " visit, Do you want to avail? ";
                                else
                                    str = "<br>Patient can avail " + BaseOrderName.ToUpper() + " visit, Do you want to avail? ";
                            }
                            else
                            { str = ""; }
                            if (ts.Days < FollowupDays)
                            {
                                if (TagId == 0)
                                {
                                    if (CheckType)
                                    {
                                        StringBuilder strMsg = new StringBuilder();
                                        // if (MaxStr != null)
                                        if (!string.IsNullOrEmpty(MaxStr))
                                        {
                                            string strmax = MaxStr.ToString();
                                            strMsg.Append(strmax + " <br/>");
                                            Maxconsult = null;
                                            MaxStr = null;
                                        }
                                        strMsg.Append("Patients previous visits to this Speciality is within the Follow Up limit defined. ");
                                        strMsg.Append("" + str + "");

                                        // if (FollowUpConsulStatus != null)
                                        if (!string.IsNullOrEmpty(FollowUpConsulStatus))
                                        {
                                            //if (DoctorAvailable != null)
                                            if (!string.IsNullOrEmpty(DoctorAvailable))
                                                if (DoctorAvailable.ToString() == "NO")
                                                    DoctorAvailable = "YES";
                                            if ((FollowUpConsulStatus.ToString()).ToUpper() != "YES")
                                            {
                                                if (ConfigurationSettings.AppSettings["FollowUpMsgYESNO"].ToString().ToLower() == "yes")
                                                {
                                                    //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','YESNO','Information');", true);
                                                    hdnIsFollowUp = "YESNO";
                                                }
                                                else
                                                {
                                                    //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                                    hdnIsFollowUp = "YES";
                                                }
                                            }
                                        }
                                        OrderTypeID = OrderTypeId;
                                        BaseOrderTypeM = BaseOrderType;
                                        OrderTypeM = OrderType;
                                        hfConfirmNext = "ORDERTYPE";
                                        return OrderTypeId + "/" + OrderType;
                                    }
                                    else
                                    {

                                        StringBuilder strMsg = new StringBuilder();
                                        if (MaxStr != null)
                                        {
                                            string strmax = MaxStr.ToString();
                                            strMsg.Append(strmax + " <br/>");
                                            Maxconsult = null;
                                            MaxStr = null;
                                        }
                                        strMsg.Append("Patients previous visits to this doctor is within the Follow up limit defined. ");
                                        strMsg.Append("" + str + "");

                                        if (FollowUpConsulStatus != null)
                                        {
                                            if ((FollowUpConsulStatus.ToString()).ToUpper() != "YES")
                                            {
                                                if (ConfigurationSettings.AppSettings["FollowUpMsgYESNO"].ToString().ToLower() == "yes")
                                                {
                                                    // ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','YESNO','Information');", true);
                                                    hdnIsFollowUp = "YESNO";
                                                }
                                                else
                                                {
                                                    //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                                    hdnIsFollowUp = "YES";
                                                }
                                            }
                                        }
                                        OrderTypeID = OrderTypeId; BaseOrderTypeM = BaseOrderType;
                                        OrderTypeM = OrderType;
                                        hfConfirmNext = "ORDERTYPE";
                                        return OrderTypeId + "/" + OrderType;
                                    }
                                }
                                else
                                {

                                    if (CheckType)
                                    {

                                        StringBuilder strMsg = new StringBuilder();
                                        if (MaxStr != null)
                                        {
                                            string strmax = MaxStr.ToString();
                                            strMsg.Append(strmax + " <br/>");
                                            Maxconsult = null;
                                            MaxStr = null;
                                        }
                                        strMsg.Append("Patients previous visits to this Speciality is within the follow up limit defined. ");
                                        strMsg.Append("" + str + "");

                                        if (FollowUpConsulStatus != null)
                                        {
                                            if ((FollowUpConsulStatus.ToString()).ToUpper() != "YES")
                                            {
                                                if (System.Configuration.ConfigurationSettings.AppSettings["FollowUpMsgYESNO"].ToString().ToLower() == "yes")
                                                {
                                                    //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','YESNO','Information');", true);
                                                    hdnIsFollowUp = "YESNO";
                                                }
                                                else
                                                {
                                                    // ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                                    hdnIsFollowUp = "YES";
                                                }
                                            }
                                        }
                                        OrderTypeID = OrderTypeId; BaseOrderTypeM = BaseOrderType;
                                        OrderTypeM = OrderType;
                                        hfConfirmNext = "ORDERTYPE";
                                        return OrderTypeId + "/" + OrderType;
                                    }
                                    else
                                    {
                                        StringBuilder strMsg = new StringBuilder();
                                        if (MaxStr != null)
                                        {
                                            string strmax = MaxStr.ToString();
                                            strMsg.Append(strmax + " <br/>");
                                            Maxconsult = null;
                                            MaxStr = null;
                                        }
                                        strMsg.Append("Patients previous visits to this doctor is within the follow up Limit defined. ");
                                        strMsg.Append("" + str + "");

                                        if (FollowUpConsulStatus != null)
                                        {
                                            if ((FollowUpConsulStatus.ToString()).ToUpper() != "YES")
                                            {
                                                if (ConfigurationSettings.AppSettings["FollowUpMsgYESNO"].ToString().ToLower() == "yes")
                                                {
                                                    //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','YESNO','Information');", true);
                                                    hdnIsFollowUp = "YESNO";
                                                }
                                                else
                                                {
                                                    //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                                    hdnIsFollowUp = "YES";
                                                }
                                            }
                                        }
                                        OrderTypeID = OrderTypeId; BaseOrderTypeM = BaseOrderType;
                                        OrderTypeM = OrderType;
                                        hfConfirmNext = "ORDERTYPE";
                                        return OrderTypeId + "/" + OrderType;
                                    }
                                }
                            }
                            else
                            { return OrderTypeId + "/" + OrderType; }
                        }
                        else
                        { return OrderTypeId + "/" + OrderType; }
                    }
                    else if (OrderTypeId == BaseOrderType)
                    {
                        DataTable dttemp = dtItems.Clone();
                        string strFilter = "";

                        if (CheckType)
                        { strFilter = "OrderTypeId = " + BaseOrderType; }
                        else
                        { strFilter = "OrderTypeId = " + BaseOrderType + " and ServiceDocId = " + DocId; }

                        //Added below condition to validate, if consultation bill is having different company/grade
                        if (hdnrblbilltypeCredit == true && dtItems.Rows.Count > 0)
                        {
                            var dr = dtItems.Select("COMPANYID is not null and GRADEID is not null and COMPANYID=" + strCompanyID + " AND GRADEID=" + strGradeID);
                            if (dr.Length == 0)
                                dr = dtItems.Select("COMPANYID is null and GRADEID is null");
                            if (dr.Length == 0)
                            {
                                strMsg = new StringBuilder();
                                strMsg.Append("FollowUp cannot be possible, As the Consultation bill is under different Company or Grade.");

                                //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                return "";
                            }
                        }

                        foreach (DataRow dr in dtItems.Select(strFilter, " OrderDate Desc"))
                        { dttemp.ImportRow(dr); }
                        dttemp.AcceptChanges();

                        if (dttemp.Rows.Count > 0)
                        {
                            DataRow[] drItem = dttemp.Select();
                            TimeSpan ts = DateTime.Today.Date - Convert.ToDateTime(drItem[0]["OrderDate"]).Date;
                            StringBuilder strMsg = null;
                            if (ts.Days >= FollowupDays)
                            {

                                if (TagId == 0)
                                {
                                    if (CheckType)
                                    {
                                        strMsg = new StringBuilder();
                                        strMsg.Append("FollowUp cannot be possible for the selected Speciality <br/>");
                                        strMsg.Append("As the patient doesnot have previous visits to this Speciality exceeds the follow up limit defined ");

                                        //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                        return "";
                                        //MessageBox.Show("FollowUp cannot be possible for the selected speciality \nAs the patients previous visits to this Speciality exceeds the followup limit days defined ", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    else
                                    {
                                        strMsg = new StringBuilder();
                                        strMsg.Append("FollowUp cannot be possible for the selected doctor <br/>");
                                        strMsg.Append("As the patient doesnot have previous visits to this doctor exceeds the followup limit defined ");

                                        //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);
                                        return "";
                                        //MessageBox.Show("FollowUp cannot be possible for the selected Doctor \nAs the patients previous visits to this doctor exceeds the followup limit days defined ", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                }
                                else
                                {
                                    if (CheckType)
                                    {
                                        strMsg = new StringBuilder();
                                        strMsg.Append("FollowUp cannot be possible for the selected Speciality <br/>");
                                        strMsg.Append("As the patient does not have previous visits to this Speciality exceeds the follow up limit defined ");
                                        // ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);

                                        //MessageBox.Show(resources1.GetString("frmOPBillingNew.cs.MSG11"), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information); 
                                    }
                                    else
                                    {
                                        strMsg = new StringBuilder();
                                        strMsg.Append("FollowUp cannot be possible for the selected doctor <br/>");
                                        strMsg.Append("As the patient doesnot have previous visits to this doctor exceeds the followup limit defined ");
                                        //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strMsg.ToString() + "','OK','Information');", true);

                                        //MessageBox.Show(resources1.GetString("frmOPBillingNew.cs.MSG12"), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                }
                                return "";// false;
                            }
                            else
                            { return OrderTypeId + "/" + OrderType; }
                        }
                        else
                        { return OrderTypeId + "/" + OrderType; }
                    }
                    else
                    { return OrderTypeId + "/" + OrderType; }
                }
                else
                { return OrderTypeId + "/" + OrderType; }
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in CheckforDays", "");
                return string.Empty;
            }

        }

        private bool DischargeFollowUp()
        {

            bool blnFlag = false;
            try
            {
                int intUserId = Convert.ToInt32(strDefaultUserId);
                int intWorkstationid = Convert.ToInt32(strDefWorkstationId);
                DataSet ds = new DataSet();
                ds = FetchAllBillDetailsAdv(0, "RegCode = '" + UHID.Trim() + "' and Status <>3 and patienttype<>6 and SpecialiseID='" + Convert.ToInt32(specilaize.ToString()) + "'", intUserId, intWorkstationid, 0);

                DataTable dt = new DataTable();
                int days = 0;
                int noOfVisits = 0;
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                    DataView dv = dt.DefaultView;
                    dv.Sort = "BillDate Desc";
                    dt = dv.ToTable();
                    if (dt.Rows.Count > 0)
                    {
                        System.TimeSpan ts = DateTime.Now - Convert.ToDateTime(dt.Rows[0]["BillDate"].ToString());
                        days = ts.Days;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (Convert.ToInt32(dt.Rows[i]["PatientType"]) == 2)
                                break;
                            noOfVisits++;
                        }
                    }
                }
                DataTable dtConfollowupD = FetchFollowupdays(Convert.ToInt32(Doctorid.ToString()), Convert.ToInt32(specilaize.ToString()), intUserId, intWorkstationid, 0, Convert.ToInt32(hdnPatientID)).Tables[0];
                dtConfollowup = dtConfollowupD;
                int intDischargeFollowUpDays = 0;
                int intDischargeFollowUpLimit = 0;
                if (dtConfollowup.Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(dtConfollowup.Rows[1]["DischargeFollowUpDays"].ToString()))
                        intDischargeFollowUpDays = Convert.ToInt32(dtConfollowup.Rows[1]["DischargeFollowUpDays"].ToString());
                    if (!string.IsNullOrEmpty(dtConfollowup.Rows[1]["DischargeFollowUpLimit"].ToString()))
                        intDischargeFollowUpLimit = Convert.ToInt32(dtConfollowup.Rows[1]["DischargeFollowUpLimit"].ToString());
                }
                if (days <= intDischargeFollowUpDays && noOfVisits < intDischargeFollowUpLimit)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in DischargeFollowUp", "");
                return false;
            }
            finally
            {

            }
            return blnFlag;
        }

        public Boolean GetDoctorConfiguration()
        {
            try
            {

                DataSet dsConsultConfig = null;

                string strReturnMsg = "";

                int DocID = Convert.ToInt32(Doctorid);
                int HospID = Convert.ToInt32(strDefaultHospitalId);
                int intUId = Convert.ToInt32(strDefaultUserId);
                int intWId = Convert.ToInt32(strDefWorkstationId);
                dsConsultConfig = FetchConsultantConfiguration(DocID, Convert.ToInt32(HospID), "1", intUId, intWId, 0);

                if (dsConsultConfig != null)
                {
                    if (dsConsultConfig.Tables[0].Rows.Count > 0)
                    {
                        bool bWalkinLimit = false;
                        int MCCCount = 0, intActAppt = 0, intWalkinBilled = 0;
                        System.Collections.Generic.Dictionary<int, string> dictWeekdays = GetWeekDaysPair();
                        System.Collections.Generic.List<string> strWeekDays = GetWeekDays();
                        dsConsultConfig.Tables[0].Columns.Add("WeekDayName");

                        DateTime ScheduleDate = DateTime.Today;

                        string str = ScheduleDate.ToString("dd-MMM-yyyy");
                        DateTime dt = Convert.ToDateTime(str);

                        string strFilter = " Blocked = 0 AND DoctorID=" + DocID + " AND Convert(Varchar(10),ScheduleDate,120)='" + ScheduleDate.ToString("yyyy-MM-dd") + "'";
                        DataSet dsAppointments = FetchAppointment(strFilter, intUId, intWId, 0);

                        string strWalkinFilter = "Status = 1 and StartTime is null and Convert(Varchar(10),OrderDate,110) = '" + ScheduleDate.ToString("MM-dd-yyyy") + "' and DoctorID = " + DocID.ToString();
                        DataSet dsWalkinBilled = FetchConsultations_Perf(1, strWalkinFilter, intUId, intWId, 0, 0, DocID, ScheduleDate, HospID);

                        if (dsAppointments.Tables.Count > 0)
                        {
                            if (dsAppointments.Tables[0].Rows.Count > 0)
                            {
                                intActAppt = Convert.ToInt32(dsAppointments.Tables[0].Rows[0]["Count"]);
                            }
                        }
                        if (dsWalkinBilled.Tables[0].Rows.Count > 0)
                        {
                            intWalkinBilled = dsWalkinBilled.Tables[0].Rows.Count;
                        }
                        foreach (DataRow drv in dsConsultConfig.Tables[0].Rows)
                        {
                            foreach (System.Collections.Generic.KeyValuePair<int, string> item in dictWeekdays)
                            {
                                if (item.Key.ToString() == drv["weekday"].ToString())
                                {
                                    drv["WeekdayName"] = item.Value;
                                }
                            }
                        }
                        dsConsultConfig.Tables[0].AcceptChanges();
                        foreach (DataRow dr in dsConsultConfig.Tables[0].Rows)
                        {
                            if (dr["WeekDayName"].ToString() == DateTime.Today.DayOfWeek.ToString())
                            {
                                bWalkinLimit = (Boolean)dr["WalkinLimitAlert"];
                                MCCCount = Convert.ToInt32(dr["MaxConsultation"]);
                                break;
                            }
                        }
                        if (bWalkinLimit)
                        {
                            strReturnMsg = "The Doctor has Walkin Limit specified for " + DateTime.Today.DayOfWeek.ToString();
                            return true;
                        }
                        else
                        {
                            int balWalkin = MCCCount - intActAppt - intWalkinBilled;

                            if (MCCCount != 0 && balWalkin <= 0)
                            {
                                if (WalkinBalance == null || WalkinBalance.ToString() == "false")
                                {
                                    //ScriptManager.RegisterStartupScript(Page, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','There is no balance walkin for the Doctor.<br>Max Consultation Count: " + MCCCount.ToString() + "<br>Walkin Balance: " + balWalkin.ToString() + "<br>Do you want to continue ?','YESNO','Warning');", true);
                                    WalkinBalance = "true";
                                    strReturnMsg = "There is no balance walkin for the Doctor.<br>Max Consultation Count: " + MCCCount.ToString() + "<br>Walkin Balance: " + balWalkin.ToString() + "<br>Do you want to continue ?";
                                    // ScriptManager.RegisterStartupScript(this, Page.GetType(), "OutPatient Billing", "ShowMsgBox('" + this.Title + "','" + strReturnMsg + "','YESNO','Information');", true);
                                    return true;

                                }

                            }
                        }
                    }

                }
                return false;


            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in GetDoctorConfiguration", "");
                return false;
            }
            finally
            {

            }
        }

        public DataSet FetchPackageLOA(int intType, string strFilter, int intUserId, int intWorkstationid, int intError, int HospitalID)
        {
            try
            {
                objFOClient = new FrontOfficeServiceContractClient();
                return objFOClient.FetchHospitalPackageLOA(intType, strFilter, intUserId, intWorkstationid, intError, HospitalID);
            }

            finally
            {
                objFOClient.Close();
            }
        }

        public DataSet FetchPackageLOA(int intType, string strFilter, int intUserId, int intWorkstationid, int intError)
        {
            try
            {
                objFOClient = new FrontOfficeServiceContractClient();
                return objFOClient.FetchPackageLOA(intType, strFilter, intUserId, intWorkstationid, intError);
            }

            finally
            {
                objFOClient.Close();
            }
        }

        public DataSet FetchMaxconsultdays(int DocID, string TBL, int UserId, int WstationId, int intError)
        {

            FrontOfficeServiceContractClient objfrontoffice = new FrontOfficeServiceContractClient();
            try
            {
                return objfrontoffice.FetchMaxconsultdays(DocID, TBL, UserId, WstationId, intError);
            }
            finally
            {
                objfrontoffice.Close();
            }
        }

        private DataTable GetGeneralExclusionCategory(int TariffID)
        {
            try
            {

                DataSet dsData = FetchfromAdv("Pr_FetchGeneralExculusiongroupsAdv", "2", "Blocked = 0 and TariffID=" + TariffID + "", Convert.ToInt32(strDefaultUserId), Convert.ToInt32(strDefWorkstationId), null, null, "FetchGeneralExculusion");
                DataTable dtExclusionsD = null;
                if (dsData.Tables.Count > 0)
                {


                    for (int i = 0; i < dsData.Tables[0].Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            dtExclusionsD = BindCategoryItems(Convert.ToInt32(dsData.Tables[0].Rows[i]["PackageItemCategoryID"]));
                        }
                        else
                        {
                            dtExclusionsD.Merge(BindCategoryItems(Convert.ToInt32(dsData.Tables[0].Rows[i]["PackageItemCategoryID"])));
                        }
                    }
                    if (dtExclusionsD != null)
                    {
                        DataTable dtExclusions = dtExclusionsD.DefaultView.ToTable( /*distinct*/ true);
                        return dtExclusions;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in GetGeneralExclusionCategory", "");
                return null;
            }
        }

        public DataSet FetchGeneralExculusionMAPI(int CompanyID, int GradeID, string Serviceitemid, int HospitalID, int intWorkstationId, PatientBillList PatientBillList)
        {
            DataHelper objDataHelper = new DataHelper(DEFAULTWORKSTATION, (int)Database.Master);
            DataSet dsSpecConfig = new DataSet();
            try
            {
                List<IDbDataParameter> objIDbDataParameters = new List<IDbDataParameter>();
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@CompanyID", CompanyID.ToString(), DbType.Int32, ParameterDirection.Input));
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@GradeID", GradeID.ToString(), DbType.Int32, ParameterDirection.Input));
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@Serviceitemid", Serviceitemid.ToString(), DbType.String, ParameterDirection.Input));
                dsSpecConfig = objDataHelper.RunSPReturnDS("Pr_FetchGeneralExculusion_MAPI", objIDbDataParameters.ToArray());
                return dsSpecConfig;
            }
            finally
            {

            }
        }

        public DataSet GetDiscountsConfig(int DiscountId, string strTableId, int FeatureId, int FunctionId, string CallContext, int UserId, int WstationId, int intError)
        {

            DataSet ds = new DataSet();
            FrontOfficeServiceContractClient objFrontOfficeContractClient = new FrontOfficeServiceContractClient();
            try
            {
                ds = objFrontOfficeContractClient.FetchDiscountsConfig(DiscountId, strTableId, FeatureId, FunctionId, CallContext, UserId, WstationId, intError);
                return ds;
            }
            finally
            {
                objFrontOfficeContractClient.Close();
            }
        }

        public DataSet GetDiscountConfigNew(DataSet ds, int intDiscountId, double TotalAmount)
        {
            DataSet dsConfiguredDetails = new DataSet();
            dsConfiguredDetails.Clear();
            int intDiscountLevel = 0;
            string strDiscountType = string.Empty;
            try
            {

                bool IsPercentage = false;
                double DisValue = 0;
                DataRow[] drConf = ds.Tables[0].Select("Blocked=0");
                if (drConf.Length > 0)
                {
                    intDiscountLevel = Convert.ToInt32(drConf[0]["DiscountLevel"].ToString());
                    strDiscountType = Convert.ToString(drConf[0]["DiscountName"].ToString());
                    IsPercentage = Convert.ToBoolean(drConf[0]["IsPercentage"]);
                    DisValue = Convert.ToDouble(drConf[0]["Value"].ToString());
                    if (intDiscountLevel > -1)
                    {
                        DataTable dtOnBill = new DataTable("OnBill");
                        dtOnBill = GetDiscountDetails().Clone();
                        if (!dtOnBill.Columns.Contains("IsPercentage"))
                        { dtOnBill.Columns.Add("IsPercentage", typeof(bool)); }

                        dtOnBill.AcceptChanges();

                        DataTable dtOnItem = new DataTable("OnItem");
                        dtOnItem = dtOnBill.Clone();

                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            DataRow drnew = dtOnBill.NewRow();
                            drnew["mLevel"] = intDiscountLevel;
                            drnew["Type"] = strDiscountType;
                            drnew["ServiceId"] = 0;
                            drnew["ServiceName"] = "";
                            drnew["HospDeptID"] = 0;
                            drnew["DepartmentName"] = "";
                            drnew["SpecialiseID"] = 0;
                            drnew["SpecialiseName"] = "";
                            drnew["ServiceItemID"] = 0;
                            drnew["ItemName"] = "";
                            drnew["Total"] = TotalAmount;
                            drnew["Discount"] = dr["Value"];
                            drnew["IsPercentage"] = dr["IsPercentage"];
                            dtOnBill.Rows.Add(drnew);
                            DataRow drnewOB = dtOnItem.NewRow();
                            drnewOB["mLevel"] = intDiscountLevel;
                            drnewOB["Type"] = strDiscountType;
                            drnewOB["ServiceId"] = 0;
                            drnewOB["ServiceName"] = "";
                            drnewOB["HospDeptID"] = 0;
                            drnewOB["DepartmentName"] = "";
                            drnewOB["SpecialiseID"] = 0;
                            drnewOB["SpecialiseName"] = "";
                            drnewOB["ServiceItemID"] = 0;
                            drnewOB["ItemName"] = "";
                            drnewOB["Total"] = TotalAmount;
                            drnewOB["Discount"] = dr["Value"];
                            drnewOB["IsPercentage"] = dr["IsPercentage"];

                            dtOnItem.Rows.Add(drnewOB);

                        }
                        dtOnBill.Columns["SpecialiseName"].ColumnName = "Specialisation";
                        dtOnBill.Columns["ItemName"].ColumnName = "ServiceItemName";
                        dtOnBill.AcceptChanges();
                        dtOnBill.TableName = "OnBill";
                        dsConfiguredDetails.Tables.Add(dtOnBill);


                        if (ds.Tables[1].Rows.Count > 0)
                        {
                            DataRow[] dr = ds.Tables[1].Select("Blocked = 0");
                            for (int intctr = 0; intctr < dr.Length; intctr++)
                            {
                                DataRow drnew = dtOnItem.NewRow();
                                drnew["mLevel"] = dr[intctr]["Mlevel"];
                                drnew["Type"] = strDiscountType;
                                drnew["ServiceId"] = dr[intctr]["ServiceId"];
                                drnew["ServiceName"] = dr[intctr]["ServiceName"];
                                drnew["HospDeptID"] = dr[intctr]["HospDeptId"];
                                drnew["DepartmentName"] = dr[intctr]["HospDeptName"];
                                drnew["SpecialiseID"] = dr[intctr]["SpecialiseID"];
                                drnew["SpecialiseName"] = dr[intctr]["Specialisation"];
                                drnew["ServiceItemID"] = dr[intctr]["ItemId"];
                                drnew["ItemName"] = dr[intctr]["DisplayName"];
                                drnew["Total"] = 0;
                                drnew["Discount"] = dr[intctr]["Value"];
                                drnew["IsPercentage"] = dr[intctr]["IsPercentage"];
                                dtOnItem.Rows.Add(drnew);

                            }
                            dtOnItem.Columns["SpecialiseName"].ColumnName = "Specialisation";
                            dtOnItem.Columns["ItemName"].ColumnName = "ServiceItemName";
                            dtOnItem.AcceptChanges();
                            dtOnItem.TableName = "OnItem";
                            dsConfiguredDetails.Tables.Add(dtOnItem);
                        }
                        else
                        {
                            dtOnItem.Columns["SpecialiseName"].ColumnName = "Specialisation";
                            dtOnItem.Columns["ItemName"].ColumnName = "ServiceItemName";
                            dtOnItem.AcceptChanges();
                            dtOnItem.TableName = "OnItem";
                            dsConfiguredDetails.Tables.Add(dtOnItem);
                        }

                    }
                }
                return dsConfiguredDetails;
            }
            catch (Exception ex)
            {

                return dsConfiguredDetails;
            }
        }

        public DataTable ProcessDiscountConfigurationLatest(int DiscLevel, DataTable dtSelected, DataTable dtServices, int IPOP, DataSet dsConfiguration, string StrBillType, string DiscountType, string Check, decimal Pay)
        {
            if (Check == "0")
            {
                radPatient = true;
                radpayer = false;
                PAmount = Pay;
                CAmount = 0;
            }
            else
            {
                radpayer = true;
                radPatient = false;
                PAmount = 0;
                CAmount = Pay;
            }
            string[] strFilterType = FilterType(StrBillType, IPOP);
            if (!dtSelected.Columns.Contains("ISDISCAPP"))
                dtSelected.Columns.Add("ISDISCAPP", typeof(bool));
            if (dsConfiguration.Tables.Count > 0)
            {
                if (dsConfiguration.Tables[1].Rows.Count > 0)
                {
                    if (!dsConfiguration.Tables[1].Columns.Contains("ISDISCAPP"))
                        dsConfiguration.Tables[1].Columns.Add("ISDISCAPP", typeof(bool));
                }
            }
            DataTable dtfinal = new DataTable();
            decimal dcCPAY, dcPPAY = 0; string strFilter = string.Empty;
            try
            {
                if (dtSelected.Columns.Contains("ISDISCPR"))
                {
                    foreach (DataRow dr1 in dtSelected.Select("mlevel=5 and serviceid=9 and ISDISCPR is not null"))
                    {
                        DataRow[] drsplz = dtSelected.Select("mlevel=4 and serviceid=9 and hospdeptid=" + dr1["hospdeptid"] + " and specialiseid=" + dr1["SpecialiseId"]);
                        if (drsplz.Length > 0)
                            drsplz[0]["CPAY"] = Convert.ToDouble(drsplz[0]["CPAY"]) - Convert.ToDouble(dr1["CPAY"]);

                        DataRow[] drdept = dtSelected.Select("mlevel=3 and serviceid=9 and hospdeptid=" + dr1["hospdeptid"]);
                        if (drdept.Length > 0)
                            drdept[0]["CPAY"] = Convert.ToDouble(drdept[0]["CPAY"]) - Convert.ToDouble(dr1["CPAY"]);

                        DataRow[] drsrv = dtSelected.Select("mlevel=2 and serviceid=9 ");
                        if (drsrv.Length > 0)
                            drsrv[0]["CPAY"] = Convert.ToDouble(drsrv[0]["CPAY"]) - Convert.ToDouble(dr1["CPAY"]);


                        dtSelected.Rows.Remove(dr1);
                    }
                }
                dtBillDetails = dtSelected.Copy();
                /*  Discount Types 
                    *  -1 - Open
                    *   0 - On Bill
                    *   2 - On Service
                    *   3 - On Department
                    *   4 - On Specialisation
                    *   5 - On Item  					*/


                DataTable dtTempFinal = new DataTable();
                DataTable dtSelectedforSorting = new DataTable();

                decimal dDCOM = 0; decimal dDPAT = 0; decimal decPat = 0; decimal decComp = 0;

                decimal dcDiscountAmount = 0;
                decimal dblVal = 0;

                dtSelectedforSorting = dtSelected.Copy();

                if (!dtSelected.Columns.Contains("BatchID"))
                {
                    if (dsConfiguration.Tables.Count > 0)
                        dsConfiguration.Tables["OnItem"].Columns.Add("BatchID", typeof(int));
                }
                dtTempFinal = dsConfiguration.Tables["OnItem"].Clone();
                DataRow[] drconfig = dtSelected.Select("", " mlevel Desc");
                for (int intctr = 0; intctr < drconfig.Length; intctr++)
                {
                    DiscLevel = Convert.ToInt32(drconfig[intctr]["mLevel"].ToString());

                    if (dsConfiguration.Tables["OnItem"].Select(" mlevel= " + DiscLevel).Length == 0)
                        continue;

                    switch (DiscLevel)
                    {
                        case 0: // O N   B I L L 
                            if (dtTempFinal.Rows.Count == 0)
                            {
                                dtfinal = dsConfiguration.Tables["OnItem"].Clone();
                                DataTable dttemp = LoadDetails(dtSelected, DiscLevel, strFilterType[0]).Copy();
                                foreach (DataRow dr in dsConfiguration.Tables["OnItem"].Select(" mlevel= " + DiscLevel))
                                {
                                    DataRow[] drcon = dttemp.Select(" mLevel= " + dr["mLevel"]);
                                    for (int ictr = 0; ictr < drcon.Length; ictr++)
                                    { dtfinal.ImportRow(drcon[ictr]); }
                                    dtfinal.AcceptChanges();
                                }
                                DataRow[] drbill = dtfinal.Select();
                                if (drbill.Length > 0)
                                {

                                    if (radpayer)
                                    {
                                        if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                        {
                                            if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                            {
                                                dDCOM = Convert.ToDecimal(drbill[0]["CPAY"].ToString());
                                                dDPAT = Convert.ToDecimal(drbill[0]["PPAY"].ToString());
                                                drbill[0]["Total"] = dDCOM + dDPAT;
                                            }
                                            else
                                            {

                                                drbill[0]["Total"] = Convert.ToDecimal(CAmount).ToString();
                                            }
                                        }
                                        else
                                        {

                                            drbill[0]["Total"] = Convert.ToDecimal(CAmount).ToString();
                                        }
                                    }
                                    else
                                    { drbill[0]["Total"] = Convert.ToDecimal(PAmount).ToString(); }


                                    DataRow[] DrDisbill = dsConfiguration.Tables["OnItem"].Select();

                                    dcDiscountAmount = 0;
                                    dblVal = 0;
                                    if (Convert.ToBoolean(DrDisbill[0]["IsPercentage"]))
                                    {
                                        if (Convert.ToDecimal(DrDisbill[0]["Discount"]) >= Convert.ToDecimal(100))
                                        {
                                            dcDiscountAmount = Convert.ToDecimal(drbill[0]["Total"].ToString());
                                        }
                                        else
                                        {
                                            if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                            {
                                                if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                                {
                                                    decComp = (dDCOM * Convert.ToDecimal(DrDisbill[0]["Discount"])) / 100;
                                                    decPat = (dDPAT * Convert.ToDecimal(DrDisbill[0]["Discount"])) / 100;
                                                    dblVal = decComp + decPat;
                                                    dcDiscountAmount = Convert.ToDecimal(dblVal.ToString());
                                                }
                                                else
                                                {
                                                    dblVal = (Convert.ToDecimal(drbill[0]["Total"]) * Convert.ToDecimal(DrDisbill[0]["Discount"])) / 100;
                                                    dcDiscountAmount = Convert.ToDecimal(dblVal.ToString());
                                                }
                                            }
                                            else
                                            {
                                                dblVal = (Convert.ToDecimal(drbill[0]["Total"]) * Convert.ToDecimal(DrDisbill[0]["Discount"])) / 100;
                                                dcDiscountAmount = Convert.ToDecimal(dblVal.ToString());
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (Convert.ToDecimal(drbill[0]["Total"]) > Convert.ToDecimal(DrDisbill[0]["Discount"]))
                                        {
                                            dcDiscountAmount = Convert.ToDecimal(DrDisbill[0]["Discount"]);
                                        }
                                        else
                                        { dcDiscountAmount = Convert.ToDecimal(drbill[0]["Total"]); }
                                    }
                                    drbill[0]["Discount"] = Convert.ToDecimal(dcDiscountAmount).ToString();

                                    drbill[0]["Type"] = DiscountType;
                                    if (radPatient)
                                    {
                                        drbill[0]["DCOM"] = 0;
                                        drbill[0]["DPAT"] = drbill[0]["Discount"];
                                        drbill[0]["DPER"] = 0;
                                    }
                                    else if (radpayer)
                                    {
                                        if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                        {
                                            if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                            {
                                                drbill[0]["DCOM"] = decComp;
                                                drbill[0]["DPAT"] = decPat;
                                                drbill[0]["DPER"] = 0;
                                            }
                                            else
                                            {
                                                drbill[0]["DCOM"] = drbill[0]["Discount"];
                                                drbill[0]["DPAT"] = 0;
                                                drbill[0]["DPER"] = 0;
                                            }
                                        }
                                        else
                                        {
                                            drbill[0]["DCOM"] = drbill[0]["Discount"];
                                            drbill[0]["DPAT"] = 0;
                                            drbill[0]["DPER"] = 0;
                                        }
                                    }
                                    drbill[0]["CPAY"] = 0;
                                    drbill[0]["PPAY"] = 0;
                                    drbill[0]["QTY"] = 0;
                                    drbill[0]["BQTY"] = 0;
                                    drbill[0]["SEQ"] = 1;

                                    DataRow[] drBlFnl = dtfinal.Select();
                                    for (int ictr = 0; ictr < drBlFnl.Length; ictr++)
                                    { dtTempFinal.ImportRow(drBlFnl[ictr]); }
                                    dtTempFinal.AcceptChanges();
                                }
                            }

                            break;
                        case 2:// O N    S E R V I C E 
                            dtfinal = dsConfiguration.Tables["OnItem"].Clone();
                            DataTable dttempBill = LoadDetails(dtSelected, DiscLevel, strFilterType[0]).Copy();
                            dcCPAY = dcPPAY = 0;
                            strFilter = string.Empty;
                            DataTable dtLevelAppliedSer = LoadDetailsMPHL(dtSelected, 5, strFilterType[0]).Copy();

                            if (dtLevelAppliedSer.Rows.Count > 0)
                            {
                                DataRow[] droServ = dttempBill.Select();

                                for (int iLvl = 0; iLvl < droServ.Length; iLvl++)
                                {
                                    strFilter = "Serviceid=" + droServ[iLvl]["Serviceid"];
                                    dcCPAY = Convert.ToDecimal(dtLevelAppliedSer.Compute("sum(CPAY)", strFilter) == DBNull.Value ? 0 : dtLevelAppliedSer.Compute("sum(CPAY)", strFilter));
                                    dcPPAY = Convert.ToDecimal(dtLevelAppliedSer.Compute("sum(PPAY)", strFilter) == DBNull.Value ? 0 : dtLevelAppliedSer.Compute("sum(PPAY)", strFilter));
                                    droServ[iLvl]["CPAY"] = dcCPAY > 0 ? Convert.ToDecimal(droServ[iLvl]["CPAY"]) - dcCPAY : droServ[iLvl]["CPAY"];
                                    droServ[iLvl]["PPAY"] = dcPPAY > 0 ? Convert.ToDecimal(droServ[iLvl]["PPAY"]) - dcPPAY : droServ[iLvl]["PPAY"];
                                    if (Convert.ToString(strFilterType[0]) == "CPAY")
                                        droServ[iLvl]["Total"] = Convert.ToDecimal(droServ[iLvl]["Total"]) - dcCPAY;
                                    if (Convert.ToString(strFilterType[0]) == "PPAY")
                                        droServ[iLvl]["Total"] = Convert.ToDecimal(droServ[iLvl]["Total"]) - dcPPAY;

                                }
                                dttempBill.AcceptChanges();
                            }
                            strFilter = string.Empty;

                            foreach (DataRow dr in dsConfiguration.Tables["OnItem"].Select(" mlevel= " + DiscLevel + " and serviceid=" + drconfig[intctr]["serviceid"]))
                            {
                                strFilter = " ServiceId= " + dr["ServiceId"];
                                DataRow[] drcon = dttempBill.Select(" ServiceId= " + dr["ServiceId"]);
                                for (int ictr = 0; ictr < drcon.Length; ictr++)
                                {
                                    dtfinal.ImportRow(drcon[ictr]);
                                }
                                dtfinal.AcceptChanges();

                                dtSelected.AcceptChanges();
                            }



                            DataRow[] drfinal = dtfinal.Select();
                            for (int ictr = 0; ictr < drfinal.Length; ictr++)
                            {

                                string strCondition = "";
                                if (IPOP == 1)
                                { strCondition = " IsContribution = 1 and mLevel=2 and ServiceID=" + drfinal[ictr]["ServiceId"]; }
                                else if (IPOP == 2)
                                { strCondition = " mLevel=2 and ServiceID=" + drfinal[ictr]["ServiceId"]; }

                                string str = "";
                                if (IPOP == 1)
                                { str = strCondition.Substring(23); }
                                else if (IPOP == 2)
                                { str = strCondition; }

                                DataRow[] DrBillDet = dtBillDetails.Select(strCondition);

                                dcCPAY = Convert.ToDecimal(dtLevelAppliedSer.Compute("sum(CPAY)", "ServiceID=" + drfinal[ictr]["ServiceId"]) == DBNull.Value ? 0 : dtLevelAppliedSer.Compute("sum(CPAY)", "ServiceID=" + drfinal[ictr]["ServiceId"]));
                                dcPPAY = Convert.ToDecimal(dtLevelAppliedSer.Compute("sum(PPAY)", "ServiceID=" + drfinal[ictr]["ServiceId"]) == DBNull.Value ? 0 : dtLevelAppliedSer.Compute("sum(PPAY)", "ServiceID=" + drfinal[ictr]["ServiceId"]));


                                if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                {
                                    if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                    {
                                        dDCOM = Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString());
                                        dDPAT = Convert.ToDecimal(DrBillDet[0]["PPAY"].ToString());
                                        drfinal[ictr]["Total"] = (dDCOM - dcCPAY) + (dDPAT - dcPPAY);
                                    }
                                    else
                                    {
                                        drfinal[ictr]["Total"] = Convert.ToString(strFilterType[0]) == "PPAY" ? (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcPPAY).ToString() : (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcCPAY).ToString();
                                    }
                                }
                                else
                                {
                                    drfinal[ictr]["Total"] = Convert.ToString(strFilterType[0]) == "PPAY" ? (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcPPAY).ToString() : (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcCPAY).ToString();
                                }


                                DataRow[] DrDis = dsConfiguration.Tables["OnItem"].Select(str);


                                dcDiscountAmount = 0;
                                dblVal = 0;
                                if (Convert.ToBoolean(DrDis[0]["IsPercentage"]))
                                {
                                    if (Convert.ToDecimal(DrDis[0]["Discount"]) >= Convert.ToDecimal(100))
                                    {
                                        dcDiscountAmount = Convert.ToDecimal(drfinal[ictr]["Total"]);
                                    }
                                    else
                                    {
                                        if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                        {
                                            if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                            {
                                                decComp = (dDCOM * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;
                                                decPat = (dDPAT * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;

                                                dblVal = decComp + decPat;
                                                dcDiscountAmount = Convert.ToDecimal(dblVal.ToString());
                                            }
                                            else
                                            {
                                                dblVal = (Convert.ToDecimal(drfinal[ictr]["Total"]) * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;
                                                dcDiscountAmount = Convert.ToDecimal(dblVal.ToString());
                                            }
                                        }
                                        else
                                        {
                                            dblVal = (Convert.ToDecimal(drfinal[ictr]["Total"]) * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;
                                            dcDiscountAmount = Convert.ToDecimal(dblVal.ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    if (Convert.ToDecimal(drfinal[ictr]["Total"]) > Convert.ToDecimal(DrDis[0]["Discount"]))
                                    {
                                        dcDiscountAmount = Convert.ToDecimal(DrDis[0]["Discount"]);
                                    }
                                    else
                                    { dcDiscountAmount = Convert.ToDecimal(drfinal[ictr]["Total"]); }
                                }
                                if (ConfigurationSettings.AppSettings["Yaico"].ToString().ToUpper() == "YES")
                                    dcDiscountAmount = Math.Round(dcDiscountAmount, 2);

                                drfinal[ictr]["Discount"] = Convert.ToDecimal(dcDiscountAmount).ToString();

                                drfinal[ictr]["Type"] = DiscountType;
                                if (radPatient)
                                {
                                    drfinal[ictr]["DCOM"] = 0;
                                    drfinal[ictr]["DPAT"] = drfinal[ictr]["Discount"];
                                    drfinal[ictr]["DPER"] = 0;
                                }
                                else if (radpayer)
                                {
                                    if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                    {
                                        if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                        {
                                            drfinal[ictr]["DCOM"] = decComp;
                                            drfinal[ictr]["DPAT"] = decPat;
                                            drfinal[ictr]["DPER"] = 0;
                                        }
                                        else
                                        {
                                            drfinal[ictr]["DCOM"] = drfinal[ictr]["Discount"];
                                            drfinal[ictr]["DPAT"] = 0;
                                            drfinal[ictr]["DPER"] = 0;
                                        }
                                    }
                                    else
                                    {
                                        drfinal[ictr]["DCOM"] = drfinal[ictr]["Discount"];
                                        drfinal[ictr]["DPAT"] = 0;
                                        drfinal[ictr]["DPER"] = 0;
                                    }
                                }
                                drfinal[ictr]["CPAY"] = 0;
                                drfinal[ictr]["PPAY"] = 0;
                                drfinal[ictr]["QTY"] = 0;
                                drfinal[ictr]["BQTY"] = 0;
                                if (!string.IsNullOrEmpty(DrBillDet[0]["SEQ"].ToString()))
                                    drfinal[ictr]["SEQ"] = Convert.ToInt32(DrBillDet[0]["SEQ"].ToString());
                                else
                                    drfinal[ictr]["SEQ"] = 0;
                            }
                            DataRow[] drSrFnl = dtfinal.Select();
                            for (int ictr = 0; ictr < drSrFnl.Length; ictr++)
                            {
                                dtTempFinal.ImportRow(drSrFnl[ictr]);
                            }
                            dtTempFinal.AcceptChanges();

                            break;
                        case 3:// O N     D E P A R T M E N T 
                            dtfinal = dsConfiguration.Tables["OnItem"].Clone();
                            DataTable dttempDep = LoadDetails(dtSelected, DiscLevel, strFilterType[0]).Copy();


                            DataTable dtLevelAppliedDep = LoadDetailsMPHL(dtSelected, 5, strFilterType[0]).Copy();
                            dcCPAY = 0; dcPPAY = 0; strFilter = string.Empty;
                            if (dtLevelAppliedDep.Rows.Count > 0)
                            {

                                DataRow[] droDep = dttempDep.Select();

                                for (int iLvl = 0; iLvl < droDep.Length; iLvl++)
                                {
                                    strFilter = "Serviceid=" + droDep[iLvl]["Serviceid"] + " and HospDeptID=" + droDep[iLvl]["HospDeptID"];
                                    dcCPAY = Convert.ToDecimal(dtLevelAppliedDep.Compute("sum(CPAY)", strFilter) == DBNull.Value ? 0 : dtLevelAppliedDep.Compute("sum(CPAY)", strFilter));
                                    dcPPAY = Convert.ToDecimal(dtLevelAppliedDep.Compute("sum(PPAY)", strFilter) == DBNull.Value ? 0 : dtLevelAppliedDep.Compute("sum(PPAY)", strFilter));
                                    droDep[iLvl]["CPAY"] = dcCPAY > 0 ? Convert.ToDecimal(droDep[iLvl]["CPAY"]) - dcCPAY : droDep[iLvl]["CPAY"];
                                    droDep[iLvl]["PPAY"] = dcPPAY > 0 ? Convert.ToDecimal(droDep[iLvl]["PPAY"]) - dcPPAY : droDep[iLvl]["PPAY"];
                                }
                                dttempDep.AcceptChanges();
                            }
                            strFilter = string.Empty;

                            foreach (DataRow dr in dsConfiguration.Tables["OnItem"].Select(" mlevel= " + DiscLevel + " and ServiceId= " + drconfig[intctr]["ServiceId"] + " and HospDeptID= " + drconfig[intctr]["HospDeptID"]))
                            {
                                strFilter = " ServiceId= " + dr["ServiceId"] + " and HospDeptID= " + dr["HospDeptID"];
                                DataRow[] drcon = dttempDep.Select(" ServiceId= " + dr["ServiceId"] + " and HospDeptID= " + dr["HospDeptID"]);
                                for (int ictr = 0; ictr < drcon.Length; ictr++)
                                {
                                    dtfinal.ImportRow(drcon[ictr]);
                                    SetDiscLevelAppliedMPHL(dtSelected, strFilter);
                                }
                                dtfinal.AcceptChanges();
                                dtSelected.AcceptChanges();
                            }

                            DataRow[] drfinalDep = dtfinal.Select();
                            for (int ictr = 0; ictr < drfinalDep.Length; ictr++)
                            {

                                string strCondition = "";
                                if (IPOP == 1)
                                { strCondition = " IsContribution = 1 and mLevel=3 and ServiceID=" + drfinalDep[ictr]["ServiceId"] + " and HospDeptId=" + drfinalDep[ictr]["HospDeptId"]; }
                                else if (IPOP == 2)
                                { strCondition = " mLevel=3 and ServiceID=" + drfinalDep[ictr]["ServiceId"] + " and HospDeptId=" + drfinalDep[ictr]["HospDeptId"]; }

                                string str = "";
                                if (IPOP == 1)
                                { str = strCondition.Substring(23); }
                                else if (IPOP == 2)
                                { str = strCondition; }

                                DataRow[] DrBillDet = dtBillDetails.Select(strCondition);
                                if (DrBillDet.Length > 0)
                                {
                                    dcCPAY = Convert.ToDecimal(dtLevelAppliedDep.Compute("sum(CPAY)", "ServiceID=" + drfinalDep[ictr]["ServiceId"] + " and HospDeptId=" + drfinalDep[ictr]["HospDeptId"]) == DBNull.Value ? 0 : dtLevelAppliedDep.Compute("sum(CPAY)", "ServiceID=" + drfinalDep[ictr]["ServiceId"] + " and HospDeptId=" + drfinalDep[ictr]["HospDeptId"]));
                                    dcPPAY = Convert.ToDecimal(dtLevelAppliedDep.Compute("sum(PPAY)", "ServiceID=" + drfinalDep[ictr]["ServiceId"] + " and HospDeptId=" + drfinalDep[ictr]["HospDeptId"]) == DBNull.Value ? 0 : dtLevelAppliedDep.Compute("sum(PPAY)", "ServiceID=" + drfinalDep[ictr]["ServiceId"] + " and HospDeptId=" + drfinalDep[ictr]["HospDeptId"]));
                                    drfinalDep[ictr]["Total"] = Convert.ToString(strFilterType[0]) == "PPAY" ? (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcPPAY).ToString() : (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcCPAY).ToString();

                                    if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                    {
                                        if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                        {
                                            dDCOM = Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString());
                                            dDPAT = Convert.ToDecimal(DrBillDet[0]["PPAY"].ToString());
                                            drfinalDep[ictr]["Total"] = (dDCOM - dcCPAY) + (dDPAT - dcPPAY);
                                        }
                                        else
                                        {
                                            drfinalDep[ictr]["Total"] = Convert.ToString(strFilterType[0]) == "PPAY" ? (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcPPAY).ToString() : (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcCPAY).ToString();

                                        }
                                    }
                                    else
                                    {
                                        drfinalDep[ictr]["Total"] = Convert.ToString(strFilterType[0]) == "PPAY" ? (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcPPAY).ToString() : (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcCPAY).ToString();
                                    }
                                }
                                DataRow[] DrDis = dsConfiguration.Tables["OnItem"].Select(str);
                                dcDiscountAmount = 0;
                                dblVal = 0;
                                if (Convert.ToBoolean(DrDis[0]["IsPercentage"]))
                                {
                                    if (Convert.ToDecimal(DrDis[0]["Discount"]) >= Convert.ToDecimal(100))
                                    {
                                        dcDiscountAmount = Convert.ToDecimal(drfinalDep[ictr]["Total"]);
                                    }
                                    else
                                    {
                                        if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                        {
                                            if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                            {
                                                decComp = (dDCOM * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;
                                                decPat = (dDPAT * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;
                                                dblVal = decComp + decPat;
                                                dcDiscountAmount = Convert.ToDecimal(dblVal.ToString());
                                            }
                                            else
                                            {
                                                dblVal = (Convert.ToDecimal(drfinalDep[ictr]["Total"]) * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;
                                                dcDiscountAmount = Convert.ToDecimal(dblVal.ToString());
                                            }
                                        }
                                        else
                                        {
                                            dblVal = (Convert.ToDecimal(drfinalDep[ictr]["Total"]) * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;
                                            dcDiscountAmount = Convert.ToDecimal(dblVal.ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    if (Convert.ToDecimal(drfinalDep[ictr]["Total"]) > Convert.ToDecimal(DrDis[0]["Discount"]))
                                    {
                                        dcDiscountAmount = Convert.ToDecimal(DrDis[0]["Discount"]);
                                    }
                                    else
                                    { dcDiscountAmount = Convert.ToDecimal(drfinalDep[ictr]["Total"]); }
                                }

                                drfinalDep[ictr]["Discount"] = Convert.ToDecimal(dcDiscountAmount).ToString();

                                drfinalDep[ictr]["Type"] = DiscountType;
                                if (radPatient)
                                {
                                    drfinalDep[ictr]["DCOM"] = 0;
                                    drfinalDep[ictr]["DPAT"] = drfinalDep[ictr]["Discount"];
                                    drfinalDep[ictr]["DPER"] = 0;
                                }
                                else if (radpayer)
                                {
                                    if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                    {
                                        if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                        {
                                            drfinalDep[ictr]["DCOM"] = decComp;
                                            drfinalDep[ictr]["DPAT"] = decPat;
                                            drfinalDep[ictr]["DPER"] = 0;
                                        }
                                        else
                                        {
                                            drfinalDep[ictr]["DCOM"] = drfinalDep[ictr]["Discount"];
                                            drfinalDep[ictr]["DPAT"] = 0;
                                            drfinalDep[ictr]["DPER"] = 0;
                                        }
                                    }
                                    else
                                    {
                                        drfinalDep[ictr]["DCOM"] = drfinalDep[ictr]["Discount"];
                                        drfinalDep[ictr]["DPAT"] = 0;
                                        drfinalDep[ictr]["DPER"] = 0;
                                    }
                                }
                                drfinalDep[ictr]["CPAY"] = 0;
                                drfinalDep[ictr]["PPAY"] = 0;
                                drfinalDep[ictr]["QTY"] = 0;
                                drfinalDep[ictr]["BQTY"] = 0;

                                if (!string.IsNullOrEmpty(DrBillDet[0]["SEQ"].ToString()))
                                    drfinalDep[ictr]["SEQ"] = Convert.ToInt32(DrBillDet[0]["SEQ"].ToString());
                                else
                                    drfinalDep[ictr]["SEQ"] = 0;

                            }
                            DataRow[] drDpFnl = dtfinal.Select();
                            for (int ictr = 0; ictr < drDpFnl.Length; ictr++)
                            { dtTempFinal.ImportRow(drDpFnl[ictr]); }
                            dtTempFinal.AcceptChanges();

                            break;
                        case 4://  O N    S P E C I A L I S A T I O N 
                            dtfinal = dsConfiguration.Tables["OnItem"].Clone();
                            DataTable dttempspl = LoadDetails(dtSelected, DiscLevel, strFilterType[0]).Copy();
                            DataTable dtLevelAppliedItm = LoadDetailsMPHL(dtSelected, 5, strFilterType[0]).Copy();
                            dcCPAY = 0; dcPPAY = 0; strFilter = string.Empty;

                            if (dtLevelAppliedItm.Rows.Count > 0)
                            {

                                DataRow[] droSpecial = dttempspl.Select();

                                for (int iLvl = 0; iLvl < droSpecial.Length; iLvl++)
                                {
                                    strFilter = "Serviceid=" + droSpecial[iLvl]["Serviceid"] + " and HospDeptID=" + droSpecial[iLvl]["HospDeptID"] + " and SpecialiseID=" + droSpecial[iLvl]["SpecialiseID"];
                                    dcCPAY = Convert.ToDecimal(dtLevelAppliedItm.Compute("sum(CPAY)", strFilter) == DBNull.Value ? 0 : dtLevelAppliedItm.Compute("sum(CPAY)", strFilter));
                                    dcPPAY = Convert.ToDecimal(dtLevelAppliedItm.Compute("sum(PPAY)", strFilter) == DBNull.Value ? 0 : dtLevelAppliedItm.Compute("sum(PPAY)", strFilter));
                                    droSpecial[iLvl]["CPAY"] = dcCPAY > 0 ? Convert.ToDecimal(droSpecial[iLvl]["CPAY"]) - dcCPAY : droSpecial[iLvl]["CPAY"];
                                    droSpecial[iLvl]["PPAY"] = dcPPAY > 0 ? Convert.ToDecimal(droSpecial[iLvl]["PPAY"]) - dcPPAY : droSpecial[iLvl]["PPAY"];


                                }
                                dttempspl.AcceptChanges();
                            }
                            strFilter = string.Empty;

                            foreach (DataRow dr in dsConfiguration.Tables["OnItem"].Select(" mlevel= " + DiscLevel + " and ServiceId= " + drconfig[intctr]["ServiceId"] + " and HospDeptID= " + drconfig[intctr]["HospDeptID"] + " and SpecialiseID= " + drconfig[intctr]["SpecialiseID"]))
                            {
                                strFilter = " ServiceId= " + dr["ServiceId"] + " and HospDeptID= " + dr["HospDeptID"] + " and SpecialiseID= " + dr["SpecialiseID"];

                                DataRow[] drcon = dttempspl.Select(" ServiceId= " + dr["ServiceId"] + " and HospDeptID= " + dr["HospDeptID"] + " and SpecialiseID= " + dr["SpecialiseID"]);
                                for (int ictr = 0; ictr < drcon.Length; ictr++)
                                {
                                    dtfinal.ImportRow(drcon[ictr]);
                                    SetDiscLevelAppliedMPHL(dtSelected, strFilter);
                                }
                                dtfinal.AcceptChanges();

                                dtSelected.AcceptChanges();
                            }



                            DataRow[] drfinalSpl = dtfinal.Select();
                            for (int ictr = 0; ictr < drfinalSpl.Length; ictr++)
                            {

                                string strCondition = "";
                                if (IPOP == 1)
                                { strCondition = " IsContribution = 1 and mLevel=4 and ServiceID=" + drfinalSpl[ictr]["ServiceId"] + " and HospDeptId=" + drfinalSpl[ictr]["HospDeptId"] + " and SpecialiseID=" + drfinalSpl[ictr]["SpecialiseId"]; }
                                else if (IPOP == 2)
                                { strCondition = " mLevel=4 and ServiceID=" + drfinalSpl[ictr]["ServiceId"] + " and HospDeptId=" + drfinalSpl[ictr]["HospDeptId"] + " and SpecialiseID=" + drfinalSpl[ictr]["SpecialiseId"]; }


                                string str = string.Empty;
                                if (IPOP == 1)
                                { str = strCondition.Substring(23); }
                                else if (IPOP == 2)
                                { str = strCondition; }

                                DataRow[] DrBillDet = dtBillDetails.Select(strCondition);

                                dcCPAY = Convert.ToDecimal(dtLevelAppliedItm.Compute("sum(CPAY)", "ServiceID=" + drfinalSpl[ictr]["ServiceId"] + " and HospDeptId=" + drfinalSpl[ictr]["HospDeptId"] + " and SpecialiseID=" + drfinalSpl[ictr]["SpecialiseId"]) == DBNull.Value ? 0 : dtLevelAppliedItm.Compute("sum(CPAY)", "ServiceID=" + drfinalSpl[ictr]["ServiceId"] + " and HospDeptId=" + drfinalSpl[ictr]["HospDeptId"] + " and SpecialiseID=" + drfinalSpl[ictr]["SpecialiseId"]));
                                dcPPAY = Convert.ToDecimal(dtLevelAppliedItm.Compute("sum(PPAY)", "ServiceID=" + drfinalSpl[ictr]["ServiceId"] + " and HospDeptId=" + drfinalSpl[ictr]["HospDeptId"] + " and SpecialiseID=" + drfinalSpl[ictr]["SpecialiseId"]) == DBNull.Value ? 0 : dtLevelAppliedItm.Compute("sum(PPAY)", "ServiceID=" + drfinalSpl[ictr]["ServiceId"] + " and HospDeptId=" + drfinalSpl[ictr]["HospDeptId"] + " and SpecialiseID=" + drfinalSpl[ictr]["SpecialiseId"]));
                                drfinalSpl[ictr]["Total"] = Convert.ToString(strFilterType[0]) == "PPAY" ? (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcPPAY).ToString() : (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcCPAY).ToString();

                                if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                {
                                    if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                    {
                                        dDCOM = Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString());
                                        dDPAT = Convert.ToDecimal(DrBillDet[0]["PPAY"].ToString());
                                        drfinalSpl[ictr]["Total"] = (dDCOM - dcCPAY) + (dDPAT - dcPPAY);
                                    }
                                    else
                                    {
                                        drfinalSpl[ictr]["Total"] = Convert.ToString(strFilterType[0]) == "PPAY" ? (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcPPAY).ToString() : (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcCPAY).ToString();
                                    }
                                }
                                else
                                {
                                    drfinalSpl[ictr]["Total"] = Convert.ToString(strFilterType[0]) == "PPAY" ? (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcPPAY).ToString() : (Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()) - dcCPAY).ToString();
                                }
                                DataRow[] DrDis = dsConfiguration.Tables["OnItem"].Select(str);
                                if (Convert.ToBoolean(DrDis[0]["IsPercentage"]))
                                {
                                    if (Convert.ToDecimal(DrDis[0]["Discount"]) >= Convert.ToDecimal(100))
                                    {
                                        dcDiscountAmount = Convert.ToDecimal(drfinalSpl[ictr]["Total"]);
                                    }
                                    else
                                    {
                                        if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                        {
                                            if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                            {
                                                decComp = (dDCOM * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;
                                                decPat = (dDPAT * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;
                                                dblVal = decComp + decPat;
                                                dcDiscountAmount = Convert.ToDecimal(dblVal.ToString());
                                            }
                                            else
                                            {
                                                dblVal = (Convert.ToDecimal(drfinalSpl[ictr]["Total"]) * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;
                                                dcDiscountAmount = Convert.ToDecimal(dblVal.ToString());
                                            }
                                        }
                                        else
                                        {
                                            dblVal = (Convert.ToDecimal(drfinalSpl[ictr]["Total"]) * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;
                                            dcDiscountAmount = Convert.ToDecimal(dblVal.ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    if (Convert.ToDecimal(drfinalSpl[ictr]["Total"]) > Convert.ToDecimal(DrDis[0]["Discount"]))
                                    {
                                        dcDiscountAmount = Convert.ToDecimal(DrDis[0]["Discount"]);
                                    }
                                    else
                                    { dcDiscountAmount = Convert.ToDecimal(drfinalSpl[ictr]["Total"]); }
                                }
                                drfinalSpl[ictr]["Discount"] = Convert.ToDecimal(dcDiscountAmount).ToString();

                                drfinalSpl[ictr]["Type"] = DiscountType;
                                if (radPatient)
                                {
                                    drfinalSpl[ictr]["DCOM"] = 0;
                                    drfinalSpl[ictr]["DPAT"] = drfinalSpl[ictr]["Discount"];
                                    drfinalSpl[ictr]["DPER"] = 0;
                                }
                                else if (radpayer)
                                {
                                    if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                    {
                                        if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                        {
                                            drfinalSpl[ictr]["DCOM"] = decComp;
                                            drfinalSpl[ictr]["DPAT"] = decPat;
                                            drfinalSpl[ictr]["DPER"] = 0;
                                        }
                                        else
                                        {
                                            drfinalSpl[ictr]["DCOM"] = drfinalSpl[ictr]["Discount"];
                                            drfinalSpl[ictr]["DPAT"] = 0;
                                            drfinalSpl[ictr]["DPER"] = 0;
                                        }
                                    }
                                    else
                                    {
                                        drfinalSpl[ictr]["DCOM"] = drfinalSpl[ictr]["Discount"];
                                        drfinalSpl[ictr]["DPAT"] = 0;
                                        drfinalSpl[ictr]["DPER"] = 0;
                                    }
                                }
                                drfinalSpl[ictr]["CPAY"] = 0;
                                drfinalSpl[ictr]["PPAY"] = 0;
                                drfinalSpl[ictr]["QTY"] = 0;
                                drfinalSpl[ictr]["BQTY"] = 0;
                                if (!string.IsNullOrEmpty(DrBillDet[0]["SEQ"].ToString()))
                                    drfinalSpl[ictr]["SEQ"] = Convert.ToInt32(DrBillDet[0]["SEQ"].ToString());
                                else
                                    drfinalSpl[ictr]["SEQ"] = 0;
                            }
                            DataRow[] drSpFnl = dtfinal.Select();
                            for (int ictr = 0; ictr < drSpFnl.Length; ictr++)
                            { dtTempFinal.ImportRow(drSpFnl[ictr]); }
                            dtTempFinal.AcceptChanges();

                            break;
                        case 5:// O N   I T E M

                            dtfinal = dsConfiguration.Tables["OnItem"].Clone();
                            DataTable dttempItm = LoadDetails(dtSelected, DiscLevel, strFilterType[0]).Copy();

                            if (dttempItm.Rows.Count > 0)
                            {

                                foreach (DataRow dr in dsConfiguration.Tables["OnItem"].Select(" mlevel= " + DiscLevel + " and serviceid = " + drconfig[intctr]["serviceid"] + " and serviceitemid = " + drconfig[intctr]["serviceitemid"]))
                                {

                                    DataRow[] DrSelService = dtServices.Select(" id = " + dr["ServiceId"]);
                                    if (DrSelService[0]["ItemHop"].ToString() == "1001")
                                        blnSkipDeptSpec = true;
                                    string strCond = "";
                                    if (blnSkipDeptSpec == false)
                                    {
                                        strCond = " ServiceId= " + dr["ServiceId"] + " and HospDeptID= " + dr["HospDeptID"] + " and SpecialiseID= " + dr["SpecialiseID"] + " and ServiceItemID= " + dr["ServiceItemID"];
                                    }
                                    else if (blnSkipDeptSpec == true)
                                    { strCond = " ServiceId= " + dr["ServiceId"] + " and ServiceItemID= " + dr["ServiceItemID"]; }

                                    DataRow[] drcon = dttempItm.Select(strCond);

                                    if (drcon.Length > 0)
                                    {
                                        dtfinal.ImportRow(drcon[0]);
                                        SetDiscLevelAppliedMPHL(dtSelected, strCond);
                                    }
                                    dtfinal.AcceptChanges();

                                    dtSelected.AcceptChanges();
                                }
                            }

                            DataRow[] drfinalItm = dtfinal.Select();
                            for (int ictr = 0; ictr < drfinalItm.Length; ictr++)
                            {
                                DataRow[] DrSelService = dtServices.Select(" id = " + drfinalItm[ictr]["ServiceId"]);
                                if (DrSelService[0]["ItemHop"].ToString() == "1001")
                                    blnSkipDeptSpec = true;
                                string strCondition = "";
                                if (blnSkipDeptSpec == false)
                                {
                                    if (IPOP == 1)
                                    { strCondition = " IsContribution = 1 and mLevel=5 and ServiceID=" + drfinalItm[ictr]["ServiceId"] + " and HospDeptId=" + drfinalItm[ictr]["HospDeptId"] + " and SpecialiseID=" + drfinalItm[ictr]["SpecialiseId"] + " and ServiceItemID=" + drfinalItm[ictr]["ServiceItemId"]; }
                                    else if (IPOP == 2)
                                    {
                                        strCondition = " mLevel=5 and ServiceID=" + drfinalItm[ictr]["ServiceId"] + " and HospDeptId=" + drfinalItm[ictr]["HospDeptId"] + " and SpecialiseID=" + drfinalItm[ictr]["SpecialiseId"] + " and ServiceItemID=" + drfinalItm[ictr]["ServiceItemId"];
                                    }
                                }
                                else if (blnSkipDeptSpec == true)
                                {
                                    if (IPOP == 1)
                                    {
                                        strCondition = " IsContribution = 1 and mLevel=5 and ServiceID=" + drfinalItm[ictr]["ServiceId"] + " and ServiceItemID=" + drfinalItm[ictr]["ServiceItemId"];
                                    }
                                    else if (IPOP == 2)
                                    {
                                        strCondition = " mLevel=5 and ServiceID=" + drfinalItm[ictr]["ServiceId"] + " and ServiceItemID=" + drfinalItm[ictr]["ServiceItemId"];
                                    }
                                }
                                DataRow[] DrBillDet = dtBillDetails.Select(strCondition);
                                if (IPOP == 2 && drconfig[intctr]["orderid"] != DBNull.Value && drconfig[intctr]["orderid"].ToString() != string.Empty)
                                    DrBillDet = dtBillDetails.Select(strCondition + " and orderid = " + drconfig[intctr]["orderid"]);

                                string str = "";
                                if (IPOP == 1)
                                { str = strCondition.Substring(23); }
                                else if (IPOP == 2)
                                { str = strCondition; }
                                if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                {
                                    if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                    {
                                        dDCOM = Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString());
                                        dDPAT = Convert.ToDecimal(DrBillDet[0]["PPAY"].ToString());
                                        drfinalItm[ictr]["Total"] = dDCOM + dDPAT;
                                    }
                                    else
                                    {
                                        drfinalItm[ictr]["Total"] = Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()).ToString();

                                    }
                                }
                                else
                                {
                                    drfinalItm[ictr]["Total"] = Convert.ToDecimal(DrBillDet[0][strFilterType[0]].ToString()).ToString();
                                }
                                DataRow[] DrDis = dsConfiguration.Tables["OnItem"].Select(str);
                                dcDiscountAmount = 0;
                                dblVal = 0;
                                if (Convert.ToBoolean(DrDis[0]["IsPercentage"]))
                                {
                                    if (Convert.ToDecimal(DrDis[0]["Discount"]) >= Convert.ToDecimal(100))
                                    {
                                        dcDiscountAmount = Convert.ToDecimal(drfinalItm[ictr]["Total"]);
                                    }
                                    else
                                    {
                                        if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                        {
                                            if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                            {
                                                decComp = (dDCOM * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;
                                                decPat = (dDPAT * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;

                                                dblVal = decComp + decPat;
                                                dcDiscountAmount = Convert.ToDecimal(dblVal.ToString());
                                            }
                                            else
                                            {
                                                dblVal = (Convert.ToDecimal(drfinalItm[ictr]["Total"]) * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;
                                                dcDiscountAmount = Convert.ToDecimal(dblVal.ToString());

                                            }
                                        }
                                        else
                                        {
                                            dblVal = (Convert.ToDecimal(drfinalItm[ictr]["Total"]) * Convert.ToDecimal(DrDis[0]["Discount"])) / 100;
                                            dcDiscountAmount = Convert.ToDecimal(dblVal.ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    if (Convert.ToDecimal(drfinalItm[ictr]["Total"]) / Convert.ToDecimal(drfinalItm[ictr]["BQTY"]) > Convert.ToDecimal(DrDis[0]["Discount"]))
                                    {
                                        dcDiscountAmount = Convert.ToDecimal(DrDis[0]["Discount"]) * Convert.ToDecimal(drfinalItm[ictr]["BQTY"]);
                                    }
                                    else
                                    { dcDiscountAmount = Convert.ToDecimal(drfinalItm[ictr]["Total"]); }
                                }
                                drfinalItm[ictr]["Discount"] = Convert.ToDecimal(dcDiscountAmount).ToString();
                                drfinalItm[ictr]["Type"] = DiscountType;
                                if (radPatient)
                                {
                                    drfinalItm[ictr]["DCOM"] = 0;
                                    drfinalItm[ictr]["DPAT"] = drfinalItm[ictr]["Discount"];
                                    drfinalItm[ictr]["DPER"] = 0;
                                }
                                else if (radpayer)
                                {
                                    if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                    {
                                        if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                        {
                                            drfinalItm[ictr]["DCOM"] = decComp;
                                            drfinalItm[ictr]["DPAT"] = decPat;
                                            drfinalItm[ictr]["DPER"] = 0;
                                        }
                                        else
                                        {
                                            drfinalItm[ictr]["DCOM"] = drfinalItm[ictr]["Discount"];
                                            drfinalItm[ictr]["DPAT"] = 0;
                                            drfinalItm[ictr]["DPER"] = 0;
                                        }
                                    }
                                    else
                                    {
                                        drfinalItm[ictr]["DCOM"] = drfinalItm[ictr]["Discount"];
                                        drfinalItm[ictr]["DPAT"] = 0;
                                        drfinalItm[ictr]["DPER"] = 0;
                                    }
                                }
                                drfinalItm[ictr]["CPAY"] = 0;
                                drfinalItm[ictr]["PPAY"] = 0;
                                drfinalItm[ictr]["QTY"] = Convert.ToDecimal(DrBillDet[0]["BQTY"].ToString());
                                drfinalItm[ictr]["BQTY"] = Convert.ToDecimal(DrBillDet[0][strFilterType[1]].ToString());
                                if (!string.IsNullOrEmpty(DrBillDet[0]["SEQ"].ToString()))
                                    drfinalItm[ictr]["SEQ"] = Convert.ToInt32(DrBillDet[0]["SEQ"].ToString());
                                else
                                    drfinalItm[ictr]["SEQ"] = 0;
                                if (!dtSelected.Columns.Contains("ISDISCAPP"))
                                    drfinalItm[ictr]["ISDISCAPP"] = true;

                            }
                            DataRow[] drIFnl = dtfinal.Select();
                            for (int ictr = 0; ictr < drIFnl.Length; ictr++)
                            { dtTempFinal.ImportRow(drIFnl[ictr]); }
                            dtTempFinal.AcceptChanges();
                            break;

                    }
                }

                if (dtTempFinal.Select("Mlevel=5").Length > 0)
                {
                    foreach (DataRow DrowIsApp in dtTempFinal.Select("Mlevel=5"))
                    {
                        DataRow[] droNewUpdate = dtSelectedforSorting.Select("Mlevel=5 and ServiceItemID=" + DrowIsApp["ServiceItemID"]);
                        for (int iUpdate = 0; iUpdate < droNewUpdate.Length; iUpdate++)
                        {
                            droNewUpdate[iUpdate]["ISDISCAPP"] = true;
                        }
                    }
                }
                dtSelectedforSorting.AcceptChanges();
                dtTempFinal = TempFinal(dsConfiguration.Tables[1], dtSelectedforSorting, dtTempFinal, 0, StrBillType, IPOP);
                return dtTempFinal;

            }
            catch (Exception ex)
            {
                return dtfinal;
            }
        }

        public DataSet FetchPatientAdmissionLetters(int intIPID, int intUserId, int intWorkstationid)
        {
            objFOClient = new FrontOfficeServiceContractClient();
            DataSet dsLetters = new DataSet();
            try
            {
                dsLetters = objFOClient.FetchPatientAdmissionLetters(intIPID, intUserId, intWorkstationid);
            }
            finally
            {
                objFOClient.Close();
            }
            return dsLetters;
        }

        public DataSet GetCategoryItems(int CategoryID, int intUserId, int intWorkStationId, int FeatureID, int FunctionID, string CallContext, int intError)
        {
            ContractMgmtServiceContractClient objContractMgmtServiceContractClient = new ContractMgmtServiceContractClient();
            DataSet dsCategoryItems = new DataSet();

            try
            {
                dsCategoryItems = objContractMgmtServiceContractClient.GetCategoryItems(CategoryID, intUserId, intWorkStationId, FeatureID, FunctionID, CallContext, intError);
            }

            finally
            {
                objContractMgmtServiceContractClient.Close();

            }
            return dsCategoryItems;
        }

        public DataSet FetchApprovalRequest(int EntryID, string tableid, int intUserID, int intWorkstationID, int intFeatureID, int intFunctionID, string strCallContext)
        {
            ARServiceContractClient objARServiceContractClient = new ARServiceContractClient();
            DataSet dsFetchApprovalRequest = new DataSet();
            try
            {
                dsFetchApprovalRequest = objARServiceContractClient.FetchApprovalRequest(EntryID, tableid, intUserID, intWorkstationID, intFeatureID, intFunctionID, strCallContext);
            }

            finally
            {
                objARServiceContractClient.Close();

            }
            return dsFetchApprovalRequest;
        }

        public DataSet FetchApprovalRequestEntryIDMAPI(int Visitid, int Customerid, int GradeID, int Specialiseid, int intWorkstationId, PatientBillList PatientBillList)
        {
            DataHelper objDataHelper = new DataHelper(DEFAULTWORKSTATION, (int)Database.Master);
            DataSet dsSpecConfig = new DataSet("ChkPatient");
            try
            {
                List<IDbDataParameter> objIDbDataParameters = new List<IDbDataParameter>();
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@Visitid", Visitid.ToString(), DbType.Int32, ParameterDirection.Input));
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@Customerid", Customerid, DbType.Int32, ParameterDirection.Input));
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@GradeID", GradeID, DbType.Int32, ParameterDirection.Input));
                objIDbDataParameters.Add(CreateParam(objDataHelper, "@Specialiseid", Specialiseid, DbType.Int32, ParameterDirection.Input));
                dsSpecConfig = objDataHelper.RunSPReturnDS("Pr_FetchApprovalRequestEntryIDs_MAPI", objIDbDataParameters.ToArray());
                return dsSpecConfig;
            }
            finally
            {

            }
        }

        public DataSet GetLOA(int intLetterId, int intLevel, string strTableId, int intUserId, int intWorkStationId, int intError, int intFeatureID, int intFunctionID, string strCallContext, int HospitalID)
        {

            ContractMgmtServiceContractClient objContractMgmtServiceContractClient = new ContractMgmtServiceContractClient();
            DataSet dsGetLOA = new DataSet();
            try
            {
                dsGetLOA = objContractMgmtServiceContractClient.GetLOAHospital(intLetterId, intLevel, strTableId, intUserId, intWorkStationId, intError, intFeatureID, intFunctionID, strCallContext, HospitalID);
            }

            finally
            {
                objContractMgmtServiceContractClient.Close();

            }
            return dsGetLOA;
        }
        public DataSet GetCoverageConfiguration(int intcompanyid, string strtbl, bool blnisinsurance, int intpatienttype, int intUserId, int intWorkStationId, int intError)
        {
            ContractMgmtServiceContractClient objContractMgmtService = new ContractMgmtServiceContractClient();
            DataSet dsGeneralExclusions = new DataSet();
            try
            {
                dsGeneralExclusions = objContractMgmtService.GetSavedConfiguration(intcompanyid, strtbl, blnisinsurance, intpatienttype, intUserId, intWorkStationId, intError);
            }

            finally
            {
                objContractMgmtService.Close();

            }

            return dsGeneralExclusions;
        }

        public DataSet GetMasterConfigurationForVAT(int PatientType, int HospitalId, int TaxType)
        {
            FrontOfficeServiceContractClient objfrontoffice = new FrontOfficeServiceContractClient();
            try
            {
                DataSet dsMasterVATConfig = objfrontoffice.GetMasterVATConfig(PatientType, HospitalId, TaxType);
                objfrontoffice.Close();
                return dsMasterVATConfig;
            }
            finally
            {
                objfrontoffice.Close();
            }
        }

        private DataTable DTPatientDetails()
        {
            DataTable DTPatientData = new DataTable("PatientDetails");
            try
            {
                DTPatientData.Columns.Add("PatientId", typeof(int));
                DTPatientData.Columns.Add("InterDocId", typeof(int));
                DTPatientData.Columns.Add("ExterDocId", typeof(int));
                DTPatientData.Columns.Add("BillType", typeof(int));
                DTPatientData.Columns.Add("CompanyID", typeof(int));
                DTPatientData.Columns.Add("TariffID", typeof(int));
                DTPatientData.Columns.Add("GradeID", typeof(int));
                DTPatientData.Columns.Add("GradeName", typeof(string));
                DTPatientData.Columns.Add("BedTypeID", typeof(int));
                DTPatientData.Columns.Add("BedTypeName", typeof(string));
                DTPatientData.Columns.Add("LetterNo", typeof(string));
                DTPatientData.Columns.Add("cmbOPackageSelected", typeof(int));
                DTPatientData.Columns.Add("PatientType", typeof(int));
                DTPatientData.Columns.Add("CollectableType", typeof(int));
                DTPatientData.Columns.Add("IsCardCollectable", typeof(bool));
                DTPatientData.Columns.Add("MaxCollectable", typeof(int));
                DTPatientData.Columns.Add("Priority", typeof(int));
                DTPatientData.Columns.Add("SessionId", typeof(string));
                DTPatientData.Columns.Add("UHID", typeof(string));
                DTPatientData.Columns.Add("PackageId", typeof(int));
                DTPatientData.Columns.Add("BillID", typeof(int));
                DTPatientData.Columns.Add("AuthenticationUserId", typeof(int));
                DTPatientData.Columns.Add("RemarksOrReasons", typeof(string));
                DTPatientData.Columns.Add("IsDefaultLOA", typeof(bool));
                DTPatientData.Columns.Add("RefDocSpecId", typeof(string));
                DTPatientData.Columns.Add("HospID", typeof(int));
                return DTPatientData;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in DTPatientDetails", "");
                return null;
            }
        }

        private DataTable DTOPPackage()
        {
            DataTable dtOPPackage = new DataTable("OPPackage");
            try
            {
                dtOPPackage.Columns.Add("Level", typeof(string));
                dtOPPackage.Columns.Add("ServiceItemID", typeof(int));
                dtOPPackage.Columns.Add("PackageId", typeof(int));
                dtOPPackage.Columns.Add("ServiceID", typeof(int));
                dtOPPackage.Columns.Add("Name", typeof(string));
                dtOPPackage.Columns.Add("HospDeptId", typeof(int));
                dtOPPackage.Columns.Add("HospId", typeof(int));
                dtOPPackage.Columns.Add("SpecialiseId", typeof(int));
                dtOPPackage.Columns.Add("LimitType", typeof(string));
                dtOPPackage.Columns.Add("LTName", typeof(string));
                dtOPPackage.Columns.Add("Limit", typeof(decimal));
                dtOPPackage.Columns.Add("ISR", typeof(string));
                dtOPPackage.Columns.Add("SEQ", typeof(int));
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in DTOPPackage", "");
                return null;
            }
            return dtOPPackage;
        }

        private void GetMaxCollectable(int PatentLOAID)
        {
            try
            {
                int intUserid = 0, intworkstationid = 0, intError = 0;
                intUserid = Convert.ToInt32(strDefaultUserId); intworkstationid = Convert.ToInt32(strDefWorkstationId);
                if (MaxCollectable > 0)
                {

                    decimal TempMax = 0;

                    TempMax = FetchMaxCollectable(DateTime.Now, DateTime.Now, 0, 0, 0, 0, PatentLOAID, "null", intUserid, intworkstationid, 0, 0, 0, "");
                    MaxCollectable = (MaxCollectable - Convert.ToInt32(TempMax));
                }
                else if (MaxCollectable < 0)
                {
                    decimal TempMax = 0;
                    //WebRefFrontOffice.WSFrontOffice objMax = new WebRefFrontOffice.WSFrontOffice();
                    TempMax = FetchMaxCollectable(DateTime.Now, DateTime.Now, 0, 0, 0, 0, PatentLOAID, "null", intUserid, intworkstationid, 0, 0, 0, "");
                    if (TempMax > 0)
                        MaxCollectable = (0 - Convert.ToInt32(TempMax));
                    else
                        MaxCollectable = (MaxCollectable - Convert.ToInt32(TempMax));

                    //hdnMaxCollectable.Value = (MaxCollectable).ToString();
                }
                if (MaxCollectable < 0 & MaxCollectable != -1) MaxCollectable = 0;
                if (intBillMaxCollectable == -1 & MaxCollectable <= 0) MaxCollectable = -1;
            }
            finally
            {

            }
        }

        private DataTable DtOtherOrders()
        {
            DataTable DtOtherOrders = new DataTable("OtherOrderBillDetail");
            try
            {
                DtOtherOrders.Columns.Add("ServiceName", typeof(String));
                DtOtherOrders.Columns.Add("ServiceId", typeof(int));
                DtOtherOrders.Columns.Add("Procedure", typeof(String));
                DtOtherOrders.Columns.Add("ProcedureId", typeof(int));
                DtOtherOrders.Columns.Add("Sample", typeof(String));
                DtOtherOrders.Columns.Add("SampleId", typeof(int));
                DtOtherOrders.Columns.Add("DeptId", typeof(int));//
                DtOtherOrders.Columns.Add("DeptName", typeof(String));//
                DtOtherOrders.Columns.Add("SpecialiseId", typeof(int));
                DtOtherOrders.Columns.Add("SpecialiseName", typeof(String));//
                DtOtherOrders.Columns.Add("Qty", typeof(int));
                DtOtherOrders.Columns.Add("PPAY", typeof(decimal));
                DtOtherOrders.Columns.Add("CPAY", typeof(decimal));
                DtOtherOrders.Columns.Add("ScheduleId", typeof(int));
                DtOtherOrders.Columns.Add("OrderId", typeof(int));

                DtOtherOrders.Columns.Add("DPAY", typeof(decimal));
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in DtOtherOrders", "");
                return null;
            }
            return DtOtherOrders;
        }

        public DataTable GetDiscountDetails()
        {
            dtDiscountDetails.Columns.Add("mLevel", typeof(int));
            dtDiscountDetails.Columns.Add("Type", typeof(string));
            dtDiscountDetails.Columns.Add("ServiceId", typeof(int));
            dtDiscountDetails.Columns.Add("ServiceName", typeof(string));
            dtDiscountDetails.Columns.Add("HospDeptID", typeof(int));
            dtDiscountDetails.Columns.Add("DepartmentName", typeof(string));
            dtDiscountDetails.Columns.Add("SpecialiseID", typeof(int));
            dtDiscountDetails.Columns.Add("SpecialiseName", typeof(string));
            dtDiscountDetails.Columns.Add("ServiceItemID", typeof(int));
            dtDiscountDetails.Columns.Add("ItemName", typeof(string));
            dtDiscountDetails.Columns.Add("Total", typeof(decimal));
            dtDiscountDetails.Columns.Add("Discount", typeof(decimal));

            dtDiscountDetails.Columns.Add("CPAY", typeof(decimal));
            dtDiscountDetails.Columns.Add("PPAY", typeof(decimal));
            dtDiscountDetails.Columns.Add("DCOM", typeof(decimal));
            dtDiscountDetails.Columns.Add("DPAT", typeof(decimal));
            dtDiscountDetails.Columns.Add("DPER", typeof(decimal));
            dtDiscountDetails.Columns.Add("QTY", typeof(decimal));
            dtDiscountDetails.Columns.Add("BQTY", typeof(decimal));
            dtDiscountDetails.Columns.Add("SEQ", typeof(int));
            dtDiscountDetails.Columns.Add("Level", typeof(int));
            return dtDiscountDetails;
        }

        private DataTable SplitCollectables(DataTable dtBillContribution, DataTable dtBillSummary, DataTable dtDiscountDetails)//DsCreditBillDetails.Tables[1]-->BillSummary Table
        {
            try
            {
                decimal decDPAY = 0;
                decimal decCPAY = 0;
                decimal decPercentageCollectable = 1;
                bool blnLevelCollectable = false;
                decimal decDiscountonItem = 0;
                bool blnLimitExceed = false;
                decimal decTotalBillAmount = 0;
                decimal decCollAmount = 0;
                decimal decDiscountSum = 0;
                decimal decCollectedAmount = 0;
                bool blnCollectableInPercentage = true;
                bool blnHasMaxCollectable = true;
                int intMaxCollDeclaredAmount = 0;
                bool blnServicelevelamountCollected = false;
                bool blnDepartmentlevelamountCollected = false;
                bool blnSpecializationlevelamountCollected = false;
                bool blnNeedpercentCalculation = true;
                DataRow[] drDtbillcontribution = dtBillContribution.Select("TYP=5", "SID ASC");
                decTotalBillAmount = Convert.ToDecimal(dtBillSummary.Rows[0]["Amount"]);

                if (CollectableType == 0)
                {
                    return dtBillContribution;
                }

                if (CollectableType == 1 || CollectableType == 3 && drDtbillcontribution.Length > 0)
                {
                    if (decTotalBillAmount > 0)
                    {
                        if (dtDiscountDetails.Rows.Count > 0)
                            decDiscountSum = Convert.ToDecimal(dtDiscountDetails.Compute("sum([DCOM])", "TYP=5"));

                        #region Limit exceeds billing                       

                        decTotalBillAmount = Convert.ToDecimal(dtBillSummary.Rows[0]["CPAY"]) + Convert.ToDecimal(dtBillSummary.Rows[0]["DPAY"]);
                        decCollAmount = Convert.ToDecimal(dtBillSummary.Rows[0]["DPAY"]);
                        #endregion

                        if (decTotalBillAmount == 0)
                            decTotalBillAmount = 1;

                        decPercentageCollectable = Convert.ToDecimal((decCollAmount * 100) / decTotalBillAmount);
                    }
                    dtBillContribution.AcceptChanges();

                }
                else if (CollectableType == 2 && drDtbillcontribution.Length > 0)
                {
                    if (decTotalBillAmount > 0)
                    {
                        if (dtDiscountDetails.Rows.Count > 0)
                            decDiscountSum = Convert.ToDecimal(dtDiscountDetails.Compute("sum([DCOM])", "TYP=5"));

                        #region Limit exceeds billing


                        decTotalBillAmount = Convert.ToDecimal(dtBillSummary.Rows[0]["CPAY"]) + Convert.ToDecimal(dtBillSummary.Rows[0]["DPAY"]);

                        decTotalBillAmount = (decTotalBillAmount - decDiscountSum);

                        decCollAmount = Convert.ToDecimal(dtBillSummary.Rows[0]["DPAY"]);
                        #endregion                       
                        if (decTotalBillAmount == 0)
                            decTotalBillAmount = 1;

                        decPercentageCollectable = Convert.ToDecimal((decCollAmount * 100) / decTotalBillAmount);
                    }
                }

                #region LimitTag Display Value Calculation Newly Added by shankar
                int intLimitDisplayValu = 0;
                if (MaxCollectable == -1)
                {
                    intLimitDisplayValu = 0;
                }
                else
                    intLimitDisplayValu = Convert.ToInt32(hdnMaxCollectable);
                #endregion LimitTag Display Value Calculation Newly Added


                decCollectedAmount = Convert.ToDecimal(Convert.ToInt32(intLimitDisplayValu) - MaxCollectable);
                intMaxCollDeclaredAmount = Convert.ToInt32(intLimitDisplayValu);


                if (MaxCollectable == -1)
                    blnHasMaxCollectable = false;


                // this code is general to both collectables
                for (int iCount = 0; iCount < drDtbillcontribution.Length; iCount++)
                {
                    blnLevelCollectable = false;
                    string strFilterDiscount = "";

                    strFilterDiscount = "SIID=" + Convert.ToInt32(drDtbillcontribution[iCount]["SIID"]);

                    // this is for fetching the collectable percentage at different levels
                    if (dsDeductables_ColSplit.Tables.Count > 1)
                    {
                        // Item
                        if (dsDeductables_ColSplit.Tables[4].Rows.Count != 0 & dsDeductables_ColSplit.Tables[4].Select("ServiceID=" + drDtbillcontribution[iCount]["SID"] + " and  HospDeptId = " + drDtbillcontribution[iCount]["DID"] + " and SpecialiseID=" + drDtbillcontribution[iCount]["SPID"] + " and ServiceItemID=" + drDtbillcontribution[iCount]["SIID"]).Length > 0)
                        {
                            string strF = "";
                            strF = "ServiceID=" + drDtbillcontribution[iCount]["SID"] + " and  HospDeptId = " + drDtbillcontribution[iCount]["DID"] + " and SpecialiseID=" + drDtbillcontribution[iCount]["SPID"] + " and ServiceItemID=" + drDtbillcontribution[iCount]["SIID"];
                            DataRow[] drColLevel = dsDeductables_ColSplit.Tables[4].Select(strF, "");
                            if (drColLevel.Length > 0)
                            {
                                blnLevelCollectable = true;
                                if (drColLevel[0]["LimitType"].ToString() == "2")
                                {
                                    blnCollectableInPercentage = true;
                                    if (blnHasMaxCollectable)
                                    {
                                        if ((decCollAmount + decCollectedAmount) < intMaxCollDeclaredAmount)
                                        {
                                            decPercentageCollectable = Convert.ToDecimal(drColLevel[0]["Limit"]);
                                            blnNeedpercentCalculation = true;
                                        }

                                    }

                                }
                                else if (drColLevel[0]["LimitType"].ToString() == "6")// limit given as amount
                                {
                                    blnCollectableInPercentage = false;
                                    if (blnHasMaxCollectable)
                                    {
                                        if ((decCollAmount + decCollectedAmount) < intMaxCollDeclaredAmount)
                                            decPercentageCollectable = Convert.ToDecimal(drColLevel[0]["Limit"]);
                                        else
                                            decPercentageCollectable = Convert.ToDecimal(Convert.ToDecimal(decCollAmount / drDtbillcontribution.Length));//decPercentageCollectable = Convert.ToDecimal(Convert.ToDecimal(decCollAmount / drDtbillcontribution.Length).ToString(objfrmCommon.CurrencyFormat));
                                    }


                                }
                            }

                        }
                        // Specialization
                        else if (dsDeductables_ColSplit.Tables[3].Rows.Count != 0 & dsDeductables_ColSplit.Tables[3].Select("ServiceID=" + drDtbillcontribution[iCount]["SID"] + " and  HospDeptId = " + drDtbillcontribution[iCount]["DID"] + " and SpecialiseID=" + drDtbillcontribution[iCount]["SPID"]).Length > 0)
                        {
                            string strF = "";
                            strF = "ServiceID=" + drDtbillcontribution[iCount]["SID"] + " and  HospDeptId = " + drDtbillcontribution[iCount]["DID"] + " and SpecialiseID=" + drDtbillcontribution[iCount]["SPID"];
                            DataRow[] drColLevel = dsDeductables_ColSplit.Tables[3].Select(strF, "");
                            if (drColLevel.Length > 0)
                            {
                                if (drColLevel[0]["LimitType"].ToString() == "2")
                                {
                                    blnCollectableInPercentage = true;
                                    if (blnHasMaxCollectable)
                                    {
                                        if ((decCollAmount + decCollectedAmount) < intMaxCollDeclaredAmount)
                                        {
                                            decPercentageCollectable = Convert.ToDecimal(drColLevel[0]["Limit"]);
                                            blnNeedpercentCalculation = true;
                                        }

                                    }

                                }
                                else if (drColLevel[0]["LimitType"].ToString() == "6")// limit given as amount
                                {
                                    blnCollectableInPercentage = false;
                                    if (blnHasMaxCollectable)
                                    {
                                        if (blnSpecializationlevelamountCollected == false)
                                        {
                                            if ((decCollAmount + decCollectedAmount) < intMaxCollDeclaredAmount)
                                            {
                                                decPercentageCollectable = Convert.ToDecimal(drColLevel[0]["Limit"]);
                                                blnSpecializationlevelamountCollected = true;
                                            }
                                            else
                                            {
                                                decPercentageCollectable = Convert.ToDecimal(drColLevel[0]["Limit"]);
                                                blnServicelevelamountCollected = true;
                                                if (decCollAmount <= decPercentageCollectable)
                                                {
                                                    decPercentageCollectable = decCollAmount;
                                                    decCollAmount = 0;
                                                }
                                                else
                                                {
                                                    decCollAmount = decCollAmount - decPercentageCollectable;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            decPercentageCollectable = 0;
                                        }
                                    }
                                    else // no maxcollectable but amount is given
                                    {
                                        if (blnServicelevelamountCollected == false)
                                        {
                                            blnServicelevelamountCollected = true;
                                            decPercentageCollectable = Convert.ToDecimal(drColLevel[0]["Limit"]);
                                        }
                                        else
                                        {
                                            decPercentageCollectable = 0;
                                        }
                                    }
                                }
                            }
                        }
                        // Department
                        else if (dsDeductables_ColSplit.Tables[2].Rows.Count != 0 & dsDeductables_ColSplit.Tables[2].Select("ServiceID=" + drDtbillcontribution[iCount]["SID"] + " and HospDeptId = " + drDtbillcontribution[iCount]["DID"]).Length > 0)
                        {
                            string strF = "";
                            strF = "ServiceID=" + drDtbillcontribution[iCount]["SID"] + " and HospDeptId = " + drDtbillcontribution[iCount]["DID"];
                            DataRow[] drColLevel = dsDeductables_ColSplit.Tables[2].Select(strF, "");
                            if (drColLevel.Length > 0)
                            {
                                blnLevelCollectable = true;
                                if (drColLevel[0]["LimitType"].ToString() == "2")
                                {
                                    blnCollectableInPercentage = true;
                                    if (blnHasMaxCollectable)
                                    {
                                        if ((decCollAmount + decCollectedAmount) < intMaxCollDeclaredAmount)
                                        {
                                            decPercentageCollectable = Convert.ToDecimal(drColLevel[0]["Limit"]);
                                            blnNeedpercentCalculation = true;
                                        }
                                    }
                                }
                                else if (drColLevel[0]["LimitType"].ToString() == "6")// limit given as amount
                                {
                                    blnCollectableInPercentage = false;
                                    if (blnHasMaxCollectable)
                                    {
                                        if (blnDepartmentlevelamountCollected == false)
                                        {
                                            if ((decCollAmount + decCollectedAmount) < intMaxCollDeclaredAmount)
                                            {
                                                decPercentageCollectable = Convert.ToDecimal(drColLevel[0]["Limit"]);
                                                blnDepartmentlevelamountCollected = true;
                                            }
                                            else
                                            {
                                                decPercentageCollectable = Convert.ToDecimal(drColLevel[0]["Limit"]);
                                                blnDepartmentlevelamountCollected = true;
                                                if (decCollAmount <= decPercentageCollectable)
                                                {
                                                    decPercentageCollectable = decCollAmount;
                                                    decCollAmount = 0;
                                                }
                                                else
                                                {
                                                    decCollAmount = decCollAmount - decPercentageCollectable;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            decPercentageCollectable = 0;
                                        }
                                    }
                                    else // no maxcollectable but amount is given
                                    {
                                        if (blnDepartmentlevelamountCollected == false)
                                        {
                                            blnDepartmentlevelamountCollected = true;
                                            decPercentageCollectable = Convert.ToDecimal(drColLevel[0]["Limit"]);
                                        }
                                        else
                                        {
                                            decPercentageCollectable = 0;
                                        }
                                    }
                                }
                            }
                        }
                        // Service
                        else if (dsDeductables_ColSplit.Tables[1].Rows.Count != 0 & dsDeductables_ColSplit.Tables[1].Select("ServiceID=" + drDtbillcontribution[iCount]["SID"]).Length > 0)
                        {
                            string strF = "";
                            strF = "ServiceID=" + drDtbillcontribution[iCount]["SID"];
                            DataRow[] drColLevel = dsDeductables_ColSplit.Tables[1].Select(strF, "");

                            if (drColLevel.Length > 0)
                            {
                                blnLevelCollectable = true;
                                if (drColLevel[0]["LimitType"].ToString() == "2")
                                {
                                    blnCollectableInPercentage = true;
                                    if (blnHasMaxCollectable)
                                    {
                                        if ((decCollAmount + decCollectedAmount) < intMaxCollDeclaredAmount)
                                        {
                                            decPercentageCollectable = Convert.ToDecimal(drColLevel[0]["Limit"]);
                                            blnNeedpercentCalculation = true;
                                        }

                                    }

                                }
                                else if (drColLevel[0]["LimitType"].ToString() == "6")// limit given as amount
                                {
                                    blnCollectableInPercentage = false;
                                    blnNeedpercentCalculation = false;
                                    if (blnHasMaxCollectable)
                                    {
                                        if (blnServicelevelamountCollected == false)
                                        {
                                            if ((decCollAmount + decCollectedAmount) < intMaxCollDeclaredAmount)
                                            {
                                                decPercentageCollectable = Convert.ToDecimal(drColLevel[0]["Limit"]);
                                                blnServicelevelamountCollected = true;
                                            }
                                            else
                                            {
                                                decPercentageCollectable = Convert.ToDecimal(drColLevel[0]["Limit"]);
                                                blnServicelevelamountCollected = true;
                                                if (decCollAmount <= decPercentageCollectable)
                                                {
                                                    decPercentageCollectable = decCollAmount;
                                                    decCollAmount = 0;
                                                }
                                                else
                                                {
                                                    decCollAmount = decCollAmount - decPercentageCollectable;

                                                }
                                            }
                                        }
                                        else
                                        {
                                            decPercentageCollectable = 0;
                                        }
                                    }
                                    else // no maxcollectable but amount is given
                                    {
                                        if (blnServicelevelamountCollected == false)
                                        {
                                            blnServicelevelamountCollected = true;
                                            decPercentageCollectable = Convert.ToDecimal(drColLevel[0]["Limit"]);
                                        }
                                        else
                                        {
                                            decPercentageCollectable = 0;
                                        }

                                    }

                                }
                            }
                        }
                        // bill level or Grade level
                        else if (dsDeductables_ColSplit.Tables[0].Rows.Count != 0)
                        {
                            // limit given as percentage
                            if (dsDeductables_ColSplit.Tables[0].Rows[0]["LimitType"].ToString() == "2")
                            {
                                blnLevelCollectable = true;
                                blnCollectableInPercentage = true;
                                if (blnHasMaxCollectable)
                                {
                                    if ((decCollAmount + decCollectedAmount) < intMaxCollDeclaredAmount)
                                    {
                                        decPercentageCollectable = Convert.ToDecimal(dsDeductables_ColSplit.Tables[0].Rows[0]["Limit"]);
                                        blnNeedpercentCalculation = true;
                                    }

                                }

                            }
                            else if (dsDeductables_ColSplit.Tables[0].Rows[0]["LimitType"].ToString() == "6")// limit given as amount
                            {
                                blnCollectableInPercentage = false;
                                blnNeedpercentCalculation = false;
                                blnLevelCollectable = true;
                                if (blnHasMaxCollectable)
                                {
                                    if (blnServicelevelamountCollected == false)
                                    {
                                        if ((decCollAmount + decCollectedAmount) < intMaxCollDeclaredAmount)
                                        {
                                            decPercentageCollectable = Convert.ToDecimal(dsDeductables_ColSplit.Tables[0].Rows[0]["Limit"]);
                                            blnServicelevelamountCollected = true;
                                        }
                                        else
                                        {
                                            decPercentageCollectable = Convert.ToDecimal(dsDeductables_ColSplit.Tables[0].Rows[0]["Limit"]);
                                            blnServicelevelamountCollected = true;
                                            if (decCollAmount <= decPercentageCollectable)
                                            {
                                                decPercentageCollectable = decCollAmount;
                                                decCollAmount = 0;
                                            }
                                            else
                                            {
                                                decCollAmount = decCollAmount - decPercentageCollectable;

                                            }
                                        }
                                    }
                                    else
                                    {
                                        decPercentageCollectable = 0;
                                    }
                                }
                                else // no maxcollectable but amount is given
                                {
                                    if (blnServicelevelamountCollected == false)
                                    {
                                        blnServicelevelamountCollected = true;
                                        decPercentageCollectable = Convert.ToDecimal(dsDeductables_ColSplit.Tables[0].Rows[0]["Limit"]);
                                    }
                                    else
                                    {
                                        decPercentageCollectable = 0;
                                    }

                                }
                            }
                        }
                    }

                    //END this is for fetching the collectable percentage at different levels
                    //to make collectable zero for EMR Auto raised items 
                    if (dtBillContribution.Select("SID=2").Length > 0 && dtBillContribution.Select("SID=5").Length > 0 && drDtbillcontribution[iCount]["SID"].ToString() == "5" && hdnPatientType.Trim() == "1")
                    {
                        decPercentageCollectable = 0;
                    }

                    if (blnLevelCollectable)
                    {
                        decDiscountonItem = 0;
                        decCPAY = 0;
                        decDPAY = 0;

                        if (dtDiscountDetails.Rows.Count <= 0)
                            strFilterDiscount = "";

                        DataRow[] DrdtDiscountDetails = dtDiscountDetails.Select(strFilterDiscount, "");
                        if (DrdtDiscountDetails.Length > 0)
                        {
                            decDiscountonItem = Convert.ToDecimal(dtDiscountDetails.Compute("Sum([DCOM])", strFilterDiscount));

                        }
                        decCPAY = Convert.ToDecimal(drDtbillcontribution[iCount]["CPAY"]) * Convert.ToDecimal(drDtbillcontribution[iCount]["Quantity"]);
                        if (CollectableType == 1 || CollectableType == 3) // amount without any discount
                            decCPAY = decCPAY;
                        else if (CollectableType == 2)
                            decCPAY = decCPAY - decDiscountonItem;
                        // this is for sending the unit price with out multiplying with qty
                        // this code for assigning the percentage collected or amount given in the collectable configuration.
                        decDPAY = blnNeedpercentCalculation ? Convert.ToDecimal(Convert.ToDecimal((decCPAY * decPercentageCollectable) / 100)) : decPercentageCollectable;
                        string strYiaco = ConfigurationSettings.AppSettings["Yaico"].ToString();
                        if (strYiaco.ToUpper() == "YES")
                        {
                            decDPAY = Convert.ToDecimal(RoundCorrect(Convert.ToDouble(decDPAY), 2));
                        }
                        if (decCPAY == 0)
                            decDPAY = 0;

                        if (Convert.ToDecimal(drDtbillcontribution[iCount]["PPAY"].ToString()) == 0)
                            drDtbillcontribution[iCount]["PPAY"] = decDPAY;
                        else
                        {
                            blnLimitExceed = true;
                            drDtbillcontribution[iCount]["PPAY"] = Convert.ToDecimal(drDtbillcontribution[iCount]["PPAY"].ToString()) + decDPAY;
                        }

                        drDtbillcontribution[iCount]["DPAY"] = decDPAY;

                        decimal decCpayUnitamt = Convert.ToDecimal(drDtbillcontribution[iCount]["CPAY"]) * Convert.ToDecimal(drDtbillcontribution[iCount]["Quantity"]);
                        drDtbillcontribution[iCount]["CPAY"] = decCpayUnitamt - decDPAY;
                    }
                    else
                    {
                        drDtbillcontribution[iCount]["CPAY"] = Convert.ToDecimal(drDtbillcontribution[iCount]["CPAY"]) * Convert.ToDecimal(drDtbillcontribution[iCount]["Quantity"]);
                    }

                }

                dtBillContribution.AcceptChanges();


                if (blnLimitExceed)
                    return dtBillContribution;


                // this is to check whether the collectable amount is exceeding the actual amount
                decimal decsumofcol = 0, decsumofcolppay = 0;
                decsumofcol = Convert.ToDecimal(dtBillContribution.Compute("sum([PPAY])", "TYP = 5"));
                decsumofcolppay = Convert.ToDecimal(dtBillContribution.Compute("sum([PPAY])", "TYP = 5"));
                if (decsumofcol > decCollAmount)
                {
                    DataRow[] drUpdateColldiffamt = dtBillContribution.Select("TYP=5");
                    if (decCollAmount == 0)
                        decCollAmount = decPercentageCollectable;
                    if (drUpdateColldiffamt.Length > 0)
                    {
                        string strYiaco = ConfigurationSettings.AppSettings["Yaico"].ToString();
                        if (strYiaco.ToUpper() == "YES")
                        {
                            decCollAmount = Math.Round(decCollAmount, 2);
                        }

                        decimal decsumofppay = Convert.ToDecimal(drUpdateColldiffamt[0]["PPAY"]);
                        decimal decsumofdpay = Convert.ToDecimal(drUpdateColldiffamt[0]["DPAY"]);
                        drUpdateColldiffamt[0]["PPAY"] = (decsumofppay - (decsumofcolppay - decCollAmount));
                        drUpdateColldiffamt[0]["DPAY"] = (decsumofdpay - (decsumofcol - decCollAmount));
                    }
                    dtBillContribution.AcceptChanges();

                }
                else if (decsumofcol < decCollAmount)
                {
                    DataRow[] drUpdateColldiffamt = dtBillContribution.Select("TYP=5");
                    if (drUpdateColldiffamt.Length > 0)
                    {
                        decimal decsumofppay = Convert.ToDecimal(drUpdateColldiffamt[0]["PPAY"]);
                        decimal decsumofdpay = Convert.ToDecimal(drUpdateColldiffamt[0]["DPAY"]);
                        drUpdateColldiffamt[0]["PPAY"] = (decsumofppay + (decsumofcolppay - decsumofcol));
                        drUpdateColldiffamt[0]["DPAY"] = (decsumofdpay + (decCollAmount - decsumofcol));
                    }
                    dtBillContribution.AcceptChanges();
                }

                // this is to check whether the collectable amount is exceeding the actual amount
                // this is to update service ppay
                if (dtBillContribution.Rows.Count > 0)
                {

                }
                // this is to update service ppay
                return dtBillContribution;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in SplitCollectables", "");
                return null;
            }


        }

        private DataTable TempFinal(DataTable dsConfiguration, DataTable dtBillItems, DataTable dtFinaltable, int discountType, string strBillType, int IPOP)
        {
            try
            {

                if (IPOP == 1)
                {
                    return dtFinaltable;
                }

                bool flag = false;
                if (dtBillItems.Columns.Contains("BatchID"))
                    flag = true;

                if (!dtFinaltable.Columns.Contains("OrderID"))
                {
                    dtFinaltable.Columns.Add("OrderID", typeof(int));
                }
                if (!dtFinaltable.Columns.Contains("BedSeq"))
                {
                    dtFinaltable.Columns.Add("BedSeq", typeof(int));
                }

                if (!dtFinaltable.Columns.Contains("OrderItemID"))
                {
                    dtFinaltable.Columns.Add("OrderItemID", typeof(int));
                }


                if (dtBillItems != null)
                {
                    if (dtBillItems.Columns.Contains("BatchID"))
                    {
                        if (!dtFinaltable.Columns.Contains("BatchID"))
                            dtFinaltable.Columns.Add("BatchID", typeof(int));
                    }
                }


                DataTable dtFinalTableTemp = dtFinaltable.Copy();
                # region predefined Discount
                decimal decCPAY = 0; decimal decPPAY = 0; double decDCOM = 0; double decDPAT = 0;

                {
                    DataRow[] drdtTempFinal = dtFinaltable.Select("serviceid >0 and mlevel <>5", "");
                    DataRow drNewrow = null;

                    #region Service to Item level for Predefined Discount

                    if (drdtTempFinal.Length > 0)
                    {

                        for (int iCount = 0; iCount < drdtTempFinal.Length; iCount++)
                        {
                            string strFilter = "";
                            // this filter for fetching the items based on levels
                            if (drdtTempFinal[iCount]["mlevel"].ToString() == "2")
                            {
                                strFilter = "serviceid =" + drdtTempFinal[iCount]["serviceid"] + " and mlevel =5 and ISDISCAPP is null ";
                            }
                            else if (drdtTempFinal[iCount]["mlevel"].ToString() == "3")
                            {
                                strFilter = "serviceid =" + drdtTempFinal[iCount]["serviceid"] + " and HospDeptId =" + drdtTempFinal[iCount]["HospDeptId"] + " and mlevel =5 and ISDISCAPP is null ";
                            }
                            else if (drdtTempFinal[iCount]["mlevel"].ToString() == "4")
                            {
                                strFilter = "serviceid =" + drdtTempFinal[iCount]["serviceid"] + " and HospDeptId =" + drdtTempFinal[iCount]["HospDeptId"] + " and  SpecialiseID =" + drdtTempFinal[iCount]["SpecialiseID"] + " and mlevel =5 and ISDISCAPP is null ";
                            }

                            //this filter for fetching the items based on levels
                            DataRow[] drdtBillItems = dtBillItems.Select(strFilter, "");

                            if (drdtBillItems.Length > 0)
                            {
                                decimal decDiscount = 0;
                                string strDisName = "";
                                decimal decTotal = 0;
                                bool blnIspercentage = false;
                                decimal decIScompany = 0;
                                decimal decISpatient = 0;
                                strFilter = "";

                                if (drdtTempFinal[iCount]["mlevel"].ToString() == "2")
                                    strFilter = "serviceid =" + drdtBillItems[0]["serviceid"] + "  ";
                                else if (drdtTempFinal[iCount]["mlevel"].ToString() == "3")
                                    strFilter = "serviceid =" + drdtBillItems[0]["serviceid"] + " and HospDeptId =" + drdtBillItems[0]["HospDeptId"] + "  ";
                                else if (drdtTempFinal[iCount]["mlevel"].ToString() == "4")
                                    strFilter = "serviceid =" + drdtBillItems[0]["serviceid"] + " and HospDeptId =" + drdtBillItems[0]["HospDeptId"] + " and  SpecialiseID =" + drdtBillItems[0]["SpecialiseID"] + "   ";

                                DataRow[] drDsconfiguration = dsConfiguration.Select(strFilter, "");
                                decDiscount = Convert.ToDecimal(drDsconfiguration[0]["Discount"]);
                                strDisName = Convert.ToString(drDsconfiguration[0]["Type"]);
                                decTotal = Convert.ToDecimal(drDsconfiguration[0]["Total"]);
                                blnIspercentage = Convert.ToBoolean(drDsconfiguration[0]["IsPercentage"]);

                                if (drDsconfiguration[0]["DCOM"].ToString() != "")
                                    decIScompany = Convert.ToDecimal(drDsconfiguration[0]["DCOM"]);
                                if (drDsconfiguration[0]["DPAT"].ToString() != "")
                                    decISpatient = Convert.ToDecimal(drDsconfiguration[0]["DPAT"]);

                                if (drdtBillItems.Length > 0)
                                {
                                    for (int jCount = 0; jCount < drdtBillItems.Length; jCount++)
                                    {
                                        if (drdtTempFinal[iCount]["mlevel"].ToString() == "2")
                                        {
                                            if (dtFinalTableTemp.Select("mlevel=4 and serviceid =" + drdtBillItems[jCount]["serviceid"] + " and HospDeptId =" + drdtBillItems[jCount]["HospDeptId"] + " and  SpecialiseID =" + drdtBillItems[jCount]["SpecialiseID"]).Length > 0)
                                            { continue; }
                                            else if (dtFinalTableTemp.Select("mlevel=3 and serviceid =" + drdtBillItems[jCount]["serviceid"] + " and HospDeptId =" + drdtBillItems[jCount]["HospDeptId"]).Length > 0)
                                            { continue; }
                                        }
                                        else if (drdtTempFinal[iCount]["mlevel"].ToString() == "3")
                                        {
                                            if (dtFinalTableTemp.Select("mlevel=4 and serviceid =" + drdtBillItems[jCount]["serviceid"] + " and HospDeptId =" + drdtBillItems[jCount]["HospDeptId"] + " and  SpecialiseID =" + drdtBillItems[jCount]["SpecialiseID"]).Length > 0)
                                            { continue; }
                                        }
                                        decimal intItemQty = 0;
                                        double factor = 1;
                                        intItemQty = Math.Abs(Convert.ToDecimal(drdtBillItems[jCount]["BQTY"]));
                                        if (intItemQty != 0)
                                            factor = (Convert.ToDouble(drdtBillItems[jCount]["BQTY"])) / Math.Abs(Convert.ToDouble(drdtBillItems[jCount]["BQTY"]));
                                        if (intItemQty == 0)
                                        {
                                            intItemQty = Math.Abs(Convert.ToDecimal(drdtBillItems[jCount]["MQTY"]));
                                            if (intItemQty != 0)
                                                factor = (Convert.ToDouble(drdtBillItems[jCount]["MQTY"])) / Math.Abs(Convert.ToDouble(drdtBillItems[jCount]["MQTY"]));
                                        }


                                        for (decimal iCC = 0.5M; iCC <= intItemQty; iCC++)
                                        {
                                            decimal Amount = 0;

                                            decimal decPrevDiscount = 0;
                                            decimal decPrevTotalDiscount = 0;
                                            decimal decTotalbillAmount = 0;
                                            decimal diff = intItemQty - iCC;
                                            if (blnIspercentage == false)
                                            {
                                                string strFilt = "";

                                                if (dtBillItems.Columns.Contains("IsContribution"))
                                                {
                                                    strFilt = "SEQ = " + drdtBillItems[jCount]["SEQ"] + " and mLevel= " + drdtBillItems[jCount]["mLevel"] + " and IsContribution=0" + " and ServiceID=" + drdtBillItems[jCount]["ServiceID"];
                                                    DataRow[] drPrevDiscount = dtBillItems.Select(strFilt, "");
                                                    if (drPrevDiscount.Length > 0)
                                                    {
                                                        foreach (DataRow drr in drPrevDiscount)
                                                            decPrevDiscount += Convert.ToDecimal(drr["Discount"]);
                                                    }
                                                    decPrevTotalDiscount = Convert.ToDecimal(dtBillItems.Compute("Sum([Discount])", "mLevel=5"));
                                                }

                                                string strFilterLevel = "";
                                                if (Convert.ToDecimal(drdtTempFinal[iCount]["mlevel"]) == 2)
                                                    strFilterLevel = "mLevel=5 and Serviceid=" + drdtBillItems[jCount]["ServiceID"];
                                                else if (Convert.ToDecimal(drdtTempFinal[iCount]["mlevel"]) == 3)
                                                    strFilterLevel = "mLevel=5 and Serviceid=" + drdtBillItems[jCount]["ServiceID"] + " and HospDeptId=" + drdtBillItems[jCount]["HospDeptId"];
                                                else if (Convert.ToDecimal(drdtTempFinal[iCount]["mlevel"]) == 4)
                                                    strFilterLevel = "mLevel=5 and Serviceid=" + drdtBillItems[jCount]["ServiceID"] + " and SpecialiseId=" + drdtBillItems[jCount]["SpecialiseId"] + " and  HospDeptId= " + drdtBillItems[jCount]["HospDeptId"];


                                                if (dtBillItems.Columns.Contains("Discount"))
                                                    decPrevTotalDiscount = Convert.ToDecimal(dtBillItems.Compute("Sum([Discount])", strFilterLevel));

                                                foreach (DataRow drt in drdtBillItems)
                                                {
                                                    if (strBillType == "CR")
                                                        decTotalbillAmount += Convert.ToDecimal(drt["CPAY"]);
                                                    else
                                                    {
                                                        if (Convert.ToDecimal(drt["BQTY"]) != 0)
                                                            decTotalbillAmount += Convert.ToDecimal(drt["PPAY"]) * Convert.ToDecimal(drt["BQTY"]);
                                                        else
                                                            decTotalbillAmount += Convert.ToDecimal(drt["PPAY"]) * Convert.ToDecimal(drt["MQTY"]);

                                                    }
                                                }

                                            }

                                            if (strBillType == "CR")
                                            {
                                                if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                                {
                                                    if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                                    {
                                                        if (diff == 0)
                                                        {
                                                            decCPAY = (Convert.ToDecimal(drdtBillItems[jCount]["cpay"]) * 0.5M) / Convert.ToDecimal(drdtBillItems[jCount]["BQTY"]);
                                                            if (Convert.ToInt32(drdtBillItems[jCount]["ServiceID"]) == 10 || Convert.ToInt32(drdtBillItems[jCount]["ServiceID"]) == 17)
                                                                decPPAY = (Convert.ToDecimal(drdtBillItems[jCount]["ppay"]) * 0.5M);
                                                            else
                                                            {
                                                                decPPAY = (Convert.ToDecimal(drdtBillItems[jCount]["ppay"]) * 0.5M) / Convert.ToDecimal(drdtBillItems[jCount]["BQTY"]);
                                                            }
                                                            Amount = (decCPAY + decPPAY);
                                                        }
                                                        else
                                                        {
                                                            decCPAY = Convert.ToDecimal(drdtBillItems[jCount]["cpay"]) / Convert.ToDecimal(drdtBillItems[jCount]["BQTY"]);
                                                            if (Convert.ToInt32(drdtBillItems[jCount]["ServiceID"]) == 10 || Convert.ToInt32(drdtBillItems[jCount]["ServiceID"]) == 17)
                                                            {
                                                                decPPAY = Convert.ToDecimal(drdtBillItems[jCount]["ppay"]);
                                                            }
                                                            else
                                                            {
                                                                decPPAY = Convert.ToDecimal(drdtBillItems[jCount]["ppay"]) / Convert.ToDecimal(drdtBillItems[jCount]["BQTY"]);
                                                            }
                                                            Amount = (decCPAY + decPPAY);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (diff == 0)
                                                        {
                                                            Amount = Convert.ToDecimal(drdtBillItems[jCount]["cpay"]) / Convert.ToDecimal(drdtBillItems[jCount]["BQTY"]) * 0.5M;
                                                        }
                                                        else
                                                        {
                                                            Amount = Convert.ToDecimal(drdtBillItems[jCount]["cpay"]) / Convert.ToDecimal(drdtBillItems[jCount]["BQTY"]);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (diff == 0)
                                                    {
                                                        Amount = Convert.ToDecimal(drdtBillItems[jCount]["cpay"]) / Math.Abs(Convert.ToDecimal(drdtBillItems[jCount]["BQTY"])) * 0.5M;
                                                    }
                                                    else
                                                    {
                                                        Amount = Convert.ToDecimal(drdtBillItems[jCount]["cpay"]) / Math.Abs(Convert.ToDecimal(drdtBillItems[jCount]["BQTY"]));
                                                    }
                                                }
                                            }
                                            else
                                                Amount = Convert.ToDecimal(drdtBillItems[jCount]["ppay"]);


                                            if (Amount != 0)
                                            {
                                                drNewrow = dtFinaltable.NewRow();
                                                drNewrow["mLevel"] = drdtBillItems[jCount]["mlevel"];
                                                drNewrow["Type"] = strDisName;
                                                drNewrow["serviceid"] = drdtBillItems[jCount]["serviceid"];
                                                drNewrow["ServiceName"] = drdtBillItems[jCount]["ServiceName"];
                                                drNewrow["HospDeptID"] = drdtBillItems[jCount]["HospDeptID"];
                                                drNewrow["DepartmentName"] = drdtBillItems[jCount]["DepartmentName"];
                                                drNewrow["SpecialiseID"] = drdtBillItems[jCount]["SpecialiseID"];
                                                drNewrow["Specialisation"] = drdtBillItems[jCount]["Specialisation"];
                                                drNewrow["ServiceItemID"] = drdtBillItems[jCount]["ServiceItemID"];
                                                drNewrow["ServiceItemName"] = drdtBillItems[jCount]["ServiceItemName"];
                                                drNewrow["Total"] = Amount;
                                                drNewrow["OrderID"] = drdtBillItems[jCount]["OrderID"];
                                                if (dtBillItems.Columns.Contains("BedSeq"))
                                                {
                                                    if (!string.IsNullOrEmpty(drdtBillItems[jCount]["BedSeq"].ToString()))
                                                        drNewrow["BedSeq"] = drdtBillItems[jCount]["BedSeq"];
                                                }
                                                if (dtBillItems.Columns.Contains("OrderItemID"))
                                                {
                                                    if (!string.IsNullOrEmpty(drdtBillItems[jCount]["OrderItemID"].ToString()))
                                                        drNewrow["OrderItemID"] = drdtBillItems[jCount]["OrderItemID"];
                                                }

                                                if (dtBillItems.Columns.Contains("BatchID"))
                                                {
                                                    if (!string.IsNullOrEmpty(drdtBillItems[jCount]["BatchID"].ToString()))
                                                        drNewrow["BatchID"] = drdtBillItems[jCount]["BatchID"];
                                                }

                                                double DiscountAmount = 0;
                                                if (blnIspercentage == true)
                                                {
                                                    if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                                    {
                                                        if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                                        {
                                                            decDCOM = Convert.ToDouble((decCPAY * decDiscount) / 100);
                                                            decDPAT = Convert.ToDouble((decPPAY * decDiscount) / 100);
                                                            DiscountAmount = decDCOM + decDPAT;
                                                            drNewrow["Discount"] = DiscountAmount * factor;
                                                        }
                                                        else
                                                        {
                                                            DiscountAmount = Convert.ToDouble((Amount * decDiscount) / 100);
                                                            drNewrow["Discount"] = DiscountAmount * factor;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        DiscountAmount = Convert.ToDouble((Amount * decDiscount) / 100);
                                                        drNewrow["Discount"] = DiscountAmount * factor;
                                                    }
                                                }
                                                else
                                                {
                                                    DiscountAmount = Convert.ToDouble((Convert.ToDecimal(Amount - decPrevDiscount) * decDiscount) / (decTotalbillAmount - decPrevTotalDiscount));
                                                    drNewrow["Discount"] = DiscountAmount * factor;
                                                }

                                                drNewrow["CPAY"] = 0;
                                                drNewrow["PPAY"] = 0;


                                                if (strBillType == "CR")
                                                {
                                                    if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                                    {
                                                        if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                                        {
                                                            drNewrow["DCOM"] = decDCOM * factor;
                                                            drNewrow["DPAT"] = decDPAT * factor;
                                                        }
                                                        else
                                                        {
                                                            drNewrow["DCOM"] = DiscountAmount * factor;
                                                            drNewrow["DPAT"] = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        drNewrow["DCOM"] = DiscountAmount * factor;
                                                        drNewrow["DPAT"] = 0;
                                                    }
                                                }

                                                else
                                                {
                                                    drNewrow["DPAT"] = DiscountAmount * factor;
                                                    drNewrow["DCOM"] = 0;
                                                }

                                                drNewrow["DPER"] = 0;
                                                if (diff == 0) drNewrow["QTY"] = Convert.ToDouble(0.5) * factor; else drNewrow["QTY"] = 1 * factor; ;
                                                if (diff == 0) drNewrow["BQTY"] = Convert.ToDouble(0.5) * factor; else drNewrow["BQTY"] = 1 * factor; ;
                                                drNewrow["SEQ"] = 122;

                                                dtFinaltable.Rows.Add(drNewrow);
                                                dtFinaltable.AcceptChanges();

                                            }
                                        }

                                    }

                                }

                            }

                        }

                    }

                    #endregion

                    #region Bill level for Predefined Discount
                    else
                    {
                        int DiscountLevel = 5;

                        drdtTempFinal = dtFinaltable.Select("mlevel = 0 ", "");

                        if (drdtTempFinal.Length > 0)
                        {
                            for (int iCount = 0; iCount < drdtTempFinal.Length; iCount++)
                            {
                                string strFilter = "";
                                // this filter for fetching the items based on levels
                                strFilter = " mlevel =5";
                                DataRow[] drdtBillItems = dtBillItems.Select(strFilter, "");

                                if (drdtBillItems.Length > 0)
                                {
                                    decimal decDiscount = 0;
                                    string strDisName = "";
                                    decimal decTotal = 0;
                                    bool blnIspercentage = false;
                                    decimal decIScompany = 0;
                                    decimal decISpatient = 0;
                                    strFilter = "";
                                    strFilter = " mlevel <>5";
                                    DataRow[] drDsconfiguration = dsConfiguration.Select(strFilter, "");
                                    decDiscount = Convert.ToDecimal(drDsconfiguration[0]["Discount"]);
                                    strDisName = Convert.ToString(drDsconfiguration[0]["Type"]);
                                    decTotal = Convert.ToDecimal(drDsconfiguration[0]["Total"]);
                                    blnIspercentage = Convert.ToBoolean(drDsconfiguration[0]["IsPercentage"]);
                                    if (drDsconfiguration[0]["DCOM"].ToString() != "")
                                        decIScompany = Convert.ToDecimal(drDsconfiguration[0]["DCOM"]);
                                    if (drDsconfiguration[0]["DPAT"].ToString() != "")
                                        decISpatient = Convert.ToDecimal(drDsconfiguration[0]["DPAT"]);

                                    if (drdtBillItems.Length > 0)
                                    {
                                        for (int jCount = 0; jCount < drdtBillItems.Length; jCount++)
                                        {
                                            int intItemQty = 0;
                                            intItemQty = Math.Abs(Convert.ToInt32(drdtBillItems[jCount]["BQTY"]));
                                            if (intItemQty == 0)
                                                intItemQty = Math.Abs(Convert.ToInt32(drdtBillItems[jCount]["MQTY"]));

                                            for (int iCC = 0; iCC < intItemQty; iCC++)
                                            {
                                                decimal Amount = 0;
                                                decimal decPrevDiscount = 0;
                                                decimal decPrevTotalDiscount = 0;
                                                decimal decTotalbillAmount = 0;

                                                if (blnIspercentage == false)
                                                {
                                                    string strFilt = "";
                                                    if (dtBillItems.Columns.Contains("IsContribution"))
                                                    {
                                                        strFilt = "SEQ = " + drdtBillItems[jCount]["SEQ"] + " and mLevel= " + drdtBillItems[jCount]["mLevel"] + " and IsContribution=0" + " and ServiceID=" + drdtBillItems[jCount]["ServiceID"];
                                                        DataRow[] drPrevDiscount = dtBillItems.Select(strFilt, "");
                                                        if (drPrevDiscount.Length > 0)
                                                        {
                                                            foreach (DataRow drr in drPrevDiscount)
                                                                decPrevDiscount += Convert.ToDecimal(drr["Discount"]);
                                                        }
                                                        decPrevTotalDiscount = Convert.ToDecimal(dtBillItems.Compute("Sum([Discount])", "mLevel=5"));
                                                    }
                                                    foreach (DataRow drt in drdtBillItems)
                                                    {
                                                        if (strBillType == "CR")
                                                            decTotalbillAmount += Convert.ToDecimal(drt["CPAY"]);
                                                        else
                                                            decTotalbillAmount += Convert.ToDecimal(drt["PPAY"]) * Convert.ToDecimal(drt["BQTY"]);
                                                    }
                                                }
                                                if (strBillType == "CR")
                                                {
                                                    if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                                    {
                                                        if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                                        {
                                                            decCPAY = Convert.ToDecimal(drdtBillItems[jCount]["cpay"]) / Convert.ToDecimal(drdtBillItems[jCount]["BQTY"]);
                                                            decPPAY = Convert.ToDecimal(drdtBillItems[jCount]["ppay"]); ;
                                                            Amount = decCPAY + decPPAY;
                                                        }
                                                        else
                                                        {
                                                            Amount = Convert.ToDecimal(drdtBillItems[jCount]["cpay"]) / Math.Abs(Convert.ToDecimal(drdtBillItems[jCount]["BQTY"]));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Amount = Convert.ToDecimal(drdtBillItems[jCount]["cpay"]) / Math.Abs(Convert.ToDecimal(drdtBillItems[jCount]["BQTY"]));
                                                    }
                                                }
                                                else
                                                    Amount = Convert.ToDecimal(drdtBillItems[jCount]["ppay"]);


                                                if (Amount != 0)
                                                {
                                                    drNewrow = dtFinaltable.NewRow();
                                                    drNewrow["mLevel"] = drdtBillItems[jCount]["mlevel"];
                                                    drNewrow["Type"] = strDisName;
                                                    drNewrow["serviceid"] = drdtBillItems[jCount]["serviceid"];
                                                    drNewrow["ServiceName"] = drdtBillItems[jCount]["ServiceName"];
                                                    drNewrow["HospDeptID"] = drdtBillItems[jCount]["HospDeptID"];
                                                    drNewrow["DepartmentName"] = drdtBillItems[jCount]["DepartmentName"];
                                                    drNewrow["SpecialiseID"] = drdtBillItems[jCount]["SpecialiseID"];
                                                    drNewrow["Specialisation"] = drdtBillItems[jCount]["Specialisation"];
                                                    drNewrow["ServiceItemID"] = drdtBillItems[jCount]["ServiceItemID"];
                                                    drNewrow["ServiceItemName"] = drdtBillItems[jCount]["ServiceItemName"];
                                                    drNewrow["Total"] = Amount;
                                                    drNewrow["OrderID"] = drdtBillItems[jCount]["orderID"];
                                                    if (dtBillItems.Columns.Contains("BatchID"))
                                                    {
                                                        if (!string.IsNullOrEmpty(drdtBillItems[jCount]["BatchID"].ToString()))
                                                            drNewrow["BatchID"] = drdtBillItems[jCount]["BatchID"];
                                                    }

                                                    double DiscountAmount = 0;
                                                    if (blnIspercentage == true)
                                                    {
                                                        if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                                        {
                                                            if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                                            {
                                                                decDCOM = Convert.ToDouble((decCPAY * decDiscount) / 100);
                                                                decDPAT = Convert.ToDouble((decPPAY * decDiscount) / 100);
                                                                DiscountAmount = decDCOM + decDPAT;
                                                                DiscountAmount = Math.Round(Convert.ToDouble(DiscountAmount), 2);
                                                                drNewrow["Discount"] = DiscountAmount;
                                                            }
                                                            else
                                                            {
                                                                DiscountAmount = Convert.ToDouble((Amount * decDiscount) / 100);
                                                                DiscountAmount = Math.Round(Convert.ToDouble(DiscountAmount), 2);
                                                                drNewrow["Discount"] = DiscountAmount;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            DiscountAmount = Convert.ToDouble((Amount * decDiscount) / 100);
                                                            DiscountAmount = Math.Round(Convert.ToDouble(DiscountAmount), 2);
                                                            drNewrow["Discount"] = DiscountAmount;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        DiscountAmount = Convert.ToDouble((Convert.ToDecimal(Amount - decPrevDiscount) * decDiscount) / (decTotalbillAmount - decPrevTotalDiscount));
                                                        DiscountAmount = Math.Round(Convert.ToDouble(DiscountAmount), 2);
                                                        drNewrow["Discount"] = DiscountAmount;
                                                    }

                                                    drNewrow["CPAY"] = 0;
                                                    drNewrow["PPAY"] = 0;

                                                    if (strBillType == "CR")
                                                    {
                                                        if ((object)(ConfigurationManager.AppSettings["CRDiscountforPatient"]) != null)
                                                        {
                                                            if (ConfigurationManager.AppSettings["CRDiscountforPatient"].ToString().ToUpper() == "YES")
                                                            {
                                                                drNewrow["DCOM"] = decDCOM;
                                                                drNewrow["DPAT"] = decDPAT;
                                                            }
                                                            else
                                                            {
                                                                drNewrow["DCOM"] = DiscountAmount;
                                                                drNewrow["DPAT"] = 0;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            drNewrow["DCOM"] = DiscountAmount;
                                                            drNewrow["DPAT"] = 0;
                                                        }
                                                    }
                                                    else
                                                        drNewrow["DPAT"] = DiscountAmount;

                                                    drNewrow["DPER"] = 0;
                                                    drNewrow["QTY"] = 1;
                                                    drNewrow["BQTY"] = 1;
                                                    drNewrow["SEQ"] = 122;
                                                    if (flag)
                                                        drNewrow["BatchID"] = drdtBillItems[jCount]["BatchID"];
                                                    dtFinaltable.Rows.Add(drNewrow);
                                                    dtFinaltable.AcceptChanges();

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    if (dtFinaltable.Rows.Count > 0)
                    {
                        DataRow[] drr = dtFinaltable.Select("mlevel <>5", "");
                        decimal dectotaldiscont = 0;
                        decimal decitemlevelDiscont = 0;
                        dectotaldiscont = Convert.ToDecimal(dtFinaltable.Compute("Sum([Discount])", "seq <> 122"));
                        if (dtFinaltable.Compute("Sum([Discount])", "mlevel = 5").ToString() == "")
                            decitemlevelDiscont = 0;
                        else
                            decitemlevelDiscont = Convert.ToDecimal(dtFinaltable.Compute("Sum([Discount])", "mlevel = 5"));
                        if (drr.Length > 0)
                        {
                            for (int ij = 0; ij < drr.Length; ij++)
                            {
                                drr[ij].Delete();
                            }

                        }
                        dtFinaltable.AcceptChanges();

                        if (decitemlevelDiscont < dectotaldiscont)
                        {
                            if (dtFinaltable.Rows.Count > 0)
                            {
                                if (strBillType == "CR")
                                    drNewrow["DCOM"] = Convert.ToDecimal(drNewrow["DCOM"]) + (dectotaldiscont - decitemlevelDiscont);
                                else
                                    drNewrow["DPAT"] = Convert.ToDecimal(drNewrow["DPAT"]) + (dectotaldiscont - decitemlevelDiscont);

                                drNewrow["Discount"] = Convert.ToDecimal(drNewrow["Discount"]) + (dectotaldiscont - decitemlevelDiscont);

                            }

                        }
                        else if (decitemlevelDiscont > dectotaldiscont)
                        {
                            if (dtFinaltable.Rows.Count > 0)
                            {
                                if (strBillType == "CR")
                                    drNewrow["DCOM"] = Convert.ToDecimal(drNewrow["DCOM"]) + (dectotaldiscont - decitemlevelDiscont);
                                else
                                    drNewrow["DPAT"] = Convert.ToDecimal(drNewrow["DPAT"]) + (dectotaldiscont - decitemlevelDiscont);

                                drNewrow["Discount"] = Convert.ToDecimal(drNewrow["Discount"]) + (dectotaldiscont - decitemlevelDiscont);

                            }
                        }
                        dtFinaltable.AcceptChanges();



                        for (int ij = 0; ij < dtFinaltable.Rows.Count; ij++)
                        {
                            if (dtFinaltable.Rows[ij]["OrderID"].ToString() != null && dtFinaltable.Rows[ij]["OrderID"].ToString() != "")
                            {

                                string strfilter = string.Empty;
                                if (dtBillItems.Columns.Contains("BatchID") & dtFinaltable.Columns.Contains("BatchID"))
                                {
                                    if (!string.IsNullOrEmpty(dtFinaltable.Rows[ij]["BatchID"].ToString()))
                                        strfilter = "mlevel = 5 and serviceitemid=" + dtFinaltable.Rows[ij]["serviceitemid"].ToString() + " and OrderID =" + dtFinaltable.Rows[ij]["OrderID"].ToString() + " and BatchID=" + dtFinaltable.Rows[ij]["BatchID"].ToString() + " and ServiceID=" + dtFinaltable.Rows[ij]["ServiceID"];
                                    else
                                        strfilter = "mlevel = 5 and serviceitemid=" + dtFinaltable.Rows[ij]["serviceitemid"].ToString() + " and OrderID =" + dtFinaltable.Rows[ij]["OrderID"].ToString() + " and ServiceID=" + dtFinaltable.Rows[ij]["ServiceID"];
                                }
                                else
                                    strfilter = "mlevel = 5 and serviceitemid=" + dtFinaltable.Rows[ij]["serviceitemid"].ToString() + " and OrderID =" + dtFinaltable.Rows[ij]["OrderID"].ToString() + " and ServiceID=" + dtFinaltable.Rows[ij]["ServiceID"];

                                if (dtBillItems.Columns.Contains("OrderItemID") & dtFinaltable.Columns.Contains("OrderItemID"))
                                {
                                    if (!string.IsNullOrEmpty(dtFinaltable.Rows[ij]["OrderItemID"].ToString()))
                                        strfilter = strfilter + " AND OrderItemID =" + dtFinaltable.Rows[ij]["OrderItemID"].ToString();
                                }

                                if (dtBillItems.Columns.Contains("BedSeq") & dtFinaltable.Columns.Contains("BedSeq"))
                                {
                                    if (!string.IsNullOrEmpty(dtFinaltable.Rows[ij]["BedSeq"].ToString()))
                                        strfilter = strfilter + " AND BedSeq =" + dtFinaltable.Rows[ij]["BedSeq"].ToString();
                                }
                                DataRow[] drDtbills = dtBillItems.Select(strfilter, "");
                                if (IPOP == 2)
                                {
                                    if (dtBillItems.Columns.Contains("BillItemSequence"))
                                    {
                                        dtFinaltable.Rows[ij]["SEQ"] = drDtbills[0]["BillItemSequence"];
                                    }
                                    else
                                    {
                                        dtFinaltable.Rows[ij]["SEQ"] = drDtbills[0]["Seq"];
                                    }

                                }
                                else
                                {
                                    dtFinaltable.Rows[ij]["SEQ"] = drDtbills[0]["Seq"];
                                }
                                dtFinaltable.AcceptChanges();
                            }
                            else
                            {

                                if (dtFinaltable.Rows[ij]["OrderID"] == DBNull.Value || dtFinaltable.Rows[ij]["OrderID"].ToString() == "")
                                {

                                    string strfilter;
                                    if (dtBillItems.Columns.Contains("BatchID") & dtFinaltable.Columns.Contains("BatchID"))
                                    {
                                        if (!string.IsNullOrEmpty(dtFinaltable.Rows[ij]["BatchID"].ToString()))
                                            strfilter = "mlevel = 5 and serviceitemid=" + dtFinaltable.Rows[ij]["serviceitemid"].ToString() + " and BatchID=" + dtFinaltable.Rows[ij]["BatchID"].ToString() + " and ServiceID=" + dtFinaltable.Rows[ij]["ServiceID"];
                                        else
                                            strfilter = "mlevel = 5 and serviceitemid=" + dtFinaltable.Rows[ij]["serviceitemid"].ToString() + " and ServiceID=" + dtFinaltable.Rows[ij]["ServiceID"];

                                    }
                                    else
                                        strfilter = "mlevel = 5 and serviceitemid=" + dtFinaltable.Rows[ij]["serviceitemid"].ToString() + " and ServiceID=" + dtFinaltable.Rows[ij]["ServiceID"];

                                    if (dtBillItems.Columns.Contains("BedSeq") & dtFinaltable.Columns.Contains("BedSeq"))
                                    {
                                        if (!string.IsNullOrEmpty(dtFinaltable.Rows[ij]["BedSeq"].ToString()))
                                            strfilter = strfilter + " AND BedSeq =" + dtFinaltable.Rows[ij]["BedSeq"].ToString();
                                    }

                                    DataRow[] drDtbills = dtBillItems.Select(strfilter, "");
                                    if (IPOP == 2)
                                    {
                                        if (dtBillItems.Columns.Contains("BillItemSequence"))
                                            dtFinaltable.Rows[ij]["SEQ"] = drDtbills[0]["BillItemSequence"];
                                        else dtFinaltable.Rows[ij]["SEQ"] = drDtbills[0]["Seq"];

                                        dtFinaltable.Rows[ij]["OrderID"] = drDtbills[0]["OrderID"];
                                        dtFinaltable.Rows[ij]["OrderItemID"] = drDtbills[0]["OrderItemID"];
                                    }
                                    else
                                    {
                                        dtFinaltable.Rows[ij]["SEQ"] = drDtbills[0]["Seq"];
                                    }
                                    dtFinaltable.AcceptChanges();
                                }

                            }
                        }
                    }
                }
                #endregion

                #region Discount definition for IP Package
                DataRow[] drFinaltbl = dtFinaltable.Select("Serviceid=4");
                foreach (DataRow dr in drFinaltbl)
                {
                    DataRow[] drowdtBillItems = dtBillItems.Select("Serviceid=4 and mlevel=5");
                    foreach (DataRow drow in drowdtBillItems)
                    {
                        if (Convert.ToInt32(drow["Serviceitemid"].ToString()) == Convert.ToInt32(dr["serviceitemid"].ToString()))
                            dr["OrderID"] = string.IsNullOrEmpty(dr["OrderID"].ToString()) ? drow["OrderID"] : dr["OrderID"];

                    }
                }
                #endregion

                return dtFinaltable;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string[] FilterType(string strBillType, int IPOP)
        {

            if (strBillType == "CR")
            {
                if (Convert.ToInt32(IPOP) == 2)
                    FilterQty = "BQTY";
                else
                    FilterQty = "MQTY";
                Filter = "CPAY";

            }
            else if (strBillType == "SP")
            {
                if (Convert.ToInt32(IPOP) == 2)
                {
                    FilterQty = "BQTY";
                    Filter = "PPAY";
                }
                else
                {
                    FilterQty = "MQTY";
                    Filter = "SPAY";
                }
            }
            else
            {
                if (IPOP == 2)
                    FilterQty = "BQTY";
                else
                    FilterQty = "MQTY";
                Filter = "PPAY";
            }
            string[] strFilter = new string[2];
            strFilter[0] = Filter;
            strFilter[1] = FilterQty;
            return strFilter;

        }

        private DataTable BindCategoryItems(int CategoryID)
        {
            try
            {
                int userID = Convert.ToInt32(strDefaultUserId);
                int workStationID = Convert.ToInt32(strDefWorkstationId);
                int featureID = 22566; int functionID = 5; int error = 0;
                string callContext = "Fetching Pharmacy Items";
                DataSet dsCategoryItems = GetCategoryItems(CategoryID, userID, workStationID, featureID, functionID, callContext, error);
                DataTable dtCategorysItems = dsCategoryItems.Tables[0];
                return dtCategorysItems;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in BindCategoryItems", "");
                return null;
            }
        }

        private DataTable LoadDetailsMPHL(DataTable dt, int intLevel, string strFilter)
        {
            DataTable dtDetails = new DataTable();
            try
            {
                dtDetails = dt.Clone();
                DataRow[] drDetails = null;
                DataTable dtAppliedLvls = dt.Copy();
                drDetails = dt.Select("mLevel=" + intLevel + " and " + strFilter + ">0 and ISDISCAPP=" + true);
                foreach (DataRow dr in drDetails)
                { dtDetails.ImportRow(dr); }
                return dtDetails;
            }
            catch (Exception ex)
            {
                return dtDetails;
            }
        }

        public DataSet FetchAllBillDetailsAdv(int Type, string Filter, int intUserID, int intWorkStnId, int intError)
        {
            try
            {
                objFOClient = new FrontOfficeServiceContractClient();
                return objFOClient.FetchAllBillDetailsAdv(Type, Filter, intUserID, intWorkStnId, intError);
            }

            finally
            {
                objFOClient.Close();
            }
        }

        public double RoundCorrect(double d, int decimals)
        {
            try
            {
                double multiplier = Math.Pow(10, decimals);

                if (d < 0)
                    multiplier *= -1;

                return Math.Floor((d * multiplier) + 0.5) / multiplier;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in RoundCorrect", "");
                return 0;
            }
        }

        public DataSet FetchfromAdv(string strProcedureName, string strType, string strFilter, int intUserID, int intWorkstationID, int? intFeatureID, int? intFunctionID, string strCallContext)
        {
            ContractMgmtServiceContractClient objContractMgmtServiceContractClient = new ContractMgmtServiceContractClient();
            DataSet dsFetchfromAdv = new DataSet();
            try
            {
                dsFetchfromAdv = objContractMgmtServiceContractClient.FetchfromAdv(strProcedureName, strType, strFilter, intUserID, intWorkstationID, intFeatureID, intFunctionID, strCallContext);
            }

            finally
            {
                objContractMgmtServiceContractClient.Close();

            }
            return dsFetchfromAdv;
        }

        private DataTable LoadDetails(DataTable dt, int intLevel, string strFilter)
        {
            DataTable dtDetails = new DataTable();
            try
            {
                dtDetails = dt.Clone();
                DataRow[] drDetails = null;
                drDetails = dt.Select("mLevel=" + intLevel + " and " + strFilter + ">0");

                foreach (DataRow dr in drDetails)
                { dtDetails.ImportRow(dr); }
                return dtDetails;
            }
            catch (Exception ex)
            {

                return dtDetails;
            }
        }

        private void SetDiscLevelAppliedMPHL(DataTable dtSelectedItem, string strFilterCond)
        {
            DataRow[] drow = dtSelectedItem.Select("Mlevel=5 and " + strFilterCond);
            for (int i = 0; i < drow.Length; i++)
            {
                drow[i]["ISDISCAPP"] = true;
            }
            dtSelectedItem.AcceptChanges();
        }

        private DataTable CreateDtSummary()
        {
            DataTable DtBillSummary = new DataTable();
            try
            {
                DtBillSummary.Columns.Add("Description", typeof(string));
                DtBillSummary.Columns.Add("Amount", typeof(string));

                DataRow drSumryRow = DtBillSummary.NewRow();
                drSumryRow["Description"] = "Bill Amount";
                DtBillSummary.Rows.Add(drSumryRow);
                drSumryRow = DtBillSummary.NewRow();
                drSumryRow["Description"] = "Payer Amount";
                DtBillSummary.Rows.Add(drSumryRow);

                drSumryRow = DtBillSummary.NewRow();
                drSumryRow["Description"] = "Discount Amount";
                DtBillSummary.Rows.Add(drSumryRow);
                drSumryRow = DtBillSummary.NewRow();
                drSumryRow["Description"] = "VAT";
                DtBillSummary.Rows.Add(drSumryRow);
                drSumryRow = DtBillSummary.NewRow();

                drSumryRow["Description"] = "Deposit Amount";
                DtBillSummary.Rows.Add(drSumryRow);
                drSumryRow = DtBillSummary.NewRow();
                drSumryRow["Description"] = "Refund Amount";
                DtBillSummary.Rows.Add(drSumryRow);

                drSumryRow = DtBillSummary.NewRow();
                drSumryRow["Description"] = "Receipt Amount";
                DtBillSummary.Rows.Add(drSumryRow);
                drSumryRow = DtBillSummary.NewRow();
                drSumryRow["Description"] = "Balance Amount";
                DtBillSummary.Rows.Add(drSumryRow);

                drSumryRow = DtBillSummary.NewRow();
                drSumryRow["Description"] = "Collectable";
                DtBillSummary.Rows.Add(drSumryRow);
                return DtBillSummary;
            }
            catch (Exception ex)
            {
                //HIS.TOOLS.Logger.ErrorLog.ErrorRoutine(ex, MODULE_NAME, "Error in CreateDtSummary", "");
                return null;
            }
        }

        public decimal FetchMaxCollectable(DateTime dtFromDate, DateTime dtToDate, int intPatientID, int intGradeID, int intCompanyid, int intSpecialisationid, int PatentLOAID, string strTbl, int intUserid, int intworkstationid, int intError, int intFeatureid, int intFunctionid, string strCallcontext)
        {
            try
            {
                objFOClient = new FrontOfficeServiceContractClient();
                return objFOClient.FetchMaxCollectable(dtFromDate, dtToDate, intPatientID, intGradeID, intCompanyid, intSpecialisationid, PatentLOAID, strTbl, intUserid, intworkstationid, intError, intFeatureid, intFunctionid, strCallcontext);
            }

            finally
            {
                objFOClient.Close();
            }
        }

        public class PatientBiillInfoList
        {
            public int Code;
            public string Status;
            public string Message;
            public string Message2L;
            public List<PatientBiillInfoListN> BillSummary;
        }

        public enum ProcessStatus
        {
            Success = 1,
            Fail = 2
        }

    }

    public class PatientBiillInfoListN
    {
        public string RegCode;
        public string HospitalID;
        public string ScheduleID;
        public string BillAmount;
        public string PayerAmount;
        public string DiscountAmount;
        public string VAT;
        public string DepositAmount;
        public string RefundAmount;
        public string ReceiptAmount;
        public string BalanceAmount;
        public string Collectable;
        public string OrderType;

        public PatientBiillInfoListN()
        {
        }
    }
}