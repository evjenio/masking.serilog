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
        private readonly IDictionary<Type, Properties> cache = new Dictionary<Type, Properties>();
        private readonly MaskingOptions maskingOptions = new MaskingOptions();        
        private readonly object sync = new object();

        public DestructureByMaskingPolicy(params string[] maskedProperties)
        {
            maskingOptions.PropertyNames.AddRange(maskedProperties);
        }

        public DestructureByMaskingPolicy(MaskingOptions opts)
        {
            maskingOptions = opts ?? throw new ArgumentNullException(nameof(opts));
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
                    SelfLog.WriteLine("The property {0} contains indexed values", pi);
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

        private static Properties GetProperties(Type type)
        {
            IEnumerable<PropertyInfo> typeProperties = type.GetRuntimeProperties()
                .Where(p => p.CanRead);
            
            var entry = new Properties(typeProperties.ToArray(), new PropertyInfo[]{});

            return entry;
        }
        
        private Properties GetCachedProperties(Type type)
        {
            Properties entry;
            
            lock (sync)
            {
                if (cache.TryGetValue(type, out entry))
                {
                    return entry;
                }
            }

            IEnumerable<PropertyInfo> typeProperties = type.GetRuntimeProperties()
                .Where(p => p.CanRead);

            if (maskingOptions.ExcludeStaticProperties)
            {
                typeProperties = typeProperties
                    .Where(p => !p.GetMethod.IsStatic);
            }
            
            PropertyInfo[] includedProps = typeProperties
                .Where(p => !ShouldMask(p))
                .ToArray();

            PropertyInfo[] maskedProps = typeProperties
                .Where(p => ShouldMask(p))
                .ToArray();

            entry = new Properties(includedProps, maskedProps);
            
            lock (sync)
            {
                cache[type] = entry;
            }

            return entry;
        }

        private bool ShouldMask(PropertyInfo p) => maskingOptions.PropertyNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase);
        
        private bool ShouldIgnoreMasking(Type t) => maskingOptions.IgnoredNamespaces.Contains(t.Namespace, StringComparer.OrdinalIgnoreCase);
        
        private LogEventPropertyValue Structure(object o, ILogEventPropertyValueFactory propertyValueFactory)
        {
            var structureProperties = new List<LogEventProperty>();

            var type = o.GetType();
            
            var properties = ShouldIgnoreMasking(type) ? GetProperties(type) : GetCachedProperties(type);

            foreach (var p in properties.ToInclude)
            {
                var propertyValue = SafeGetPropertyValue(o, p);
                var logEventPropertyValue = BuildLogEventProperty(propertyValue, propertyValueFactory);
                structureProperties.Add(new LogEventProperty(p.Name, logEventPropertyValue));
            }

            foreach (var p in properties.ToMask)
            {
                var logEventPropertyValue = BuildLogEventProperty(maskingOptions.Mask, propertyValueFactory);
                structureProperties.Add(new LogEventProperty(p.Name, logEventPropertyValue));
            }

            return new StructureValue(structureProperties, type.Name);
        }

        private class Properties
        {
            public Properties(PropertyInfo[] toInclude, PropertyInfo[] toMask)
            {
                ToInclude = toInclude;
                ToMask = toMask;
            }

            public PropertyInfo[] ToInclude { get; }
            public PropertyInfo[] ToMask { get; }
        }
    }
}
