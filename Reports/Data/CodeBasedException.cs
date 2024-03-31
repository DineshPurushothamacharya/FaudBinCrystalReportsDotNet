// Decompiled with JetBrains decompiler
// Type: HISException.CodeBasedException
// Assembly: HISData, Version=1.0.0.1, Culture=neutral, PublicKeyToken=c0cdbf5bd60ca267
// MVID: 8940F0D6-312D-4723-B4E1-FE4E7304123C
// Assembly location: C:\Users\Swathi\Sangamesh\Reports\onlinePayment\bin\HISData.dll

using HISResource;
using System;
using System.Text;

namespace HISException
{
    public class CodeBasedException : ApplicationException
    {
        private readonly int errCode;

        private CodeBasedException()
        {
        }

        private static string GetExceptionMessage(int erCode, string[] placeHolderValues)
        {
            StringBuilder stringBuilder = new StringBuilder(150);
            int num = 0;
            try
            {
                stringBuilder.Append(ResourceFileManager.GetMessage(erCode));
            }
            catch
            {
                return "Error trying to fetch exception message from 'ResourceCodes.dll' for the code='" + erCode.ToString() + "'.";
            }
            if (placeHolderValues != null)
            {
                foreach (string placeHolderValue in placeHolderValues)
                {
                    stringBuilder.Replace("[PlaceHolder" + (object)num + "]", placeHolderValue);
                    ++num;
                }
            }
            return stringBuilder.ToString();
        }

        public CodeBasedException(int erCode, params string[] placeHolderValues)
          : base(CodeBasedException.GetExceptionMessage(erCode, placeHolderValues))
        {
            this.errCode = erCode;
        }

        public CodeBasedException(
          int erCode,
          Exception innerException,
          params string[] placeHolderValues)
          : base(CodeBasedException.GetExceptionMessage(erCode, placeHolderValues), innerException)
        {
            this.errCode = erCode;
        }

        protected CodeBasedException(string message)
          : base(message)
        {
        }

        protected CodeBasedException(string message, Exception innerException)
          : base(message, innerException)
        {
        }

        public int ErrorCode => this.errCode;
    }
}
