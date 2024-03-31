// Decompiled with JetBrains decompiler
// Type: HISException.ExceptionManager
// Assembly: HISData, Version=1.0.0.1, Culture=neutral, PublicKeyToken=c0cdbf5bd60ca267
// MVID: 8940F0D6-312D-4723-B4E1-FE4E7304123C
// Assembly location: C:\Users\Swathi\Sangamesh\Reports\onlinePayment\bin\HISData.dll

using HISResource;
using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Reflection;
using System.Security.Principal;
using System.Text;

namespace HISException
{
    public class ExceptionManager
    {
        private ExceptionManager()
        {
        }

        private static void SetSysMessage(CustomException customException, Exception ex)
        {
            if (customException == null || ex == null)
                return;
            if (ex.Message != null && ex.Message != "")
                customException.SysErrorDesc = ex.Message;
            else if (ex.InnerException != null)
                ExceptionManager.SetSysMessage(customException, ex.InnerException);
            else
                customException.SysErrorDesc = "Could not retrieve an exception message.Retrieving only Exception Class Name,which is:'" + ex.GetType().FullName + "'.";
        }

        private static void SetAppMessage(
          CustomException customException,
          Exception ex,
          string appErrorDesc)
        {
            if (appErrorDesc == null || appErrorDesc.Equals(""))
            {
                if (ex.Message != null && ex.Message != "")
                    customException.AppErrorDesc = ex.Message;
                else if (ex.InnerException != null)
                    ExceptionManager.SetAppMessage(customException, ex.InnerException, (string)null);
                else
                    customException.AppErrorDesc = "Could not retrieve an exception message.Retrieving only Exception Class Name,which is:'" + ex.GetType().FullName + "'.";
            }
            else
                customException.AppErrorDesc = appErrorDesc;
        }

        private static void GetMessageLog(Exception ex, ref StringBuilder messageLog)
        {
            try
            {
                if (ex == null || messageLog == null)
                    return;
                if (ex.Message != null && ex.Message != "")
                {
                    if (messageLog.Length == 0)
                        messageLog.AppendFormat("{0}", (object)ex.Message);
                    else
                        messageLog.AppendFormat("{0}Additional Information : {1}", (object)Environment.NewLine, (object)ex.Message);
                    if (ex.InnerException == null)
                        return;
                    ExceptionManager.GetMessageLog(ex.InnerException, ref messageLog);
                }
                else if (ex.InnerException != null)
                {
                    ExceptionManager.GetMessageLog(ex.InnerException, ref messageLog);
                }
                else
                {
                    if (messageLog.Length != 0)
                        return;
                    messageLog.AppendFormat("{0}:{1}", (object)"Could not retrieve an exception message.Retrieving only Exception Class Name,which is", (object)("'" + ex.GetType().FullName + "'."));
                }
            }
            catch
            {
            }
        }

        public static CustomException HandleException(
          Exception ex,
          string source,
          MethodBase currentMethod,
          params object[] listOfParams)
        {
            int messageCode = 0;
            string message = "";
            string appErrorDesc = "";
            CustomException customException = (CustomException)null;
            try
            {
                if (ex == null || source == null || currentMethod == (MethodBase)null)
                    return customException;
                customException = ex as CustomException;
                string name = WindowsIdentity.GetCurrent().Name;
                if (customException != null)
                    return customException;
                string str;
                StringBuilder messageLog;
                switch (ex)
                {
                    case CodeBasedException _:
                        str = "A";
                        messageCode = ((CodeBasedException)ex).ErrorCode;
                        appErrorDesc = ex.Message;
                        messageLog = new StringBuilder(150);
                        ExceptionManager.GetMessageLog(ex, ref messageLog);
                        message = messageLog.ToString();
                        break;
                    case OleDbException _:
                    //case OracleException _:
                    //    int num1 = ex.Message.IndexOf("ORA");
                    //    if (num1 > -1)
                    //    {
                    //        int startIndex = num1 + 3;
                    //        int num2 = ex.Message.IndexOf(":", startIndex);
                    //        if (num2 > -1)
                    //            messageCode = Convert.ToInt32(ex.Message.Substring(startIndex, num2 - startIndex));
                    //        if (messageCode >= -20999 && messageCode <= -20000)
                    //        {
                    //            str = "A";
                    //            message = ResourceFileManager.GetMessage(messageCode);
                    //            appErrorDesc = message;
                    //            break;
                    //        }
                    //        str = "S";
                    //        break;
                    //    }
                    //    str = "S";
                    //    break;
                    case SqlException _:
                        messageCode = ((SqlException)ex).Number;
                        str = "S";
                        break;
                    default:
                        str = "S";
                        break;
                }
                switch (str)
                {
                    case "A":
                        customException = new CustomException(message, ex.InnerException);
                        customException.ErrorType = ErrorType.Application;
                        if (messageCode == 2627)
                        {
                            customException.AppErrorCode = message;
                            ExceptionManager.SetAppMessage(customException, ex, "");
                            break;
                        }
                        customException.AppErrorCode = messageCode.ToString();
                        ExceptionManager.SetAppMessage(customException, ex, appErrorDesc);
                        break;
                    case "S":
                        messageLog = new StringBuilder(150);
                        ExceptionManager.GetMessageLog(ex, ref messageLog);
                        customException = new CustomException(messageLog.ToString(), ex);
                        customException.ErrorType = ErrorType.System;
                        ExceptionManager.SetSysMessage(customException, ex);
                        break;
                }
                customException.ObjectInfo = listOfParams;
                customException.Source = source;
                customException.NTUserID = name;
                customException.MethodInfo = currentMethod;
            }
            catch (CustomException ex1)
            {
                customException = ex1;
            }
            catch (DataException ex2)
            {
                customException = new CustomException(ex2.Message, (Exception)ex2);
                customException.ErrorType = ErrorType.WithinComponent;
                customException.Source = nameof(ExceptionManager);
                customException.MethodInfo = MethodBase.GetCurrentMethod();
                ExceptionManager.SetSysMessage(customException, (Exception)ex2);
            }
            catch
            {
                return customException;
            }
            return customException;
        }
    }
}
