namespace Narumikazuchi.Networking;

/// <summary>
/// Represents a standadized MAC-Address.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public readonly partial struct MacAddress
{
    /// <summary>
    /// Initializes a new <see cref="MacAddress"/> instance.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public MacAddress([DisallowNull] Byte[] address)
    {
        if (address is null)
        {
            throw new ArgumentNullException(nameof(address));
        }
        if (address.Length != ADDRESSLENGTH)
        {
            throw new ArgumentOutOfRangeException(nameof(address),
                                                  WRONG_LENGTH);
        }
        this._address = address;
    }

    /// <summary>
    /// Returns this object as an array of bytes.
    /// </summary>
    /// <returns>An array of bytes.</returns>
    [Pure]
    [return: NotNull]
    public Byte[] ToBytes()
    {
        Byte[] result = new Byte[ADDRESSLENGTH];
        Array.Copy(this._address,
                   result,
                   ADDRESSLENGTH);
        return result;
    }

    /// <inheritdoc/>
    [Pure]
    public override Int32 GetHashCode() =>
        this._address.Sum(b => b.GetHashCode());

    /// <inheritdoc/>
    [Pure]
    public override Boolean Equals([AllowNull] Object? obj) =>
        (obj is MacAddress other &&
        this.Equals(other)) ||
        (obj is Byte[] address &&
        this.Equals(address));

    /// <inheritdoc/>
    [Pure]
    [return: MaybeNull]
    public override String? ToString() =>
        String.Join(':',
                    this._address);
}

// Static

partial struct MacAddress
{
    /// <summary>
    /// Parses the specified input string into a <see cref="MacAddress"/> object.
    /// </summary>
    /// <param name="macAddress">The string to parse.</param>
    /// <returns>A <see cref="MacAddress"/> representing the input string.</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="FormatException"/>
    public static MacAddress Parse([DisallowNull] String macAddress) =>
        ParseInternal(macAddress);

    /// <summary>
    /// Parses the specified input string into a <see cref="MacAddress"/> object.
    /// </summary>
    /// <param name="macAddress">The string to parse.</param>
    /// <param name="address">A <see cref="MacAddress"/> representing the input string.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; else <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"/>
    public static Boolean TryParse([DisallowNull] String macAddress,
                                   [NotNullWhen(true)] out MacAddress? address)
    {
        try
        {
            address = ParseInternal(macAddress);
            return address is not null;
        }
        catch
        {
            address = null;
            return false;
        }
    }
}

// Non-Public
partial struct MacAddress
{
    private static MacAddress ParseInternal(String? macAddress)
    {
        if (macAddress is null)
        {
            throw new ArgumentNullException(nameof(macAddress));
        }

        String raw = String.Empty;

        foreach (Char c in macAddress)
        {
            if (Char.IsWhiteSpace(c))
            {
                continue;
            }
            raw += c;
        }

        if (raw.IndexOf(':') > -1)
        {
            String[] segments = raw.Split(':');
            if (segments.Length != ADDRESSLENGTH)
            {
                throw new FormatException(WRONG_LENGTH);
            }

            Byte[] bytes = new Byte[ADDRESSLENGTH];
            for (Int32 i = 0; i < ADDRESSLENGTH; i++)
            {
                if (!Byte.TryParse(segments[i],
                                   NumberStyles.HexNumber,
                                   null,
                                   out Byte b))
                {
                    throw new FormatException(INCORRECT_FORMAT);
                }
                bytes[i] = b;
            }

            return new(bytes);
        }
        else
        {
            if (raw.Length != ADDRESSLENGTH * 2)
            {
                throw new FormatException();
            }

            Byte[] bytes = new Byte[ADDRESSLENGTH];
            for (Int32 i = 0; i < ADDRESSLENGTH; i++)
            {
                String sub = raw.Substring(i * 2,
                                           2);
                if (!Byte.TryParse(sub,
                                   NumberStyles.HexNumber,
                                   null,
                                   out Byte b))
                {
                    throw new FormatException(INCORRECT_FORMAT);
                }
                bytes[i] = b;
            }

            return new(bytes);
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Byte[] _address;

#pragma warning disable

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const Int32 ADDRESSLENGTH = 6;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String WRONG_LENGTH = "The address does not have the correct length for a MAC address.";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String INCORRECT_FORMAT = "The address does not have the correct format for a MAC address.";

#pragma warning restore
}

partial struct MacAddress : IEquatable<MacAddress>
{
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

#pragma warning disable

    [Pure]
    public static Boolean operator ==([AllowNull] MacAddress? left, [AllowNull] MacAddress? right) =>
        left is null
            ? right is null
            : left.Equals(right);

    [Pure]
    public static Boolean operator !=([AllowNull] MacAddress? left, [AllowNull] MacAddress? right) =>
        left is null
            ? right is not null
            : !left.Equals(right);

#pragma warning restore
}

partial struct MacAddress : IEquatable<Byte[]>
{
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"/>
    [Pure]
    public Boolean Equals(Byte[]? other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other));
        }
        if (other.Length != ADDRESSLENGTH)
        {
            throw new ArgumentOutOfRangeException(nameof(other),
                                                  WRONG_LENGTH);
        }

        return this._address.SequenceEqual(other);
    }

#pragma warning disable

    [Pure]
    public static Boolean operator ==([AllowNull] MacAddress? left, [AllowNull] Byte[]? right) =>
        left is null
            ? right is null
            : left.Equals(right);

    [Pure]
    public static Boolean operator ==([AllowNull] Byte[]? left, [AllowNull] MacAddress? right) =>
        left is null
            ? right is null
            : right.Equals(left);

    [Pure]
    public static Boolean operator !=([AllowNull] MacAddress? left, [AllowNull] Byte[]? right) =>
        left is null
            ? right is not null
            : !left.Equals(right);

    [Pure]
    public static Boolean operator !=([AllowNull] Byte[]? left, [AllowNull] MacAddress? right) =>
        left is null
            ? right is not null
            : !right.Equals(left);

#pragma warning restore
}