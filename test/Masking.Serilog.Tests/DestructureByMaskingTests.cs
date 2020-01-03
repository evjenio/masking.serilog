using System;
using System.Linq;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Masking.Serilog.Tests.Support;
using System.Collections.Generic;

namespace Masking.Serilog.Tests
{
    [TestFixture]
    public class DestructureByMaskingTests
    {
        [Test]
        public void ClassWithAPropertyOnlyWithSetterDoesNotCrash()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.ByMaskingProperties("Name", "Password")
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var ignored = new DestructureMeWithPropertyWithOnlySetter
            {
                Id = 2,
                Name = "Name",
                Password = "Password"
            };

            log.Information("Here is {@Ignored}", ignored);

            Assert.IsTrue(true, "We did not throw!");
        }

        [Test]
        public void ComplexTypesAreMaskedWhenDestructuring()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.ByMaskingProperties(opts =>
                {
                    opts.PropertyNames.Add("Hash");
                    opts.Mask = "*removed*";
                })
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var ignored = new Complex
            {
                HashData = new DestructMe
                { Hash = 1234 }
            };

            log.Information("Here is {@Ignored}", ignored);

            var props = GetPropsFromEvent("Ignored", evt);
            var hashData = ((StructureValue)props["HashData"]).Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.AreEqual("*removed*", hashData["Hash"].LiteralValue());
        }

        [Test]
        public void PropertyNamesAreMaskedWhenDestructuring()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.ByMaskingProperties("password", "secret")
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            DestructureMe.StaticProp = 1337;

            var ignored = new DestructureMe
            {
                Id = 2,
                Name = "Name",
                Password = "Password",
                Secret = 25673433
            };

            log.Information("Here is {@Ignored}", ignored);

            var props = GetPropsFromEvent("Ignored", evt);

            Assert.AreEqual(2, props[nameof(DestructureMe.Id)].LiteralValue());
            Assert.AreEqual("Name", props[nameof(DestructureMe.Name)].LiteralValue());
            Assert.AreEqual("******", props[nameof(DestructureMe.Password)].LiteralValue());
            Assert.AreEqual("******", props[nameof(DestructureMe.Secret)].LiteralValue());
            Assert.AreEqual(1337, props[nameof(DestructureMe.StaticProp)].LiteralValue());
        }

        [Test]
        public void PropertyNamesAreMaskedWhenDestructuringStruct()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.ByMaskingProperties("HASH")
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var ignored = new DestructMe
            {
                Id = 2,
                Hash = 25673433
            };

            log.Information("Here is {@Ignored}", ignored);

            var props = GetPropsFromEvent("Ignored", evt);

            Assert.AreEqual(2, props[nameof(DestructMe.Id)].LiteralValue());
            Assert.AreEqual("******", props[nameof(DestructMe.Hash)].LiteralValue());
        }

        [Test]
        public void IntIndexedPropertyNamesDoesNotBreakWhenDestructuring()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.ByMaskingProperties("password", "secret")
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var data = new IntIndexed();
            data[0] = "boo";

            log.Information("Here is {@data}", data);

            var props = GetPropsFromEvent("data", evt);

            Assert.IsNull(props["Item"].LiteralValue());
        }

        [Test]
        public void StringIndexedPropertyNamesDoesNotBreakWhenDestructuring()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.ByMaskingProperties("password", "secret")
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var data = new StringIndexed();
            data["woo"] = "boo";

            log.Information("Here is {@data}", data);

            var props = GetPropsFromEvent("data", evt);

            Assert.IsNull(props["Item"].LiteralValue());
        }

        [Test]
        public void PropertyNamesOfTypesInCollectionsAreMaskedWhenDestructuring()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.ByMaskingProperties("password", "secret")
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var ignored = new[] {
                new DestructureMe
                {
                    Id = 2,
                    Name = "Name",
                    Password = "Password",
                    Secret = 25673433
                }
            };

            log.Information("Here is {@Ignored}", ignored);

            var props = GetPropsFromEvent("Ignored", evt);

            Assert.AreEqual(2, props[nameof(DestructureMe.Id)].LiteralValue());
            Assert.AreEqual("Name", props[nameof(DestructureMe.Name)].LiteralValue());
            Assert.AreEqual("******", props[nameof(DestructureMe.Password)].LiteralValue());
            Assert.AreEqual("******", props[nameof(DestructureMe.Secret)].LiteralValue());
        }

        [Test]
        public void ValuesOfStaticPropertiesAreNotIncluded()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.ByMaskingProperties(opts =>
                {
                    opts.ExcludeStaticProperties = true;
                    opts.PropertyNames.AddRange(new[] { "password", "secret" });
                })
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            DestructureMe.StaticProp = 1337;

            var ignored = new DestructureMe
            {
                Id = 2,
                Name = "Name",
                Password = "Password",
                Secret = 25673433
            };

            log.Information("Here is {@Ignored}", ignored);

            var props = GetPropsFromEvent("Ignored", evt);

            Assert.AreEqual(2, props[nameof(DestructureMe.Id)].LiteralValue());
            Assert.AreEqual("Name", props[nameof(DestructureMe.Name)].LiteralValue());
            Assert.AreEqual("******", props[nameof(DestructureMe.Password)].LiteralValue());
            Assert.AreEqual("******", props[nameof(DestructureMe.Secret)].LiteralValue());
            Assert.IsFalse(props.ContainsKey(nameof(DestructureMe.StaticProp)), $"{nameof(props)} contains the key {nameof(DestructureMe.StaticProp)}.");
        }

        private static Dictionary<string, LogEventPropertyValue> GetPropsFromEvent(string name, LogEvent evt)
        {
            Dictionary<string, LogEventPropertyValue> result = null;

            if (evt.Properties[name] is StructureValue structureValue)
            {
                result = structureValue.Properties.ToDictionary(p => p.Name, p => p.Value);
            }
            else if (evt.Properties[name] is SequenceValue sequenceValue)
            {
                result = sequenceValue.Elements
                    .OfType<StructureValue>()
                    .SelectMany(v => v.Properties)
                    .ToDictionary(p => p.Name, p => p.Value);
            }

            return result;
        }

        private struct DestructMe
        {
            public int Id { get; set; }
            public int Hash { get; set; }
        }

        private class Complex
        {
            public DestructMe HashData { get; set; }
        }

        private class DestructureMe
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Password { get; set; }
            public int Secret { get; set; }
            public static int StaticProp { get; set; }
        }

        private class StringIndexed
        {
            public string this[string index]
            {
                get { return indexed[index]; }
                set { indexed[index] = value; }
            }
            private Dictionary<string, string> indexed = new Dictionary<string, string>();
        }

        private class IntIndexed
        {
            public string this[int index]
            {
                get { return indexed[index]; }
                set { indexed[index] = value; }
            }
            private string[] indexed = new string[20];
        }

        private class DestructureMeWithPropertyWithOnlySetter
        {
            private string onlySetter;
            public int Id { get; set; }
            public string Name { get; set; }

            public string OnlySetter
            {
                set => onlySetter = value;
            }

            public string Password { get; set; }
        }
    }
}
