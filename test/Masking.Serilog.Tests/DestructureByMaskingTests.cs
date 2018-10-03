using System;
using System.Linq;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Masking.Serilog.Tests.Support;

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

            var sv = (StructureValue)evt.Properties["Ignored"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);
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

            var ignored = new DestructureMe
            {
                Id = 2,
                Name = "Name",
                Password = "Password",
                Secret = 25673433
            };

            log.Information("Here is {@Ignored}", ignored);

            var sv = (StructureValue)evt.Properties["Ignored"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.AreEqual(2, props["Id"].LiteralValue());
            Assert.AreEqual("Name", props["Name"].LiteralValue());
            Assert.AreEqual("******", props["Password"].LiteralValue());
            Assert.AreEqual("******", props["Secret"].LiteralValue());
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

            var sv = (StructureValue)evt.Properties["Ignored"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.AreEqual(2, props["Id"].LiteralValue());
            Assert.AreEqual("******", props["Hash"].LiteralValue());
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
