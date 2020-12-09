using System;
using System.Collections.Generic;

namespace Masking.Serilog.Tests.Support.Models
{
    public class StringIndexed
    {
        public string this[string index]
        {
            get { return indexed[index]; }
            set { indexed[index] = value; }
        }
        private Dictionary<string, string> indexed = new Dictionary<string, string>();
    }
}