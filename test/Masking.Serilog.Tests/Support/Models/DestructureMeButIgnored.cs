using System;

namespace Masking.Serilog.Tests.Support.Models.Ignore
{
    public class DestructureMeButIgnored
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Secret { get; set; }
    }
}
