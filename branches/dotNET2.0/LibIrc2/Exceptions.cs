using System;
using System.Collections.Generic;
using System.Text;

namespace NielsRask.LibIrc
{
    public class ConnectionRefusedException : Exception
    {
        public ConnectionRefusedException(string message) : base(message)
        { }

        public ConnectionRefusedException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}
