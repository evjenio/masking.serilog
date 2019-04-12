using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Masking.Serilog.ByMasking
{
    internal class DestructureByMaskingPolicy : IDestructuringPolicy
    {
        private readonly IDictionary<Type, CachedProperties> cache = new Dictionary<Type, CachedProperties>();
        private readonly string mask = "******";
        private readonly IEnumerable<string> maskedProperties;
        private readonly object sync = new object();

        public DestructureByMaskingPolicy(params string[] maskedProperties)
        {
            this.maskedProperties = maskedProperties;
        }

        public DestructureByMaskingPolicy(MaskingOptions opts)
        {
            if (opts == null)
            {
                throw new ArgumentNullException(nameof(opts));
            }

            maskedProperties = opts.PropertyNames;
            mask = opts.Mask;
        }

        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            if (value == null || value is IEnumerable)
            {
                result = null;
                return false;
            }

            result = Structure(value, propertyValueFactory);

            return true;
        }

        private static LogEventPropertyValue BuildLogEventProperty(object o, ILogEventPropertyValueFactory propertyValueFactory)
        {
            return o == null ? new ScalarValue(null) : propertyValueFactory.CreatePropertyValue(o, true);
        }

        private static object SafeGetPropertyValue(object o, PropertyInfo pi)
        {
            try
            {
                if (pi.GetIndexParameters().Any())
                {
                    return null;
                }
                return pi.GetValue(o);
            }
            catch (TargetInvocationException ex)
            {
                SelfLog.WriteLine("The property accessor {0} threw exception {1}", pi, ex);
                return "The property accessor threw an exception: " + ex.InnerException.GetType().Name;
            }
        }

        private CachedProperties GetCachedProperties(Type type)
        {
            CachedProperties entry;
            lock (sync)
            {
                if (cache.TryGetValue(type, out entry))
                {
                    return entry;
                }
            }

            PropertyInfo[] includedProps = type.GetRuntimeProperties()
                .Where(p => p.CanRead)
                .Where(p => !ShouldMask(p))
                .ToArray();

            PropertyInfo[] maskedProps = type.GetRuntimeProperties()
                .Where(p => p.CanRead)
                .Where(p => ShouldMask(p))
                .ToArray();

            entry = new CachedProperties(includedProps, maskedProps);
            lock (sync)
            {
                cache[type] = entry;
            }

            return entry;
        }

        private bool ShouldMask(PropertyInfo p) => maskedProperties.Contains(p.Name, StringComparer.OrdinalIgnoreCase);

        private LogEventPropertyValue Structure(object o, ILogEventPropertyValueFactory propertyValueFactory)
        {
            var structureProperties = new List<LogEventProperty>();

            var type = o.GetType();
            var cached = GetCachedProperties(type);

            foreach (var p in cached.ToInclude)
            {
                var propertyValue = SafeGetPropertyValue(o, p);
                var logEventPropertyValue = BuildLogEventProperty(propertyValue, propertyValueFactory);
                structureProperties.Add(new LogEventProperty(p.Name, logEventPropertyValue));
            }

            foreach (var p in cached.ToMask)
            {
                var logEventPropertyValue = BuildLogEventProperty(mask, propertyValueFactory);
                structureProperties.Add(new LogEventProperty(p.Name, logEventPropertyValue));
            }

            return new StructureValue(structureProperties, type.Name);
        }

        private class CachedProperties
        {
            public CachedProperties(PropertyInfo[] toInclude, PropertyInfo[] toMask)
            {
                ToInclude = toInclude;
                ToMask = toMask;
            }

            public PropertyInfo[] ToInclude { get; }
            public PropertyInfo[] ToMask { get; }
        }
    }
}
