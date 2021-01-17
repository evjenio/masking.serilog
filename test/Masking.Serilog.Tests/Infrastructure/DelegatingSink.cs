using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Masking.Serilog.Tests.Infrastructure
{
    public class DelegatingSink : ILogEventSink
    {
        readonly Action<LogEvent> write;

        public DelegatingSink(Action<LogEvent> write)
        {
            this.write = write ?? throw new ArgumentNullException(nameof(write));
        }

        public void Emit(LogEvent logEvent)
        {
            write(logEvent);
        }

        public static LogEvent GetLogEvent(Action<ILogger> writeAction)
        {
            LogEvent logEvent = null;
            var loggerConfiguration = new LoggerConfiguration()
                .WriteTo.Sink(new DelegatingSink(le => logEvent = le))
                .CreateLogger();

            writeAction(loggerConfiguration);
            return logEvent;
        }
    }
}
