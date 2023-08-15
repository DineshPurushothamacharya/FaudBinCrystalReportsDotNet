// Decompiled with JetBrains decompiler
// Type: HISException.CustomException
// Assembly: HISData, Version=1.0.0.1, Culture=neutral, PublicKeyToken=c0cdbf5bd60ca267
// MVID: 8940F0D6-312D-4723-B4E1-FE4E7304123C
// Assembly location: C:\Users\Swathi\Sangamesh\Reports\onlinePayment\bin\HISData.dll

using System;
using System.Reflection;

namespace HISException
{
    public enum ErrorType
    {
        System,
        Application,
        WithinComponent,
    }

    public class CustomException : ApplicationException
    {
        private string _appErrCode;
        private string _appErrDesc;
        private string _sysErrDesc;
        private string _ntUserId;
        private MethodBase _methodInfo;
        private object[] _objectInfo;
        private ErrorType _errorType;

        private CustomException()
        {
        }

        public CustomException(string message)
        {
        }

        internal CustomException(string message, Exception innerException)
          : base(message, innerException)
        {
        }

        public string AppErrorCode
        {
            get => this._appErrCode;
            set => this._appErrCode = value;
        }

        public string AppErrorDesc
        {
            get => this._appErrDesc;
            set => this._appErrDesc = value;
        }

        public string SysErrorDesc
        {
            get => this._sysErrDesc;
            set => this._sysErrDesc = value;
        }

        public string NTUserID
        {
            get => this._ntUserId;
            set => this._ntUserId = value;
        }

        public MethodBase MethodInfo
        {
            get => this._methodInfo;
            set => this._methodInfo = value;
        }

        public object[] ObjectInfo
        {
            get => this._objectInfo;
            set => this._objectInfo = value;
        }

        public ErrorType ErrorType
        {
            get => this._errorType;
            set => this._errorType = value;
        }
    }
}
