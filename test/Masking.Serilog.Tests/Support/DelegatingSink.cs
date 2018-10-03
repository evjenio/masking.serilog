using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Masking.Serilog.Tests.Support
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
            LogEvent result = null;
            var l = new LoggerConfiguration()
                .WriteTo.Sink(new DelegatingSink(le => result = le))
                .CreateLogger();

            writeAction(l);
            return result;
        }
    }
}
