namespace nyingi.Kafa.Reader;

public static class ColParser
{
    public static bool? ParseBool(this KafaReader.Col col)
    {
        return bool.Parse(col.Value);
    }
    public static int? ParseInt(this KafaReader.Col col, IFormatProvider? formatProvider=null)
    {
        return int.Parse(col.Value, formatProvider);
    }

    public static long? ParseLong(this KafaReader.Col col, IFormatProvider? formatProvider=null)
    {
        return long.Parse(col.Value, formatProvider);
    }

    public static float? ParseFloat(this KafaReader.Col col, IFormatProvider? formatProvider=null)
    {
        return float.Parse(col.Value, formatProvider);
    }

    public static double? ParseDouble(this KafaReader.Col col, IFormatProvider? formatProvider=null)
    {
        return double.Parse(col.Value, formatProvider);
    }

    public static decimal? ParseDecimal(this KafaReader.Col col, IFormatProvider? formatProvider=null)
    {
        return decimal.Parse(col.Value, formatProvider);
    }

    public static DateTime? ParseDateTime(this KafaReader.Col col, IFormatProvider? formatProvider=null)
    {
        return DateTime.Parse(col.Value, formatProvider);
    }

    public static DateTimeOffset? ParseDateTimeOffSet(this KafaReader.Col col, IFormatProvider? formatProvider=null)
    {
        return DateTimeOffset.Parse(col.Value, formatProvider);
    }

    public static Guid? ParseGuid(this KafaReader.Col col, IFormatProvider? formatProvider=null)
    {
        return Guid.Parse(col.Value, formatProvider);
    }
}