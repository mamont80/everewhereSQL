using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Data.Common;
using ParserCore.Expr.Sql;

namespace ParserCore
{
    public static class ParserDbUtils
    {
        public static void AddParam(DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }
        public static HashSet<string> CreateInvariantStringSet(IEnumerable<string> ienum)
        {
            HashSet<string> set = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            if (ienum != null) foreach (string s in ienum) set.Add(s);
            return set;
        }
    }

    internal static class CommonUtils
    {
        public static bool EqualsIgnoreCase(this string s, string val)
        {
            return StringComparer.InvariantCultureIgnoreCase.Equals(s, val);
        }
        public static string ToStr(this double d)
        {
            return d.ToString(CultureInfo.InvariantCulture);
        }

        public static double ParseDouble(this string value)
        {
            string s = value.Replace(',', '.');
            return double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        public static bool TryParseDouble(this string value, out double result)
        {
            string s = value.Replace(',', '.');
            return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }

        public static T Convert<T>(object obj)
        {
            Type typeT = typeof(T);
            Type underlyingT = Nullable.GetUnderlyingType(typeT);

            if (obj == null || obj is DBNull)
                if (typeT.IsClass || underlyingT != null) return default(T);
                else throw new InvalidCastException("преобразование null в value type");
            return (T)System.Convert.ChangeType(obj, underlyingT ?? typeT);
            // Convert.ChangeType(1, typeof(int?)); Invalid cast from 'System.Int32' to 'System.Nullable'
            // Convert.ChangeType(null, typeof(int?)); Null object cannot be converted to a value type
        }

        public static ParserDateTimeStatus ParseDateTime(string str, out DateTime dt)
        {
            dt = DateTime.Now;
            if (string.IsNullOrEmpty(str)) return ParserDateTimeStatus.Error;
            DateTimeParser dtp = new DateTimeParser();
            return dtp.ParseDateTime(str, out dt);
        }
        public static DateTime? ParseDateTime(string str)
        {
            DateTime dt;
            if (string.IsNullOrEmpty(str)) return null;
            DateTimeParser dtp = new DateTimeParser();
            var r = dtp.ParseDateTime(str, out dt);
            if (r == ParserDateTimeStatus.Date || r == ParserDateTimeStatus.DateTime) return dt;
            return null;
        }

        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }


        public static int ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return (int)Math.Floor(diff.TotalSeconds);
        }

        public static int ConvertToGeomixerTime(TimeSpan ts)
        {
            return (int)ts.TotalSeconds;
        }

        public static TimeSpan ConvertFromGeomixerTime(int ts)
        {
            return new TimeSpan(0, 0, 0, ts);
        }

        public static string BytesToStr(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(2 * bytes.Length);
            foreach (byte b in bytes) sb.Append(b.ToString("x2"));
            string s = sb.ToString();
            return s;
        }

        /// <summary>
        /// Преобразует строку вида FFFFAA в массив байт
        /// </summary>
        public static byte[] StrToBytes(string hex)
        {
            byte[] buf = new byte[hex.Length / 2];
            for (int i = 0, i1 = 0; i < hex.Length; i += 2, i1++)
            {
                buf[i1] = System.Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return buf;
        }


        /*
        public static SimpleTypes? DotNetTypeToSimpleTypeInteral(object value)
        {
            if (value == null) return null;
            var type = value.GetType();
            if (type.IsPrimitive)//The primitive types are Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
            {
                if (type == typeof(bool)) return SimpleTypes.Boolean;
                if (type == typeof(Char)) return SimpleTypes.String;
                if (type == typeof(double)) return SimpleTypes.Float;
                if (type == typeof(Single)) return SimpleTypes.Float;
                
                return SimpleTypes.Integer;
            }
            if (type == typeof(string)) return SimpleTypes.String;
            if (type == typeof(Single)) return SimpleTypes.Float;
            if (type == typeof(decimal)) return SimpleTypes.Float;
            if (type == typeof(DateTime)) return SimpleTypes.DateTime;
            if (type == typeof(TimeSpan)) return SimpleTypes.Time;
            if (type == typeof(OSGeo.OGR.Geometry)) return SimpleTypes.Geometry;
            return null;
        }*/

        public static ISelect FindParentSelect(this SqlToken token)
        {
            var p = token.ParentToken;
            if (p == null) return null;
            if (p is ISelect) return (ISelect)p;
            return p.FindParentSelect();
        }

        public static ITableSource FindParentTableSource(this ISqlConvertible token)
        {
            var p = token.ParentToken;
            if (p == null) return null;
            if (p is ITableSource) return (ITableSource)p;
            return p.FindParentTableSource();
        }
    }
}
