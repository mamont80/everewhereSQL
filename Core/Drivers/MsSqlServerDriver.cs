using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class MsSqlServerDriver : IDbDriver
    {
        public DbDriverType DbDriverType { get { return DbDriverType.SqlServer; } }

        public string ToSql(Expression exp, ExpressionSqlBuilder builder)
        {
            if (exp is ConstExpr)
            {
                ConstExpr e = exp as ConstExpr;
                return ObjectToStringSql(e.GetObjectResultOut(null));
            }
            ///TODO: Fix for geomixer
            /*
            if (exp is GeometryFromWkbHex)
            {
                return "geometry::STGeomFromWKB(CONVERT(VARBINARY(MAX),'0x'+" + (exp as GeometryFromWkbHex).Operand1.ToSQL(builder) + ", 1), 0)";
            }
            
            //if (exp is GeometryFromWKT)
            //{
            //    return "geometry::STGeomFromText(" + (exp as GeometryFromWKT).Operand.ToSQL(builder) + ", 0)";
            //}
            if (exp is GeometryToWkbHex)
            {//берём байты, переводим в строку и удаляем в начале строки символы 0x
                return "STUFF(master.sys.fn_varbintohexstr(" + (exp as GeometryToWkbHex).Operand.ToSQL(builder) + ".STAsBinary()),1,2,'')";
            }
            if (exp is GeometryIntersects)
            {
                return "(" + (exp as GeometryIntersects).Operand1.ToSQL(builder) + ".STIntersects(" + (exp as GeometryIntersects).Operand2.ToSQL(builder) + ") =1)";
            }
            if (exp is MakePoint)
            {
                return "CASE WHEN ((" + (exp as MakePoint).Operand1.ToSQL(builder) + " IS NOT NULL) and (" + (exp as MakePoint).Operand2.ToSQL(builder) + " IS NOT NULL))" +
                    " THEN geometry::STGeomFromText('POINT ('+cast(" + (exp as MakePoint).Operand1.ToSQL(builder) + " as nvarchar)+' '+cast(" + (exp as MakePoint).Operand2.ToSQL(builder) + " as nvarchar)+')',0)"
                    + " ELSE geometry::STGeomFromText('POINT EMPTY',0) END";
            }
            if (exp is GeomIsEmpty)
            {
                return string.Format("COALESCE({0}.STIsEmpty(), 1)", (exp as GeomIsEmpty).Operand.ToSQL(builder));
            }
            if (exp is GeometryEnvelopeMinX)
            {
                return string.Format("{0}.STEnvelope().STPointN(1).STX", (exp as FuncExpr_OneOperand).Operand.ToSQL(builder));
            }
            if (exp is GeometryEnvelopeMinY)
            {
                return string.Format("{0}.STEnvelope().STPointN(1).STY", (exp as FuncExpr_OneOperand).Operand.ToSQL(builder));
            }
            if (exp is GeometryEnvelopeMaxX)
            {
                return string.Format("{0}.STEnvelope().STPointN(3).STX", (exp as FuncExpr_OneOperand).Operand.ToSQL(builder));
            }
            if (exp is GeometryEnvelopeMaxY)
            {
                return string.Format("{0}.STEnvelope().STPointN(3).STY", (exp as FuncExpr_OneOperand).Operand.ToSQL(builder));
            }*/
            if (exp is StrToTime)
            {
                return string.Format("CAST ({0} as time(7))", (exp as StrToTime).Operand.ToSQL(builder));
            }
            if (exp is StrToDateTime)
            {
                return string.Format("CAST ({0} as datetime(7))", (exp as StrToDateTime).Operand.ToSQL(builder));
            }
            if (exp is Now)
            {
                return "GetDate()";
            }

            if (exp is Plus_Arifmetic || exp is Minus_Arifmetic)
            {
                string op = "+";
                if (exp is Minus_Arifmetic) op = "-";
                ColumnSimpleTypes t1 = (exp as Plus_Arifmetic).Operand1.GetResultType();
                ColumnSimpleTypes t2 = (exp as Plus_Arifmetic).Operand2.GetResultType();
                if (t2 == ColumnSimpleTypes.Time && t1 == ColumnSimpleTypes.Time)
                {
                    return "CONVERT(time, CONVERT(datetime, " + (exp as Custom_Arifmetic).Operand1.ToSQL(builder) + ")" + op + "CONVERT(datetime, " + (exp as Custom_Arifmetic).Operand2.ToSQL(builder) + "))";
                }
                if (t1 == ColumnSimpleTypes.Date && t2 == ColumnSimpleTypes.Time)
                {
                    return "(CONVERT(datetime, " + (exp as Custom_Arifmetic).Operand1.ToSQL(builder) + ")" + op + (exp as Custom_Arifmetic).Operand2.ToSQL(builder) + ")";
                }
                if (t2 == ColumnSimpleTypes.Date && t1 == ColumnSimpleTypes.Time)
                {
                    return "(CONVERT(datetime, " + (exp as Custom_Arifmetic).Operand2.ToSQL(builder) + ")" + op + (exp as Custom_Arifmetic).Operand1.ToSQL(builder) + ")";
                }
                if (t1 == ColumnSimpleTypes.Geometry && t2 == ColumnSimpleTypes.Geometry)
                {
                    if (op == "+") return "(" + (exp as Custom_Arifmetic).Operand2.ToSQL(builder) + ".STUnion(" + (exp as Custom_Arifmetic).Operand1.ToSQL(builder) + "))";
                    if (op == "-") return "(" + (exp as Custom_Arifmetic).Operand2.ToSQL(builder) + ".STDifference(" + (exp as Custom_Arifmetic).Operand1.ToSQL(builder) + "))";
                }
                return "(" + (exp as Plus_Arifmetic).Operand1.ToSQL(builder) + op + (exp as Plus_Arifmetic).Operand2.ToSQL(builder) + ")";
            }
            if (exp is AddMinutes)
            {
                return "DATEADD(minute," + (exp as AddMinutes).Operand2.ToSQL(builder) + "," + (exp as AddMinutes).Operand1.ToSQL(builder) + ")";
            }
            if (exp is AddHours)
            {
                return "DATEADD(hour," + (exp as AddMinutes).Operand2.ToSQL(builder) + "," + (exp as AddMinutes).Operand1.ToSQL(builder) + ")";
            }
            if (exp is AddDays)
            {
                return "DATEADD(day," + (exp as AddMinutes).Operand2.ToSQL(builder) + "," + (exp as AddMinutes).Operand1.ToSQL(builder) + ")";
            }
            if (exp is AddSeconds)
            {
                return "DATEADD(second," + (exp as AddMinutes).Operand2.ToSQL(builder) + "," + (exp as AddMinutes).Operand1.ToSQL(builder) + ")";
            }
            if (exp is Day)
            {
                return "(DAY(" + (exp as Day).Operand.ToSQL(builder) + "))";
            }
            if (exp is Month)
            {
                return "(MONTH(" + (exp as Month).Operand.ToSQL(builder) + "))";
            }
            if (exp is Year)
            {
                return "(YEAR(" + (exp as Year).Operand.ToSQL(builder) + "))";
            }
            if (exp is Length_operation)
            {
                return "(len(" + (exp as Length_operation).Operand.ToSQL(builder) + "))";
            }
            throw new Exception("Unknow expression");
        }

        public virtual string ObjectToStringSql(object obj)
        {
            char quote = '\'';
            if (obj == null) return "null";
            if (obj is string)
            {
                return quote + EncodeStrForSql(obj.ToString()) + quote;
            }
            ///TODO: fix for geometry
            /*
            else if (obj is Geometry)
            {
                string wkt = ((Geometry)obj).ToStringWKT();
                return " geometry::STGeomFromText('" + wkt + "',0)";
            }*/
            else if (obj is bool)
            {
                if ((bool)obj) return "(1=1)";
                else return "(1<>1)";
            }
            else
                if (obj is decimal || obj is double || obj is float)
                {
                    return Convert.ToDouble(obj).ToStr();
                }
                else if (obj is DateTime)
                {
                    DateTime d = (DateTime)obj;
                    return " CAST(" + quote + d.ToString("yyyy-MM-dd HH:mm:ss") + quote + " AS datetime2) ";
                }
                else if (obj is TimeSpan)
                {
                    TimeSpan d = (TimeSpan)obj;
                    return " CAST(" + quote + d.ToString("c") + quote + " AS time) ";
                }
                else return obj.ToString();
        }

        public string EncodeStrForSql(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            StringBuilder sb = new StringBuilder();
            foreach (var c in s)
            {
                if (c == '\'') sb.Append("''"); else sb.Append(c);

            }
            return sb.ToString();
        }
    }
}
