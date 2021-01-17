using System;
using Serilog.Events;

namespace Masking.Serilog.Tests.Infrastructure
{
    public static class Extensions
    {
        public static object LiteralValue(this LogEventPropertyValue @this)
        {
            return ((ScalarValue)@this).Value;
        }
    }
}
