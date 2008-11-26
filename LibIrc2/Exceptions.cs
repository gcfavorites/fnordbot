using System;

namespace NielsRask.LibIrc
{
    /// <summary>
    /// 
    /// </summary>
    public class ConnectionRefusedException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public ConnectionRefusedException(string message) : base(message)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ConnectionRefusedException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}
