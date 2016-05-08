using Microsoft.AspNet.Identity;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SilentAuction.Common
{
    public enum LogLevel
    {
        Trace=0,
        Debug=1,
        Info=2,
        Warn=4,
        Error=8
    }

    public static class AppLog
    {
        private static NLog.Logger _logger;

        public static NLog.Logger logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = NLog.LogManager.GetCurrentClassLogger();
                }
                return _logger;
            }
        }


        public static void Debug(string text)
        {
            LogData data = new LogData(LogLevel.Debug, text);
            Log(data);
        }

        public static void Error(string text)
        {
            LogData data = new LogData(LogLevel.Error, text);
            Log(data);
        }
        public static void Info(string text)
        {
            LogData data = new LogData(LogLevel.Info, text);
            Log(data);
        }

        public static void Trace(string text)
        {
            LogData data = new LogData(LogLevel.Trace, text);
            Log(data);
        }
        public static void Warn(string text)
        {
            LogData data = new LogData(LogLevel.Warn, text);
            Log(data);
        }

        // TODO
        // Massive potential for re-entrancy and corrupt logs
        public static void Log(LogData data)
        {
            string logMessage = data.ToString();
            switch (data.Level)
            {
                case LogLevel.Debug:
                    logger.Debug(logMessage);
                    break;
                case LogLevel.Error:
                    logger.Error(logMessage);
                    break;
                case LogLevel.Info:
                    logger.Info(logMessage);
                    break;
                case LogLevel.Trace:
                    logger.Trace(logMessage);
                    break;
                case LogLevel.Warn:
                    logger.Warn(logMessage);
                    break;
            }
        }
    }
}