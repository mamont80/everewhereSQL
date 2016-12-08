using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public enum DbColumnType
    {
        Unknow,
        Char,
        VarChar,
        Text,
        Byte,
        SmallInt,
        Integer,
        BigInt,
        Numeric, //decimal
        Real,
        Double,
        Money,
        Blob,
        Date,
        DateTime,
        DateTimeWithTimeZone,
        Time,
        TimeWithTimeZome,
        Boolean,
        Geometry,
        Geography
    }

    public struct ExactType
    {
        public DbColumnType Type;
        /// <summary>
        /// Characters lengths. 0 for text
        /// </summary>
        public int MaxTextLength { get; set; }
        /// <summary>
        /// Precision for NUMERIC
        /// </summary>
        public int Precision { get; set; }
        /// <summary>
        /// Scale for NUMERIC
        /// </summary>
        public int Scale { get; set; }

        public static ExactType Create(DbColumnType tp, int precision = 0, int scale = 0, int maxCharLength = 0)
        {
            ExactType r = new ExactType();
            r.MaxTextLength = maxCharLength;
            r.Precision = precision;
            r.Scale = scale;
            r.Type = tp;
            return r;
        }

        public static ExactType Create(SimpleTypes tp)
        {
            ExactType r = new ExactType();
            r.Type = TypeSimpleToDb(tp);
            return r;
        }

        public SimpleTypes GetSimpleType()
        {
            return TypeDbToSimple(Type, Scale);
        }

        public static int MaxParamsByDbType(DbColumnType dbType)
        {
            if (dbType == DbColumnType.Numeric) return 2;
            if (dbType == DbColumnType.Char || dbType == DbColumnType.VarChar) return 1;
            return 0;
        }

        public static int MinParamsByDbType(DbColumnType dbType)
        {
            if (dbType == DbColumnType.Numeric) return 0;
            if (dbType == DbColumnType.Char || dbType == DbColumnType.VarChar) return 1;
            return 0;
        }

        public static DbColumnType TypeSimpleToDb(SimpleTypes tp)
        {
            switch (tp)
            {
                case SimpleTypes.Blob:
                    return DbColumnType.Blob;
                case SimpleTypes.Boolean:
                    return DbColumnType.Boolean;
                case SimpleTypes.Date:
                    return DbColumnType.Date;
                case SimpleTypes.DateTime:
                    return DbColumnType.DateTime;
                case SimpleTypes.Float:
                    return DbColumnType.Double;
                case SimpleTypes.Geometry:
                    return DbColumnType.Geometry;
                case SimpleTypes.Integer:
                    return DbColumnType.BigInt;
                case SimpleTypes.String:
                    return DbColumnType.Text;
                case SimpleTypes.Time:
                    return DbColumnType.Time;
                default:
                    throw new Exception("Unknow data type");
            }
        }

        public string ToSql(ExpressionSqlBuilder builder)
        {
            switch (Type)
            {
                case DbColumnType.Unknow:
                    return "UNKNOW";
                case DbColumnType.Char:
                    if (builder.DbType == DriverType.SqlServer)
                        return "NCHAR(" + MaxTextLength.ToString() + ")";
                    else
                    return "CHAR(" + MaxTextLength.ToString() + ")";
                case DbColumnType.VarChar:
                    if (builder.DbType == DriverType.SqlServer)
                        return "NVARCHAR(" + MaxTextLength.ToString() + ")";
                    else
                        return "VARCHAR(" + MaxTextLength.ToString() + ")";
                case DbColumnType.Text:
                    if (builder.DbType == DriverType.SqlServer)
                        return "NVARCHAR(MAX)";
                    else
                        return "text";
                case DbColumnType.Byte:
                case DbColumnType.SmallInt:
                    if (builder.DbType == DriverType.SqlServer)
                        return "smallint";
                    else return "int8";
                case DbColumnType.Integer:
                    return "int";
                case DbColumnType.BigInt:
                    return "bigint";
                case DbColumnType.Real:
                    return "real";
                case DbColumnType.Double:
                    if (builder.DbType == DriverType.PostgreSQL) return "double precision";
                    return "float";
                case DbColumnType.Money:
                    return "money";
                case DbColumnType.Blob:
                    if (builder.DbType == DriverType.SqlServer)
                        return "varbinary(max)";
                    else return "bytea";
                case DbColumnType.Date:
                    return "date";
                case DbColumnType.DateTime:
                    if (builder.DbType == DriverType.SqlServer)
                        return "datetime";
                    else return "timestamp without time zone";
                case DbColumnType.Time:
                    if (builder.DbType == DriverType.SqlServer)
                        return "time";
                    else return "time without time zone";
                case DbColumnType.Boolean:
                    if (builder.DbType == DriverType.SqlServer)
                        return "bit";
                    else return "boolean";
                case DbColumnType.Geometry:
                    return "geometry";
                case DbColumnType.Geography:
                    return "geometry";
                case DbColumnType.Numeric:
                    string s = "numeric";
                    if (Precision > 0)
                    {
                        s += "(" + Precision.ToString();
                        if (Scale > 0) s += ", " + Scale.ToString();
                        s += ")";
                    }
                    return s;
                case DbColumnType.DateTimeWithTimeZone:
                    if (builder.DbType == DriverType.SqlServer)
                        return "DATETIME";
                    else return "timestamp with time zone";
                case DbColumnType.TimeWithTimeZome:
                    if (builder.DbType == DriverType.SqlServer)
                        return "time";
                    else
                        return "time with time zone";
                default:
                    throw new Exception("Unknow data type");
            }
        }

        public override string ToString()
        {
            return ToStr();
        }

        public string ToStr()
        {
            switch (Type)
            {
                case DbColumnType.Unknow:
                    return "UNKNOW";
                case DbColumnType.Char:
                    return "CHAR(" + MaxTextLength.ToString() + ")";
                case DbColumnType.VarChar:
                    return "VARCHAR(" + MaxTextLength.ToString() + ")";
                case DbColumnType.Text:
                case DbColumnType.Byte:
                case DbColumnType.SmallInt:
                case DbColumnType.Integer:
                case DbColumnType.BigInt:
                case DbColumnType.Real:
                case DbColumnType.Double:
                case DbColumnType.Money:
                case DbColumnType.Blob:
                case DbColumnType.Date:
                case DbColumnType.DateTime:
                case DbColumnType.Time:
                case DbColumnType.Boolean:
                case DbColumnType.Geometry:
                case DbColumnType.Geography:
                    return Type.ToString().ToUpper();
                case DbColumnType.Numeric:
                    string s = "NUMERIC";
                    if (Precision > 0)
                    {
                        s += "(" + Precision.ToString();
                        if (Scale > 0) s += ", " + Scale.ToString();
                        s += ")";
                    }
                    return s;
                case DbColumnType.DateTimeWithTimeZone:
                    return "DATETIME WITH TIME ZONE";
                case DbColumnType.TimeWithTimeZome:
                    return "TIME WITH TIME ZONE";
                default:
                    throw new Exception("Unknow data type");
            }
        }

        public static SimpleTypes TypeDbToSimple(DbColumnType tp, int scale)
        {
            if (tp == DbColumnType.Numeric)
            {
                if (scale > 0) return SimpleTypes.Float;
                else return SimpleTypes.Integer;
            }
            switch (tp)
            {
                case DbColumnType.Byte:
                case DbColumnType.BigInt:
                case DbColumnType.Integer:
                case DbColumnType.SmallInt:
                    return SimpleTypes.Integer;
                case DbColumnType.Blob:
                    return SimpleTypes.Blob;
                case DbColumnType.Boolean:
                    return SimpleTypes.Boolean;
                case DbColumnType.Char:
                case DbColumnType.VarChar:
                case DbColumnType.Text:
                    return SimpleTypes.String;
                case DbColumnType.Date:
                    return SimpleTypes.Date;
                case DbColumnType.DateTime:
                case DbColumnType.DateTimeWithTimeZone:
                    return SimpleTypes.DateTime;
                case DbColumnType.Time:
                case DbColumnType.TimeWithTimeZome:
                    return SimpleTypes.Time;
                case DbColumnType.Money:
                case DbColumnType.Double:
                case DbColumnType.Real:
                    return SimpleTypes.Float;
                case DbColumnType.Geography:
                case DbColumnType.Geometry:
                    return SimpleTypes.Geometry;
                default:
                    throw new Exception("Unknow data type");
            }
        }

        public static ExactType? Parse(LexemCollection collection)
        {
            ExactType res;
            var lex = collection.CurrentLexem();
            if (lex.LexemType != LexType.Command) collection.Error("Unknow data type", collection.CurrentLexem());
            string s = lex.LexemText.ToLower();
            DbColumnType tp = DbColumnType.Unknow;
            int param1 = 0;
            int param2 = 0;
            switch (s)
            {
                case "datetime":
                    tp = DbColumnType.DateTime;
                    ReadSub1Number(collection, out param1);//ignore
                    break;
                case "timestamp":
                    tp = DbColumnType.DateTime;
                    if (ParserUtils.ParseCommandPhrase(collection, "timestamp without time zone", true, false)) tp = DbColumnType.DateTime;
                    if (ParserUtils.ParseCommandPhrase(collection, "timestamp with time zone", true, false)) tp = DbColumnType.DateTimeWithTimeZone;
                    ReadSub1Number(collection, out param1);//ignore
                    break;
                case "date":
                    tp = DbColumnType.Date;
                    ReadSub1Number(collection, out param1);//ignore
                    break;
                case "time":
                    tp = DbColumnType.Time;
                    if (ParserUtils.ParseCommandPhrase(collection, "time without time zone", true, false)) tp = DbColumnType.Time;
                    if (ParserUtils.ParseCommandPhrase(collection, "time with time zone", true, false)) tp = DbColumnType.TimeWithTimeZome;
                    ReadSub1Number(collection, out param1);//ignore
                    break;
                case "int8":
                case "byte":
                case "tinyint":
                    tp = DbColumnType.Byte;
                    break;
                case "int16":
                case "smallint":
                    tp = DbColumnType.SmallInt;
                    break;
                case "int32":
                case "integer":
                case "int":
                    tp = DbColumnType.Integer;
                    break;
                case "float":
                case "real":
                    tp = DbColumnType.Real;
                    ReadSub1Number(collection, out param1);//ignore
                    break;
                case "double":
                    ParserUtils.ParseCommandPhrase(collection, "double precision");
                    tp = DbColumnType.Double;
                    break;
                case "bigint":
                case "int64":
                    tp = DbColumnType.BigInt;
                    break;
                case "decimal":
                case "numeric":
                    tp = DbColumnType.Numeric;
                    ReadSub2Number(collection, out param1, out param2);
                    break;
                case "nvarchar":
                case "varchar":
                    tp = DbColumnType.VarChar;
                    if (ReadSub1Number(collection, out param1))
                    {
                        if (param1 == 0) tp = DbColumnType.Text;
                    }
                    break;
                case "nchar":
                case "char":
                    tp = DbColumnType.VarChar;
                    if (!ReadSub1Number(collection, out param1))
                    {
                        collection.ErrorUnexpected(collection.CurrentOrLast());
                    }
                    break;
                case "character":
                    tp = DbColumnType.Char;
                    if (ParserUtils.ParseCommandPhrase(collection, "character varying"))
                    {
                        tp = DbColumnType.VarChar;
                    }
                    if (!ReadSub1Number(collection, out param1))
                    {
                        collection.ErrorUnexpected(collection.CurrentOrLast());
                    }
                    break;
                case "text":
                    tp = DbColumnType.Text;
                    break;
                case "blob":
                    tp = DbColumnType.Blob;
                    break;
                case "bit":
                case "bool":
                case "boolean":
                    tp = DbColumnType.Boolean;
                    break;
                case "geometry":
                    tp = DbColumnType.Geometry;
                    break;
                case "geography":
                    tp = DbColumnType.Geography;
                    break;
                default:
                    return null;
            }
            res = ExactType.Create(tp);
            if (tp == DbColumnType.Numeric)
            {
                res.Precision = param1;
                res.Scale = param2;
            }
            if (tp == DbColumnType.Char || tp == DbColumnType.VarChar)
            {
                res.MaxTextLength = param1;
            }
            return res;
        }

        private static bool ReadSub1Number(LexemCollection collection, out int i1)
        {
            i1 = 0;
            var lex = collection.GetNext();
            if (lex != null && lex.IsSkobraOpen())
            {
                collection.GotoNextMust();
                lex = collection.GotoNextMust();
                if (lex.LexemType == LexType.Number)
                {
                    i1 = int.Parse(lex.LexemText);
                }
                else if (lex.LexemType == LexType.Command)
                {
                    if (lex.LexemText.ToLower() == "max") i1 = 0;
                }
                else collection.ErrorUnexpected(collection.CurrentOrLast());
                lex = collection.GotoNextMust();
                if (lex == null || !lex.IsSkobraClose()) collection.ErrorUnexpected(collection.CurrentOrLast());
                return true;
            }
            return false;
        }

        private static bool ReadSub2Number(LexemCollection collection, out int i1, out int i2)
        {
            i1 = 0;
            i2 = 0;
            var lex = collection.GetNext();
            if (lex != null && lex.IsSkobraOpen())
            {
                collection.GotoNextMust();
                lex = collection.GotoNextMust();
                if (lex.LexemType == LexType.Number)
                {
                    i1 = int.Parse(lex.LexemText);
                }
                else collection.ErrorUnexpected(collection.CurrentOrLast());
                lex = collection.GotoNextMust();
                if (lex == null) collection.ErrorUnexpected(collection.CurrentOrLast());
                if (lex.LexemType == LexType.Zpt)
                {
                    lex = collection.GotoNextMust();
                    if (lex.LexemType != LexType.Number) collection.ErrorUnexpected(collection.CurrentOrLast());
                    i2 = int.Parse(lex.LexemText);
                    lex = collection.GotoNextMust();
                }
                if (lex == null || !lex.IsSkobraClose()) collection.ErrorUnexpected(collection.CurrentOrLast());
                return true;
            }
            return false;
        }
    }
}
