using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using SimonRolfe.Common;

namespace SimonRolfe.Data.Oracle
{
    public static class OracleParameterExtensions
    {
        public static dynamic ToNative(this OracleParameter param)
        {
            return DataAccess.OracleDBTypeToNative(param);
        }

        public static dynamic ToNative(this DataRow Row, DataColumn Column)
        {
            return DataAccess.OracleDBTypeToNative(Row, Column);
        }

        public static dynamic ToNative(this DataRow Row, string ColumnName)
        {
            return DataAccess.OracleDBTypeToNative(Row, ColumnName);
        }
    }
    /// <summary>
    /// A set of globally accessible static members for basic data access tasks. 
    /// Rather than creating your own versions of these, please subclass this and/or add new functionality.
    /// </summary>
    public class DataAccess
    {
        /// <summary>
        /// This is the current maximum size of BLOB to fetch
        /// </summary>
        private const int Max_Blob_Size = (1024 * 1024 * 10);

        /// <summary>
        /// Consts related to Oracle errors we special-case
        /// </summary>
        private const int Oracle_Error_Package_Recompile = 4068;
        private const int Oracle_Error_Lost_Connection = 4069;

        /// <summary>
        /// Returns a connection string from the "Main" ConnectionStrings entry in web.config.
        /// </summary>
        /// <param name="ConnectionStringName">An optional ConnectionStrings web.config entry to use. Defaults to "Main".</param>
        /// <returns>The connection string.</returns>
        /// 
        public static string ConnectionString(string ConnectionStringName = "Main")
        {
            return ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;
        }

        /// <summary>
        /// Executes the specified Oracle command, returning a single piece of data.
        /// </summary>
        /// <typeparam name="DataType">The data type to return</typeparam>
        /// <param name="cmd">The Oracle command to execute, including all parameters. If the command name does not include a package specifier, the value of the public constant Package_Name_Prefix will be used.</param>
        /// <param name="columnName">The column name of the requested data. If not specified, the column number will be used. If neither are specified, the first column will be returned.</param>
        /// <param name="columnNumber">The column number of the requested data, if no column name is specified. If neither are specified, the first column will be returned.</param>
        /// <param name="ConnectionStringName">An optional ConnectionStrings web.config entry to use. Defaults to "Main".</param>
        /// <returns>A single item of the specified data type, containing the requested data.</returns>
        public static DataType FetchSingleItem<DataType>(OracleCommand cmd, string columnName = "", int columnNumber = 0, string ConnectionStringName = "Main")
        {
            using (DataSet ds = FetchDataSet(cmd, ConnectionStringName: ConnectionStringName))
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    if (string.IsNullOrWhiteSpace(columnName))
                    {
                        return ds.Tables[0].Rows[0].Field<DataType>(columnNumber);
                    }
                    else
                    {
                        return ds.Tables[0].Rows[0].Field<DataType>(columnName);
                    }
                }

                else
                {
                    return default(DataType);
                }
            }
        }

        /// <summary>
        /// Executes the specified Oracle command, returning a list with all rows of the data.
        /// </summary>
        /// <typeparam name="DataType">The data type of the list to return</typeparam>
        /// <param name="cmd">The Oracle command to execute, including all parameters. If the command name does not include a package specifier, the value of the public constant Package_Name_Prefix will be used.</param>
        /// <param name="columnName">The column name of the requested data. If not specified, the column number will be used. If neither are specified, the first column will be returned.</param>
        /// <param name="columnNumber">The column number of the requested data, if no column name is specified. If neither are specified, the first column will be returned.</param>
        /// <param name="ConnectionStringName">An optional ConnectionStrings web.config entry to use. Defaults to "Main".</param>
        /// <returns>A list of the specified data type, containing the requested data.</returns>
        public static List<DataType> FetchList<DataType>(OracleCommand cmd, string columnName = "", int columnNumber = 0, string ConnectionStringName = "Main")
        {
            using (DataSet ds = FetchDataSet(cmd, ConnectionStringName: ConnectionStringName))
            {
                List<DataType> lstData = new List<DataType>();

                if (ds.Tables[0].Rows.Count > 0)
                {
                    if (string.IsNullOrWhiteSpace(columnName))
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            lstData.Add(row.Field<DataType>(columnNumber));
                        }
                    }
                    else
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            lstData.Add(row.Field<DataType>(columnName));
                        }
                    }
                }

                return lstData;
            }
        }

      

        /// <summary>
        /// Executes the specified Oracle command, returning a filled data set for the first returned table.
        /// </summary>
        /// <param name="cmd">The Oracle command to execute, including all parameters. If the command name does not include a package specifier, the value of the public constant Package_Name_Prefix will be used.</param>
        /// <param name="ConnectionStringName">An optional ConnectionStrings web.config entry to use. Defaults to "Main".</param>
        /// <returns>The data set requested.</returns>
        public static DataSet FetchDataSet(OracleCommand cmd, string ConnectionStringName = "Main")
        {

            _Logging.LogInfo("FetchDataSet called using command " + cmd.CommandText + ".");
            if (_Logging.log.IsDebugEnabled)
            {
                _Logging.log.DebugFormat("{0} parameters:", cmd.Parameters.Count);
                foreach (OracleParameter p in cmd.Parameters)
                {
                    _Logging.log.DebugFormat("  Name: {0}. Value: {1}", p.ParameterName, p.Value);
                }
                _Logging.log.DebugFormat("Fetch size: {0}.", DataAccessConfig.OracleFetchSize);
            }

            using (OracleConnection con = new OracleConnection(ConnectionString(ConnectionStringName)))
            {
                if (con == null)
                {
                    throw new Exception("Could not connect to database with Connection string " + ConnectionString(ConnectionStringName));
                }

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = con;
                cmd.FetchSize = DataAccessConfig.OracleFetchSize;

                con.Open();
                _Logging.LogDebug("Connection open.");
                if (DataAccessConfig.Debug.DataSet)
                {
                    _Logging.LogDebug("Using dataset debug mode.");
                    try
                    {
                        DataSet dsDebug = DebugFillDataSet(cmd);
                        return dsDebug;
                    }
                    catch (OracleException DebugDBError)
                    {
                        //handle case when packages are recompiled, which causes a transient error
                        if (DebugDBError.Number == Oracle_Error_Package_Recompile || DebugDBError.Number == Oracle_Error_Lost_Connection)
                        {
                            try
                            {
                                if (_Logging.log.IsDebugEnabled)
                                {
                                    if (DebugDBError.Number == Oracle_Error_Package_Recompile)
                                    {
                                        _Logging.LogDebug(string.Format("(Debug) Command failed (Error {0}: Package recompile), retrying.", Oracle_Error_Package_Recompile));
                                    }
                                    else
                                    {
                                        _Logging.LogDebug(string.Format("(Debug) Command failed (Error {0}: Lost connection), retrying.", Oracle_Error_Lost_Connection));
                                    }
                                }
                                DataSet dsDebugRetry = DebugFillDataSet(cmd);
                                return dsDebugRetry;
                            }
                            catch (Exception Debuge2)
                            {
                                _Logging.LogError("(Debug) Caught error after retry.", Debuge2);
                                throw Debuge2;
                            }
                        }

                        _Logging.LogError("(Debug) Caught Oracle error not related to package recompilation.", DebugDBError);
                        throw DebugDBError;
                    }
                    catch (Exception Debugex)
                    {
                        _Logging.LogError("(Debug) Caught non-Oracle error.", Debugex);
                        throw Debugex;
                    }
                    finally
                    {
                        _Logging.LogDebug("(Debug) Closing connection.");
                        con.Close();
                    }
                }
                else
                {
                    try
                    {
                        DataSet dsCommandResults = new DataSet();
                        using (OracleDataAdapter commandAdapter = new OracleDataAdapter(cmd))
                        {
                            _Logging.LogDebug("Data Adapter created.");
                            commandAdapter.Fill(dsCommandResults);

                            if (_Logging.log.IsDebugEnabled)
                            {
                                _Logging.log.DebugFormat("Data set filled, {0} tables.", dsCommandResults.Tables.Count);
                                foreach (DataTable tLog in dsCommandResults.Tables)
                                {
                                    _Logging.log.DebugFormat("  Table name '{0}' has {1} rows.", tLog.TableName, tLog.Rows.Count);
                                }
                            }
                        }
                        return dsCommandResults;
                    }
                    catch (OracleException DBError)
                    {
                        //handle case when packages are recompiled or connection is lost, which causes a transient error
                        if (DBError.Number == Oracle_Error_Package_Recompile || DBError.Number == Oracle_Error_Lost_Connection)
                        {
                            try
                            {
                                if (_Logging.log.IsDebugEnabled)
                                {
                                    if (DBError.Number == Oracle_Error_Package_Recompile)
                                    {
                                        _Logging.LogDebug(string.Format("Command failed (Error {0}: Package recompile), retrying.", Oracle_Error_Package_Recompile));
                                    }
                                    else
                                    {
                                        _Logging.LogDebug(string.Format("Command failed (Error {0}: Lost connection), retrying.", Oracle_Error_Lost_Connection));
                                    }
                                }
                                DataSet dsCommandResults = new DataSet();
                                using (OracleDataAdapter commandAdapter = new OracleDataAdapter(cmd))
                                {
                                    _Logging.LogDebug("Retried Data Adapter created.");
                                    commandAdapter.Fill(dsCommandResults);
                                    if (_Logging.log.IsDebugEnabled)
                                    {
                                        _Logging.log.DebugFormat("Retried Data set filled, {0} tables.", dsCommandResults.Tables.Count);
                                        foreach (DataTable tLog in dsCommandResults.Tables)
                                        {
                                            _Logging.log.DebugFormat("  Table name '{0}' has {1} rows.", tLog.TableName, tLog.Rows.Count);
                                        }
                                    }
                                }
                                return dsCommandResults;
                            }
                            catch (Exception e2)
                            {
                                _Logging.LogError("Caught error after retry.", e2);
                                throw e2;
                            }
                        }

                        _Logging.LogError("Caught Oracle error not related to package recompilation.", DBError);
                        throw DBError;
                    }
                    catch (Exception ex)
                    {
                        _Logging.LogError("Caught non-Oracle error.", ex);
                        throw ex;
                    }
                    finally
                    {
                        _Logging.LogDebug("Closing connection.");
                        con.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Executes the specified Oracle command, returning a filled data table for the first returned table.
        /// </summary>
        /// <param name="cmd">The Oracle command to execute, including all parameters.</param>
        /// <returns>The data set requested.</returns>
        public static DataTable FetchDataTable(OracleCommand cmd)
        {
            using (DataSet ds = FetchDataSet(cmd))
            {
                if (ds.Tables.Count == 0)
                {
                    return null;
                }
                else
                {
                    return ds.Tables[0];
                }
            }
        }

        /// <summary>
        /// Executes the specified Oracle command, returning an OracleDataReader.
        /// </summary>
        /// <param name="cmd">The Oracle command to execute, including all parameters. If the command name does not include a package specifier, the value of the public constant Package_Name_Prefix will be used.</param>
        /// <param name="ConnectionStringName">An optional ConnectionStrings web.config entry to use. Defaults to "Main".</param>
        /// <returns>The data reader requested.</returns>
        public static OracleDataReader FetchDataReader(OracleCommand cmd, OracleConnection con = null, string ConnectionStringName = "Main")
        {
            _Logging.LogInfo("FetchDataReader called using command " + cmd.CommandText + ".");
            if (_Logging.log.IsDebugEnabled)
            {
                _Logging.log.DebugFormat("{0} parameters:", cmd.Parameters.Count);
                foreach (OracleParameter p in cmd.Parameters)
                {
                    _Logging.log.DebugFormat("  Name: {0}. Value: {1}", p.ParameterName, p.Value);
                }
                _Logging.log.DebugFormat("Fetch size: {0}.", DataAccessConfig.OracleFetchSize);
            }

            if (con == null)
            {
                con = new OracleConnection(ConnectionString(ConnectionStringName));
                if (con == null)
                {
                    throw new Exception("Could not connect to database with Connection string " + ConnectionString(ConnectionStringName));
                }
            }

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = con;
            cmd.FetchSize = DataAccessConfig.OracleFetchSize;

            con.Open();
            _Logging.LogDebug("Connection opened.");
            try
            {
                OracleDataReader rdr = cmd.ExecuteReader();
                _Logging.LogDebug("DataReader created.");
                return rdr;
            }
            catch (OracleException DBError)
            {
                //handle case when packages are recompiled or connection is lost, which causes a transient error
                if (DBError.Number == Oracle_Error_Package_Recompile || DBError.Number == Oracle_Error_Lost_Connection)
                {
                    try
                    {
                        if (_Logging.log.IsDebugEnabled)
                        {
                            if (DBError.Number == Oracle_Error_Package_Recompile)
                            {
                                _Logging.LogDebug(string.Format("Command failed (Error {0}: Package recompile), retrying.", Oracle_Error_Package_Recompile));
                            }
                            else
                            {
                                _Logging.LogDebug(string.Format("Command failed (Error {0}: Lost connection), retrying.", Oracle_Error_Lost_Connection));
                            }
                        }
                        OracleDataReader rdr2 = cmd.ExecuteReader();
                        _Logging.LogDebug("Retried DataReader created.");

                        return rdr2;
                    }
                    catch (Exception e2)
                    {
                        _Logging.LogError("Caught error after retry.", e2);

                        con.Close();
                        throw e2;
                    }
                }
                _Logging.LogError("Caught Oracle error not related to package recompilation.", DBError);
                throw DBError;
            }
            catch (Exception Debugex)
            {
                _Logging.LogError("Caught non-Oracle error.", Debugex);
                _Logging.LogDebug("Closing connection.");
                con.Close();
                throw Debugex;
            }
        }

        /// <summary>
        /// Executes an Oracle command, discarding any returned data (but populating output parameters).
        /// </summary>
        /// <param name="cmd">The Oracle command to execute, including all parameters. If the command name does not include a package specifier, the value of the public constant Package_Name_Prefix will be used.</param>
        /// <param name="KeepConnection">If true, this keeps the command's connection open. Should only be used if the calling code is happy to manage closing and disposing of the connection, for example when accessing LOB data.</param>
        /// <param name="ConnectionStringName">An optional ConnectionStrings web.config entry to use. Defaults to "Main".</param>
        public static void ExecuteNonQuery(OracleCommand cmd, bool KeepConnection = false, string ConnectionStringName = "Main")
        {
            if (KeepConnection)
            {
                OracleConnection con = new OracleConnection(ConnectionString(ConnectionStringName));
                if (con == null)
                {
                    throw new Exception("Could not connect to database with Connection string " + ConnectionString(ConnectionStringName));
                }

                try
                {
                    ExecuteCommandWithConnection(cmd, con);
                }
                catch (Exception ex)
                {
                    con.Close();
                    con.Dispose();
                    con = null;
                    throw ex;
                }
            }
            else
            {
                using (OracleConnection con = new OracleConnection(ConnectionString(ConnectionStringName)))
                {
                    if (con == null)
                    {
                        throw new Exception("Could not connect to database with Connection string " + ConnectionString(ConnectionStringName));
                    }
                    try
                    {
                        ExecuteCommandWithConnection(cmd, con);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }
        }

        private static void ExecuteCommandWithConnection(OracleCommand cmd, OracleConnection con)
        {
            _Logging.LogInfo("ExecuteCommandWithConnection called from ExecuteNonQuery using command " + cmd.CommandText + ".");
            if (_Logging.log.IsDebugEnabled)
            {
                _Logging.log.DebugFormat("{0} parameters:", cmd.Parameters.Count);
                foreach (OracleParameter p in cmd.Parameters)
                {
                    _Logging.log.DebugFormat("  Name: {0}. Value: {1}.", p.ParameterName, p.Value);
                }
                _Logging.log.DebugFormat("Fetch size: {0}.", DataAccessConfig.OracleFetchSize);
            }

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = con;
            cmd.FetchSize = DataAccessConfig.OracleFetchSize;

            con.Open();
            _Logging.LogDebug("Connection open.");
            try
            {
                cmd.ExecuteNonQuery();
                _Logging.LogDebug("Command executed.");
            }
            catch (OracleException DBError)
            {
                //handle case when packages are recompiled or connection is lost, which causes a transient error
                if (DBError.Number == Oracle_Error_Package_Recompile || DBError.Number == Oracle_Error_Lost_Connection)
                {
                    try
                    {
                        if (_Logging.log.IsDebugEnabled)
                        {
                            if (DBError.Number == Oracle_Error_Package_Recompile)
                            {
                                _Logging.LogDebug(string.Format("Command failed (Error {0}: Package recompile), retrying.", Oracle_Error_Package_Recompile));
                            }
                            else
                            {
                                _Logging.LogDebug(string.Format("Command failed (Error {0}: Lost connection), retrying.", Oracle_Error_Lost_Connection));
                            }
                        }
                        cmd.ExecuteNonQuery();
                        _Logging.LogDebug("Command retried.");
                    }
                    catch (Exception e2)
                    {
                        throw e2;
                    }
                }

                throw DBError;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Helper function to add a number of same-type, output parameters to a command quickly.
        /// </summary>
        /// <param name="cmd">The Oracle command to add parameters to</param>
        /// <param name="dataType">The Oracle data type of the parameters to add.</param>
        /// <param name="paramNames">The parameter names to add.</param>
        public static void AddMultipleOutputParams(OracleCommand cmd, OracleDbType dataType, int size, params string[] paramNames)
        {
            foreach (string paramName in paramNames)
            {
                OracleParameter prm;

                if (size == 0)
                {
                    prm = new OracleParameter(paramName, dataType);
                }
                else
                {
                    prm = new OracleParameter(paramName, dataType, size);
                }
                prm.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(prm);
            }
        }

        /// <summary>
        /// Helper function to add a number of same-type, output parameters to a command quickly.
        /// </summary>
        /// <param name="cmd">The Oracle command to add parameters to</param>
        /// <param name="dataType">The Oracle data type of the parameters to add.</param>
        /// <param name="paramNames">The parameter names to add.</param>
        public static void AddMultipleInputParams(OracleCommand cmd, OracleDbType dataType, params string[] paramNames)
        {
            foreach (string param in paramNames)
            {
                cmd.Parameters.Add(param, dataType, ParameterDirection.Input);
            }
        }

        public static Guid OracleGUID(byte[] value)
        {
            if (value == null)
            {
                return Guid.Empty;
            }
            else
            {
                return new Guid(value);
            }
        }

        public static byte[] OracleGUID(Guid value)
        {
            if (value == Guid.Empty)
            {
                return null;
            }
            else
            {
                return value.ToByteArray();
            }
        }

        public static int OracleBool(bool value)
        {
            return value ? 1 : 0;
        }

        public static bool OracleBool(object value)
        {
            string TypeName = value.GetType().Name;

            switch (TypeName)
            {
                case "OracleDecimal":
                    return OracleBool(((OracleDecimal)value).Value);
                case "Integer":
                    return OracleBool((int)value);
                case "Decimal":
                    return OracleBool((decimal)value);
                case "String":
                    return OracleBool((string)value);
                case "OracleString":
                    return OracleBool(((OracleString)value).Value);
                default:
                    throw new NotImplementedException("OracleDecimal, Integer, Decimal, String, and OracleString are the only supported types at the moment.");
            }
        }

        public static bool OracleBool(int value)
        {
            return (value == 1);
        }

        public static bool OracleBool(long value)
        {
            return (value == 1);
        }

        public static bool OracleBool(decimal value)
        {
            return (value == 1);
        }

        public static bool OracleBool(string value)
        {
            return (value == "1");
        }

        /// <summary>
        /// Attempts to match an Oracle Parameter Collection's values with properties of an object, by matching on name.
        /// </summary>
        /// <remarks>p_ and po_ prefixes and underscores are ignored for the purposes of the name check.</remarks>
        /// <param name="outputObject">Any object. Public writeable properties of the object are checked.</param>
        /// <param name="commandParams">A collection of command parameters, typically OracleCommand.Parameters.</param>
        public static void SetObjectPropertiesFromParams(object outputObject, OracleParameterCollection commandParams)
        {
            foreach (OracleParameter param in commandParams)
            {
                if (param.Direction == ParameterDirection.Output && param.Status != OracleParameterStatus.NullFetched)
                {
                    //properties are case-sensitive, but don't re-use names: this is a case-insensitive compare
                    string ParamName = param.ParameterName;
                    if (ParamName.StartsWith("p_"))
                    {
                        ParamName = ParamName.Remove(0, "p_".Length);
                    }
                    if (ParamName.StartsWith("po_"))
                    {
                        ParamName = ParamName.Remove(0, "po_".Length);
                    }

                    ParamName = ParamName.Replace("_", "").ToLower();
                    foreach (System.Reflection.PropertyInfo prop in outputObject.GetType().GetProperties())
                    {
                        if (prop.Name.ToLower() == ParamName)
                        {
                            if (prop.CanWrite)
                            {
                                dynamic nativeValue = OracleDBTypeToNative(param);

                                //special case handling for booleans and guids
                                switch (prop.PropertyType.Name.ToLower())
                                {
                                    case "boolean":
                                        prop.SetValue(outputObject, OracleBool((int)nativeValue), null);
                                        break;
                                    case "guid":
                                        prop.SetValue(outputObject, OracleGUID((byte[])nativeValue), null);
                                        break;
                                    default:
                                        prop.SetValue(outputObject, nativeValue, null);
                                        break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        public static void GetParamsFromObjectProperties(object inputObject, OracleParameterCollection commandParams)
        {
            foreach (OracleParameter param in commandParams)
            {
                if (param.Direction == ParameterDirection.Input)
                {
                    //properties are case-sensitive in c#, which is a pain for these sorts of functions.
                    //this is a case-insensitive compare, so don't reuse property names unless you want problems.
                    string ParamName = param.ParameterName.Replace("_", "").ToLower();
                    foreach (System.Reflection.PropertyInfo prop in inputObject.GetType().GetProperties())
                    {
                        if (prop.Name.ToLower() == ParamName)
                        {
                            if (prop.CanRead)
                            {
                                param.Value = prop.GetValue(inputObject, null);
                            }
                            break;
                        }
                    }
                }
            }
        }

        public static dynamic OracleDBTypeToNative(DataRow row, string ColumnName)
        {
            return OracleDBTypeToNative(row, row.Table.Columns[ColumnName]);
        }

        public static dynamic OracleDBTypeToNative(DataRow row, DataColumn Column)
        {
            if (Column.AllowDBNull && row.IsNull(Column))
            {
                return null;
            }
            else
            {
                switch (Column.DataType.Name)
                {
                    case "Boolean":
                        return row.Field<Boolean>(Column);
                    case "Byte":
                        return row.Field<Byte>(Column);
                    case "Char":
                        return row.Field<Char>(Column);
                    case "DateTime":
                        return row.Field<DateTime>(Column);
                    case "Decimal":
                        return row.Field<Decimal>(Column);
                    case "Double":
                        return row.Field<Double>(Column);
                    case "Int16":
                        return row.Field<Int16>(Column);
                    case "Int32":
                        return row.Field<Int32>(Column);
                    case "Int64":
                        return row.Field<Int64>(Column);
                    case "SByte":
                        return row.Field<SByte>(Column);
                    case "Single":
                        return row.Field<Single>(Column);
                    case "String":
                        return row.Field<String>(Column);
                    case "TimeSpan":
                        return row.Field<TimeSpan>(Column);
                    case "UInt16":
                        return row.Field<UInt16>(Column);
                    case "UInt32":
                        return row.Field<UInt32>(Column);
                    case "UInt64":
                        return row.Field<UInt64>(Column);
                    default:
                        throw new NotImplementedException("Type not handled yet");
                }
            }

        }

        public static dynamic OracleDBTypeToNative(OracleParameter param)
        {
            switch (param.OracleDbTypeEx)
            {
                case OracleDbType.Array:
                case OracleDbType.BFile:
                case OracleDbType.BinaryDouble:
                case OracleDbType.BinaryFloat:
                case OracleDbType.Ref:
                case OracleDbType.RefCursor:
                case OracleDbType.XmlType:
                    return param.Value;
                case OracleDbType.LongRaw:
                case OracleDbType.Raw:
                case OracleDbType.Blob:
                    if (param.Value == null)
                    {
                        return default(byte[]);
                    }
                    else
                    {
                        /*TODO: this is an extremely naive implementation, and will eat RAM if large BLOBS are used.
                            *      Currently, I'm limiting at 10MB of data (defined in a const inside _Common.cs), 
                            *      and raising an error if this limit is exceeded. */
                        byte[] BlobBytes;

                        OracleBlob Blob = (OracleBlob)param.Value;
                        if (Blob.Length < Max_Blob_Size)
                        {
                            BlobBytes = new byte[Blob.Length];
                            Blob.Read(BlobBytes, 0, (int)Blob.Length);
                            return BlobBytes;
                        }
                        else
                        {
                            throw new NotSupportedException("This function will return a maximum of " + Max_Blob_Size + " bytes to avoid excessive RAM consumption.");
                        }
                    }

                //this case will probably never work, so I may as well ignore it
                /*case OracleDbType.Byte:
                    if(param.Value == null)
                    {
                        return default(byte);
                    }
                    else
                    {
                        return (byte)param.Value;
                    }*/
                case OracleDbType.Char:
                case OracleDbType.NChar:
                case OracleDbType.NVarchar2:
                case OracleDbType.Varchar2:
                    OracleString paramValueString = (OracleString)param.Value;
                    if (paramValueString == null || paramValueString.IsNull)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return paramValueString.Value;
                    }

                case OracleDbType.Clob:
                case OracleDbType.NClob:
                    if (param.Value == null)
                    {
                        return default(string);
                    }
                    else
                    {
                        return ((OracleClob)param.Value).Value;
                    }

                case OracleDbType.Date:
                    OracleDate paramValueDate = (OracleDate)param.Value;
                    if (paramValueDate == null || paramValueDate.IsNull)
                    {
                        return default(DateTime);
                    }
                    else
                    {
                        return paramValueDate.Value;
                    }
                case OracleDbType.IntervalDS:
                    if (param.Value == null)
                    {
                        return default(TimeSpan);
                    }
                    else
                    {
                        return ((OracleIntervalDS)param.Value).Value;
                    }
                case OracleDbType.IntervalYM:
                    if (param.Value == null)
                    {
                        return default(TimeSpan);
                    }
                    else
                    {
                        return ((OracleIntervalYM)param.Value).Value;
                    }
                case OracleDbType.TimeStamp:
                    if (param.Value == null)
                    {
                        return default(DateTime);
                    }
                    else
                    {
                        return ((OracleTimeStamp)param.Value).Value;
                    }
                case OracleDbType.TimeStampLTZ:
                    if (param.Value == null)
                    {
                        return default(DateTime);
                    }
                    else
                    {
                        return ((OracleTimeStampLTZ)param.Value).Value;
                    }
                case OracleDbType.TimeStampTZ:
                    if (param.Value == null)
                    {
                        return default(DateTime);
                    }
                    else
                    {
                        return ((OracleTimeStampTZ)param.Value).Value;
                    }

                case OracleDbType.Int16:
                case OracleDbType.Int32:
                    OracleDecimal paramValueInt32 = (OracleDecimal)param.Value;
                    if (paramValueInt32 == null || paramValueInt32.IsNull)
                    {
                        return default(int);
                    }
                    else
                    {
                        return paramValueInt32.ToInt32();
                    }
                case OracleDbType.Int64:
                    OracleDecimal paramValueInt64 = (OracleDecimal)param.Value;
                    if (paramValueInt64 == null || paramValueInt64.IsNull)
                    {
                        return default(Int64);
                    }
                    else
                    {
                        return paramValueInt64.ToInt64();
                    }
                case OracleDbType.Decimal:
                    OracleDecimal paramValueDecimal = (OracleDecimal)param.Value;
                    if (paramValueDecimal == null || paramValueDecimal.IsNull)
                    {
                        return default(decimal);
                    }
                    else
                    {
                        return paramValueDecimal.Value;
                    }
                case OracleDbType.Double:
                case OracleDbType.Single: //we don't care internally about single.
                    if (param.Value == null)
                    {
                        return default(double);
                    }
                    else
                    {
                        return ((OracleDecimal)param.Value).ToDouble();
                    }

                default:
                    throw new NotImplementedException("Type not handled yet");
            }
        }

        private static DataSet DebugFillDataSet(OracleCommand cmd)
        {
            _Logging.LogDebug("Entered DebugFillDataSet.");
            OracleDataReader rdr = cmd.ExecuteReader();
            _Logging.LogDebug("Executed command and returned DataReader.", 1);
            DataSet ds = new DataSet("DebugSet");
            DataTable dt = ds.Tables.Add("DebugTable");
            _Logging.LogDebug("Created Debug DataSet and DataTable.", 1);
            for (int i = 0; i < rdr.FieldCount; i++)
            {
                _Logging.LogDebug(string.Format("Column {0}: {1}, Type {2}.", i, rdr.GetName(i), rdr.GetDataTypeName(i)), 2);
                dt.Columns.Add(rdr.GetName(i), rdr.GetFieldType(i));
            }

            while (rdr.Read())
            {
                DataRow dr = dt.NewRow();
                _Logging.LogDebug(string.Format("Created row {0}, Size {1}.", dt.Rows.Count, rdr.RowSize), 1);
                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    _Logging.LogDebug(string.Format("Setting column {0} ({1}) to {2}.", col, dt.Columns[col].ColumnName, rdr.GetValue(col)), 2);
                    dr.SetField(dt.Columns[col], rdr.GetValue(col));
                }
                dt.Rows.Add(dr);
                _Logging.LogDebug(string.Format("Added row {0} to table.", dt.Rows.Count), 1);
            }

            return ds;
        }
    }
}
