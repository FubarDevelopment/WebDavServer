using System;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;

using NH = NHibernate;

namespace FubarDev.WebDavServer.Sample.AspNetCore.NhSupport
{
    public class NHibernateLoggerFactory : NH.INHibernateLoggerFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public NHibernateLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public NH.INHibernateLogger LoggerFor(string keyName)
        {
            return new NHibernateLogger(_loggerFactory.CreateLogger(keyName));
        }

        public NH.INHibernateLogger LoggerFor(Type type)
        {
            return new NHibernateLogger(_loggerFactory.CreateLogger(type));
        }

        private class NHibernateLogger : NH.INHibernateLogger
        {
            private readonly ILogger _logger;

            public NHibernateLogger(ILogger logger)
            {
                _logger = logger;
            }

            public void Log(NH.NHibernateLogLevel logLevel, NH.NHibernateLogValues state, Exception exception)
            {
                _logger.Log(
                    GetLogLevel(logLevel),
                    0,
                    new FormattedLogValues(state.Format, state.Args),
                    exception,
                    MessageFormatter);
            }

            public bool IsEnabled(NH.NHibernateLogLevel logLevel)
            {
                return _logger.IsEnabled(GetLogLevel(logLevel));
            }

            private static string MessageFormatter(object state, Exception error)
            {
                return state.ToString();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static LogLevel GetLogLevel(NH.NHibernateLogLevel logLevel)
            {
                switch (logLevel)
                {
                    case NH.NHibernateLogLevel.Trace:
                        return LogLevel.Trace;
                    case NH.NHibernateLogLevel.Debug:
                        return LogLevel.Debug;
                    case NH.NHibernateLogLevel.Info:
                        return LogLevel.Information;
                    case NH.NHibernateLogLevel.Warn:
                        return LogLevel.Warning;
                    case NH.NHibernateLogLevel.Error:
                        return LogLevel.Error;
                    case NH.NHibernateLogLevel.Fatal:
                        return LogLevel.Critical;
                }

                return LogLevel.None;
            }
        }
    }
}
