using System;

namespace Masking.Serilog.Tests.Support.Models
{
    public class IntIndexed
    {
        public string this[int index]
        {
            get { return indexed[index]; }
            set { indexed[index] = value; }
        }
        private string[] indexed = new string[20];
    }
}
