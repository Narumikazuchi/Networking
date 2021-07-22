using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

namespace Narumikazuchi.Networking
{
    /// <summary>
    /// Represents a standadized MAC-Address.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public readonly struct MacAddress : IEquatable<MacAddress>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="MacAddress"/> instance.
        /// </summary>
        /// <param name="address">The raw bytes of the address.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public MacAddress(in Byte[] address)
        {
            if (address is null)
            {
                throw new ArgumentNullException(nameof(address));
            }
            if (address.Length != ADDRESSLENGTH)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "The address doesn't have the correct format for a MAC address.");
            }
            this._address = address;
        }

        #endregion

        #region Parsing

        /// <summary>
        /// Parses the specified input string into a <see cref="MacAddress"/> object.
        /// </summary>
        /// <param name="macAddress">The string to parse.</param>
        /// <param name="address">A <see cref="MacAddress"/> representing the input string.</param>
        /// <returns><see langword="true"/> if the parsing succeeded; else <see langword="false"/></returns>
        public static Boolean TryParse(String? macAddress, out MacAddress? address)
        {
            address = ParseInternal(macAddress, true);
            return address is not null;
        }
        /// <summary>
        /// Parses the specified input string into a <see cref="MacAddress"/> object.
        /// </summary>
        /// <param name="macAddress">The string to parse.</param>
        /// <returns>A <see cref="MacAddress"/> representing the input string.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
#pragma warning disable
        public static MacAddress Parse([DisallowNull] String macAddress) => ParseInternal(macAddress, false);
#pragma warning restore

        private static MacAddress ParseInternal(String? macAddress, Boolean tryParse)
        {
            if (macAddress is null)
            {
                return tryParse ? default : throw new ArgumentNullException(nameof(macAddress));
            }

            if (macAddress.IndexOf(':') > -1)
            {
                String[] segments = macAddress.Split(':');
                if (segments.Length != ADDRESSLENGTH)
                {
                    return tryParse ? default : throw new FormatException("The address doesn't have the correct format for a MAC address.");
                }

                Byte[] bytes = new Byte[ADDRESSLENGTH];
                for (Int32 i = 0; i < ADDRESSLENGTH; i++)
                {
                    if (!Byte.TryParse(segments[i], NumberStyles.HexNumber, null, out Byte b))
                    {
                        return tryParse ? default : throw new FormatException("The address doesn't have the correct format for a MAC address.");
                    }
                    bytes[i] = b;
                }

                return new MacAddress(bytes);
            }
            else
            {
                if (macAddress.Length != ADDRESSLENGTH * 2)
                {
                    return tryParse ? default : throw new FormatException("The address doesn't have the correct format for a MAC address.");
                }

                Byte[] bytes = new Byte[ADDRESSLENGTH];
                for (Int32 i = 0; i < ADDRESSLENGTH; i++)
                {
                    String sub = macAddress.Substring(i * 2, 2);
                    if (!Byte.TryParse(sub, NumberStyles.HexNumber, null, out Byte b))
                    {
                        return tryParse ? default : throw new FormatException("The address doesn't have the correct format for a MAC address.");
                    }
                    bytes[i] = b;
                }

                return new MacAddress(bytes);
            }
        }

        #endregion

        #region ToBytes

        /// <summary>
        /// Returns this object as an array of bytes.
        /// </summary>
        /// <returns>An array of bytes.</returns>
        [Pure]
        public Byte[] ToBytes()
        {
            Byte[] result = new Byte[ADDRESSLENGTH];
            Array.Copy(this._address, result, ADDRESSLENGTH);
            return result;
        }

        #endregion

        #region ToString

        /// <inheritdoc/>
        [Pure]
        public override String ToString() => String.Join(':', this._address);

        #endregion

        #region Equality

        /// <inheritdoc/>
        [Pure]
        public override Int32 GetHashCode() => this._address.Sum(b => b.GetHashCode());

        /// <inheritdoc/>
        [Pure]
        public override Boolean Equals(Object? obj) => obj is MacAddress other && this.Equals(other);

        #endregion

        #region IEquatable

        /// <inheritdoc/>
        [Pure]
        public Boolean Equals(MacAddress other)
        {
            for (Int32 i = 0; i < ADDRESSLENGTH; i++)
            {
                if (this._address[i] != other._address[i])
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Operators

#pragma warning disable
        public static Boolean operator ==(MacAddress? left, MacAddress? right) => 
            left is null 
                ? right is null 
                : left.Equals(right);

        public static Boolean operator !=(MacAddress? left, MacAddress? right) =>
            left is null
                ? right is not null
                : !left.Equals(right);
#pragma warning restore

        #endregion

        #region Fields

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Byte[] _address;

        #endregion

        #region Constants

#pragma warning disable
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const Int32 ADDRESSLENGTH = 6;
#pragma warning restore

        #endregion
    }
}
