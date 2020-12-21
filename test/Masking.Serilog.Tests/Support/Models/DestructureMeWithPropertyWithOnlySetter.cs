using System;

namespace Masking.Serilog.Tests.Support.Models
{
    public class DestructureMeWithPropertyWithOnlySetter
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
