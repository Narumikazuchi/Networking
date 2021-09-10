using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Represents errors which happen when connecting two endpoints.
    /// </summary>
    public sealed partial class MaximumAttemptsExceededException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaximumAttemptsExceededException"/> class.
        /// </summary>
        public MaximumAttemptsExceededException() : 
            base(MESSAGE) 
        { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MaximumAttemptsExceededException"/> class.
        /// </summary>
        public MaximumAttemptsExceededException([AllowNull] String? auxMessage) : 
            base(String.Format("{0} - {1}", 
                               MESSAGE, 
                               auxMessage)) 
        { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MaximumAttemptsExceededException"/> class.
        /// </summary>
        public MaximumAttemptsExceededException([AllowNull] String? auxMessage,
                                                [AllowNull] Exception? inner) : 
            base(String.Format("{0} - {1}", 
                               MESSAGE, 
                               auxMessage), 
                inner) 
        { }
    }

    // Non-Public
    partial class MaximumAttemptsExceededException
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const String MESSAGE = "Maximum number of attempts when connecting to endpoint exceeded.";
    }
}
