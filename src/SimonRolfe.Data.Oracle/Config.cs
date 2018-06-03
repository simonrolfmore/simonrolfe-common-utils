
using System.Configuration;

namespace SimonRolfe.Common
{
    public static class DataAccessConfig
    {
        private static int _OracleFetchSize = 0;
        private static bool _DebugDataSetHasValue = false;
        private static bool _DebugDataSet = false;
        public static int OracleFetchSize 
        { 
            get 
            {
                if (_OracleFetchSize == 0 && !int.TryParse(ConfigurationManager.AppSettings["DataAccess_Oracle_FetchSize"], out _OracleFetchSize))
                {
                    _OracleFetchSize = 4096;
                }

                return _OracleFetchSize;
            } 
        } 
        public static class Debug
        {
            public static bool DataSet 
            { 
                get 
                {
                    if (!_DebugDataSetHasValue)
                    {
                        bool.TryParse(ConfigurationManager.AppSettings["DataAccess_Debug_DataSet"], out _DebugDataSet);
                    }
                    return _DebugDataSet;
                } 
            }
        }
    }
}
