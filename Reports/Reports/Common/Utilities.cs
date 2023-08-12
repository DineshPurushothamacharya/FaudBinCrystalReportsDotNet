using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services.Protocols;
using System.Xml;

namespace Reports.Common
{
    

    public sealed class Utilities
    {
        private Utilities()
        {
        }

        private static string ValidationTypeLookup(ValidationType vt)
        {
            switch (vt)
            {
                case ValidationType.NumberWithOutPeriod:
                case ValidationType.Age:
                    return "0123456789\b";
                case ValidationType.NumberWithPeriod:
                    return "0123456789.\b";
                case ValidationType.PhoneNo:
                    return "-ext()+nEXTN, 0123456789\b";
                case ValidationType.Address:
                case ValidationType.ItemName:
                case ValidationType.ProcedureName:
                case ValidationType.Remarks:
                    return "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789-/.,()@#_'%&*[]+\\`\"\b";
                case ValidationType.Alphabets:
                case ValidationType.PersonName:
                case ValidationType.OrganizationName:
                    return "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ .\b";
                case ValidationType.AlphaNumerics:
                case ValidationType.LocationName:
                    return "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .\b";
                case ValidationType.Name:
                    return "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ' .\b";
                default:
                    return "ABCD";
            }
        }

        public static bool IsCharacterValid(string strChar, ValidationType vt)
        {
            string str = Utilities.ValidationTypeLookup(vt);
            bool flag = false;
            switch (strChar)
            {
                case "":
                    return false;
                case "\b":
                    return true;
                case "-":
                    return true;
                default:
                    for (int index = 1; index < str.Length; ++index)
                    {
                        if (str.Substring(index - 1, 1).Equals(strChar))
                            return true;
                    }
                    return flag;
            }
        }

        public static string ValidateText(string strString, ValidationType vt)
        {
            StringBuilder stringBuilder = new StringBuilder("");
            string str1 = Utilities.ValidationTypeLookup(vt);
            for (int index1 = 1; index1 <= strString.Length; ++index1)
            {
                bool flag = false;
                string str2 = strString.Substring(index1 - 1, 1);
                for (int index2 = 1; index2 <= str1.Length; ++index2)
                {
                    string str3 = str1.Substring(index2 - 1, 1);
                    if (str2 == str3)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    return stringBuilder.ToString();
                stringBuilder.Append(str2.ToCharArray());
            }
            return stringBuilder.ToString();
        }

        public static bool IsValidNumberWithPeriod(string strChar, string strText)
        {
            string str1 = ".0123456789";
            bool flag = false;
            if (strChar.Equals(""))
                return false;
            for (int index = 1; index <= str1.Length; ++index)
            {
                string str2 = str1.Substring(index - 1, 1);
                switch (strChar)
                {
                    case "\b":
                        return true;
                    case ".":
                        if (strText.Length > 0)
                        {
                            for (int startIndex = 0; startIndex < strText.Length; ++startIndex)
                            {
                                if (strText.Substring(startIndex, 1) == ".")
                                    return false;
                            }
                            break;
                        }
                        break;
                }
                if (str2.Equals(strChar))
                    return true;
            }
            return flag;
        }

        public static string ValidNumberWithPeriod(string strString)
        {
            StringBuilder stringBuilder = new StringBuilder("");
            string str1 = ".0123456789";
            for (int index1 = 1; index1 <= strString.Length; ++index1)
            {
                bool flag = false;
                string str2 = strString.Substring(index1 - 1, 1);
                for (int index2 = 1; index2 <= str1.Length; ++index2)
                {
                    string str3 = str1.Substring(index2 - 1, 1);
                    if (str2 == str3)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    return stringBuilder.ToString();
                stringBuilder.Append(str2.ToCharArray());
            }
            return stringBuilder.ToString();
        }

        public static string ConvertToSql(string strText)
        {
            StringBuilder stringBuilder = new StringBuilder(strText);
            stringBuilder.Replace("'", "''");
            return stringBuilder.ToString();
        }

        public static string GetUHID(string strIACode, long lngRegNo)
        {
            if (strIACode.Trim().Length > 4)
                strIACode = strIACode.Remove(4, strIACode.Length - 4);
            StringBuilder stringBuilder = new StringBuilder("");
            if (lngRegNo.ToString().Length == 0 || lngRegNo.ToString().Length >= 10)
                return strIACode.ToUpper() + "." + lngRegNo.ToString();
            stringBuilder.Insert(0, "0", 10 - lngRegNo.ToString().Length);
            return strIACode.ToUpper() + "." + stringBuilder.ToString() + lngRegNo.ToString();
        }

        public static string GetIACode(string strUHID)
        {
            if (!Utilities.IsValidUHID(strUHID))
                return string.Empty;
            int length = strUHID.IndexOf(".", 0);
            return strUHID.Substring(0, length).ToString();
        }

        public static long GetRegistrationNo(string strUHID)
        {
            if (!Utilities.IsValidUHID(strUHID))
                return 0;
            int num = strUHID.IndexOf(".", 0);
            return Convert.ToInt64(strUHID.Substring(num + 1));
        }

        public static bool IsValidUHID(string strUHID)
        {
            int num = strUHID.IndexOf(".", 0);
            if (num != 4)
                return false;
            string str = strUHID.Substring(num + 1);
            if (str.Trim().Length == 0)
                return false;
            for (int index = 1; index <= str.Length; ++index)
            {
                foreach (char c in str.Substring(index - 1, 1).ToCharArray())
                {
                    if (!char.IsNumber(c))
                        return false;
                }
            }
            return true;
        }

        public static bool IsValidCreditCardNo(string strCardNo)
        {
            if (strCardNo == null || strCardNo.Trim() == string.Empty)
                return false;
            int[] numArray1 = new int[16];
            int[] numArray2 = new int[16];
            int[] numArray3 = new int[16];
            int[] numArray4 = new int[16];
            bool flag = false;
            string str = strCardNo;
            for (int index = 0; index <= 15; ++index)
                numArray1[index] = -1;
            char[] charArray = str.ToCharArray();
            for (int index = 0; index <= charArray.Length - 1; ++index)
                numArray1[index] = Convert.ToInt32(charArray[index]) - 48;
            if (numArray1[15] != -1)
            {
                for (int index = 1; index <= 15; index += 2)
                    numArray2[index] = 1;
                for (int index = 0; index <= 15; index += 2)
                    numArray2[index] = 2;
                numArray4[0] = 0;
                for (int index = 0; index <= 15; ++index)
                {
                    numArray3[index] = numArray1[index] * numArray2[index];
                    numArray3[index] = numArray3[index] <= 9 ? numArray3[index] : numArray3[index] - 9;
                    if (index == 0)
                        numArray4[index] = numArray3[index];
                    if (index != 0)
                        numArray4[index] = numArray4[index - 1] + numArray3[index];
                    flag = numArray4[index] % 10 == 0;
                }
            }
            if (numArray1[14] != -1 && numArray1[15] == -1)
            {
                for (int index = 1; index <= 14; index += 2)
                    numArray2[index] = 2;
                for (int index = 0; index <= 14; index += 2)
                    numArray2[index] = 1;
                for (int index = 0; index <= 14; ++index)
                    numArray4[0] = 0;
                for (int index = 0; index <= 14; ++index)
                {
                    numArray3[index] = numArray1[index] * numArray2[index];
                    numArray3[index] = numArray3[index] <= 9 ? numArray3[index] : numArray3[index] - 9;
                    if (index == 0)
                        numArray4[index] = numArray3[index];
                    if (index != 0)
                        numArray4[index] = numArray4[index - 1] + numArray3[index];
                    flag = numArray4[index] % 10 == 0;
                }
            }
            return flag;
        }

        //public static string Number2Words(string strNumber)
        //{
        //    NumbersRupees numbersRupees = new NumbersRupees();
        //    numbersRupees.InitNumbers();
        //    return numbersRupees.RupeesNPaise((object)strNumber);
        //}

        //public static SoapException ConvertToSoapException(
        //  string uri,
        //  string webServiceNamespace,
        //  string errorMessage,
        //  string errorNumber,
        //  string errorSource,
        //  FaultCode code)
        //{
        //    XmlQualifiedName code1 = (XmlQualifiedName)null;
        //    switch (code)
        //    {
        //        case FaultCode.Client:
        //            code1 = SoapException.ClientFaultCode;
        //            break;
        //        case FaultCode.Server:
        //            code1 = SoapException.ServerFaultCode;
        //            break;
        //    }
        //    XmlDocument xmlDocument = new XmlDocument();
        //    XmlNode node1 = xmlDocument.CreateNode(XmlNodeType.Element, SoapException.DetailElementName.Name, SoapException.DetailElementName.Namespace);
        //    XmlNode node2 = xmlDocument.CreateNode(XmlNodeType.Element, "Error", webServiceNamespace);
        //    XmlNode node3 = xmlDocument.CreateNode(XmlNodeType.Element, "ErrorNumber", webServiceNamespace);
        //    node3.InnerText = errorNumber;
        //    XmlNode node4 = xmlDocument.CreateNode(XmlNodeType.Element, "ErrorMessage", webServiceNamespace);
        //    node4.InnerText = errorMessage;
        //    XmlNode node5 = xmlDocument.CreateNode(XmlNodeType.Element, "Source", webServiceNamespace);
        //    node5.InnerText = errorSource;
        //    node2.AppendChild(node3);
        //    node2.AppendChild(node4);
        //    node2.AppendChild(node5);
        //    node1.AppendChild(node2);
        //    SoapException soapException = new SoapException(errorMessage, code1, uri, node1);
        //    soapException.Source = errorSource;
        //    return soapException;
        //}

        public static string ConvertDTToXML(string RootTableName, string TableName, DataTable dt)
        {
            StringBuilder stringBuilder1 = new StringBuilder();
            if (dt == null)
                return stringBuilder1.ToString();
            if (RootTableName.Length == 0 || TableName.Length == 0 || dt.Rows.Count == 0)
                return stringBuilder1.ToString();
            stringBuilder1.Append("<" + RootTableName.ToUpper() + "> ");
            foreach (DataRow row in (InternalDataCollectionBase)dt.Rows)
            {
                stringBuilder1.Append(" <" + TableName.ToUpper() + " ");
                for (int index = 0; index < dt.Columns.Count; ++index)
                {
                    if (row[index] is DBNull)
                    {
                        stringBuilder1.Append(" ");
                    }
                    else
                    {
                        stringBuilder1.Append(dt.Columns[index].ColumnName.ToUpper());
                        stringBuilder1.Append("=\"");
                        if (dt.Columns[index].DataType.ToString() == "System.Boolean")
                        {
                            if (row[index].ToString() == "False")
                                stringBuilder1.Append("0\" ");
                            else
                                stringBuilder1.Append("1\" ");
                        }
                        else if (dt.Columns[index].DataType.ToString() == "System.String")
                        {
                            StringBuilder stringBuilder2 = new StringBuilder();
                            stringBuilder2.Append(row[index].ToString());
                            stringBuilder2.Replace("&", "&amp;");
                            stringBuilder2.Replace("\"", "&quot;");
                            stringBuilder2.Replace("<", "&lt;");
                            stringBuilder2.Replace(">", "&gt;");
                            stringBuilder1.Append(stringBuilder2.ToString() + "\" ");
                        }
                        else
                            stringBuilder1.Append(row[index].ToString() + "\" ");
                    }
                }
                stringBuilder1.Append("/> ");
            }
            stringBuilder1.Append("</" + RootTableName.ToUpper() + ">");
            return stringBuilder1.ToString();
        }

        //public static IDbDataParameter CreateDbDataParameter(
        //  string ParamName,
        //  DbType ParamType,
        //  ParameterDirection ParamDirection,
        //  object ParamValue)
        //{
        //    IDbDataParameter dataParameter = new DataHelper().CreateDataParameter();
        //    if (dataParameter == null)
        //        return (IDbDataParameter)null;
        //    if (ParamName != null)
        //        dataParameter.ParameterName = ParamName;
        //    else
        //        dataParameter.ParameterName = string.Empty;
        //    dataParameter.DbType = ParamType;
        //    dataParameter.Direction = ParamDirection;
        //    if (ParamValue == null)
        //        dataParameter.Value = (object)DBNull.Value;
        //    else
        //        dataParameter.Value = ParamValue;
        //    return dataParameter;
        //}

        //public static IDbDataParameter CreateDbDataParameter(
        //  string ParamName,
        //  DbType ParamType,
        //  ParameterDirection ParamDirection,
        //  object ParamValue,
        //  int ParamSize)
        //{
        //    IDbDataParameter dataParameter = new DataHelper().CreateDataParameter();
        //    if (dataParameter == null)
        //        return (IDbDataParameter)null;
        //    if (ParamName != null)
        //        dataParameter.ParameterName = ParamName;
        //    else
        //        dataParameter.ParameterName = string.Empty;
        //    dataParameter.DbType = ParamType;
        //    dataParameter.Direction = ParamDirection;
        //    if (ParamValue == null)
        //        dataParameter.Value = (object)DBNull.Value;
        //    else
        //        dataParameter.Value = ParamValue;
        //    dataParameter.Size = ParamSize;
        //    return dataParameter;
        //}

        public static string GetErrorNoinSoapException(SoapException Soapex)
        {
            try
            {
                if (Soapex.Detail.OuterXml.ToString() == null)
                    return string.Empty;
                XmlTextReader xmlTextReader = new XmlTextReader((TextReader)new StringReader(Soapex.Detail.OuterXml.ToString()));
                xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
                while (xmlTextReader.Read())
                {
                    if (xmlTextReader.NodeType == XmlNodeType.Element && xmlTextReader.Name == "ErrorNumber")
                        return xmlTextReader.ReadString().ToString();
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    public enum ValidationType
    {
        NumberWithOutPeriod,
        NumberWithPeriod,
        PhoneNo,
        Address,
        Alphabets,
        AlphaNumerics,
        Name,
        PersonName,
        OrganizationName,
        ItemName,
        ProcedureName,
        LocationName,
        Remarks,
        Age,
    }
}