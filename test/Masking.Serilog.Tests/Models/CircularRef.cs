using System;

namespace Masking.Serilog.Tests.Models
{
    public class CircularRefA
    {
        public CircularRefA(CircularRefB child)
        {
            Child = child;
        }

        public CircularRefB Child { get; }
    }

    public class CircularRefB
    {
        public CircularRefB(CircularRefA child)
        {
            Child = child;
        }

        public CircularRefA Child { get; }
    }
}
