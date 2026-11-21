using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using System;
using System.Diagnostics;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace WinFrmTalk
{
    public static class LogHelper
    {
        // 2026-08-26 方案A：多进程日志隔离
        // 5个PC端进程同时运行时，日志文件名加入进程ID，避免多进程写同一日志文件互相覆盖/竞争
        private static string GetProcessId()
        {
            try
            {
                return Process.GetCurrentProcess().Id.ToString();
            }
            catch (Exception)
            {
                return "0";
            }
        }

        static LogHelper()
        {
            try
            {
                // 动态配置日志文件路径（含进程ID），覆盖 App.config 中的静态配置
                var repository = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
                var rootLogger = repository.Root;

                // 移除原有 appender
                rootLogger.RemoveAllAppenders();

                var appender = new RollingFileAppender();
                appender.Name = "RollingLogFileAppender";
                appender.File = @"C:\temp\ShikuLog\";
                appender.AppendToFile = true;
                appender.ImmediateFlush = true;
                appender.RollingStyle = RollingFileAppender.RollingMode.Date;
                appender.DatePattern = "yyyy-MM-dd_HH_'" + GetProcessId() + "'.log";
                appender.StaticLogFileName = false;
                appender.MaxSizeRollBackups = 100;

                var layout = new PatternLayout();
                layout.ConversionPattern = "[%date]  %message%newline";
                layout.ActivateOptions();
                appender.Layout = layout;

                appender.ActivateOptions();
                rootLogger.AddAppender(appender);
                rootLogger.Level = Level.All;
                repository.Configured = true;
            }
            catch (Exception)
            {
                // 动态配置失败时退回默认配置（App.config）
            }
        }

        public static ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region 记录异常
        /// <summary>
        /// 记录异常
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        public static void LogException(string message, string stackTrace)
        {
            log.Debug(string.Format("Exception occured: {0}, stack trace: {1}", message, stackTrace));
        }
        #endregion

        #region 记录日志
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="info"></param>
        public static void LogInfo(string info)
        {
            log.Info(info);
        }
        #endregion
    }
}
