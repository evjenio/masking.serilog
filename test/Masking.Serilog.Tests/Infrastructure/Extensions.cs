using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Events;

namespace Masking.Serilog.Tests.Infrastructure
{
    public static class Extensions
    {
        public static object LiteralValue(this LogEventPropertyValue @this)
        {
            return ((ScalarValue)@this).Value;
        }

        public static Dictionary<string, LogEventPropertyValue> GetProps(this LogEvent evt, string name)
        {
            Dictionary<string, LogEventPropertyValue> result = evt.Properties[name] switch
            {
                StructureValue structureValue => structureValue.Properties.ToDictionary(p => p.Name, p => p.Value),
                SequenceValue sequenceValue => sequenceValue.Elements.OfType<StructureValue>().SelectMany(v => v.Properties).ToDictionary(p => p.Name, p => p.Value),
                _ => null
            };

            return result;
        }
    }
}
