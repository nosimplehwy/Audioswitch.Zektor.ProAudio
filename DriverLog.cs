using System;
using Crestron.RAD.Common.Logging;
using Crestron.SimplSharp;

namespace AudioSwitchZektorProAudio
{
    public class DriverLog
    {

        public static void Log(bool logEnabled, Action<string> logMethod, LoggingLevel logLevel, string methodName, string message)
        {
            var logMessage = $"{logLevel}: {methodName} - {message}";

            if (logEnabled)
            {
                switch (logLevel)
                {
                    case LoggingLevel.Error:
                        logMethod(logMessage);
                        ErrorLog.Error(logMessage);
                        break;

                    case LoggingLevel.Warning:
                        logMethod(logMessage);
                        ErrorLog.Warn(logMessage);
                        break;

                    case LoggingLevel.Debug:
                        logMethod(logMessage);
                        ErrorLog.Notice(logMessage);
                        break;
                }
            }

        }
    }
}