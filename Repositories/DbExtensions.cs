using System.Data;

namespace RotaDoSabor.Server.Repositories;

public static class DbExtensions
{
    public static int GetInt32(this IDataRecord reader, string column) => reader.GetInt32(reader.GetOrdinal(column));
    public static long GetInt64(this IDataRecord reader, string column) => reader.GetInt64(reader.GetOrdinal(column));
    public static string GetString(this IDataRecord reader, string column) => reader.GetString(reader.GetOrdinal(column));
    public static string GetSafeString(this IDataRecord reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }
    public static bool GetBoolean(this IDataRecord reader, string column) => reader.GetBoolean(reader.GetOrdinal(column));
    public static Guid GetGuid(this IDataRecord reader, string column) => reader.GetGuid(reader.GetOrdinal(column));
    public static Guid? GetNullableGuid(this IDataRecord reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetGuid(ordinal);
    }
    public static decimal GetDecimal(this IDataRecord reader, string column) => reader.GetDecimal(reader.GetOrdinal(column));
    public static decimal? GetNullableDecimal(this IDataRecord reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
    }
    public static DateTime GetDateTime(this IDataRecord reader, string column) => reader.GetDateTime(reader.GetOrdinal(column));
    public static DateTime? GetNullableDateTime(this IDataRecord reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }
}
