using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Contains the <see cref="System.Exception"/> that occured during runtime.
    /// </summary>
    public sealed class ExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEventArgs"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public ExceptionEventArgs([DisallowNull] Exception exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            this.Exception = exception;
        }

        /// <summary>
        /// Gets the <see cref="System.Exception"/> wrapped in this <see cref="EventArgs"/>.
        /// </summary>
        [NotNull]
        [Pure]
        public Exception Exception { get; }
    }
}
