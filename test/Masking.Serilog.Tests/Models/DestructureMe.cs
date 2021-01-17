using System;

namespace Masking.Serilog.Tests.Models
{
    public class DestructureMe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Secret { get; set; }
        public static int StaticProp { get; set; }
    }
}
