namespace Narumikazuchi.Networking;

/// <summary>
/// Represents a standardized MAC-Address.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public readonly partial struct MacAddress
{
    /// <summary>
    /// Initializes a new <see cref="MacAddress"/> instance.
    /// </summary>
    public MacAddress()
    {
        m_Address = new Byte[ADDRESSLENGTH];
        m_Hashcode = 0;
        m_IntegerValue = 0;
        m_StringValue = "00:00:00:00:00:00";
    }

    /// <summary>
    /// Initializes a new <see cref="MacAddress"/> instance.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public MacAddress([DisallowNull] Byte[] address)
    {
        ArgumentNullException.ThrowIfNull(address);

        if (address.Length != ADDRESSLENGTH)
        {
            throw new ArgumentOutOfRangeException(paramName: nameof(address),
                                                  message: WRONG_LENGTH);
        }
        m_Address = address;
        m_Hashcode = m_Address.Sum(b => b.GetHashCode());
        m_StringValue = String.Join(separator: ':',
                                    values: m_Address.Select(b => b.ToString("X2")));

        Int64 myValue = 0L;
        for (Int32 i = 0; i < ADDRESSLENGTH; i++)
        {
            myValue += (Int64)m_Address[^(i + 1)] << (i * 8);
        }
        m_IntegerValue = myValue;
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
        Array.Copy(sourceArray: m_Address,
                   destinationArray: result,
                   length: ADDRESSLENGTH);
        return result;
    }

    /// <inheritdoc/>
    [Pure]
    public override Int32 GetHashCode() =>
        m_Hashcode;

    /// <inheritdoc/>
    [Pure]
    public override Boolean Equals([AllowNull] Object? obj) =>
        (obj is MacAddress other &&
        this.Equals(other)) ||
        (obj is Byte[] address &&
        this.Equals(address));

    /// <inheritdoc/>
    [Pure]
    [return: NotNull]
    public override String ToString() =>
        m_StringValue;
}

// Non-Public
partial struct MacAddress
{
    private static MacAddress ParseInternal(String macAddress)
    {
        ArgumentNullException.ThrowIfNull(macAddress);

        if (!s_Regex.IsMatch(input: macAddress))
        {
            throw new FormatException(INCORRECT_FORMAT);
        }

        Match match = s_Regex.Match(input: macAddress);
        String raw = match.Value
                          .Replace(oldValue: " ", newValue: "")
                          .Replace(oldValue: "-", newValue: "")
                          .Replace(oldValue: ":", newValue: "");

        Byte[] bytes = new Byte[ADDRESSLENGTH];
        for (Int32 i = 0; i < ADDRESSLENGTH; i++)
        {
            String segment = raw.Substring(startIndex: i * 2, 
                                           length: 2);
            bytes[i] = Byte.Parse(s: segment, 
                                  style: NumberStyles.HexNumber,
                                  provider: null);
        }
        return new(bytes);
    }

    // Matches the first group (any digit or letter a-f * 2) exactly 5 times and then another time, but here without the optional seperators (-, :)
    private static readonly Regex s_Regex = new(@"(?:\s*[\dA-Fa-f][\dA-Fa-f]\s*[:-]?){5}\s*[\dA-Fa-f][\dA-Fa-f]\s*");
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Byte[] m_Address;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Int32 m_Hashcode;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly String m_StringValue;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Int64 m_IntegerValue;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const Int32 ADDRESSLENGTH = 6;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String WRONG_LENGTH = "The address does not have the correct length for a MAC address.";
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const String INCORRECT_FORMAT = "The address does not have the correct format for a MAC address.";
}

// IComparable
partial struct MacAddress : IComparable
{
    /// <inheritdoc/>
    [Pure]
    Int32 IComparable.CompareTo(Object? other)
    {
        if (other is MacAddress address)
        {
            return this.CompareTo(address);
        }
        return -1;
    }
}

// IComparable<T>
partial struct MacAddress : IComparable<MacAddress>
{
    /// <inheritdoc/>
    [Pure]
    public Int32 CompareTo(MacAddress other) =>
        m_IntegerValue.CompareTo(value: other.m_IntegerValue);
}

// IComparisonOperators<T, U>
partial struct MacAddress : IComparisonOperators<MacAddress, MacAddress>
{
#pragma warning disable CS1591
    [Pure]
    public static Boolean operator >(MacAddress left, MacAddress right)
    {
        return left.CompareTo(right) > 0;
    }

    [Pure]
    public static Boolean operator >=(MacAddress left, MacAddress right)
    {
        return left.CompareTo(right) >= 0;
    }

    [Pure]
    public static Boolean operator <(MacAddress left, MacAddress right)
    {
        return left.CompareTo(right) < 0;
    }

    [Pure]
    public static Boolean operator <=(MacAddress left, MacAddress right)
    {
        return left.CompareTo(right) <= 0;
    }
#pragma warning restore
}

// IEqualityOperators<T, U>
partial struct MacAddress : IEqualityOperators<MacAddress, MacAddress>
{
#pragma warning disable CS1591
    [Pure]
    public static Boolean operator ==(MacAddress left, MacAddress right)
    {
        return left.Equals(right);
    }

    [Pure]
    public static Boolean operator !=(MacAddress left, MacAddress right)
    {
        return !left.Equals(right);
    }
#pragma warning restore
}

// IEquatable<T>
partial struct MacAddress : IEquatable<MacAddress>
{
    /// <inheritdoc/>
    [Pure]
    public Boolean Equals(MacAddress other)
    {
        if (m_Address is null &&
            other.m_Address is null)
        {
            return true;
        }
        if ((m_Address is not null &&
            other.m_Address is null) ||
            (m_Address is null &&
            other.m_Address is not null))
        {
            return false;
        }
        for (Int32 i = 0;
             i < m_Address!.Length; 
             i++)
        {
            if (m_Address[i] != other.m_Address![i])
            {
                return false;
            }
        }
        return true;
    }
}

// IParseable<T>
partial struct MacAddress : IParseable<MacAddress>
{
    /// <summary>
    /// Parses the specified input string into a <see cref="MacAddress"/> object.
    /// </summary>
    /// <param name="macAddress">The string to parse.</param>
    /// <param name="provider">An object that supplies culture-specific information about the format of <paramref name="macAddress"/>. If it's <see langword="null"/> then the current threads culture will be used.</param>
    /// <returns>A <see cref="MacAddress"/> representing the input string.</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="FormatException"/>
    public static MacAddress Parse([DisallowNull] String macAddress,
                                   [AllowNull] IFormatProvider? provider) =>
        ParseInternal(macAddress);

    /// <summary>
    /// Parses the specified input string into a <see cref="MacAddress"/> object.
    /// </summary>
    /// <param name="macAddress">The string to parse.</param>
    /// <param name="provider">An object that supplies culture-specific information about the format of <paramref name="macAddress"/>. If it's <see langword="null"/> then the current threads culture will be used.</param>
    /// <param name="address">A <see cref="MacAddress"/> representing the input string.</param>
    /// <returns><see langword="true"/> if the parsing succeeded; else <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"/>
    public static Boolean TryParse([NotNullWhen(true)] String? macAddress,
                                   [AllowNull] IFormatProvider? provider,
                                   out MacAddress address)
    {
        if (macAddress is null)
        {
            address = default;
            return false;
        }

        try
        {
            address = ParseInternal(macAddress);
            return true;
        }
        catch
        {
            address = default;
            return false;
        }
    }
}