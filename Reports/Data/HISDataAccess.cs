using HISException;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;

namespace HISDataAccess
{
    public enum ProviderType
    {
        NotSet,
        OleDb,
        Sql,
        Oracle,
    }

    public class DataHelper
    {
        private const string HTTP_HEADER = "HCITUserAgent";
        public string connString = "";
        public ProviderType provider = ProviderType.Sql;
        public int intConnectionTimeOut = 30;
        private const string trackExecution = "TrackExecution";
        private const string trackAfter = "TrackAfter";
        private const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private bool trackToExecution = Convert.ToBoolean(ConfigurationManager.AppSettings["TrackExecution"]);
        private int trackToAfter = Convert.ToInt32(ConfigurationManager.AppSettings["TrackAfter"]);

        public DataHelper()
        {
        }

        public DataHelper(int WorkStationID, int DataBaseID)
        {
            try
            {
                //this.GetConKeyFromService(DataBaseID);
                if (!(this.connString == string.Empty))
                    return;
                switch (DataBaseID)
                {
                    case 1:
                        this.connString = System.Configuration.ConfigurationManager.AppSettings["DBConnectionStringMasters"].ToString();
                        break;
                    case 2:
                        this.connString = System.Configuration.ConfigurationManager.AppSettings["DBConnectionStringTrans"].ToString();
                        break;
                    case 3:
                        this.connString = System.Configuration.ConfigurationManager.AppSettings["DBConnectionStringReports"].ToString();
                        break;
                    default:
                        throw new Exception("Database ID Error");
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText("C:\\Temp\\DataHelperLog.txt", ex.Message + ex.StackTrace);
                throw ex;
            }
        }

        private string GetConnectionStringForMaster(string MasterVal, string strRootWebSitePath)
        {
            int num1 = 0;
            string str1 = "";
            bool flag = true;
            string connectionStringForMaster = "";
            string path = strRootWebSitePath + "Web.config";
            if (File.Exists(path))
            {
                StreamReader streamReader = new StreamReader(path);
                num1 = 0;
                str1 = "";
                while (!streamReader.EndOfStream)
                {
                    string str2 = streamReader.ReadLine().Trim();
                    string str3;
                    if (flag && str2.Contains(MasterVal) && str2.Contains("connectionString="))
                    {
                        int num2 = str2.IndexOf("connectionString=");
                        str3 = str2.Substring(num2 + "connectionString=".Length + 1).Replace("\"", "").Replace("/>", "").Trim();
                    }
                    else
                        str3 = "";
                    if (str3.Trim().Length > 0)
                    {
                        connectionStringForMaster = str3.Trim();
                        break;
                    }
                }
            }
            else
                connectionStringForMaster = "File Not Found : " + path;
            return connectionStringForMaster;
        }

        private void AttachParameters(IDbCommand command, IDbDataParameter[] commandParameters)
        {
            if (commandParameters == null)
                return;
            foreach (IDbDataParameter commandParameter in commandParameters)
            {
                if (commandParameter != null)
                {
                    if (commandParameter.Direction == ParameterDirection.InputOutput || commandParameter.Direction == ParameterDirection.Input)
                    {
                        if (commandParameter.Value == null)
                            commandParameter.Value = (object)DBNull.Value;
                        else if (commandParameter.Value.GetType().FullName == "System.String")
                        {
                            int num = commandParameter.Value.ToString() == "" ? 1 : 0;
                        }
                    }
                    command.Parameters.Add((object)commandParameter);
                }
            }
        }

        private void PrepareCommand(
          IDbCommand command,
          IDbConnection connection,
          IDbTransaction transaction,
          CommandType commandType,
          string commandText,
          IDbDataParameter[] commandParameters,
          out bool mustCloseConnection)
        {
            if (command == null)
                throw ExceptionManager.HandleException((Exception)new DataHelper.NullCommandException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            if (commandText == null || commandText.Trim().Equals(""))
                throw ExceptionManager.HandleException((Exception)new DataHelper.NullCommandTextException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            if (File.Exists("d:\\parameterss.txt"))
            {
                FileStream fileStream = new FileStream("d:\\parameterss.txt", FileMode.Append);
                StreamWriter streamWriter = new StreamWriter((Stream)fileStream);
                string str = commandText.ToString();
                for (int index = 0; index < commandParameters.Length; ++index)
                    str = str + "    " + commandParameters[index].ParameterName + "  = " + commandParameters[index].Value + ",";
                streamWriter.WriteLine(str);
                streamWriter.Flush();
                streamWriter.Flush();
                streamWriter.WriteLine("-----------------------------------------------");
                streamWriter.Close();
                fileStream.Close();
            }
            if (commandParameters != null)
                this.AttachParameters(command, commandParameters);
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
                mustCloseConnection = false;
            command.Connection = connection;
            command.CommandText = commandText;
            if (transaction != null)
                command.Transaction = transaction.Connection != null ? transaction : throw ExceptionManager.HandleException((Exception)new DataHelper.TransactionExpiredException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            command.CommandType = commandType;
        }

        private int ExecuteNonQuery(
          DataHelper.IActivator activator,
          IDbConnection connection,
          CommandType commandType,
          string commandText,
          DataHelper.ConnectionOwnership connectionOwnership,
          params IDbDataParameter[] commandParameters)
        {
            if (connection == null)
                throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            int num1 = 0;
            bool mustCloseConnection = false;
            try
            {
                IDbCommand command = activator.CreateCommand();
                command.CommandTimeout = this.intConnectionTimeOut;
                this.PrepareCommand(command, connection, (IDbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);
                DateTime now1 = DateTime.Now;
                num1 = command.ExecuteNonQuery();
                DateTime now2 = DateTime.Now;
                long num2 = now2.Ticks - now1.Ticks;
                TimeSpan elapsedSpan = new TimeSpan(num2);
                if (this.trackToExecution)
                    this.LogParameters(commandText, commandParameters, now1, now2, num2, elapsedSpan);
                command.Parameters.Clear();
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                throw ex;
            }
            catch (FileNotFoundException ex)
            {
                throw ex;
            }
            finally
            {
                if (mustCloseConnection && connectionOwnership == DataHelper.ConnectionOwnership.Internal)
                    connection.Close();
            }
            return num1;
        }

        private int ExecuteNonQuery(
          DataHelper.IActivator activator,
          IDbTransaction transaction,
          CommandType commandType,
          string commandText,
          DataHelper.ConnectionOwnership connectionOwnership,
          params IDbDataParameter[] commandParameters)
        {
            if (transaction == null)
                throw ExceptionManager.HandleException((Exception)new DataHelper.NullTransactionException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            int num1 = 0;
            bool mustCloseConnection = false;
            try
            {
                IDbCommand command = activator.CreateCommand();
                command.CommandTimeout = this.intConnectionTimeOut;
                this.PrepareCommand(command, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
                DateTime now1 = DateTime.Now;
                num1 = command.ExecuteNonQuery();
                DateTime now2 = DateTime.Now;
                long num2 = now2.Ticks - now1.Ticks;
                TimeSpan elapsedSpan = new TimeSpan(num2);
                if (this.trackToExecution)
                    this.LogParameters(commandText, commandParameters, now1, now2, num2, elapsedSpan);
                command.Parameters.Clear();
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                throw ex;
            }
            finally
            {
                if (mustCloseConnection && connectionOwnership == DataHelper.ConnectionOwnership.Internal)
                    transaction.Connection.Close();
            }
            return num1;
        }

        private DataSet ExecuteDataset(
          DataHelper.IActivator activator,
          IDbConnection connection,
          CommandType commandType,
          string commandText,
          DataHelper.ConnectionOwnership connectionOwnership,
          params IDbDataParameter[] commandParameters)
        {
            if (connection == null)
                throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            DataSet dataSet = (DataSet)null;
            bool mustCloseConnection = false;
            try
            {
                IDbCommand command = activator.CreateCommand();
                command.CommandTimeout = this.intConnectionTimeOut;
                this.PrepareCommand(command, connection, (IDbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);
                IDbDataAdapter dataAdapter = activator.CreateDataAdapter();
                dataAdapter.SelectCommand = command;
                dataSet = new DataSet();
                DateTime now1 = DateTime.Now;
                dataAdapter.Fill(dataSet);
                DateTime now2 = DateTime.Now;
                long num = now2.Ticks - now1.Ticks;
                TimeSpan elapsedSpan = new TimeSpan(num);
                if (this.trackToExecution)
                    this.LogParameters(commandText, commandParameters, now1, now2, num, elapsedSpan);
                command.Parameters.Clear();
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                throw ex;
            }
            finally
            {
                if (mustCloseConnection && connectionOwnership == DataHelper.ConnectionOwnership.Internal)
                    connection.Close();
            }
            return dataSet;
        }

        private object ExecuteScalar(
          DataHelper.IActivator activator,
          IDbConnection connection,
          CommandType commandType,
          string commandText,
          DataHelper.ConnectionOwnership connectionOwnership,
          params IDbDataParameter[] commandParameters)
        {
            if (connection == null)
                throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            object obj = (object)null;
            bool mustCloseConnection = false;
            try
            {
                IDbCommand command = activator.CreateCommand();
                command.CommandTimeout = this.intConnectionTimeOut;
                this.PrepareCommand(command, connection, (IDbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);
                DateTime now1 = DateTime.Now;
                obj = command.ExecuteScalar();
                DateTime now2 = DateTime.Now;
                long num = now2.Ticks - now1.Ticks;
                TimeSpan elapsedSpan = new TimeSpan(num);
                if (this.trackToExecution)
                    this.LogParameters(commandText, commandParameters, now1, now2, num, elapsedSpan);
                command.Parameters.Clear();
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                throw ex;
            }
            finally
            {
                if (mustCloseConnection && connectionOwnership == DataHelper.ConnectionOwnership.Internal)
                    connection.Close();
            }
            return obj;
        }

        private IDataReader ExecuteReader(
          DataHelper.IActivator activator,
          IDbConnection connection,
          CommandType commandType,
          string commandText,
          DataHelper.ConnectionOwnership connectionOwnership,
          DataHelper.CommandState commandState,
          IDbDataParameter[] commandParameters)
        {
            if (connection == null)
                throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            bool mustCloseConnection = false;
            IDataReader dataReader;
            try
            {
                IDbCommand command = activator.CreateCommand();
                command.CommandTimeout = this.intConnectionTimeOut;
                this.PrepareCommand(command, connection, (IDbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);
                DateTime now1 = DateTime.Now;
                dataReader = commandState != DataHelper.CommandState.Close ? command.ExecuteReader() : command.ExecuteReader(CommandBehavior.CloseConnection);
                DateTime now2 = DateTime.Now;
                long num = now2.Ticks - now1.Ticks;
                TimeSpan elapsedSpan = new TimeSpan(num);
                if (this.trackToExecution)
                    this.LogParameters(commandText, commandParameters, now1, now2, num, elapsedSpan);
            }
            catch (CustomException ex)
            {
                if (mustCloseConnection && connectionOwnership == DataHelper.ConnectionOwnership.Internal)
                    connection.Close();
                throw ex;
            }
            catch (DataException ex)
            {
                if (mustCloseConnection && connectionOwnership == DataHelper.ConnectionOwnership.Internal)
                    connection.Close();
                throw ex;
            }
            return dataReader;
        }

        private bool IsProviderSet() => this.provider != ProviderType.NotSet;

        private ArrayList GetRows(IDataReader dr)
        {
            ArrayList rows = new ArrayList();
            while (dr.Read())
            {
                object[] instance = (object[])Array.CreateInstance(typeof(object), dr.FieldCount);
                dr.GetValues(instance);
                rows.Add((object)instance);
            }
            return rows;
        }

        private ArrayList GetRows(IDataReader dr, int fieldCount)
        {
            ArrayList rows = new ArrayList();
            while (dr.Read())
            {
                object[] values = new object[fieldCount];
                dr.GetValues(values);
                rows.Add((object)values);
            }
            return rows;
        }

        private Hashtable GetHashTable(IDataReader dr)
        {
            Hashtable hashTable = new Hashtable();
            while (dr.Read())
                hashTable.Add(dr.GetValue(0), dr.GetValue(1));
            return hashTable;
        }

        public string ConnectionString => this.connString;

        public ProviderType Provider
        {
            get => this.provider;
            set => this.provider = value;
        }

        public int ConnectionTimeOut
        {
            get => this.intConnectionTimeOut;
            set => this.intConnectionTimeOut = value;
        }

        public IDbConnection CreateConnection()
        {
            IDbConnection connection;
            try
            {
                connection = this.provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(this.provider).CreateConnection() : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection.ConnectionString = this.connString;
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            return connection;
        }

        public IDbConnection CreateConnection(ProviderType provider)
        {
            IDbConnection connection;
            try
            {
                connection = provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(provider).CreateConnection() : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection.ConnectionString = this.connString;
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            return connection;
        }

        public IDbCommand CreateCommand()
        {
            if (this.provider == ProviderType.NotSet)
                throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            IDbCommand command;
            try
            {
                DataHelper.Activator activator = new DataHelper.Activator();
                command = activator.CreateInstance(this.provider).CreateCommand();
                command.Connection = activator.CreateInstance(this.provider).CreateConnection();
                command.Connection.ConnectionString = this.connString;
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            return command;
        }

        public IDbCommand CreateCommand(ProviderType provider)
        {
            IDbCommand command;
            try
            {
                if (provider == ProviderType.NotSet)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.Activator activator = new DataHelper.Activator();
                command = activator.CreateInstance(provider).CreateCommand();
                command.Connection = activator.CreateInstance(provider).CreateConnection();
                command.Connection.ConnectionString = this.connString;
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            return command;
        }

        public IDbCommand CreateCommand(ProviderType provider, string commandText)
        {
            IDbCommand command;
            try
            {
                if (provider == ProviderType.NotSet)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                if (commandText == null || commandText.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullCommandTextException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.Activator activator = new DataHelper.Activator();
                command = activator.CreateInstance(provider).CreateCommand();
                command.CommandText = commandText;
                command.Connection = activator.CreateInstance(provider).CreateConnection();
                command.Connection.ConnectionString = this.connString;
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            return command;
        }

        public IDbCommand CreateCommand(
          ProviderType provider,
          string commandText,
          IDbTransaction trans)
        {
            IDbCommand command;
            try
            {
                if (provider == ProviderType.NotSet)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                if (commandText == null || commandText.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullCommandTextException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.Activator activator = new DataHelper.Activator();
                command = activator.CreateInstance(provider).CreateCommand();
                command.CommandText = commandText;
                command.Connection = activator.CreateInstance(provider).CreateConnection();
                command.Transaction = trans;
                command.Connection.ConnectionString = this.connString;
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            return command;
        }

        public IDbDataAdapter CreateDataAdapter()
        {
            if (this.provider == ProviderType.NotSet)
                throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            try
            {
                return new DataHelper.Activator().CreateInstance(this.provider).CreateDataAdapter();
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
        }

        public IDbDataAdapter CreateDataAdapter(ProviderType provider)
        {
            if (provider == ProviderType.NotSet)
                throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            try
            {
                return new DataHelper.Activator().CreateInstance(provider).CreateDataAdapter();
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
        }

        public IDbDataAdapter CreateDataAdapter(ProviderType provider, IDbCommand command)
        {
            IDbDataAdapter dataAdapter;
            try
            {
                if (provider == ProviderType.NotSet)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                if (command == null)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullCommandException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                dataAdapter = new DataHelper.Activator().CreateInstance(provider).CreateDataAdapter();
                dataAdapter.SelectCommand = command;
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            return dataAdapter;
        }

        public IDbDataAdapter CreateDataAdapter(ProviderType provider, string commandText)
        {
            IDbDataAdapter dataAdapter;
            try
            {
                if (provider == ProviderType.NotSet)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                if (commandText == null || commandText.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullCommandTextException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator instance = new DataHelper.Activator().CreateInstance(provider);
                dataAdapter = instance.CreateDataAdapter();
                dataAdapter.SelectCommand = instance.CreateCommand();
                dataAdapter.SelectCommand.CommandText = commandText;
                dataAdapter.SelectCommand.Connection = instance.CreateConnection();
                dataAdapter.SelectCommand.Connection.ConnectionString = this.connString;
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            return dataAdapter;
        }

        public IDbDataParameter CreateDataParameter()
        {
            try
            {
                return this.provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(this.provider).CreateDataParameter() : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
        }

        public IDbDataParameter CreateDataParameter(ProviderType provider)
        {
            try
            {
                return provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(provider).CreateDataParameter() : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
        }

        public IDbDataParameter CreateDataParameter(
          ProviderType provider,
          string parameterName,
          object valueOf)
        {
            if (provider == ProviderType.NotSet)
                throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            if (parameterName == null || parameterName.Trim().Equals(""))
                throw ExceptionManager.HandleException((Exception)new DataHelper.NullParameterException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            IDbDataParameter dataParameter;
            try
            {
                dataParameter = new DataHelper.Activator().CreateInstance(provider).CreateDataParameter();
                if (dataParameter != null)
                {
                    dataParameter.ParameterName = parameterName;
                    dataParameter.Value = valueOf;
                }
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            return dataParameter;
        }

        public IDbDataParameter CreateDataParameter(
          ProviderType provider,
          string parameterName,
          DbType dataType)
        {
            IDbDataParameter dataParameter;
            try
            {
                if (provider == ProviderType.NotSet)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                if (parameterName == null || parameterName.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullParameterException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                dataParameter = new DataHelper.Activator().CreateInstance(provider).CreateDataParameter();
                if (dataParameter != null)
                {
                    dataParameter.ParameterName = parameterName;
                    dataParameter.DbType = dataType;
                }
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            return dataParameter;
        }

        public IDbDataParameter CreateDataParameter(
          ProviderType provider,
          string parameterName,
          DbType dataType,
          int size)
        {
            IDbDataParameter dataParameter;
            try
            {
                if (provider == ProviderType.NotSet)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                if (parameterName == null || parameterName.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullParameterException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                dataParameter = new DataHelper.Activator().CreateInstance(provider).CreateDataParameter();
                if (dataParameter != null)
                {
                    dataParameter.ParameterName = parameterName;
                    dataParameter.DbType = dataType;
                    dataParameter.Size = size;
                }
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            return dataParameter;
        }

        public int RunSQL(string sqlText, params IDbDataParameter[] listOfParams)
        {
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = this.provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(this.provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                return this.ExecuteNonQuery(activator, connection, CommandType.Text, sqlText, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                connection?.Dispose();
            }
        }

        public int RunSQL(
          ProviderType provider,
          string sqlText,
          params IDbDataParameter[] listOfParams)
        {
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                return this.ExecuteNonQuery(activator, connection, CommandType.Text, sqlText, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                connection?.Dispose();
            }
        }

        public int RunSQL(
          IDbConnection connection,
          string sqlText,
          params IDbDataParameter[] listOfParams)
        {
            try
            {
                if (connection == null)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                if (this.provider == ProviderType.NotSet)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                return this.ExecuteNonQuery(new DataHelper.Activator().CreateInstance(this.provider), connection, CommandType.Text, sqlText, DataHelper.ConnectionOwnership.External, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
        }

        public int RunSQL(
          IDbConnection connection,
          ProviderType provider,
          string sqlText,
          params IDbDataParameter[] listOfParams)
        {
            if (connection == null)
                throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            if (provider == ProviderType.NotSet)
                throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            try
            {
                return this.ExecuteNonQuery(new DataHelper.Activator().CreateInstance(provider), connection, CommandType.Text, sqlText, DataHelper.ConnectionOwnership.External, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
        }

        public int RunSQL(
          IDbTransaction transaction,
          string sqlText,
          params IDbDataParameter[] listOfParams)
        {
            try
            {
                if (transaction == null)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullTransactionException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                if (this.provider == ProviderType.NotSet)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                return this.ExecuteNonQuery(new DataHelper.Activator().CreateInstance(this.provider), transaction, CommandType.Text, sqlText, DataHelper.ConnectionOwnership.External, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
        }

        public int RunSQL(
          IDbTransaction transaction,
          ProviderType provider,
          string sqlText,
          params IDbDataParameter[] listOfParams)
        {
            try
            {
                if (transaction == null)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullTransactionException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                if (provider == ProviderType.NotSet)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                return this.ExecuteNonQuery(new DataHelper.Activator().CreateInstance(provider), transaction, CommandType.Text, sqlText, DataHelper.ConnectionOwnership.External, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
        }

        public DataSet RunSQLReturnDS(string strSql, params IDbDataParameter[] listOfParams)
        {
            DataSet dataSet = new DataSet();
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = this.provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(this.provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                return this.ExecuteDataset(activator, connection, CommandType.Text, strSql, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                connection?.Dispose();
            }
        }

        public DataSet RunSQLReturnDS(
          ProviderType provider,
          string sqlText,
          params IDbDataParameter[] listOfParams)
        {
            DataSet dataSet = new DataSet();
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                return this.ExecuteDataset(activator, connection, CommandType.Text, sqlText, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (DataException ex)
            {
                throw ex;
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                connection?.Dispose();
            }
        }

        public DataTable RunSQLReturnDT(string strSql, params IDbDataParameter[] listOfParams)
        {
            DataSet dataSet = (DataSet)null;
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = this.provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(this.provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                dataSet = this.ExecuteDataset(activator, connection, CommandType.Text, strSql, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                connection?.Dispose();
            }
            return dataSet.Tables[0];
        }

        public DataTable RunSQLReturnDT(
          ProviderType provider,
          string sqlText,
          params IDbDataParameter[] listOfParams)
        {
            DataSet dataSet = new DataSet();
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                dataSet = this.ExecuteDataset(activator, connection, CommandType.Text, sqlText, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                connection?.Dispose();
            }
            return dataSet.Tables[0];
        }

        public object RunSQLReturnScalar(string sqlText, params IDbDataParameter[] listOfParams)
        {
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = this.provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(this.provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                return this.ExecuteScalar(activator, connection, CommandType.Text, sqlText, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                connection?.Dispose();
            }
        }

        public object RunSQLReturnScalar(
          ProviderType provider,
          string sqlText,
          params IDbDataParameter[] listOfParams)
        {
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                return this.ExecuteScalar(activator, connection, CommandType.Text, sqlText, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                connection?.Dispose();
            }
        }

        public ArrayList RunSQLReturnArrayList(string sqlText, params IDbDataParameter[] listOfParams)
        {
            IDataReader dr = (IDataReader)null;
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = this.provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(this.provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                dr = this.ExecuteReader(activator, connection, CommandType.Text, sqlText, DataHelper.ConnectionOwnership.Internal, DataHelper.CommandState.LeaveOpen, listOfParams);
                return this.GetRows(dr, dr.FieldCount);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed)
                        dr.Close();
                    dr.Dispose();
                }
                if (connection != null)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                    connection.Dispose();
                }
            }
        }

        public ArrayList RunSQLReturnArrayList(
          ProviderType provider,
          string sqlText,
          params IDbDataParameter[] listOfParams)
        {
            if (this.connString == null || this.connString.Trim().Equals(""))
                throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            if (provider == ProviderType.NotSet)
                throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            IDataReader dr = (IDataReader)null;
            IDbConnection connection = (IDbConnection)null;
            try
            {
                DataHelper.IActivator instance = new DataHelper.Activator().CreateInstance(provider);
                connection = instance.CreateConnection();
                connection.ConnectionString = this.connString;
                dr = this.ExecuteReader(instance, connection, CommandType.Text, sqlText, DataHelper.ConnectionOwnership.Internal, DataHelper.CommandState.LeaveOpen, listOfParams);
                return this.GetRows(dr, dr.FieldCount);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed)
                        dr.Close();
                    dr.Dispose();
                }
                if (connection != null)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                    connection.Dispose();
                }
            }
        }

        public Hashtable RunSQLReturnHashTable(string sqlText, params IDbDataParameter[] listOfParams)
        {
            Hashtable hashtable = new Hashtable();
            IDataReader dr = (IDataReader)null;
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = this.provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(this.provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                dr = this.ExecuteReader(activator, connection, CommandType.Text, sqlText, DataHelper.ConnectionOwnership.Internal, DataHelper.CommandState.LeaveOpen, listOfParams);
                return this.GetHashTable(dr);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed)
                        dr.Close();
                    dr.Dispose();
                }
                if (connection != null)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                    connection.Dispose();
                }
            }
        }

        public Hashtable RunSQLReturnHashTable(
          ProviderType provider,
          string sqlText,
          params IDbDataParameter[] listOfParams)
        {
            IDataReader dr = (IDataReader)null;
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                dr = this.ExecuteReader(activator, connection, CommandType.Text, sqlText, DataHelper.ConnectionOwnership.Internal, DataHelper.CommandState.LeaveOpen, listOfParams);
                return this.GetHashTable(dr);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed)
                        dr.Close();
                    dr.Dispose();
                }
                if (connection != null)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                    connection.Dispose();
                }
            }
        }

        public int RunSP(string procName, params IDbDataParameter[] listOfParams)
        {
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = this.provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(this.provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                return this.ExecuteNonQuery(activator, connection, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                connection?.Dispose();
            }
        }

        public int RunSP(
          ProviderType provider,
          string procName,
          params IDbDataParameter[] listOfParams)
        {
            if (this.connString == null || this.connString.Trim().Equals(""))
                throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            if (provider == ProviderType.NotSet)
                throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
            IDbConnection connection = (IDbConnection)null;
            try
            {
                DataHelper.IActivator instance = new DataHelper.Activator().CreateInstance(provider);
                connection = instance.CreateConnection();
                connection.ConnectionString = this.connString;
                return this.ExecuteNonQuery(instance, connection, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                connection?.Dispose();
            }
        }

        public int RunSP(
          IDbTransaction transaction,
          string procName,
          params IDbDataParameter[] listOfParams)
        {
            try
            {
                if (transaction == null)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullTransactionException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                if (this.provider == ProviderType.NotSet)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                return this.ExecuteNonQuery(new DataHelper.Activator().CreateInstance(this.provider), transaction, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.External, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
        }

        public int RunSP(
          IDbTransaction transaction,
          ProviderType provider,
          string procName,
          params IDbDataParameter[] listOfParams)
        {
            if (listOfParams == null)
            {
                int num;
                return num = 0;
            }
            try
            {
                if (transaction == null)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullTransactionException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                if (provider == ProviderType.NotSet)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                return this.ExecuteNonQuery(new DataHelper.Activator().CreateInstance(provider), transaction, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.External, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
        }

        public DataSet RunSPReturnDS(
          ProviderType provider,
          string procName,
          params IDbDataParameter[] listOfParams)
        {
            DataSet dataSet = new DataSet();
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                IDbConnection connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                return this.ExecuteDataset(activator, connection, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
        }

        public DataSet RunSPReturnDS(string procName, params IDbDataParameter[] listOfParams)
        {
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = this.provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(this.provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                return this.ExecuteDataset(activator, connection, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                connection?.Dispose();
            }
        }

        public object RunSPReturnScalar(string procName, params IDbDataParameter[] listOfParams)
        {
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = this.provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(this.provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                return this.ExecuteScalar(activator, connection, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                connection?.Dispose();
            }
        }

        public object RunSPReturnScalar(
          ProviderType provider,
          string procName,
          params IDbDataParameter[] listOfParams)
        {
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                return this.ExecuteScalar(activator, connection, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                connection?.Dispose();
            }
        }

        public void RunSPReturnParams(string procName, params IDbDataParameter[] listOfParams)
        {
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = this.provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(this.provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                IDbConnection connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                this.ExecuteNonQuery(activator, connection, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
        }

        public void RunSPReturnParams(
          ProviderType provider,
          string procName,
          params IDbDataParameter[] listOfParams)
        {
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                this.ExecuteNonQuery(activator, connection, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                connection?.Dispose();
            }
        }

        public void RunSPReturnParams(
          IDbTransaction transaction,
          string procName,
          params IDbDataParameter[] listOfParams)
        {
            IDbConnection dbConnection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = this.provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(this.provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                dbConnection = activator.CreateConnection();
                dbConnection.ConnectionString = this.connString;
                this.ExecuteNonQuery(activator, transaction, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.Internal, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                dbConnection?.Dispose();
            }
        }

        public ArrayList RunSPReturnArrayList(string procName, params IDbDataParameter[] listOfParams)
        {
            ArrayList arrayList = new ArrayList();
            IDataReader dr = (IDataReader)null;
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = this.provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(this.provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                dr = this.ExecuteReader(activator, connection, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.Internal, DataHelper.CommandState.LeaveOpen, listOfParams);
                return this.GetRows(dr, dr.FieldCount);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed)
                        dr.Close();
                    dr.Dispose();
                }
                if (connection != null)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                    connection.Dispose();
                }
            }
        }

        public ArrayList RunSPReturnArrayList(
          ProviderType provider,
          string procName,
          params IDbDataParameter[] listOfParams)
        {
            IDataReader dr = (IDataReader)null;
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                dr = this.ExecuteReader(activator, connection, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.Internal, DataHelper.CommandState.LeaveOpen, listOfParams);
                return this.GetRows(dr, dr.FieldCount);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed)
                        dr.Close();
                    dr.Dispose();
                }
                if (connection != null)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                    connection.Dispose();
                }
            }
        }

        public Hashtable RunSPReturnHashTable(string procName, params IDbDataParameter[] listOfParams)
        {
            Hashtable hashtable = new Hashtable();
            IDataReader dr = (IDataReader)null;
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = this.provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(this.provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                dr = this.ExecuteReader(activator, connection, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.Internal, DataHelper.CommandState.LeaveOpen, listOfParams);
                return this.GetHashTable(dr);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed)
                        dr.Close();
                    dr.Dispose();
                }
                if (connection != null)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                    connection.Dispose();
                }
            }
        }

        public Hashtable RunSPReturnHashTable(
          ProviderType provider,
          string procName,
          params IDbDataParameter[] listOfParams)
        {
            Hashtable hashtable = new Hashtable();
            IDataReader dr = (IDataReader)null;
            IDbConnection connection = (IDbConnection)null;
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                DataHelper.IActivator activator = provider != ProviderType.NotSet ? new DataHelper.Activator().CreateInstance(provider) : throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                connection = activator.CreateConnection();
                connection.ConnectionString = this.connString;
                dr = this.ExecuteReader(activator, connection, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.Internal, DataHelper.CommandState.LeaveOpen, listOfParams);
                return this.GetHashTable(dr);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed)
                        dr.Close();
                    dr.Dispose();
                }
                if (connection != null)
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                    connection.Dispose();
                }
            }
        }

        public bool IsConnStringCached()
        {
            try
            {
                return this.connString != null && !this.connString.Trim().Equals("");
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
        }

        public bool IsConnStringCached(string connectionString)
        {
            try
            {
                return connectionString.Equals(this.connString);
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            catch (FileNotFoundException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
        }

        public IDataReader RunSPReturnDataReader(
          IDbConnection connection,
          string procName,
          params IDbDataParameter[] listOfParams)
        {
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                if (this.provider == ProviderType.NotSet)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                return this.ExecuteReader(new DataHelper.Activator().CreateInstance(this.provider), connection, CommandType.StoredProcedure, procName, DataHelper.ConnectionOwnership.Internal, DataHelper.CommandState.LeaveOpen, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
        }

        public IDataReader RunSQLReturnDataReader(
          IDbConnection connection,
          string strSql,
          params IDbDataParameter[] listOfParams)
        {
            try
            {
                if (this.connString == null || this.connString.Trim().Equals(""))
                    throw ExceptionManager.HandleException((Exception)new DataHelper.NullConnectionStringException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                if (this.provider == ProviderType.NotSet)
                    throw ExceptionManager.HandleException((Exception)new DataHelper.ProviderNotSetException(), nameof(DataHelper), MethodBase.GetCurrentMethod());
                return this.ExecuteReader(new DataHelper.Activator().CreateInstance(this.provider), connection, CommandType.Text, strSql, DataHelper.ConnectionOwnership.Internal, DataHelper.CommandState.LeaveOpen, listOfParams);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (DataException ex)
            {
                MethodBase currentMethod = MethodBase.GetCurrentMethod();
                object[] objArray = Array.Empty<object>();
                throw ExceptionManager.HandleException((Exception)ex, nameof(DataHelper), currentMethod, objArray);
            }
            finally
            {
                connection?.Dispose();
            }
        }

        public int GetWorkLocationId(int WorkStationID)
        {
            this.connString = ConfigurationManager.ConnectionStrings["DBConnectionStringMasters"].ConnectionString;
            return int.Parse(this.RunSQLReturnScalar("select dbo.Fn_GetHospitalid(" + (object)WorkStationID + ",1)").ToString());
        }

        private void GetConKeyFromService(int dataBaseID)
        {
            this.connString = ConfigurationManager.ConnectionStrings["MasterConKey"].ConnectionString;
            //break;
            //if (dataBaseID <= 0)
            //    dataBaseID = 1;
            //IncomingWebRequestContext webRequestContext = (IncomingWebRequestContext)null;
            //Dictionary<string, string> dictionary = (Dictionary<string, string>)null;
            //try
            //{
            //    IncomingWebRequestContext incomingRequest = WebOperationContext.Current.IncomingRequest;
            //    if (incomingRequest != null && !string.IsNullOrEmpty(incomingRequest.Headers["HCITUserAgent"]))
            //    {
            //        dictionary = new Dictionary<string, string>();
            //        string header = incomingRequest.Headers["HCITUserAgent"];
            //        char[] chArray = new char[1] { ',' };
            //        foreach (string str in header.Split(chArray))
            //            dictionary.Add(str.Split(':').GetValue(0).ToString(), str.Split(':').GetValue(1).ToString());
            //    }
            //    if (dictionary == null)
            //        return;
            //    switch (dataBaseID)
            //    {
            //        case 1:
            //            this.connString = ConfigurationManager.ConnectionStrings[dictionary["MasterConKey"]].ConnectionString;
            //            break;
            //        case 2:
            //            this.connString = ConfigurationManager.ConnectionStrings[dictionary["TranConKey"]].ConnectionString;
            //            break;
            //        case 3:
            //            this.connString = ConfigurationManager.ConnectionStrings[dictionary["ReportConKey"]].ConnectionString;
            //            break;
            //        default:
            //            throw new ApplicationException("Invalid Database ID");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    File.WriteAllText("C:\\Temp\\DataHelperLog.txt", ex.Message + ex.StackTrace);
            //}
            //finally
            //{
            //    webRequestContext = (IncomingWebRequestContext)null;
            //}
        }

        private void LogParameters(
          string commandText,
          IDbDataParameter[] paramArray,
          DateTime startTime,
          DateTime endTime,
          long elapsedTicks,
          TimeSpan elapsedSpan)
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                if (elapsedSpan.Seconds <= this.trackToAfter)
                    return;
                if (paramArray != null)
                {
                    for (int index = 0; index <= paramArray.Length - 1; ++index)
                    {
                        if (paramArray[index] != null && paramArray[index].Value != null)
                            stringBuilder.AppendFormat(" {0} - {1} ", (object)paramArray[index].ParameterName, (object)paramArray[index].Value.ToString());
                    }
                }
                if (paramArray != null)
                    DataHelper.WritePerfLogText(stringBuilder.ToString(), startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), endTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), elapsedTicks, elapsedSpan, commandText.Trim().ToString(), paramArray.Length);
                else
                    DataHelper.WritePerfLogText(stringBuilder.ToString(), startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), endTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), elapsedTicks, elapsedSpan, commandText.Trim().ToString(), 0);
            }
            catch (Exception ex)
            {
                File.AppendAllText("C:\\Temp\\HISData.txt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Environment.NewLine + ex.Message + ex.StackTrace);
            }
            finally
            {
            }
        }

        public static void WritePerfLogText(
          string Log,
          string beforeTime,
          string afterTime,
          long elapsedTicks,
          TimeSpan elapsedSpan,
          string strCommandText,
          int intParamCount)
        {
            string logFilePath = DataHelper.GetLogFilePath("DBLogAnalyzer");
            string str1 = "[";
            string str2 = "]";
            if (string.IsNullOrEmpty(logFilePath))
                return;
            using (StreamWriter streamWriter = new StreamWriter(logFilePath, true))
                streamWriter.WriteLine(str1 + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + str2 + str1 + beforeTime + str2 + str1 + afterTime + str2 + str1 + (object)elapsedTicks + str2 + str1 + (object)elapsedSpan + str2 + str1 + strCommandText + str2 + str1 + (object)intParamCount + str2 + Log);
        }

        private static string GetLogFilePath(string strFileName)
        {
            string format = "dd.MM.yyyy";
            try
            {
                string str = string.IsNullOrEmpty(ConfigurationSettings.AppSettings[strFileName]) ? "C:\\Temp" : ConfigurationSettings.AppSettings[strFileName].ToString();
                string empty = string.Empty;
                object[] objArray = new object[8];
                objArray[0] = (object)str;
                objArray[1] = (object)"\\Trace\\";
                DateTime now = DateTime.Now;
                objArray[2] = (object)now.ToString(format);
                objArray[3] = (object)"_";
                now = DateTime.Now;
                objArray[4] = (object)now.Hour;
                objArray[5] = (object)"_";
                objArray[6] = (object)strFileName;
                objArray[7] = (object)"LogFile.txt";
                string path = string.Concat(objArray);
                if (!File.Exists(path))
                {
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(Path.GetDirectoryName(path)).Attributes = FileAttributes.Hidden | FileAttributes.Directory;
                    using (File.Create(path))
                        ;
                }
                return path;
            }
            catch
            {
                return string.Empty;
            }
        }

        private interface IActivator
        {
            IDbConnection CreateConnection();

            IDbCommand CreateCommand();

            IDbDataAdapter CreateDataAdapter();

            IDbDataParameter CreateDataParameter();
        }

        private class Activator
        {
            public DataHelper.IActivator CreateInstance(ProviderType provider)
            {
                switch (provider)
                {
                    case ProviderType.NotSet:
                        throw new DataHelper.ProviderNotSetException();
                    case ProviderType.OleDb:
                        return (DataHelper.IActivator)new DataHelper.OledbActivator();
                    case ProviderType.Sql:
                        return (DataHelper.IActivator)new DataHelper.SqlActivator();
                    
                    default:
                        throw new DataHelper.ProviderNotSetException();
                }
            }
        }

        private class OledbActivator : DataHelper.IActivator
        {
            public IDbConnection CreateConnection() => (IDbConnection)new OleDbConnection();

            public IDbCommand CreateCommand() => (IDbCommand)new OleDbCommand();

            public IDbDataAdapter CreateDataAdapter() => (IDbDataAdapter)new OleDbDataAdapter();

            public IDbDataParameter CreateDataParameter() => (IDbDataParameter)new OleDbParameter();
        }

        private class SqlActivator : DataHelper.IActivator
        {
            public IDbConnection CreateConnection() => (IDbConnection)new SqlConnection();

            public IDbCommand CreateCommand() => (IDbCommand)new SqlCommand();

            public IDbDataAdapter CreateDataAdapter() => (IDbDataAdapter)new SqlDataAdapter();

            public IDbDataParameter CreateDataParameter() => (IDbDataParameter)new SqlParameter();
        }

        private enum ConnectionOwnership
        {
            Internal,
            External,
        }

        private enum CommandState
        {
            LeaveOpen,
            Close,
        }

        private class ConnStrNotCachedException : CodeBasedException
        {
            public ConnStrNotCachedException()
              : base(101)
            {
            }
        }

        private class ConnStrSetOnceException : CodeBasedException
        {
            public ConnStrSetOnceException()
              : base(102)
            {
            }
        }

        private class ProviderNotSetException : CodeBasedException
        {
            public ProviderNotSetException()
              : base(103)
            {
            }
        }

        private class NullConnectionException : CodeBasedException
        {
            public NullConnectionException()
              : base(104)
            {
            }
        }

        private class NullTransactionException : CodeBasedException
        {
            public NullTransactionException()
              : base(105)
            {
            }
        }

        private class NullCommandException : CodeBasedException
        {
            public NullCommandException()
              : base(106)
            {
            }
        }

        private class NullConnectionStringException : CodeBasedException
        {
            public NullConnectionStringException()
              : base(107)
            {
            }
        }

        private class NullCommandTextException : CodeBasedException
        {
            public NullCommandTextException()
              : base(108)
            {
            }
        }

        private class SetConnectionToNullException : CodeBasedException
        {
            public SetConnectionToNullException()
              : base(109)
            {
            }
        }

        private class TransactionExpiredException : CodeBasedException
        {
            public TransactionExpiredException()
              : base(110)
            {
            }
        }

        private class NullParameterException : CodeBasedException
        {
            public NullParameterException()
              : base(111)
            {
            }
        }
    }
}