using System;

namespace SimonRolfe.Common
{
    public static class _Logging
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void LogFatal(string Fatal, Exception e = null)
        {
            if (log.IsFatalEnabled)
            {
                if (e == null)
                {
                    log.Fatal(Fatal);
                }
                else
                {
                    log.Fatal(Fatal, e);
                }
            }
        }

        public static void LogError(string Error, Exception e = null)
        {
            if (log.IsErrorEnabled)
            {
                if (e == null)
                {
                    log.Error(Error);
                }
                else
                {
                    log.Error(Error, e);
                }
            }
        }

        public static void LogWarning(string Warning)
        {
            if (log.IsWarnEnabled)
            {
                log.Warn(Warning);
            }
        }

        public static void LogInfo(string Info)
        {
            if (log.IsInfoEnabled)
            {
                log.Info(Info);
            }
        }

        public static void LogDebug(string Info, int Indent = 0)
        {
            if (log.IsDebugEnabled)
            {
                while (Indent > 0)
                {
                    Info = Info.Insert(0, "  ");
                    Indent--;
                }
                log.Debug(Info);
            }
        }
    }

}
