using System;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Represents errors which happen when connecting two endpoints.
    /// </summary>
    public sealed class MaximumAttemptsExceededException : Exception
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MaximumAttemptsExceededException"/> class.
        /// </summary>
        public MaximumAttemptsExceededException() : base(MESSAGE) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MaximumAttemptsExceededException"/> class.
        /// </summary>
        public MaximumAttemptsExceededException(String? auxMessage) : base(String.Format("{0} - {1}", MESSAGE, auxMessage)) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MaximumAttemptsExceededException"/> class.
        /// </summary>
        public MaximumAttemptsExceededException(String? auxMessage, Exception? inner) : base(String.Format("{0} - {1}", MESSAGE, auxMessage), inner) { }

        #endregion

        #region Constants

        private const String MESSAGE = "Maximum number of attempts when connecting to endpoint exceeded.";

        #endregion
    }
}
