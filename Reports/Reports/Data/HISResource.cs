// Decompiled with JetBrains decompiler
// Type: HISResource.ResourceFileManager
// Assembly: HISData, Version=1.0.0.1, Culture=neutral, PublicKeyToken=c0cdbf5bd60ca267
// MVID: 8940F0D6-312D-4723-B4E1-FE4E7304123C
// Assembly location: C:\Users\Swathi\Sangamesh\Reports\onlinePayment\bin\HISData.dll

using HISException;
using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;

namespace HISResource
{
    public class ResourceFileManager
    {
        private ResourceFileManager()
        {
        }

        public static string GetMessage(int messageCode, params string[] placeHolderValues)
        {
            StringBuilder stringBuilder = new StringBuilder(150);
            string message = "";
            int num = 0;
            Exception innerException = (Exception)null;
            try
            {
                try
                {
                    Assembly.Load("HISData");
                    ResourceManager resourceManager = new ResourceManager("HISData.ResourceCodes", Assembly.GetExecutingAssembly());
                    CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                    string name = messageCode.ToString();
                    stringBuilder.Append(resourceManager.GetString(name, currentCulture));
                }
                catch (ThreadInterruptedException ex)
                {
                    message = "Error trying to fetch exception message from 'ResourceCodes.dll' for the code='" + messageCode.ToString() + "'.";
                    innerException = (Exception)ex;
                }
                catch (ThreadAbortException ex)
                {
                    message = "Error trying to fetch exception message from 'ResourceCodes.dll' for the code='" + messageCode.ToString() + "'.";
                    innerException = (Exception)ex;
                }
                catch (ThreadStateException ex)
                {
                    message = "Error trying to fetch exception message from 'ResourceCodes.dll' for the code='" + messageCode.ToString() + "'.";
                    innerException = (Exception)ex;
                }
                if (placeHolderValues != null && innerException == null)
                {
                    foreach (string placeHolderValue in placeHolderValues)
                    {
                        stringBuilder.Replace("[PlaceHolder" + (object)num + "]", placeHolderValue);
                        ++num;
                    }
                }
                if (!message.Equals(""))
                    throw ExceptionManager.HandleException((Exception)new ResourceFileManager.ResourceParsingException(message, innerException), "ExceptionManager", MethodBase.GetCurrentMethod());
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (ThreadInterruptedException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, "ExceptionManager", currentMethod, objArray);
            }
            catch (ThreadAbortException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, "ExceptionManager", currentMethod, objArray);
            }
            catch (ThreadStateException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, "ExceptionManager", currentMethod, objArray);
            }
            return stringBuilder.ToString();
        }

        private class ResourceParsingException : CodeBasedException
        {
            public ResourceParsingException(string message, Exception innerException)
              : base(message, innerException)
            {
            }
        }
    }
}
